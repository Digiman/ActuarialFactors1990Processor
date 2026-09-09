# Improvement plan

Candidate work items captured in September 2026 after the modernization PR
(#3, branch `feature/modernization`). The project's purpose stays the same:
**data transformation from IRS PDF/spreadsheet documents to JSON/XML files and
SQL Server tables**. Items are grouped into suggested phases; each phase fits
one branch / PR / session. Nothing here is committed to any release.

## Phase 1 - pipeline hardening (high value, low effort)

- [x] **Idempotent `database` workflow.** `TableRepository.InsertTableData`
      bulk-copies without clearing first, so reruns duplicate rows. Add
      truncate-per-table (or delete by MortalityTable year), wrap in a
      transaction, and report inserted-vs-loaded counts (record counts are
      already printed by `TableWorker.LogEnd`).
- [ ] **Deploy `GetLxFrom2010`.** The stored procedure file exists
      (`db/DataProcessingApp.Database/dbo/Stored Procedures/tblMortality/GetLxFrom2010.sql`)
      but was not referenced by the SSDT project, so it never deployed; the
      `.sqlproj` Build entry was added in September 2026 - redeploy the
      database project to create it in existing databases.
- [x] **Data-invariant tests.** Encode the checks that caught the 2026 data
      bugs as automated tests: `J = K * (1+i)^(1/m)` identity, per-table
      row-count grids (100 rates x 110 ages etc.), no `" "` placeholder
      strings anywhere, factor sanity/monotonicity.
- [x] **CI drift detection.** Job that reruns the Python converters and diffs
      the output against committed `JSONFiles/`; add
      `dotnet format --verify-no-changes`; add the C# `load` smoke workflow
      as a CI step.
- [x] **Exit codes and error isolation.** One missing file currently aborts a
      whole workflow with a stack trace; catch per table, print a failure
      summary, exit non-zero for automation.

## Phase 2 - usability

- [x] **CLI options:** `--help`, `--series 90CM|2010CM`, `--table S,K`
      filters, `--dry-run`. The series plumbing already exists in
      `TableWorker`; only the CLI needs exposing. Done with the
      Spectre.Console.Cli migration below.
- [x] **Central Package Management.** All `PackageReference` versions moved to
      a root `Directory.Packages.props`.
- [x] **Spectre.Console.Cli.** The hand-rolled `Program` switch replaced with
      a `CommandApp`: one command per workflow, shared `--series` / `--table` /
      `--dry-run` options, `--help` and `--version` for free.
- [x] **Source manifest.** `DataFiles/manifest.json` with SHA256, URL and
      download date for every source file + a small download/verify script;
      makes provenance machine-checkable instead of prose-only.
- [x] **2000CM series.** The only era with neither data nor support
      (5/1/2009 - 5/31/2023). IRS publishes its spreadsheets; converters are
      now generic enough that this is mostly config + a data download. Done:
      official IRS spreadsheets downloaded into `DataFiles/2000CM/` (manifest
      entries added), `Convert2000CMToJson.py` written, series wired into the
      C# workflows and data-invariant tests; MortalityTable row order
      normalized to chronological.
- [x] **Per-table timing.** Phase-level timing exists; add a stopwatch around
      each worker call next to the start/done log lines.

## Phase 3 - bigger items

- [ ] **Reproducible 90CM extraction.** The Tabula manual step is the last
      non-automated stage. Either commit the Tabula table templates/coords,
      replace with a scripted `pdfplumber` extractor, or declare the 90CM
      JSONs frozen artifacts with checksums (no re-extraction ever).
- [ ] **Docker SQL Server in CI.** Real integration test for `SqlBulkCopy` +
      stored procedures; the database path is currently untested.
- [ ] **XML as generated artifact.** `XMLFiles/` duplicates `JSONFiles/`;
      generate XML in CI from JSON (and stop committing it) or drop XML if
      nothing downstream requires it.
- [ ] **Serializer migration (optional).** Replace Newtonsoft.Json with
      System.Text.Json across `SerializerHelper`/loaders; low value, do only
      with other Core work.

## Decisions / caveats to revisit

- `appsettings.json` defaults point at `../../../../../JSONFiles|XMLFiles`,
  which resolve only when the working directory is the build output folder;
  `DPA_BaseDataDir` / `DPA_XmlDataDir` overrides are the reliable path.
- The 90CM JSON key `pvReminderInterest` is a legacy misspelling of
  `pvRemainderInterest` (SQL column is correct); renaming it would break the
  committed 90CM files and needs a migration plan if ever done.
- Table K's 50 rows below the 2.2% 90CM grid have unknown provenance (see
  data quality notes in the Readme); harmless but unexplained.
