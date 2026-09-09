'''
Converter for IRS 2000CM actuarial factor tables (official IRS spreadsheets
from https://www.irs.gov/retirement-plans/actuarial-tables) into JSON files
that follow the same format as the other processed series in JSONFiles/.

The 2000CM era (May 2009 - May 2023) spreadsheets are a mix of legacy .xls
(read with xlrd) and .xlsx (read with openpyxl). Sheet layouts match the
2010CM files: two age blocks per section (S/C/H/Z), 10-rate column blocks
(U(1)), and 5 rate parts (U(2)/R(2)). Rate headers differ: S/C carry
"Interest at x.x%", H/Z carry "Table X(x.x)".

Source files (in DataFiles/2000CM/):
    table-s-2000cm.xlsx     Table S  - single life factors
    table-h-2000cm.xls      Table H  - commutation factors
    table-c-2000cm.xls      Table C  - factors for reducing assurances
    table-z-2000cm.xls      Table Z  - unitrust commutation factors
    table-u1-2000cm.xls     Table U(1) - one-life unitrust remainder factors
    table-u2-2009cm.xlsx    Table U(2) - two-life unitrust remainder factors (5 parts)
    table-r2-2009.xlsx      Table R(2) - two-life remainder factors (5 parts)
    table-2000cm.xls        Table 2000CM - mortality table (lx)

Output files (in JSONFiles/):
    TableS-2000CM-processed.json
    TableH-2000CM-processed.json
    TableC-2000CM-processed.json
    TableZ-2000CM-processed.json
    TableU1-2000CM-processed.json
    TableU(2)-p1..p5-2000CM-processed.json
    TableR(2)-p1..p5-2000CM-processed.json
    MortalityTable.json (regenerated with Year 2000 rows merged in)

Interest rate grid: 0.2 - 20.0 percent by 0.2 (100 rates).
Ages: Table S/H/C/Z = 0..109, Table U(1) = 0..109, Table U(2)/R(2) = 0..109.
'''

import json
import re

import openpyxl
import xlrd

SRC_DIR = 'DataFiles/2000CM'
OUT_DIR = 'JSONFiles/2000CM'
MORTALITY_FILE = 'JSONFiles/MortalityTable.json'
MORTALITY_YEAR = 2000
EXPECTED_RATES = [round(0.2 + 0.2 * i, 1) for i in range(100)]

SHEET_EXTENSIONS = ('.xls', '.xlsx')


def clean_number(value):
    '''Return value as int if integral, else as float rounded to 10 decimals.'''
    if value is None or isinstance(value, str):
        return value
    if isinstance(value, bool):
        return value
    value = float(value)
    if value.is_integer():
        return int(value)
    return round(value, 10)


def parse_rate(cell):
    '''Parse a rate cell like '0.2%', 'Interest at 0.2%' or 'Table H(0.2)'.'''
    if cell is None:
        return None
    match = re.search(r'(\d+\.\d)', str(cell).strip())
    return float(match.group(1)) if match else None


def is_interest_header(cell):
    '''True for the 2000CM rate headers: "Interest at x.x%" or "Table X(x.x)".'''
    if cell is None:
        return False
    text = str(cell)
    return 'Interest' in text or re.search(r'Table [A-Z0-9]+\(\d+\.\d\)', text) is not None


def is_age_cell(value):
    if value is None:
        return False
    if isinstance(value, bool):
        return False
    if isinstance(value, int):
        return True
    if isinstance(value, float):
        return value.is_integer()
    text = str(value).strip()
    return text.isdigit()


def to_age(value):
    return int(float(value))


def load_sheet(filename, sheet_name):
    '''Reads one sheet into a list of tuples, from .xlsx (openpyxl) or .xls (xlrd).'''
    if filename.endswith('.xlsx'):
        workbook = openpyxl.load_workbook(filename, read_only=True)
        try:
            return list(workbook[sheet_name].iter_rows(values_only=True))
        finally:
            workbook.close()
    sheet = xlrd.open_workbook(filename).sheet_by_name(sheet_name)
    return [tuple(sheet.row_values(row_index)) for row_index in range(sheet.nrows)]


def sheet_names(filename):
    if filename.endswith('.xlsx'):
        workbook = openpyxl.load_workbook(filename, read_only=True)
        try:
            return list(workbook.sheetnames)
        finally:
            workbook.close()
    return xlrd.open_workbook(filename).sheet_names()


def write_json(filename, rows):
    with open(filename, 'w') as result_file:
        json.dump(rows, result_file, separators=(',', ':'))
    print('Wrote {0}: {1} rows'.format(filename, len(rows)))


#--------------------------------------------------------------
# Tables S, C, H, Z: one worksheet, rate sections with two age
# blocks each (ages 0..54 left, 55..109 right).
#--------------------------------------------------------------

