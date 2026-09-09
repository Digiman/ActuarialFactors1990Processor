# Processed JSON data

- `90CM/` - 1990 series tables (ages/factors stored as strings).
- `2010CM/` - current series (numbers).
- Root files (TableB/D/F/J/K, MortalityTable) are not tied to a series.

## Note: TableJ.json was repaired (September 2026)

`TableJ.json` originally contained a byte-copy of Table K data. It was regenerated
from the official IRS `table-j-final.xlsx` (adjustment factors for annuities payable
at the beginning of each interval, rates 0.2%-20.0%) and verified via the identity
`J = K * (1+i)^(1/m)`. See "Data quality notes" in the repository Readme.
