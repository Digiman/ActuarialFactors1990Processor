'''
Converter for the IRS root (non-mortality) actuarial tables into JSON files
that follow the format of the processed root JSON files in JSONFiles/.

The root tables are not mortality based, so one official file serves every
series (90CM / 2000CM / 2010CM). Official sources
(https://www.irs.gov/retirement-plans/actuarial-tables):

    DataFiles/TableB.xlsx  (IRS table-b-final.xlsx)  Table B - annuity, income,
                             and remainder interests for a term certain
    DataFiles/TableD.xls   (IRS table-d.xls)         Table D - unitrust
                             remainder factors for a term of years
    DataFiles/TableF.xls   (IRS table-f.xls)         Table F - unitrust
                             adjusted payout rate factors
    DataFiles/TableK.xlsx  (IRS table-k-final.xlsx)  Table K - adjustment
                             factors, annuities paid at end of interval
    DataFiles/TableJ.xlsx  (IRS table-j-final.xlsx)  Table J - adjustment
                             factors, annuities paid at beginning of interval

Output files (in JSONFiles/):
    TableB.json  TableD.json  TableF.json  TableK.json  TableJ.json

Interest rate / payout rate grid: 0.2 - 20.0 percent by 0.2 (100 rates).
'''

import json
import re

import openpyxl
import xlrd

B_FILE = 'DataFiles/TableB.xlsx'
D_FILE = 'DataFiles/TableD.xls'
F_FILE = 'DataFiles/TableF.xls'
K_FILE = 'DataFiles/TableK.xlsx'
J_FILE = 'DataFiles/TableJ.xlsx'

FREQS = ['Annual', 'Semiannual', 'Quarterly', 'Monthly', 'Weekly']
F_FREQS = ['Annual', 'Semiannual', 'Quarterly', 'Monthly']


def clean_number(value):
    '''Return value as int if integral, else as float rounded to 10 decimals.'''
    value = float(value)
    if value.is_integer():
        return int(value)
    return round(value, 10)


def rate_percent(value):
    '''Convert an IRS decimal rate (0.002) to the repo's percent form (0.2).'''
    return round(float(value) * 100, 6)


def write_json(filename, rows):
    with open(filename, 'w') as result_file:
        json.dump(rows, result_file, separators=(',', ':'))
    print('Wrote {0}: {1} rows'.format(filename, len(rows)))


def is_number(value):
    return isinstance(value, (int, float)) and not isinstance(value, bool)


#--------------------------------------------------------------
# Table B: worksheets with two rate blocks side by side per page.
# Each page: rate header row (rates at cols 4 and 12), column header
# row, then year rows; per block: Years/Annuity/Income/Remainder
# at cols base+0/2/4/6 with base 0 and 8.
#--------------------------------------------------------------

def convert_b():
    workbook = openpyxl.load_workbook(B_FILE, read_only=True)
    try:
        rows = list(workbook.worksheets[0].iter_rows(values_only=True))
    finally:
        workbook.close()

    result = []
    rates_seen = []
    current_rates = None
    year_rows = []
    for row in rows:
        if row[0] is None and is_number(row[4]) and row[4] <= 0.2:
            if current_rates is not None:
                emit_b_page(result, current_rates, year_rows)
            current_rates = (rate_percent(row[4]), rate_percent(row[12]))
            rates_seen.extend(current_rates)
            year_rows = []
            continue
        if current_rates is not None and is_number(row[0]):
            year_rows.append(row)
    if current_rates is not None:
        emit_b_page(result, current_rates, year_rows)

    assert rates_seen == [round(0.2 + 0.2 * i, 1) for i in range(100)], rates_seen
    years = sorted(set(row['Years'] for row in result))
    assert years == [float(i) for i in range(1, 61)], years
    write_json('JSONFiles/TableB.json', result)


def emit_b_page(result, rates, year_rows):
    for block_index, rate in enumerate(rates):
        base = 8 * block_index
        for row in year_rows:
            annuity, income, remainder = row[base + 2], row[base + 4], row[base + 6]
            if annuity is None:
                continue
            result.append({'Years': float(row[0]), 'Rate': rate,
                           'PvAnnuity': clean_number(annuity),
                           'PvIncomeInterest': clean_number(income),
                           'PvRemainderInterest': clean_number(remainder)})


#--------------------------------------------------------------
# Table D: sections with 10 adjusted payout rates per rate header
# row (cols 2..11), year rows below (years in col 1).
#--------------------------------------------------------------

