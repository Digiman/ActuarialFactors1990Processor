# Improvement plan

Candidate work items captured in September 2026 after the modernization PR
(#3, branch `feature/modernization`). The project's purpose is now twofold:
**data transformation from IRS PDF/spreadsheet documents to JSON/XML files and
SQL Server tables**, and **answering valuation questions with that data**
(factor scenarios via the `factor` command and the web UI, Phase 4). Items are
grouped into suggested phases; each phase fits one branch / PR / session.
Nothing here is committed to any release.

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

- [x] **Reproducible 90CM extraction.** The Tabula manual step is the last
      non-automated stage. Either commit the Tabula table templates/coords,
      replace with a scripted `pdfplumber` extractor, or declare the 90CM
      JSONs frozen artifacts with checksums (no re-extraction ever).
      Done (September 2026): `Extract90CMFromPdf.py` (pinned `pdfplumber`,
      pure Python, no Java) regenerates all 14 Tabula CSVs plus a new
      `MortalityTable-90CM.csv` from `DataFiles/90CM/*.pdf`; every table is
      verified value-by-value against the committed 2016 Tabula CSVs (Table 90CM
      lx against `MortalityTable.json`) before anything is written, and the
      90CM JSON output of `Process90CMTables.py` is byte-identical to the
      committed files. The one PDF defect found (age 58 printed as "68" in the
      mortality table) is a documented, enforced correction — see the data
      quality notes in the Readme. CI reruns the extraction in the data-drift job.
- [x] **Docker SQL Server in CI.** Real integration test for `SqlBulkCopy` +
      stored procedures; the database path is currently untested.
      Done (September 2026): `docker-compose.yml` runs SQL Server 2022 locally
      and in CI (dedicated `database` job); the integration tests
      (`tests/DataProcessingApp.Tests/Database/`, `Category=Integration`)
      deploy the schema from the `db/` project SQL files into the container and
      verify bulk-insert idempotency, transactional rollback on failure, and
      the stored procedures including `GetLxFrom2010`. Tests skip automatically
      when no SQL Server is reachable.
- [x] **XML as generated artifact.** `XMLFiles/` duplicates `JSONFiles/`;
      generate XML in CI from JSON (and stop committing it) or drop XML if
      nothing downstream requires it. Done (September 2026): `XMLFiles/` is
      untracked (gitignored) and generated on demand with `JsonToXml.py` from
      the committed JSON; CI generates it before the C# `load` smoke test, and
      the data-drift job no longer checks it. The C# workflows keep reading the
      XML export format for root tables.
- [x] **Serializer migration (optional).** Replace Newtonsoft.Json with
      System.Text.Json across `SerializerHelper`/loaders; low value, do only
      with other Core work. Done (September 2026): JSON moved to
      System.Text.Json (case-insensitive keys, numbers readable from strings
      for the 90CM series); the XML paths are unchanged. All 555 tests pass and
      the C# `json` workflow round-trips the committed root JSON files
      value-identically; the only output difference is cosmetic (whole-number
      doubles serialize as `1` instead of `1.0`), so the committed JSONs were
      left untouched.

## Phase 4 - using the data (scenarios: CLI + web)

Done (September 2026). The pipeline only moved data around; this phase makes it
answer valuation questions with the IRS-published factors.

- [x] **Calculator library** (`DataProcessingApp.Calculator`): cached,
      thread-safe table access over the committed JSON (two-life tables from
      their five part files) plus scenario computation. Eight scenarios:
      life-estate (S), annuity (S + J/K), unitrust (F + U(1)), unitrust
      two-life (F + U(2)), annuity trust two-life (R(2)), term-certain (B),
      term-unitrust (D), mortality (lx). All results are exact published-grid
      lookups - no interpolation; off-grid inputs fail with the actual grid in
      the message. The CRUT scenarios apply the Rev. Proc. 89-21 payout
      adjustment (payout / Table F factor, nearest 0.2 step); Table F's
      (frequency, months) combos are validated against the published ranges.
      A scenario catalog (metadata + input descriptors) is the single source
      of truth for the CLI, the web UI forms and the API contract.
- [x] **`factor` CLI command**: one scenario per invocation
      (`make run ARGS="factor --scenario life-estate --age 65 --rate 5.2"`),
      Spectre table output with the table citations; `factor` without
      `--scenario` lists every scenario with its inputs.
- [x] **Web UI + JSON API** (`DataProcessingApp.WebApi`, `make web` at
      http://localhost:5000): static single-page UI (vanilla JS, no CDN) with
      scenario picker, input forms generated from the catalog, result cards,
      and a factor-vs-age SVG chart with optional 90CM/2000CM/2010CM overlay.
      API: `GET /api/scenarios`, `GET /api/series`, `POST /api/calc`,
      `POST /api/sweep/age` (age sweep, per-published-grid gaps allowed,
      compare-all-series option), `GET /api/mortality?year=`. Everything
      reads the committed JSON directly (no XMLFiles dependency).
- [x] **Calculator tests**: 22 xUnit tests against the real committed data -
      published identities (income + remainder = 1), manual-lookup
      equivalences, adjustment direction (beginning > end, more frequent
      payments => higher adjusted payout rate => smaller remainder), age
      swap for two-life scenarios, validation errors, term-certain
      identities, committed-value regressions.

## Decisions / caveats to revisit

- `appsettings.json` defaults point at `../../../../../JSONFiles|XMLFiles`,
  which resolve only when the working directory is the build output folder;
  `DPA_BaseDataDir` / `DPA_XmlDataDir` overrides are the reliable path.
- The 90CM JSON key `pvReminderInterest` is a legacy misspelling of
  `pvRemainderInterest` (SQL column is correct); renaming it would break the
  committed 90CM files and needs a migration plan if ever done.
- Table K's 50 rows below the 2.2% 90CM grid have unknown provenance (see
  data quality notes in the Readme); harmless but unexplained.
