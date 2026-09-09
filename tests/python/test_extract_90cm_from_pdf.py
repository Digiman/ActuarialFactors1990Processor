'''
Tests for Extract90CMFromPdf.py: layout parsing and validation on synthetic
page rows, plus one integration test against the real MortalityTable PDF.
Run with: python -m pytest tests/python (requires pdfplumber + pytest)
'''

import importlib.util
from pathlib import Path

import pytest

PYTHON_APP = Path(__file__).resolve().parent.parent.parent / "src" / "PythonDataApp"


def load_module(name):
    spec = importlib.util.spec_from_file_location(name, PYTHON_APP / f"{name}.py")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


Extract = load_module("Extract90CMFromPdf")


def s_like_page(rate):
    rows = [["Table", "S", f"({rate})"],
            ["Interest", "at", str(rate), "Percent"],
            ["Age", "Annuity", "Estate", "Remainder", "Age", "Annuity", "Estate", "Remainder"]]
    for age in range(55):
        rows.append([str(age), "1.0", ".5", ".5", str(age + 55), "2.0", ".4", ".6"])
    return rows


class TestExtractColumns:
    def test_extracts_header_and_data_rows(self):
        pages = [s_like_page(2.2), s_like_page(2.4)]

        lines = Extract.extract_columns(pages, "S", Extract.S_HEADER)

        assert len(lines) == 2 * (len(Extract.S_HEADER) + 55)
        assert lines[0] == '"",,Life,,,,Life,'
        assert lines[2] == ["0", "1.0", ".5", ".5", "55", "2.0", ".4", ".6"]
        assert lines[56] == ["54", "1.0", ".5", ".5", "109", "2.0", ".4", ".6"]
        assert lines[57 + 2] == ["0", "1.0", ".5", ".5", "55", "2.0", ".4", ".6"]

    def test_rate_header_mismatch_raises(self):
        pages = [s_like_page(9.9)]

        with pytest.raises(Extract.ExtractionError, match="rate header"):
            Extract.extract_columns(pages, "S", Extract.S_HEADER)

    def test_missing_rate_header_raises(self):
        pages = [[["Table", "S", "(2.2)"], ["Age", "Annuity", "Estate", "Remainder"]]]

        with pytest.raises(Extract.ExtractionError, match="no 'Interest"):
            Extract.extract_columns(pages, "S", Extract.S_HEADER)

    def test_incomplete_age_grid_raises(self):
        page = s_like_page(2.2)
        page.pop()  # drop the last data row (age 54 / 109)
        pages = [page]

        with pytest.raises(Extract.ExtractionError, match="55"):
            Extract.extract_columns(pages, "S", Extract.S_HEADER)


def u1_page(first_age, page_number, count=55):
    rates = ["2.2%", "2.4%", "2.6%", "2.8%", "3.0%"]
    rows = [["Age"] + rates]
    for offset in range(count):
        age = first_age + offset
        rows.append([str(age), ".1", ".2", ".3", ".4", ".5"])
    rows.append([str(page_number)])
    return rows


class TestExtractMatrix:
    def test_marker_rows_included_footer_skipped(self):
        pages = [u1_page(0, 1), u1_page(55, 2)]

        lines = Extract.extract_matrix(pages, "U1", age_columns=1, rows_per_rate_page=110)

        assert len(lines) == 112  # 2 markers + 2 x 55 data rows
        assert lines[0][0] == "Age" and lines[0][1] == "2.2%"
        assert lines[1] == ["0", ".1", ".2", ".3", ".4", ".5"]
        assert lines[-1] == ["109", ".1", ".2", ".3", ".4", ".5"]

    def test_incomplete_band_raises(self):
        pages = [u1_page(0, 1)]

        with pytest.raises(Extract.ExtractionError, match="rate band"):
            Extract.extract_matrix(pages, "U1", age_columns=1, rows_per_rate_page=110)

    def test_footer_not_on_last_row_raises(self):
        page = u1_page(0, 1)
        page.insert(3, ["1"])
        pages = [page]

        with pytest.raises(Extract.ExtractionError, match="not the last row"):
            Extract.extract_matrix(pages, "U1", age_columns=1, rows_per_rate_page=110)

    def test_age_pair_violation_raises(self):
        rows = [["O", "Y", "2.2%"], ["50", "60", ".1"], ["5"]]
        pages = [rows]

        with pytest.raises(Extract.ExtractionError, match="Age2"):
            Extract.extract_matrix(pages, "U2", age_columns=2, rows_per_rate_page=None)


def mortality_rows(with_misprint=True):
    rows = [["Table", "90CM"], ["Age", "Age", "Age"], ["x", "l", "x"]]
    data = []
    for row_index in range(37):
        row = [str(row_index), str(100000 - row_index * 10)]
        for block in range(1, 3):
            age = block * 37 + row_index
            lx = 90000 - age
            if with_misprint and (block, row_index) == (1, 21):
                # mirror the real PDF: age 58 printed as "68", lx 87397 is correct
                age, lx = 68, 87397
            row += [str(age), str(lx)]
        data.append(row)
    return rows + data


class TestExtractMortality:
    def test_known_age_misprint_is_corrected(self):
        lines = Extract.extract_mortality([mortality_rows()], "MortalityTable")

        assert len(lines) == 37
        assert lines[21] == ["21", "99790", "58", "87397", "95", "89905"]

    def test_unexpected_age_raises(self):
        rows = mortality_rows()
        rows[5][4] = "99"

        with pytest.raises(Extract.ExtractionError, match="printed age"):
            Extract.extract_mortality([rows], "MortalityTable")

    def test_missing_known_misprint_raises(self):
        with pytest.raises(Extract.ExtractionError, match="known age misprints"):
            Extract.extract_mortality([mortality_rows(with_misprint=False)], "MortalityTable")


class TestMortalityIntegration:
    def test_real_pdf_matches_mortality_json(self):
        pdf_path = Extract.DEFAULT_PDF_DIR / "MortalityTable-90CM.pdf"
        if not pdf_path.exists():
            pytest.skip("source PDFs are not present")

        lines = Extract.extract_table("MortalityTable", Extract.TABLES["MortalityTable"],
                                      Extract.DEFAULT_PDF_DIR)

        assert Extract.verify_against_reference("MortalityTable", lines, Extract.DEFAULT_OUT_DIR) == []
        assert lines[0][0][0:2] == ["0", "100000"]
        assert lines[0][0][4:6] == ["74", "62852"]
        assert lines[0][-1][4:6] == ["110", "0"]
