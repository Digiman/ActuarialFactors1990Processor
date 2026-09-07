# IRS Actuarial Factors processor (90CM / 2010CM series)

Utilities that process actuarial factor data published by the IRS (Publications 1457/1458/1459):

* source tables are extracted from PDF files with [Tabula](http://tabula.technology/) (manual step),
  or taken from the official 2010CM XLSX files;
* Python scripts convert the extracted CSV/XLSX data to JSON;
* the C# console application reads the JSON/XML files and can save data as
  JSON, text, Excel files, or bulk-insert it into SQL Server.

**PDF -> Tabula (manual) -> CSV -> Python (auto) -> JSON -> C# app (auto) -> Excel / text / SQL Server**

## Repository layout

```
src/                          source code
  DataProcessingApp.Core      row types, helpers, configuration
  DataProcessingApp.Logic     generic table loaders/savers (JSON, XML, Excel, text)
  DataProcessingApp.Data      SQL Server bulk-insert repositories
  DataProcessingApp.ConsoleApp  console application (workflows)
  PythonDataApp               Python scripts (CSV/XLSX -> JSON)
tests/
  DataProcessingApp.Tests     xUnit tests for the C# pipeline
  python                      pytest tests for the Python scripts
db/                           SSDT database project (tables, stored procedures)
DataFiles/                    source data (90CM PDFs, 2010CM XLSX)
JSONFiles/, XMLFiles/         processed data files (per-series subfolders)
Docs/                         research notes
```

## Requirements

* .NET SDK 10.0 (`dotnet build`, `dotnet test`, `dotnet run`)
* Python 3 with the packages from `src/PythonDataApp/requirements.txt` (`numpy`, `openpyxl`)
* SQL Server + SSDT only if you want the database features (the `db/` project builds in Visual Studio on Windows)

## Building and testing

```bash
dotnet build DataProcessingApp.sln
dotnet test tests/DataProcessingApp.Tests

python3 -m venv .venv && . .venv/bin/activate      # or use Anaconda
pip install -r src/PythonDataApp/requirements.txt -r tests/python/requirements.txt
python -m pytest tests/python
```

## Running the C# application

Configuration lives in `src/DataProcessingApp.ConsoleApp/appsettings.json`
(`BaseDataDir`, connection strings); every setting can be overridden with a
`DPA_`-prefixed environment variable, e.g. `DPA_BaseDataDir=/path/to/json/files`.

The workflow is selected by a command line argument:

```bash
dotnet run --project src/DataProcessingApp.ConsoleApp -- <workflow>
```

| Workflow    | What it does                                                        |
|-------------|---------------------------------------------------------------------|
| `load`      | loads every table from its source files (smoke check)               |
| `json`      | loads every table and saves it as JSON                              |
| `text`      | loads every table and saves it as a plain text file                 |
| `excel`     | loads every table and saves it as an Excel document (numbers as numeric cells) |
| `database`  | loads every table and bulk-inserts it into SQL Server               |
| `all`       | load + json + text + excel                                          |

`BaseDataDir` should contain the JSON files you want to process (copy the
contents of `JSONFiles/` there, or point it at the repository folder).

## Python scripts

All scripts are run from the repository root and use repo-relative paths by default:

```bash
python src/PythonDataApp/Process90CMTables.py              # 90CM CSV -> JSON (all tables)
python src/PythonDataApp/Process90CMTables.py --table S    # one table
python src/PythonDataApp/Process90CMTables.py --numeric    # emit numbers instead of strings
python src/PythonDataApp/CombineFiles.py                   # combine R(2)/U(2) parts into "-full" files
python src/PythonDataApp/Convert2010CMToJson.py            # 2010CM XLSX -> JSON (requires openpyxl)
python src/PythonDataApp/JsonToXml.py                      # root-table JSON -> SQL export XML
```

## Table details

Processed tables (90CM series - in effect 5/1/1999 to 4/30/2009; 2010CM series - effective 6/1/2023):

1. *Table C* - Factors for Reducing Assurances.
2. *Table R(2)* - two-life remainder factors (split into 5 part files per series).
3. *Table U(1)* - one-life unitrust factors.
4. *Table U(2)* - two-life unitrust factors (split into 5 part files per series).
5. *Table H* - Commutation Factors.
6. *Table S* - Single Life Factors.
7. *Table Z* - Unitrust Commutation Factors (2010CM only).

Root tables (not tied to a series): *Table B, D, F, J, K* and the multi-year *MortalityTable*.

Row fields per table are defined once in `src/DataProcessingApp.Core/DataObjects/Table*Row.cs`,
including the exact SQL Server column names (`[DbColumn]`) and any load-time rounding (`[Round]`).
Destination tables are mapped in `src/DataProcessingApp.Data/SqlTables.cs`.

## Data quality notes

Known issues in the data files, verified during the 2026 modernization:

1. **TableJ.json / TableJ.xml are a byte-copy of TableK.** The committed Table J data is
   Table K data (annuities payable at the *end* of each interval), while IRS Table J covers
   annuities payable at the *beginning* of each interval and must differ. Table K itself was
   verified against `DataFiles/TableK.pdf` (450/450 rows of the 2.2%-22.0% grid match).
   There is no Table J source anywhere in the repository, so treat `TableJ.*` as
   **unverified placeholder data - do not use it for Table J factors**.
2. **Rates 0.2%-2.0% in TableJ/K (50 rows) appear in no 90CM source** (the 90CM grid starts
   at 2.2%); their provenance is unknown.
3. **90CM JSON stores ages and factors as strings**, the 2010CM series stores numbers.
   All loaders accept both; regenerate with `Process90CMTables.py --numeric` if you want
   numeric 90CM files.
4. **2010CM mortality lx values are fractional** (e.g. 99382.28); `tblMortality.lx` is
   `float` since this fix - redeploy the schema if you use the database features.
5. The 2016-era JSON files contain numpy float artifacts in interest rates
   (e.g. `2.4000000000000004`); regenerated files emit clean rates.

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
