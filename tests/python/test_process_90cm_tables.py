'''
Tests for Process90CMTables.py: layout classes and end-to-end processing.
Run with: python -m pytest tests/python (requires numpy + pytest)
'''

import importlib.util
import json
from pathlib import Path

import pytest

PYTHON_APP = Path(__file__).resolve().parent.parent.parent / "src" / "PythonDataApp"


def load_module(name):
    spec = importlib.util.spec_from_file_location(name, PYTHON_APP / f"{name}.py")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


Process90CMTables = load_module("Process90CMTables")
CombineFiles = load_module("CombineFiles")


def make_csv(rows):
    return [line if isinstance(line, list) else line.split(",") for line in rows]


class TestColumnsLayout:
    def test_two_blocks_and_clean_rates(self):
        # page = 4 rows: 2 offset rows + 2 data rows; two side-by-side blocks
        layout = Process90CMTables.ColumnsLayout(
            rows_per_page=4, start_offset=2, rate_key="interestRate",
            block_keys=["age", "factor"])
        data = make_csv([
            "junk,junk,junk,junk",          # offset row 0
            "junk,junk,junk,junk",          # offset row 1
            "0,1.0,55,2.0",                 # data row: block1 age/factor, block2 age/factor
            "1,1.1,56,2.1",
            "junk,junk,junk,junk",          # page 2 offset rows
            "junk,junk,junk,junk",
            "2,3.0,57,4.0",
            "3,3.1,58,4.1",
        ])

        rows = layout.rows(data)

        assert len(rows) == 8  # 2 pages x 2 blocks x 2 data rows
        assert rows[0] == {"mortalityTable": 1990, "interestRate": 2.2, "age": "0", "factor": "1.0"}
        assert rows[1] == {"mortalityTable": 1990, "interestRate": 2.2, "age": "1", "factor": "1.1"}
        assert rows[2] == {"mortalityTable": 1990, "interestRate": 2.2, "age": "55", "factor": "2.0"}
        assert rows[4] == {"mortalityTable": 1990, "interestRate": 2.4, "age": "2", "factor": "3.0"}
        assert rows[6] == {"mortalityTable": 1990, "interestRate": 2.4, "age": "57", "factor": "4.0"}

    def test_rate_is_rounded_to_one_decimal(self):
        # page index 1 of the legacy rate grid carries float artifacts; must be clean
        layout = Process90CMTables.ColumnsLayout(
            rows_per_page=1, start_offset=0, rate_key="rate", block_keys=["age"])
        data = make_csv([["0", "x"], ["1", "x"]])

        rows = layout.rows(data)

        assert rows[0]["rate"] == 2.2
        assert rows[2]["rate"] == 2.4


class TestMatrixLayout:
    def test_pages_markers_and_percent_stripping(self):
        layout = Process90CMTables.MatrixLayout(
            marker="O", rate_start=2, rate_key="adjustedPayoutRate",
            age_keys=["age1", "age2"], remainder_key="remainderFactor")
        data = make_csv([
            "O,,5.0%,5.5%",          # page 1 header: rates per column
            "60,70,.123,.456",       # data row
            "61,71,.223,.556",
            "O,,6.0%,6.5%",          # page 2 header
            "62,72,.789,.012",
        ])

        rows = layout.rows(data)

        # every data row produces one output row per rate column
        assert len(rows) == 6
        assert rows[0] == {"mortalityTable": 1990, "adjustedPayoutRate": "5.0",
                           "age1": "60", "age2": "70", "remainderFactor": ".123"}
        assert rows[1] == {"mortalityTable": 1990, "adjustedPayoutRate": "5.5",
                           "age1": "60", "age2": "70", "remainderFactor": ".456"}
        assert rows[2]["age1"] == "61"
        assert rows[4] == {"mortalityTable": 1990, "adjustedPayoutRate": "6.0",
                           "age1": "62", "age2": "72", "remainderFactor": ".789"}
        assert rows[5]["adjustedPayoutRate"] == "6.5"


class TestEndToEnd:
    def test_process_table_writes_json(self, tmp_path):
        csv_dir = tmp_path / "csv"
        out_dir = tmp_path / "out"
        csv_dir.mkdir()

        # minimal single-page input for Table S config (57 rows/page, offset 2, 4+4 columns)
        pages = []
        for rate_index in range(1):  # one page
            pages.append("h,h,h,h,h,h,h,h")
            pages.append("h,h,h,h,h,h,h,h")
            pages.append("0,1.0,2.0,3.0,55,4.0,5.0,6.0")
        for i in range(57 - 3):  # pad page to rowsPerPage
            pages.append("x,x,x,x,x,x,x,x")
        (csv_dir / "TableS-90CM.csv").write_text("\n".join(pages))

        config = dict(Process90CMTables.TABLES["S"])
        config["files"] = ["TableS-90CM.csv"]

        Process90CMTables.process_table("S", config, csv_dir, out_dir)

        result = json.loads((out_dir / "TableS-90CM-processed.json").read_text())
        assert len(result) == 110  # 55 data rows x 2 blocks
        first = result[0]
        assert first["mortalityTable"] == 1990
        assert first["interestRate"] == 2.2
        assert first["age"] == "0"
        assert first["pvAnnuity"] == "1.0"
        assert first["pvLifeEstate"] == "2.0"
        assert first["pvReminderInterest"] == "3.0"
        # second block (columns 4-7) starts after all first-block rows
        second = result[55]
        assert second["age"] == "55"
        assert second["pvAnnuity"] == "4.0"


class TestCombineFiles:
    def test_combine_merges_parts_in_order(self, tmp_path):
        json_dir = tmp_path / "json"
        out_dir = tmp_path / "out"
        json_dir.mkdir()

        parts = CombineFiles.TABLE_PARTS["TableR(2)-full-90CM.json"]
        for index, part in enumerate(parts):
            (json_dir / part).write_text(json.dumps([{"part": index}, {"part": index, "second": True}]))

        CombineFiles.combine("TableR(2)-full-90CM.json", parts, json_dir, out_dir)

        combined = json.loads((out_dir / "TableR(2)-full-90CM.json").read_text())
        assert len(combined) == 10
        assert [row["part"] for row in combined] == [0, 0, 1, 1, 2, 2, 3, 3, 4, 4]
