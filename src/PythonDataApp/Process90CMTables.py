'''
Generic CSV -> JSON processor for the 90CM actuarial table series.

Replaces the six per-table legacy scripts (TableSProcessor.py, TableCProcessor.py,
TableHProcessor.py, TableU1Processor.py, TableR2/U2ProcessorDef.py and their drivers).
The per-table differences (CSV layout, rows per page, field names) live in TABLES below.

Usage:
    python3 Process90CMTables.py                       # process all tables
    python3 Process90CMTables.py --table S             # process one table
    python3 Process90CMTables.py --csv-dir DIR --out-dir DIR

Output values are strings, matching the legacy 90CM JSON files exactly.
'''

import argparse
import csv
import json
from pathlib import Path

import numpy as np

# 100 interest rates [2.2 : 0.2 : 22.0], page order for the "columns" layout.
INTEREST_RATES = np.arange(2.2, 22.1, 0.2)

MORTALITY_TABLE = 1990


class ColumnsLayout:
    """CSV pages with a fixed rows-per-page count and two side-by-side column blocks.

    Each page contains two blocks of consecutive columns with the same field
    order (e.g. ages 0-54 on the left, ages 55-104 on the right). The interest
    rate for the whole page is taken from INTEREST_RATES[page].
    """

    def __init__(self, rows_per_page, start_offset, rate_key, block_keys):
        self.rows_per_page = rows_per_page
        self.start_offset = start_offset
        self.rate_key = rate_key
        self.block_keys = block_keys

    def rows(self, data):
        pages = round(len(data) / self.rows_per_page)
        results = []
        for page in range(pages):
            page_start = page * self.rows_per_page
            page_end = page_start + self.rows_per_page
            data_start = page_start + self.start_offset
            # rounded to 1 decimal: the 2016-era numpy arange emitted float
            # artifacts (e.g. 2.4000000000000004) which every consumer rounds
            # away anyway; clean rates are emitted instead.
            interest_rate = round(float(INTEREST_RATES[page]), 1)
            for block in range(2):
                column_offset = block * len(self.block_keys)
                for i in range(data_start, page_end):
                    row = {"mortalityTable": MORTALITY_TABLE, self.rate_key: interest_rate}
                    for key_index, key in enumerate(self.block_keys):
                        row[key] = data[i][column_offset + key_index]
                    results.append(row)
        return results


class MatrixLayout:
    """CSV pages where each page starts with a header row (marker in column 0).

    The header row holds one payout rate per column (with a trailing '%'),
    the data rows hold ages in the first columns and one factor per rate column.
    """

    def __init__(self, marker, rate_start, rate_key, age_keys, remainder_key):
        self.marker = marker
        self.rate_start = rate_start
        self.rate_key = rate_key
        self.age_keys = age_keys
        self.remainder_key = remainder_key

    def rows(self, data):
        # pages start at rows where column 0 equals the marker
        indexes = [i for i, row in enumerate(data) if row and row[0] == self.marker]
        results = []
        for page, header_index in enumerate(indexes):
            page_end = indexes[page + 1] if page < len(indexes) - 1 else len(data)
            for i in range(header_index + 1, page_end):
                for j in range(self.rate_start, len(data[i])):
                    row = {
                        "mortalityTable": MORTALITY_TABLE,
                        self.rate_key: data[header_index][j].strip('%'),
                    }
                    for age_index, age_key in enumerate(self.age_keys):
                        row[age_key] = data[i][age_index]
                    row[self.remainder_key] = data[i][j]
                    results.append(row)
        return results


def parts(prefix, count):
    return ['{0}-p{1}-90CM.csv'.format(prefix, p) for p in range(1, count + 1)]


TABLES = {
    "S": {
        "files": ["TableS-90CM.csv"],
        "output": "TableS-90CM-processed.json",
        "layout": ColumnsLayout(rows_per_page=57, start_offset=2, rate_key="interestRate",
                                block_keys=["age", "pvAnnuity", "pvLifeEstate", "pvReminderInterest"]),
    },
    "C": {
        "files": ["TableC-90CM-2.csv"],
        "output": "TableC-90CM-processed.json",
        "layout": ColumnsLayout(rows_per_page=55, start_offset=0, rate_key="rate",
                                block_keys=["age", "remainderFactor", "rFactor", "dFactor"]),
    },
    "H": {
        "files": ["TableH-90CM.csv"],
        "output": "TableH-90CM-processed.json",
        "layout": ColumnsLayout(rows_per_page=57, start_offset=2, rate_key="interestRate",
                                block_keys=["age", "dFactor", "nFactor", "mFactor"]),
    },
    "U1": {
        "files": ["TableU(1)-90CM-2.csv"],
        "output": "TableU1-90CM-processed.json",
        "layout": MatrixLayout(marker="Age", rate_start=1, rate_key="adjustedPayoutRate",
                               age_keys=["age"], remainder_key="remainderFactor"),
    },
    "U2": {
        "files": parts("TableU(2)", 5),
        "output": "TableU(2)-p{part}-90CM-processed.json",
        "layout": MatrixLayout(marker="O", rate_start=2, rate_key="adjustedPayoutRate",
                               age_keys=["age1", "age2"], remainder_key="remainderFactor"),
    },
    "R2": {
        "files": parts("TableR(2)", 5),
        "output": "TableR(2)-p{part}-90CM-processed.json",
        "layout": MatrixLayout(marker="O", rate_start=2, rate_key="adjustedPayoutRate",
                               age_keys=["age1", "age2"], remainder_key="remainderFactor"),
    },
}


def process_table(name, config, csv_dir, out_dir):
    out_dir.mkdir(parents=True, exist_ok=True)
    layout = config["layout"]
    for part_index, filename in enumerate(config["files"], start=1):
        source = csv_dir / filename
        print("Processing {0} ...".format(source))

        with open(source, "r") as f:
            data = list(csv.reader(f, delimiter=","))

        results = layout.rows(data)

        output_name = config["output"].format(part=part_index)
        output_path = out_dir / output_name
        with open(output_path, "w") as result_file:
            json.dump(results, result_file)

        print("  rows: {0}, saved to {1}".format(len(results), output_path))


def main():
    script_dir = Path(__file__).resolve().parent
    repo_root = script_dir.parent.parent

    parser = argparse.ArgumentParser(description="Convert 90CM actuarial table CSV files (Tabula output) to JSON.")
    parser.add_argument("--table", choices=sorted(TABLES), help="process a single table (default: all)")
    parser.add_argument("--csv-dir", type=Path, default=script_dir / "DataFiles" / "90CM",
                        help="directory with source CSV files")
    parser.add_argument("--out-dir", type=Path, default=repo_root / "JSONFiles" / "90CM",
                        help="directory for generated JSON files")
    args = parser.parse_args()

    tables = [args.table] if args.table else sorted(TABLES)
    args.out_dir.mkdir(parents=True, exist_ok=True)

    for name in tables:
        process_table(name, TABLES[name], args.csv_dir, args.out_dir)


if __name__ == "__main__":
    main()
