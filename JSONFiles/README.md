# Processed JSON data

- `90CM/` - 1990 series tables (ages/factors stored as strings).
- `2010CM/` - current series (numbers).
- Root files (TableB/D/F/J/K, MortalityTable) are not tied to a series.

## Warning: TableJ.json contains Table K data

`TableJ.json` is a byte-copy of `TableK.json`. IRS Table J (annuities payable at the
beginning of each interval) must differ from Table K (end of interval), and no Table J
source exists in this repository. **Do not use TableJ.* for real Table J factors.**
See "Data quality notes" in the repository Readme.