def convert_section_table(filename, sheet_name, out_filename, rows_builder, label,
                          stride=4, value_indices=(1, 2, 3)):
    rows = load_sheet(filename, sheet_name)
    result = []
    current_rate = None
    rates_seen = []
    for row in rows:
        rate = None
        for cell in row:
            if is_interest_header(cell):
                rate = parse_rate(cell)
                if rate is not None:
                    break
        if rate is not None:
            # some sheets repeat the rate in a section title and a subheader
            if not rates_seen or rate != rates_seen[-1]:
                current_rate = rate
                rates_seen.append(rate)
            continue
        if current_rate is None:
            continue
        for index in (0, 1):
            left = index == 0
            offset = 0 if left else stride
            age_cell = row[offset] if offset < len(row) else None
            if not is_age_cell(age_cell):
                continue
            age = to_age(age_cell)
            values = [row[offset + value_index] for value_index in value_indices]
            if any(value is None for value in values):
                continue
            result.append(rows_builder(current_rate, age, values))
    write_json(out_filename, result)
    assert rates_seen == EXPECTED_RATES, '{0}: {1} rates != expected'.format(label, rates_seen)
    ages = sorted(set(row['age'] for row in result))
    assert ages == list(range(109 + 1)), '{0}: ages {1}'.format(label, ages)
    print('{0}: {1} rates, ages {2}..{3}'.format(label, len(rates_seen), ages[0], ages[-1]))
    return result


def build_s_row(rate, age, values):
    return {'mortalityTable': MORTALITY_YEAR, 'interestRate': rate, 'age': age,
            'pvAnnuity': clean_number(values[0]), 'pvLifeEstate': clean_number(values[1]),
            'pvReminderInterest': clean_number(values[2])}


def build_h_row(rate, age, values):
    return {'mortalityTable': MORTALITY_YEAR, 'interestRate': rate, 'age': age,
            'dFactor': clean_number(values[0]), 'nFactor': clean_number(values[1]),
            'mFactor': clean_number(values[2])}


def build_c_row(rate, age, values):
    return {'mortalityTable': MORTALITY_YEAR, 'rate': rate, 'age': age,
            'remainderFactor': clean_number(values[0]), 'rFactor': clean_number(values[1]),
            'dFactor': clean_number(values[2])}


def convert_commutation_tables():
    convert_section_table(SRC_DIR + '/table-s-2000cm.xlsx', 'Sheet1',
                          OUT_DIR + '/TableS-2000CM-processed.json', build_s_row, 'Table S (2000CM)')
    convert_section_table(SRC_DIR + '/table-h-2000cm.xls', 'H-20CM-2',
                          OUT_DIR + '/TableH-2000CM-processed.json', build_h_row, 'Table H (2000CM)',
                          stride=8, value_indices=(2, 4, 6))
    convert_section_table(SRC_DIR + '/table-c-2000cm.xls', 'Sheet1',
                          OUT_DIR + '/TableC-2000CM-processed.json', build_c_row, 'Table C (2000CM)',
                          stride=8, value_indices=(2, 4, 6))
    convert_section_table(SRC_DIR + '/table-z-2000cm.xls', 'Sheet1',
                          OUT_DIR + '/TableZ-2000CM-processed.json', build_h_row, 'Table Z (2000CM)',
                          stride=8, value_indices=(2, 4, 6))


#--------------------------------------------------------------
# Table U(1): one worksheet, 10 blocks of 10 "Adjusted Payout Rate"
# columns each, ages 0..109 per block.
#--------------------------------------------------------------

def convert_u1():
    rows = load_sheet(SRC_DIR + '/table-u1-2000cm.xls', 'Table U #1')
    result = []
    current_rates = None
    rates_seen = []
    for row in rows:
        rates = None
        if len(row) > 1 and row[1] is not None and '%' in str(row[1]):
            parsed_rates = [parse_rate(cell) for cell in row[1:11]]
            if all(rate is not None for rate in parsed_rates):
                rates = parsed_rates
        if rates is not None:
            current_rates = rates
            rates_seen.extend(rates)
            continue
        if current_rates is None or not is_age_cell(row[0]):
            continue
        age = to_age(row[0])
        for rate_index, rate in enumerate(current_rates):
            value = row[1 + rate_index]
            result.append({'mortalityTable': MORTALITY_YEAR,
                           'adjustedPayoutRate': rate, 'age': age,
                           'remainderFactor': clean_number(value)})
    write_json(OUT_DIR + '/TableU1-2000CM-processed.json', result)
    deduped_rates = list(dict.fromkeys(rates_seen))
    assert deduped_rates == EXPECTED_RATES, 'U(1): rates {0}'.format(deduped_rates)
    ages = sorted(set(row['age'] for row in result))
    assert ages == list(range(109 + 1)), 'U(1): ages {0}'.format(ages)
    print('Table U(1) (2000CM): {0} rates, ages {1}..{2}'.format(len(deduped_rates), ages[0], ages[-1]))


