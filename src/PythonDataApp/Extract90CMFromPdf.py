'''
Extract the 90CM actuarial tables from the IRS source PDFs into CSV files.

Regenerates the 2016-era Tabula extraction (src/PythonDataApp/DataFiles/90CM/*.csv)
directly from the source PDFs (Publications 1457/1458/1459, Rev. 7-1999), so the
one-time manual Tabula step is no longer needed. By default every generated table
is verified value-by-value against the committed reference CSVs before anything
is written; Table 90CM (lx) is verified against the Year-1990 rows of
JSONFiles/MortalityTable.json. The output is byte-identical to the committed
reference CSVs; MortalityTable-90CM.csv is a new provenance record with no
consumer in Process90CMTables.py.

Usage:
    python3 Extract90CMFromPdf.py                      # all tables: extract, verify, write
    python3 Extract90CMFromPdf.py --table S            # a single table
    python3 Extract90CMFromPdf.py --out-dir DIR        # write the CSVs elsewhere
    python3 Extract90CMFromPdf.py --skip-verify        # write without verification
'''

import argparse
import csv
import json
import re
import sys
from pathlib import Path

import pdfplumber

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent.parent
DEFAULT_PDF_DIR = REPO_ROOT / "DataFiles" / "90CM"
DEFAULT_OUT_DIR = SCRIPT_DIR / "DataFiles" / "90CM"
MORTALITY_JSON = REPO_ROOT / "JSONFiles" / "MortalityTable.json"

S_HEADER = [
    '"",,Life,,,,Life,',
    ["Age", "Annuity", "Estate", "Remainder", "Age", "Annuity", "Estate", "Remainder"],
]

H_HEADER = [
    ["Age", "", "o", "_", "Age", "", "o", "_"],
    ["x", "Dx", "Nx", "Mx", "x", "Dx", "Nx", "Mx"],
]


def part_pairs(prefix):
    return [
        ("{0}-p{1}-90CM.pdf".format(prefix, p), "{0}-p{1}-90CM.csv".format(prefix, p))
        for p in range(1, 6)
    ]


TABLES = {
    "S": {"kind": "columns", "pairs": [("TableS-90CM.pdf", "TableS-90CM.csv")], "header": S_HEADER},
    "C": {"kind": "columns", "pairs": [("TableC-90CM.pdf", "TableC-90CM-2.csv")], "header": []},
    "H": {"kind": "columns", "pairs": [("TableH-90CM.pdf", "TableH-90CM.csv")], "header": H_HEADER},
    "U1": {"kind": "matrix", "pairs": [("TableU(1)-90CM.pdf", "TableU(1)-90CM-2.csv")],
           "age_columns": 1, "rows_per_rate_page": 110},
    "U2": {"kind": "matrix", "pairs": part_pairs("TableU(2)"),
           "age_columns": 2, "rows_per_rate_page": None},
    "R2": {"kind": "matrix", "pairs": part_pairs("TableR(2)"),
           "age_columns": 2, "rows_per_rate_page": None},
    "MortalityTable": {"kind": "mortality", "pairs": [("MortalityTable-90CM.pdf", "MortalityTable-90CM.csv")]},
}

MATRIX_ROWS_PER_PART = 6105

# The archived MortalityTable-90CM.pdf prints age 58 as "68" (its lx value
# 87397 sits between the lx values of ages 57 and 59); MortalityTable.json
# records the corrected age. Key: (printed age, lx value) -> corrected age.
MORTALITY_AGE_MISPRINTS = {("68", 87397): "58"}


class ExtractionError(Exception):
    pass


def is_int(text):
    return bool(re.fullmatch(r"\d+", text))