def convert_d():
    book = xlrd.open_workbook(D_FILE)
    sheet = book.sheet_by_index(0)

    result = []
    rates_seen = []
    current_rates = None
    year_rows = []
    for row_index in range(sheet.nrows):
        years_cell = str(sheet.cell_value(row_index, 1)).strip()
        first = sheet.cell_value(row_index, 2)
        if years_cell == '' and is_number(first) and 0 < first <= 0.2:
            if current_rates is not None:
                emit_d_section(result, current_rates, year_rows)
            current_rates = [rate_percent(sheet.cell_value(row_index, col))
                             for col in range(2, 12)]
            rates_seen.extend(current_rates)
            year_rows = []
            continue
        if current_rates is not None and is_number(sheet.cell_value(row_index, 1)) \
                and sheet.cell_value(row_index, 1) > 0:
            year = int(sheet.cell_value(row_index, 1))
            values = [sheet.cell_value(row_index, col) for col in range(2, 12)]
            year_rows.append((year, values))
    if current_rates is not None:
        emit_d_section(result, current_rates, year_rows)

    assert rates_seen == [round(0.2 + 0.2 * i, 1) for i in range(100)], rates_seen
    years = sorted(set(row['Years'] for row in result))
    assert years == list(range(1, 21)), years
    write_json('JSONFiles/TableD.json', result)


def emit_d_section(result, rates, year_rows):
    for rate_index, rate in enumerate(rates):
        for year, values in year_rows:
            result.append({'Years': year, 'PayoutRate': rate,
                           'RemainderInterest': clean_number(values[rate_index])})


#--------------------------------------------------------------
# Table F: one section per rate ("Interest at x.x Percent"), each
# with 26 month-band rows; factor columns Annual/Semiannual/
# Quarterly/Monthly at cols 3/5/7/9 (stored as text).
#--------------------------------------------------------------

def convert_f():
    book = xlrd.open_workbook(F_FILE)
    sheet = book.sheet_by_index(0)

    result = []
    rates_seen = []
    band_rows = []
    for row_index in range(sheet.nrows):
        heading = sheet.cell_value(row_index, 1)
        match = re.match(r'Interest at (\d+\.\d) Percent', str(heading))
        if match:
            if rates_seen:
                emit_f_section(result, rates_seen[-1], band_rows)
            rates_seen.append(float(match.group(1)))
            band_rows = []
            continue
        band = str(sheet.cell_value(row_index, 0)).strip()
        if not rates_seen or (band != '--' and not band.isdigit()):
            continue
        months = 0 if band == '--' else int(band)
        values = [str(sheet.cell_value(row_index, 3 + 2 * freq_index)).strip()
                  for freq_index in range(len(F_FREQS))]
        band_rows.append((months, values))
    if rates_seen:
        emit_f_section(result, rates_seen[-1], band_rows)

    assert rates_seen == [round(0.2 + 0.2 * i, 1) for i in range(100)], rates_seen
    expected_months = {'Annual': 13, 'Semiannual': 7, 'Quarterly': 4, 'Monthly': 2}
    for freq, count in expected_months.items():
        months = sorted(set(row['Months'] for row in result if row['Frequency'] == freq))
        assert months == list(range(count)), '{0}: months {1}'.format(freq, months)
    write_json('JSONFiles/TableF.json', result)


def emit_f_section(result, rate, band_rows):
    for freq_index, freq in enumerate(F_FREQS):
        for months, values in band_rows:
            text = values[freq_index]
            if not text:
                continue
            result.append({'InterestRate': round(rate, 6), 'Frequency': freq,
                           'Months': months, 'AdjustmentFactor': round(float(text), 10)})


#--------------------------------------------------------------
# Tables K and J: one worksheet, rate rows with factor columns
# Annually/Semiannually/Quarterly/Monthly/Weekly at cols 0,2,4,6,8,10
# (empty spacer columns between).
#--------------------------------------------------------------

def convert_adjustment_table(filename, out_filename):
    workbook = openpyxl.load_workbook(filename, read_only=True)
    try:
        rows = list(workbook.worksheets[0].iter_rows(values_only=True))
    finally:
        workbook.close()

    result = []
    rates_seen = []
    for row in rows:
        by_col = {index: cell for index, cell in enumerate(row) if cell is not None}
        if set(by_col) != {0, 2, 4, 6, 8, 10}:
            continue
        rate = by_col[0]
        if not is_number(rate) or rate > 0.2:
            continue
        rates_seen.append(rate_percent(rate))
        for freq, col in zip(FREQS, (2, 4, 6, 8, 10)):
            result.append({'InterestRate': rate_percent(rate), 'Frequency': freq,
                           'AdjustmentFactor': round(float(by_col[col]), 10)})

    assert rates_seen == [round(0.2 + 0.2 * i, 1) for i in range(100)], rates_seen
    write_json(out_filename, result)


def main():
    convert_b()
    convert_d()
    convert_f()
    convert_adjustment_table(K_FILE, 'JSONFiles/TableK.json')
    convert_adjustment_table(J_FILE, 'JSONFiles/TableJ.json')
    print('Done.')


if __name__ == '__main__':
    main()