#--------------------------------------------------------------
# Tables U(2) and R(2): 5 worksheets each (rate parts of 20 rates),
# rows are (age1, age2) pairs (age2 <= age1), columns are the rates.
#--------------------------------------------------------------

def convert_two_life_table(filename, out_parts_dir, prefix, header_word, expected_rates_per_part):
    sheets = sheet_names(filename)
    for part_index, sheet in enumerate(sheets):
        rows = load_sheet(filename, sheet)
        result = []
        parse_rates = None
        for row in rows:
            header = header_word in ' '.join(str(cell) for cell in row if cell is not None)
            if header:
                continue
            rates = [parse_rate(cell) for cell in row[2:22]]
            if all(rate is not None for rate in rates):
                parse_rates = rates
                continue
            if parse_rates is None:
                continue
            if len(row) < 3 or not is_age_cell(row[0]) or not is_age_cell(row[1]):
                continue
            age1 = to_age(row[0])
            age2 = to_age(row[1])
            for rate_index, rate in enumerate(parse_rates):
                value = row[2 + rate_index]
                result.append({'mortalityTable': MORTALITY_YEAR, 'age1': age1, 'age2': age2,
                               'adjustedPayoutRate': rate, 'remainderFactor': clean_number(value)})
        rates_seen = sorted(set(parse_rates))
        part_expected = expected_rates_per_part[part_index * 20:(part_index + 1) * 20]
        assert rates_seen == part_expected, '{0} part {1}: rates {2}'.format(prefix, part_index, rates_seen)
        pairs = sorted(set((row['age1'], row['age2']) for row in result))
        assert pairs[0] == (0, 0) and (109, 109) in pairs, '{0} part {1}: pairs {2}'.format(prefix, part_index, pairs[:3])
        assert len(pairs) == 6105, '{0} part {1}: {2} pairs'.format(prefix, part_index, len(pairs))
        write_json('{0}/{1}-p{2}-2000CM-processed.json'.format(out_parts_dir, prefix, part_index + 1), result)
        print('{0} part {1} ({2}): {3} rows, pairs {4}..{5}'.format(
            prefix, part_index + 1, sheet, len(result), pairs[0], pairs[-1]))


def convert_two_life_tables():
    r2_rates = [round(0.2 + 4.0 * part + 0.2 * i, 1) for part in range(5) for i in range(20)]
    convert_two_life_table(SRC_DIR + '/table-r2-2009.xlsx', OUT_DIR, 'TableR(2)',
                           'Interest Rate', r2_rates)
    convert_two_life_table(SRC_DIR + '/table-u2-2009cm.xlsx', OUT_DIR, 'TableU(2)',
                           'Adjusted Payout Rate', r2_rates)


#--------------------------------------------------------------
# Mortality table (lx) - Table 2000CM, merged into MortalityTable.json
# keeping the rows sorted by (Year, Age).
#--------------------------------------------------------------

def convert_mortality():
    rows = load_sheet(SRC_DIR + '/table-2000cm.xls', 'Sheet1')
    pairs = []
    for row in rows:
        for age_cell, lx_cell in ((row[1], row[2]), (row[5], row[6]), (row[9], row[10])):
            if is_age_cell(age_cell) and lx_cell is not None:
                pairs.append((to_age(age_cell), clean_number(lx_cell)))
    pairs.sort()
    assert pairs[0] == (0, 100000), 'lx: first pair {0}'.format(pairs[0])
    assert len(pairs) == 111, 'lx: {0} pairs'.format(len(pairs))
    assert pairs[-1][0] == 110, 'lx: last age {0}'.format(pairs[-1])

    filename = MORTALITY_FILE
    with open(filename) as source_file:
        mortality_rows = json.load(source_file)
    mortality_rows = [row for row in mortality_rows if row['Year'] != MORTALITY_YEAR]
    mortality_rows.extend({'Year': MORTALITY_YEAR, 'Age': age, 'Lx': lx} for age, lx in pairs)
    mortality_rows.sort(key=lambda row: (row['Year'], row['Age']))
    write_json(filename, mortality_rows)
    print('MortalityTable: added Year {0} rows, total {1} rows'.format(MORTALITY_YEAR, len(mortality_rows)))


def main():
    convert_commutation_tables()
    convert_u1()
    convert_two_life_tables()
    convert_mortality()
    print('Done.')


if __name__ == '__main__':
    main()
