# ActuarialFactors1990Processor Context

This project processes the IRS-published actuarial valuation tables (census
years 1990/2000/2010) into machine-readable data and answers valuation
questions ("factor scenarios") against that data — via a CLI, a web UI, and a
SQL Server database.

## Language

**Actuarial factor table** (or **factor table**):
One of the IRS-published valuation tables (S, U(1), U(2), R(2), B, D, F, H,
J, K, C, Z, mortality/lx) used to value life estates, remainders, annuities
and unitrust interests.
_Avoid_: table, spreadsheet, grid

**Series**:
A mortality basis: 90CM (1990 census), 2000CM, or 2010CM (current). Each
series-backed factor table publishes one grid per series; rate-only tables
(B, D, F, J, K, C, Z) are shared across series.
_Avoid_: era, model, cohort

**Census year**:
The calendar year behind a series (1990/2000/2010). The database stores it as
the integer `MortalityTable` column on series-backed tables.
_Avoid_: year (when ambiguity with `Age`/`Year` matters)

**Factor scenario**:
A valuation question answered by the calculator (life-estate, annuity,
unitrust, unitrust two-life, annuity trust two-life, term-certain,
term-unitrust, mortality). Results are exact published-grid lookups.
_Avoid_: calculation, computation

**Grid lookup**:
Reading a published factor directly from the table grid by its inputs; no
interpolation or invented values.
_Avoid_: lookup, estimate

**Seeding** (or **filling**):
Loading the actuarial tables from the committed JSON data into the SQL
Server database. Idempotent by design.
_Avoid_: importing, uploading, populating (when the target is the database)

**Data source**:
Where a factor table's rows come from — the committed JSON files or the SQL
Server database. The calculator works against either.
_Avoid_: backend, storage, provider