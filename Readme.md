# IRS Actuarial Factors processor (90CM / 2010CM series)

Utilities that process actuarial factor data published by the IRS (Publications 1457/1458/1459):

* the 90CM-series source tables are extracted from the PDF files with a scripted
  `pdfplumber` extractor (`Extract90CMFromPdf.py`), verified value-by-value against the
  2016 Tabula reference CSVs; the 2010CM series and the root (non-mortality) tables are
  converted directly from the official IRS XLSX/XLS spreadsheets;
* Python scripts convert the extracted CSV/XLSX data to JSON;
* the C# console application reads the JSON/XML files and can save data as
  JSON, text, Excel files, or bulk-insert it into SQL Server.

**90CM: PDF -> Extract90CMFromPdf.py -> CSV -> Python -> JSON; 2010CM and root tables: official XLSX/XLS -> Python -> JSON; then C# app -> Excel / text / SQL Server**

## Repository layout

```
src/                          source code
  DataProcessingApp.Core      row types, helpers, configuration
  DataProcessingApp.Logic     generic table loaders/savers (JSON, XML, Excel, text)
  DataProcessingApp.Data      SQL Server bulk-insert repositories
  DataProcessingApp.ConsoleApp  console application (workflows)
  PythonDataApp               Python scripts (PDF extraction, CSV/XLSX -> JSON)
    DataFiles/90CM/           extracted CSV files (90CM series, regenerable from the PDFs)
tests/
  DataProcessingApp.Tests     xUnit tests for the C# pipeline
  python                      pytest tests for the Python scripts
db/                           SSDT database project (tables, stored procedures)
DataFiles/                    source data (90CM PDFs, root-table and 2010CM spreadsheets)
JSONFiles/                    processed data files (per-series subfolders)
XMLFiles/                     generated XML (root tables; not committed, run JsonToXml.py)
Docs/                         research notes, improvement plan
```

## Requirements

* .NET SDK 10.0 (`dotnet build`, `dotnet test`, `dotnet run`)
* Python 3 with the packages from `src/PythonDataApp/requirements.txt`
  (`numpy`, `openpyxl` for XLSX, `xlrd` for legacy XLS, `pdfplumber` for the
  90CM PDF extraction)
* Docker (e.g. OrbStack/Docker Desktop), only if you want to run the SQL Server
  integration tests locally (CI runs them always)
* SQL Server + SSDT only if you want the database features with your own
  instance (the `db/` project builds in Visual Studio on Windows; the
  integration tests deploy the same schema scripts automatically)

## Building and testing

```bash
dotnet build DataProcessingApp.sln
dotnet test tests/DataProcessingApp.Tests

python3 -m venv .venv && . .venv/bin/activate      # or use Anaconda
pip install -r src/PythonDataApp/requirements.txt -r tests/python/requirements.txt
python -m pytest tests/python

python src/PythonDataApp/JsonToXml.py              # generate XMLFiles/ (needed by the C# workflows)
```

### SQL Server integration tests

The database path (schema deployment, `SqlBulkCopy`, stored procedures) is
covered by tests tagged `Category=Integration`
(`tests/DataProcessingApp.Tests/Database/`). They deploy the schema from the
`db/` SSDT project files into a real SQL Server and are skipped automatically
when no server is reachable.

```bash
docker compose up -d --wait sqlserver                          # SQL Server 2022 container
dotnet test tests/DataProcessingApp.Tests --filter Category=Integration
docker compose down                                            # when done
```

The connection string defaults to the compose credentials
(`sa` / `YourStrong!Passw0rd`, override the password with the
`MSSQL_SA_PASSWORD` environment variable before `docker compose up`) and can be
overridden with `DPA_TEST_CONNECTION_STRING`. CI runs these tests in a
dedicated `database` job.

## How to use the console app

Configuration lives in `src/DataProcessingApp.ConsoleApp/appsettings.json`
(`BaseDataDir`, `XmlDataDir`, connection strings). Note that the shipped
defaults are relative to the *build output folder* (`bin/Debug/net10.0/...`),
not to the repository root; when in doubt use the `DPA_` overrides below.

The workflow is selected by a command line argument:

```bash
dotnet run --project src/DataProcessingApp.ConsoleApp -- <workflow> [options]
```

