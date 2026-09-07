'''
Combine per-part JSON files of Tables U(2) and R(2) into single "-full" files.

Usage:
    python3 CombineFiles.py                       # combine both tables from default locations
    python3 CombineFiles.py --json-dir DIR --out-dir DIR
'''

import argparse
import json
from pathlib import Path

TABLE_PARTS = {
    "TableU(2)-full-90CM.json": ["TableU(2)-p{0}-90CM-processed.json".format(p) for p in range(1, 6)],
    "TableR(2)-full-90CM.json": ["TableR(2)-p{0}-90CM-processed.json".format(p) for p in range(1, 6)],
}


def combine(output_name, part_files, json_dir, out_dir):
    out_dir.mkdir(parents=True, exist_ok=True)
    rows = []
    for part in part_files:
        source = json_dir / part
        print("Processing file: {0} ...".format(source))
        with open(source, "r") as datafile:
            rows.extend(json.load(datafile))

    target = out_dir / output_name
    print("Saving result file: {0}".format(target))
    with open(target, "w") as result_file:
        json.dump(rows, result_file)


def main():
    script_dir = Path(__file__).resolve().parent
    repo_root = script_dir.parent.parent

    parser = argparse.ArgumentParser(description="Combine per-part 90CM JSON files into full table files.")
    parser.add_argument("--json-dir", type=Path, default=repo_root / "JSONFiles" / "90CM",
                        help="directory with per-part JSON files")
    parser.add_argument("--out-dir", type=Path, default=repo_root / "JSONFiles" / "90CM",
                        help="directory for combined JSON files")
    args = parser.parse_args()

    args.out_dir.mkdir(parents=True, exist_ok=True)
    for output_name, part_files in TABLE_PARTS.items():
        combine(output_name, part_files, args.json_dir, args.out_dir)


if __name__ == "__main__":
    main()