def pdf_word_rows(path):
    """Read every page of a PDF as a list of rows, each row a list of word
    strings ordered left to right."""
    pages = []
    with pdfplumber.open(path) as pdf:
        for page in pdf.pages:
            words = page.extract_words()
            words.sort(key=lambda w: (w["top"], w["x0"]))
            groups = []
            for word in words:
                if groups and word["top"] - groups[-1][0]["top"] <= 4:
                    groups[-1].append(word)
                else:
                    groups.append([word])
            pages.append([
                [w["text"] for w in sorted(group, key=lambda w: w["x0"])]
                for group in groups
            ])
    return pages


def check_rate_header(rows, expected_rate, page_index, table_name):
    pattern = re.compile(r"Interest at (\d+(?:\.\d+)?) Percent")
    for row in rows:
        match = pattern.search(" ".join(row))
        if match:
            rate = float(match.group(1))
            if abs(rate - expected_rate) > 0.01:
                raise ExtractionError(
                    "{0} page {1}: rate header {2} but page position implies {3}".format(
                        table_name, page_index + 1, rate, expected_rate))
            return
    raise ExtractionError("{0} page {1}: no 'Interest at X Percent' header found".format(
        table_name, page_index + 1))


def extract_columns(pdf_pages, table_name, header):
    """Tables S, C, H: one interest rate per page, two side-by-side age blocks
    (0-54 left, 55-109 right), 55 data rows per page. The committed CSVs carry
    the two Tabula header rows on every page for S and H, none for C."""
    lines = []
    for page_index, rows in enumerate(pdf_pages):
        expected_rate = round(2.2 + 0.2 * page_index, 1)
        check_rate_header(rows, expected_rate, page_index, table_name)
        data = [row for row in rows
                if len(row) == 8 and is_int(row[0]) and is_int(row[4])
                and 0 <= int(row[0]) <= 54 and 55 <= int(row[4]) <= 109]
        if len(data) != 55:
            raise ExtractionError(
                "{0} page {1}: {2} data rows with 8 tokens and valid ages, expected 55".format(
                    table_name, page_index + 1, len(data)))
        if [int(row[0]) for row in data] != list(range(55)):
            raise ExtractionError("{0} page {1}: left ages are not 0..54".format(
                table_name, page_index + 1))
        if [int(row[4]) for row in data] != list(range(55, 110)):
            raise ExtractionError("{0} page {1}: right ages are not 55..109".format(
                table_name, page_index + 1))
        lines.extend(header)
        lines.extend(data)
    return lines


def is_rate_row(row, age_columns):
    if len(row) <= age_columns:
        return False
    if age_columns == 1:
        return row[0] == "Age" and row[1].endswith("%")
    return row[0] == "O" and row[1] == "Y" and row[2].endswith("%")


