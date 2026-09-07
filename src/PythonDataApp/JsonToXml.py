'''
Convert root-table JSON files (B, D, F, J, K, Mortality) back into the
SQL Server export XML format used by XMLFiles/, so the JSON and XML
sources stay in sync.

Usage:
    python3 JsonToXml.py                          # convert all known tables
    python3 JsonToXml.py --table MortalityTable   # convert one table
'''

import argparse
import json
from pathlib import Path
from xml.sax.saxutils import quoteattr

# JSON filename -> XML element name (SQL Server export format)
TABLES = {
    "TableB": "dbo.tblB",
    "TableD": "dbo.tblD",
    "TableF": "dbo.tblF",
    "TableJ": "dbo.tblJ",
    "TableK": "dbo.tblK",
    "MortalityTable": "dbo.tblMortality",
}


def convert(name, element, json_dir, out_dir):
    source = json_dir / f"{name}.json"
    rows = json.loads(source.read_text())
    if not rows:
        raise ValueError(f"{source} contains no rows")

    columns = list(rows[0])

    lines = ['<?xml version="1.0" encoding="utf-8"?>', "<rows>"]
    for row in rows:
        attributes = " ".join(
            f"{column}={quoteattr(str(row[column]))}" for column in columns
        )
        lines.append(f"<{element} {attributes} />")
    lines.append("</rows>")

    target = out_dir / f"{name}.xml"
    target.write_text("\n".join(lines) + "\n")
    print(f"{source} -> {target} ({len(rows)} rows)")


def main():
    script_dir = Path(__file__).resolve().parent
    repo_root = script_dir.parent.parent

    parser = argparse.ArgumentParser(description="Convert root-table JSON files to SQL Server export XML format.")
    parser.add_argument("--table", choices=sorted(TABLES), help="convert a single table (default: all)")
    parser.add_argument("--json-dir", type=Path, default=repo_root / "JSONFiles",
                        help="directory with source JSON files")
    parser.add_argument("--out-dir", type=Path, default=repo_root / "XMLFiles",
                        help="directory for generated XML files")
    args = parser.parse_args()

    args.out_dir.mkdir(parents=True, exist_ok=True)
    tables = [args.table] if args.table else sorted(TABLES)
    for name in tables:
        convert(name, TABLES[name], args.json_dir, args.out_dir)


if __name__ == "__main__":
    main()