| Workflow    | What it does                                                        |
|-------------|---------------------------------------------------------------------|
| `load`      | loads every table from its source files, all series (smoke check)   |
| `json`      | loads every root table and saves it as JSON                         |
| `text`      | loads every table, all series, and saves it as a plain text file    |
| `excel`     | loads every table, all series, and saves it as an Excel document (numbers as numeric cells) |
| `database`  | loads every table, all series, and reloads it into SQL Server (destination tables are cleared first, so reruns are idempotent) |
| `all`       | load + json + text + excel                                          |

Every workflow accepts the same options (also shown by `--help`, which is
always available):

| Option        | Effect                                                                 |
|---------------|-------------------------------------------------------------------------|
| `--series`    | restrict series tables to the given series, e.g. `--series 2010CM` or `--series 90CM,2010CM` |
| `--table`     | restrict processing to the given tables, e.g. `--table S,K`, `--table U1` or `--table MortalityTable` |
| `--dry-run`   | load and validate the source files but write nothing                   |

The process reports `Processing <table> - done, <N> records in <M> ms.` per
table while it runs. A failed table is reported as `Processing <table> - FAILED: <reason>`
and does not abort the workflow; after the run a failure summary is printed and
the process exits with code 1 (0 on full success), so automation can detect
partial failures.