def extract_matrix(pdf_pages, table_name, age_columns, rows_per_rate_page):
    """Tables U(1), U(2), R(2): borderless matrix pages. Each page starts with a
    rate-header row ('Age 2.2% 2.4% ...' for U(1), 'O Y 2.2% ...' for U(2)/R(2))
    followed by data rows of age column(s) plus one factor per rate column."""
    lines = []
    band_pages = []
    for page_index, rows in enumerate(pdf_pages):
        marker_indexes = [i for i, row in enumerate(rows) if is_rate_row(row, age_columns)]
        if len(marker_indexes) != 1:
            raise ExtractionError(
                "{0} page {1}: found {2} rate-header rows, expected 1".format(
                    table_name, page_index + 1, len(marker_indexes)))
        marker_index = marker_indexes[0]
        rates = rows[marker_index][age_columns:]
        expected_width = age_columns + len(rates)
        band_pages.append((tuple(rates), 0))
        lines.append(rows[marker_index])
        for row_position, row in enumerate(rows[marker_index + 1:]):
            if is_rate_row(row, age_columns):
                raise ExtractionError("{0} page {1}: second rate-header row".format(
                    table_name, page_index + 1))
            if len(row) == 1 and is_int(row[0]):
                if row_position != len(rows) - marker_index - 2:
                    raise ExtractionError(
                        "{0} page {1}: page number {2!r} is not the last row".format(
                            table_name, page_index + 1, row[0]))
                continue
            if len(row) != expected_width or not all(is_int(row[i]) for i in range(age_columns)):
                raise ExtractionError(
                    "{0} page {1}: unexpected row with {2} tokens (expected {3}): {4!r}".format(
                        table_name, page_index + 1, len(row), expected_width, row))
            if age_columns == 2 and int(row[1]) > int(row[0]):
                raise ExtractionError("{0} page {1}: age pair {2}/{3} violates Age2 <= Age1".format(
                    table_name, page_index + 1, row[0], row[1]))
            lines.append(row)
            band_pages[-1] = (band_pages[-1][0], band_pages[-1][1] + 1)
    if rows_per_rate_page is not None:
        for band_index, (rates, count) in enumerate(bands(band_pages)):
            if count != rows_per_rate_page:
                raise ExtractionError(
                    "{0} rate band {1} ({2} rates): {3} data rows, expected {4}".format(
                        table_name, band_index + 1, len(rates), count, rows_per_rate_page))
    else:
        data_rows = len(lines) - len(pdf_pages)
        if data_rows != MATRIX_ROWS_PER_PART:
            raise ExtractionError("{0}: {1} data rows, expected {2}".format(
                table_name, data_rows, MATRIX_ROWS_PER_PART))
        unique_rates = set(rates for rates, _ in band_pages)
        if len(unique_rates) != 1:
            raise ExtractionError("{0}: rate headers differ across pages: {1}".format(
                table_name, sorted(unique_rates)[:3]))
    return lines


def bands(band_pages):
    result = []
    for rates, count in band_pages:
        if result and result[-1][0] == rates:
            result[-1] = (rates, result[-1][1] + count)
        else:
            result.append((rates, count))
    return result


def extract_mortality(pdf_pages, table_name):
    """Table 90CM (lx): a single page with three side-by-side age/lx blocks
    (ages 0-36, 37-73, 74-110)."""
    if len(pdf_pages) != 1:
        raise ExtractionError("{0}: expected a single page, found {1}".format(
            table_name, len(pdf_pages)))
    data = [row for row in pdf_pages[0] if len(row) == 6 and all(is_int(token) for token in row)]
    if len(data) != 37:
        raise ExtractionError("{0}: {1} data rows with 6 integer tokens, expected 37".format(
            table_name, len(data)))
    misprint_corrections = 0
    for block in range(3):
        age_index = block * 2
        for row_index, row in enumerate(data):
            expected_age = block * 37 + row_index
            if row[age_index] == str(expected_age):
                continue
            correction = MORTALITY_AGE_MISPRINTS.get((row[age_index], int(row[age_index + 1])))
            if correction != str(expected_age):
                raise ExtractionError(
                    "{0}: block {1} row {2}: printed age {3} with lx {4}, expected age {5}".format(
                        table_name, block, row_index, row[age_index],
                        row[age_index + 1], expected_age))
            row[age_index] = correction
            misprint_corrections += 1
    if misprint_corrections != len(MORTALITY_AGE_MISPRINTS):
        raise ExtractionError("{0}: {1} of {2} known age misprints found".format(
            table_name, misprint_corrections, len(MORTALITY_AGE_MISPRINTS)))
    return data


def extract_table(name, config, pdf_dir):
    """Extract every PDF of a table; returns one list of CSV rows per PDF."""
    groups = []
    for pdf_name, _ in config["pairs"]:
        pages = pdf_word_rows(pdf_dir / pdf_name)
        if config["kind"] == "columns":
            groups.append(extract_columns(pages, name, config["header"]))
        elif config["kind"] == "matrix":
            groups.append(extract_matrix(pages, name, config["age_columns"],
                                         config["rows_per_rate_page"]))
        else:
            groups.append(extract_mortality(pages, name))
    return groups


def normalized_row(line):
    if isinstance(line, str):
        return next(csv.reader([line]))
    return line


def verify_against_reference(name, groups, reference_dir):
    if name == "MortalityTable":
        return verify_mortality(groups[0])
    problems = []
    for (pdf_name, csv_name), lines in zip(TABLES[name]["pairs"], groups):
        reference_path = reference_dir / csv_name
        if not reference_path.exists():
            return ["reference file {0} does not exist".format(reference_path)]
        with open(reference_path, newline="") as f:
            reference = list(csv.reader(f))
        if len(reference) != len(lines):
            problems.append("{0}: generated {1} lines vs reference {2}".format(
                csv_name, len(lines), len(reference)))
        differences = 0
        for line_index, (generated_row, reference_row) in enumerate(zip(lines, reference)):
            if normalized_row(generated_row) != reference_row:
                differences += 1
                if differences <= 5:
                    problems.append("{0} line {1}: generated {2!r} vs reference {3!r}".format(
                        csv_name, line_index + 1, generated_row, reference_row))
        if differences > 5:
            problems.append("{0}: {1} differing lines in total".format(csv_name, differences))
    return problems


def verify_mortality(lines):
    if not MORTALITY_JSON.exists():
        return ["{0} does not exist; cannot verify Table 90CM lx values".format(MORTALITY_JSON)]
    with open(MORTALITY_JSON) as f:
        mortality_rows = json.load(f)
    expected = {(int(row["Age"]), int(row["Lx"])) for row in mortality_rows if int(row["Year"]) == 1990}
    extracted = set()
    for row in lines:
        for block in range(3):
            extracted.add((int(row[block * 2]), int(row[block * 2 + 1])))
    missing = expected - extracted
    unexpected = extracted - expected
    problems = []
    if missing:
        problems.append("lx values in MortalityTable.json but not in the PDF: {0}".format(
            sorted(missing)[:5]))
    if unexpected:
        problems.append("lx values in the PDF but not in MortalityTable.json: {0}".format(
            sorted(unexpected)[:5]))
    return problems


def write_csv(path, lines):
    with open(path, "w", newline="") as f:
        writer = csv.writer(f, lineterminator="\n")
        for line in lines:
            if isinstance(line, str):
                f.write(line + "\n")
            else:
                writer.writerow(line)


def main():
    parser = argparse.ArgumentParser(
        description="Extract the 90CM actuarial tables from the IRS source PDFs into CSV files.")
    parser.add_argument("--table", choices=sorted(TABLES),
                        help="extract a single table (default: all)")
    parser.add_argument("--pdf-dir", type=Path, default=DEFAULT_PDF_DIR,
                        help="directory with the source PDF files")
    parser.add_argument("--out-dir", type=Path, default=DEFAULT_OUT_DIR,
                        help="directory for the generated CSV files")
    parser.add_argument("--skip-verify", action="store_true",
                        help="write without verifying against the committed reference CSVs")
    args = parser.parse_args()

    table_names = [args.table] if args.table else sorted(TABLES)
    args.out_dir.mkdir(parents=True, exist_ok=True)
    failures = []
    for name in table_names:
        print("Extracting {0} ...".format(name))
        try:
            groups = extract_table(name, TABLES[name], args.pdf_dir)
            print("  rows: {0}".format(sum(len(group) for group in groups)))
            if not args.skip_verify:
                problems = verify_against_reference(name, groups, DEFAULT_OUT_DIR)
                if problems:
                    raise ExtractionError("verification failed:\n  " + "\n  ".join(problems))
                print("  verified against the committed reference")
            for (_, csv_name), lines in zip(TABLES[name]["pairs"], groups):
                output_path = args.out_dir / csv_name
                write_csv(output_path, lines)
                print("  saved to {0}".format(output_path))
        except ExtractionError as error:
            failures.append(str(error))
            print("  FAILED: {0}".format(error))

    if failures:
        print("\n{0} table(s) failed:".format(len(failures)))
        for failure in failures:
            print("  " + failure)
        sys.exit(1)
    print("\nAll tables extracted successfully.")


if __name__ == "__main__":
    main()