The root tables (B, D, F, J, K, MortalityTable) are loaded from the XML export
format in `XMLFiles/`, which is a **generated artifact and not committed**:
run `python src/PythonDataApp/JsonToXml.py` once (after `pip install`, before
the C# workflows) to create it from the committed `JSONFiles/`. CI does the
same before its smoke test.

**Where the data comes from and goes:**

| Workflow | Reads | Writes |
|---|---|---|
| `load` | series JSON from `BaseDataDir/<series>/`, root JSON/XML from `BaseDataDir` / `XmlDataDir` | nothing (smoke check) |
| `json` | root tables (XML) | `BaseDataDir/Table*.json`, `MortalityTable.json` |
| `text` | series + root tables | `BaseDataDir/<series>/*.txt` |
| `excel` | series + root tables | `BaseDataDir/*.xlsx`, `BaseDataDir/<series>/*.xlsx` |
| `database` | series + root tables | SQL Server tables (`dbo.tblS`, `dbo.tblB`, ...) |

`BaseDataDir` should point at the folder with the processed JSON data in the repository
layout: root-table files directly in it and the series files in `90CM/` / `2010CM/`
subfolders. The XML-based root tables are read from `XmlDataDir` (falls back to
`BaseDataDir` when unset), so against the repository no copying is needed:

```bash
DPA_BaseDataDir=JSONFiles DPA_XmlDataDir=XMLFiles dotnet run --project src/DataProcessingApp.ConsoleApp -- load
```

Every setting can be overridden with a `DPA_`-prefixed environment variable, e.g.
`DPA_BaseDataDir=/path/to/json/files`. The `database` workflow additionally needs the
`DataProcessingAppDB` database deployed from the `db/` SSDT project.

## Python scripts

All scripts are run from the repository root and use repo-relative paths by default:

```bash
python src/PythonDataApp/Extract90CMFromPdf.py              # 90CM PDF -> CSV (all tables, verifies first)
python src/PythonDataApp/Extract90CMFromPdf.py --table S    # one table
python src/PythonDataApp/Process90CMTables.py              # 90CM CSV -> JSON (all tables)
python src/PythonDataApp/Process90CMTables.py --table S    # one table
python src/PythonDataApp/Process90CMTables.py --numeric    # emit numbers instead of strings
python src/PythonDataApp/CombineFiles.py                   # combine R(2)/U(2) parts into "-full" files
python src/PythonDataApp/Convert2000CMToJson.py            # 2000CM XLS/XLSX -> JSON (requires openpyxl + xlrd)
python src/PythonDataApp/Convert2010CMToJson.py            # 2010CM XLSX -> JSON (requires openpyxl)
python src/PythonDataApp/ConvertRootTablesToJson.py        # root tables B/D/F/J/K: XLSX/XLS -> JSON
python src/PythonDataApp/JsonToXml.py                      # root-table JSON -> SQL export XML
```

`Extract90CMFromPdf.py` replaces the one-time manual [Tabula](http://tabula.technology/)
extraction used in 2016: it reads `DataFiles/90CM/*.pdf` and regenerates the CSVs in
`src/PythonDataApp/DataFiles/90CM/`, verifying every table value-by-value against the
committed reference CSVs (and Table 90CM lx against `JSONFiles/MortalityTable.json`)
before writing anything. Pass `--skip-verify` to bypass the check.

## Table details

The IRS actuarial tables come in editions ("series") named after the census mortality
study they are based on. This repository processes:

| Series | Mortality basis | Valuation dates | Source format |
|---|---|---|---|
| 90CM | 1990 census | 5/1/1999 - 4/30/2009 | Pub 1457/1458/1459 (7-1999) PDFs |
| 2000CM | 2000 census | 5/1/2009 - 5/31/2023 | official IRS spreadsheets (mixed .xls/.xlsx) |
| 2010CM | 2010 census | 6/1/2023 onwards | official IRS XLSX spreadsheets |

### Table reference

Interest/payout grids: 2000CM/2010CM and root tables use 0.2%-20.0% in 0.2 steps
(100 rates); the 90CM grid is 2.2%-22.0% in 0.2 steps.

| Table | Purpose (IRS wording) | Series | Rows | Destination |
|---|---|---|---|---|
| S | single-life annuity, life estate and remainder factors | 90CM, 2000CM, 2010CM | 11,000 | `dbo.tblS` |
| H | commutation factors (Dx/Nx/Mx) | 90CM, 2000CM, 2010CM | 11,000 | `dbo.tblH` |
| C | factors for reducing assurances (remainders in depreciable property) | 90CM, 2000CM, 2010CM | 11,000 | `dbo.tblC` |
| U(1) | one-life unitrust remainder factors | 90CM, 2000CM, 2010CM | 11,000 | `dbo.tblU1` |
| U(2) | two-life unitrust remainder factors | 90CM, 2000CM, 2010CM | 610,500 | `dbo.tblU2` |
| R(2) | two-life remainder factors | 90CM, 2000CM, 2010CM | 610,500 | `dbo.tblR2` |
| Z | unitrust commutation factors | 2000CM, 2010CM | 11,000 | `dbo.tblZ` |
| B | annuity, income and remainder interests for a term certain | all (not mortality based) | 6,000 | `dbo.tblB` |
| D | unitrust remainder factors postponed for a term of years | all (not mortality based) | 2,000 | `dbo.tblD` |
| F | unitrust adjusted payout rate factors | all (not mortality based) | 2,600 | `dbo.tblF` |
| J | adjustment factors, annuities paid at the *beginning* of each interval | all (not mortality based) | 500 | `dbo.tblJ` |
| K | adjustment factors, annuities paid at the *end* of each interval | all (not mortality based) | 500 | `dbo.tblK` |
| MortalityTable | mortality table (lx) | 1980, 1990, 2000, 2010 | 444 | `dbo.tblMortality` |

Row counts: one-life tables = 100 rates x 110 ages (0-109); two-life tables = 6,105 age
pairs (0-109, Age2 <= Age1) x 100 rates, shipped as 5 part files; B = 100 rates x 60
years; D = 100 payout rates x 20 years; F = 100 rates x 26 month bands; J/K = 100 rates
x 5 payment frequencies; MortalityTable = 111 ages (0-110) per year.

### Column reference

Columns live in `src/DataProcessingApp.Core/DataObjects/Table*Row.cs` with their exact
SQL Server names (`[DbColumn]`) and load-time rounding (`[Round]`); destination tables
are mapped in `src/DataProcessingApp.Data/SqlTables.cs`. JSON keys are the same names
in camelCase, with one legacy exception noted under Table S.

**Table S** (`dbo.tblS`) - single-life factors:

| Column | Type | Meaning |
|---|---|---|
| MortalityTable | int | census year of the series (1990 / 2010) |
| InterestRate | float, % | §7520 interest rate |
| Age | int | 0-109 |
| pvAnnuity | float | present value of a life annuity of 1 per year |
| pvLifeEstate | float | life estate factor |
| pvRemainderInterest | float | remainder factor |

> JSON files spell the last key `pvReminderInterest` (legacy typo kept for
> compatibility); the SQL column is correctly named `pvRemainderInterest`.

**Tables H and Z** (`dbo.tblH`, `dbo.tblZ`) - commutation factors (Z = unitrust variant):

| Column | Type | Meaning |
|---|---|---|
| MortalityTable | int | census year of the series |
| InterestRate | float, % | §7520 interest rate (unitrust payout rate for Z) |
| Age | int | 0-109 |
| dFactor | float | Dx = lx * v^x (discounted survivors) |
| nFactor | float | Nx = sum of Dj from age x to the end of the table |
| mFactor | float | Mx = sum of Dl * (death probability) from age x |

**Table C** (`dbo.tblC`) - factors for reducing assurances:

| Column | Type | Meaning |
|---|---|---|
| MortalityTable | int | census year of the series |
| Rate | float, % | interest rate |
| Age | int | 0-109 |
| remainderFactor | float | remainder interest factor |
| rFactor | float | R factor (deferred remainder component) |
| dFactor | float | D factor |

**Table U(1)** (`dbo.tblU1`) - one-life unitrust remainder factors:

| Column | Type | Meaning |
|---|---|---|
| MortalityTable | int | census year of the series |
| Age | int | 0-109 |
| AdjustedPayoutRate | float, % | unitrust payout rate adjusted for frequency |
| remainderFactor | float | remainder factor |

**Tables U(2) and R(2)** (`dbo.tblU2`, `dbo.tblR2`) - two-life remainder factors:

| Column | Type | Meaning |
|---|---|---|
| MortalityTable | int | census year of the series |
| Age1 | int | older life, 0-109 |
| Age2 | int | younger life, 0-109, Age2 <= Age1 |
| AdjustedPayoutRate | float, % | unitrust payout rate (U(2)) / interest rate (R(2)) |
| remainderFactor | float | remainder factor for the two lives |

**Table B** (`dbo.tblB`) - term certain factors:

| Column | Type | Meaning |
|---|---|---|
| Years | float | term 1-60 years |
| Rate | float, % | interest rate |
| pvAnnuity | float | annuity factor for the term (end-of-year payments) |
| pvIncomeInterest | float | income interest factor for the term |
| pvRemainderInterest | float | remainder factor for the term |

**Table D** (`dbo.tblD`) - term certain unitrust remainders:

| Column | Type | Meaning |
|---|---|---|
| Years | int | term 1-20 years |
| PayoutRate | float, % | unitrust payout rate |
| remainderInterest | float | remainder factor for the term |

**Table F** (`dbo.tblF`) - unitrust payout adjustment factors:

| Column | Type | Meaning |
|---|---|---|
| InterestRate | float, % | interest rate |
| Frequency | string | Annual / Semiannual / Quarterly / Monthly |
| Months | int | months (0-12) from the annual valuation date to the first payout |
| adjustmentFactor | float | factor for computing the adjusted payout rate |

**Tables J and K** (`dbo.tblJ`, `dbo.tblK`) - annuity payment adjustment factors
(J = paid at the *beginning* of each interval, K = paid at the *end*):

| Column | Type | Meaning |
|---|---|---|
| InterestRate | float, % | interest rate |
| Frequency | string | Annual / Semiannual / Quarterly / Monthly / Weekly |
| adjustmentFactor | float | multiplies the annual annuity factor for this frequency |

**MortalityTable** (`dbo.tblMortality`) - base mortality data:

| Column | Type | Meaning |
|---|---|---|
| Year | int | census year (1980, 1990, 2000, 2010) |
| Age | int | 0-110 |
| lx | float | number of survivors at the age out of 100,000 born |

## Data files

Three kinds of files live in the repository: **source** (from the IRS - never edit),
**extracted** (intermediate PDF-extracted CSVs for the 90CM series) and **processed**
(JSON - always regenerable by the scripts above). The SQL export XML in
`XMLFiles/` is additionally **generated** from the processed JSON on demand
(`JsonToXml.py`) and is not committed.

### Source files (`DataFiles/`)

Every source file is checksummed in `DataFiles/manifest.json` (SHA256, IRS URL
and download date). `python src/PythonDataApp/VerifyManifest.py` re-verifies
all checksums and the manifest schema and fails on mismatch or unlisted files;
`--download` restores missing or corrupted files from their recorded URLs. CI
runs it in the data-drift job. The exact IRS URLs in the manifest were
verified byte-identical in September 2026 for all 21 official spreadsheets
(root tables + 2000CM + 2010CM).

| Group | Files | Format | Origin |
|---|---|---|---|
| 90CM series | `DataFiles/90CM/*.pdf` (17 files: TableS, TableC, TableH, TableR(2) full + p1..p5, TableU(1), TableU(2) full + p1..p5, MortalityTable-90CM) | PDF | Publications 1457/1458/1459 (7-1999), archived at `https://www.irs.gov/pub/irs-prior/p1457--1999.pdf` (`-1458-`, `-1459-`); per-table page extracts, publication per file recorded in the manifest (1457: S/H/R(2), 1458: Mortality/U(1)/U(2), 1459: C, per the PDF embedded titles) |
| Root tables | `DataFiles/TableB.xlsx`, `TableD.xls`, `TableF.xls`, `TableJ.xlsx`, `TableK.xlsx` | XLSX / XLS | `https://www.irs.gov/retirement-plans/actuarial-tables` (not mortality based, one edition serves all series) |
| 2000CM series | `DataFiles/2000CM/table-*.xls(x)` (8 files: s, h, c, z, r2-2009, u1, u2-2009cm, 2000cm mortality) | XLS / XLSX | `https://www.irs.gov/retirement-plans/actuarial-tables` (e.g. `https://www.irs.gov/pub/irs-tege/table-s-2000cm.xlsx`) |
| 2010CM series | `DataFiles/2010CM/table-<name>-2010cm-final.xlsx` (8 files: s, h, c, z, r2, u1, u2, 2010cm mortality) | XLSX | `https://www.irs.gov/retirement-plans/actuarial-tables` |

### Extracted files (`src/PythonDataApp/DataFiles/90CM/`)

One CSV per PDF page set, regenerated from `DataFiles/90CM/*.pdf` by
`Extract90CMFromPdf.py` (originally produced by a one-time manual
[Tabula](http://tabula.technology/) extraction in 2016; the committed files are
kept as the verification reference):

| Files | Feeds |
|---|---|
| `TableS-90CM.csv`, `TableC-90CM-2.csv`, `TableH-90CM.csv`, `TableU(1)-90CM-2.csv` | `Process90CMTables.py` |
| `TableR(2)-p1..p5-90CM.csv`, `TableU(2)-p1..p5-90CM.csv` | `Process90CMTables.py` (per-part JSONs) |
| `MortalityTable-90CM.csv` | provenance record for the Table 90CM lx values (verified against `JSONFiles/MortalityTable.json`) |

### Processed and generated files (`JSONFiles/`, `XMLFiles/`)

| Location | Contents | Produced by |
|---|---|---|
| `JSONFiles/*.json` | root tables B, D, F, J, K + multi-year MortalityTable (committed) | `ConvertRootTablesToJson.py`; MortalityTable merged by `Convert2010CMToJson.py` |
| `JSONFiles/90CM/*.json` | 90CM series (S, C, H, U(1), U(2)/R(2) p1..p5) | `Process90CMTables.py` |
| `JSONFiles/2000CM/*.json` | 2000CM series (S, C, H, Z, U(1), U(2)/R(2) p1..p5) | `Convert2000CMToJson.py` |
| `JSONFiles/2010CM/*.json` | 2010CM series (S, C, H, Z, U(1), U(2)/R(2) p1..p5) | `Convert2010CMToJson.py` |
| `JSONFiles/<series>/TableR(2)-full.json`, `TableU(2)-full.json` | combined parts, created on demand | `CombineFiles.py` or the C# `CombineTableParts` step |
| `XMLFiles/*.xml` | SQL Server bulk-insert export format for the root tables (generated, not committed) | `JsonToXml.py` from `JSONFiles/*.json` |

(JSON files in the 90CM series store ages/factors as strings; 2010CM and root files
store numbers - see the data quality notes.)

## Data quality notes

Known issues in the data files, verified during the 2026 modernization:

1. **Table J was repaired (September 2026).** The committed `TableJ.json` / `TableJ.xml`
   were a byte-copy of Table K (end-of-interval factors under Table J's name). They were
   regenerated from the official IRS `table-j-final.xlsx` (adjustment factors for term-certain
   annuities payable at the *beginning* of each interval; rates 0.2%-20.0% step 0.2,
   4-decimal factors). Verified: all 500 rows satisfy the identity
   `J = K * (1+i)^(1/m)` against Table K within publication rounding, and the file
   round-trips through the C# pipeline unchanged. Table K itself was verified against
   `DataFiles/TableK.pdf` (450/450 rows of the 2.2%-22.0% grid match). The official IRS
   source file is archived as `DataFiles/TableJ.xlsx`
   (https://www.irs.gov/pub/irs-tege/table-j-final.xlsx); Table J has no official PDF,
   unlike Table K.
2. **Table K keeps 50 rows (rates 0.2%-2.0%) that appear in no 90CM source** (the 90CM grid
   starts at 2.2%). They are consistent with the current-edition grid, but their original
   provenance is unknown. Table J covers 0.2%-20.0% from the official file.
3. **90CM JSON stores ages and factors as strings**; the 2010CM series stores numbers.
   All loaders accept both; regenerate with `Process90CMTables.py --numeric` if you want
   numeric 90CM files.
4. **2010CM mortality lx values are fractional** (e.g. 99382.28); `tblMortality.lx` is
   `float` since this fix; redeploy the schema if you use the database features.
5. The 2016-era JSON files contained numpy float artifacts in interest rates
   (e.g. `2.4000000000000004`); the root tables were regenerated in September 2026
   (see note 6) and emit clean rates.
6. **Root tables B/D/F/J/K are now sourced from official IRS spreadsheets** (September 2026):
   `DataFiles/TableB.xlsx`, `TableD.xls`, `TableF.xls`, `TableK.xlsx`, `TableJ.xlsx`
   (all from https://www.irs.gov/retirement-plans/actuarial-tables; these tables are not
   mortality based, so one edition serves every series). `ConvertRootTablesToJson.py`
   regenerates the JSON; the regenerated files differ from the 2016-era PDF-extracted
   ones only in clean float rates. The old PDFs were removed. The 90CM series has no
   official spreadsheet (IRS began publishing spreadsheets with the 2000CM era), so its
   PDFs and the Tabula step remain for it.
7. **2010CM Table H / Table Z JSON had every dFactor/mFactor blank** - repaired September
   2026. `Convert2010CMToJson.py` read the wrong worksheet columns for H and Z (the
   spacers instead of the Dx/Mx columns), so all 11000 rows carried `" "` placeholders
   while nFactor parsed correctly. Column indices fixed, files regenerated and verified
   against the official XLSX; the C# app now loads the 2010CM series end to end.
   Table Z is also wired through the whole C# pipeline now (row type, `dbo.tblZ`
   schema, workflows); previously it existed only as a JSON file.
8. **The archived `MortalityTable-90CM.pdf` misprints age 58 as "68"** (Table 90CM
   block of ages 37-73, lx 87397). The lx value itself is correct — it sits smoothly
   between the lx values of ages 57 (88214) and 59 (86506) — so the printed age digit
   is the error. The committed `JSONFiles/MortalityTable.json` carries the corrected
   age 58, and `Extract90CMFromPdf.py` applies this single documented correction
   (`MORTALITY_AGE_MISPRINTS`) while re-extracting, failing if the printed value
   ever differs from what the correction expects.

## Planned improvements

Ideas for the next rounds of work (pipeline hardening, CLI options, 2000CM support,
reproducible 90CM extraction, DB integration tests) are tracked in `Docs/Plan.md`.

## Adding a new table or series

1. Add the row type in `src/DataProcessingApp.Core/DataObjects/` with `[DbColumn]` names,
   add the table type to `FilesHelper.TableType` and filename mapping in `FilesHelper.cs`,
   and the destination table in `SqlTables.cs`.
2. For CSV-sourced tables add a config entry to `Process90CMTables.py` (`TABLES`).
3. Add the table script to `db/` and bulk-insert support comes for free through
   `TableRepository<TRow>`.

---

**Author: Andrey Kukharenko.
Created on: December 2016. Modernized: September 2026.**
