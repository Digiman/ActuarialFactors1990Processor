# IRS Actuarial Factor Tables — Editions, Official Sources, and Pipeline Implications (RESEARCH)

**Date of research:** 2026-09-06
**Verification method:** All claims verified against primary sources fetched live on 2026-09-06:
- IRS publications pages and PDFs (`irs.gov/pub/irs-pdf/*`, `irs.gov/pub/irs-tege/*`, `irs.gov/pub/irs-prior/*`) — every URL below was HTTP-fetched and returned 200 (or is explicitly marked 404).
- U.S. Government Publishing Office (GovInfo) copies of the Federal Register final rules: the 2009 final rule (TD 9448) and the 2023 final rule (TD 9974) — downloaded as PDFs and their internal text decompressed/read.
- Federal Register API metadata for TD 9974 (document 2023-11837).
- The repo's own 1990-series PDFs (`DataFiles/90CM/TableS-90CM.pdf`, `TableB.pdf`, `TableK.pdf`, `MortalityTable-90CM.pdf`), whose embedded metadata was read for edition dates.
- Not used: blogs, third-party summaries, or secondary sources.

**Headline correction to the original research question:** the "current" generation is the **2010CM** tables; the final regulations making them mandatory are **effective June 1, 2023** (TD 9974) — **not** May 1, 2019. May 1, 2019 is the start of a *permissive transition period* during which taxpayers may use either basis. There is **no** "Rev. Rul. 2019-31" publishing these tables — `https://www.irs.gov/pub/irs-drop/rr-19-31.pdf` returns 404 (verified).

---

## Section 1 — Editions timeline

Source for the period/edition rows: the "Historical Synopsis of Tables" table printed in the *current* Publication 1457 (Rev. 6-2023), pages 3–4 of the PDF [https://www.irs.gov/pub/irs-pdf/p1457.pdf]; applicability dates of the 2000CM and 2010CM series cross-checked against the two final rules (below) and the IRS "Actuarial tables" landing page [https://www.irs.gov/retirement-plans/actuarial-tables].

| Valuation dates (period of use) | Basis life table | Rate basis | Official IRS publication releases | Authoritative citation |
|---|---|---|---|---|
| 1/1/1951 – 12/31/1970 | US1938 | 3.5% | Publication 11 | per Pub 1457 synopsis [https://www.irs.gov/pub/irs-pdf/p1457.pdf] |
| 1/1/1971 – 11/30/1983 | Table LN | 6% | 723, 723A, 723B | per Pub 1457 synopsis [https://www.irs.gov/pub/irs-pdf/p1457.pdf] |
| 12/1/1983 – 4/30/1989 | Table CM | 10% | 723C, 723D, 723E | per Pub 1457 synopsis [https://www.irs.gov/pub/irs-pdf/p1457.pdf] |
| 5/1/1989 – 4/30/1999 | 80CM (1980 census) | §7520 rates | Pub 1457/1458/1459 (5-1989 release) | per Pub 1457 synopsis [https://www.irs.gov/pub/irs-pdf/p1457.pdf]; IRC §7520 enacted 10/22/1988 per same synopsis footnote |
| 5/1/1999 – 4/30/2009 | **90CM** (1990 census) — **THIS REPO'S SERIES** | §7520 rates | Pub 1457/1458/1459 (7-1999 release) | per Pub 1457 synopsis [https://www.irs.gov/pub/irs-pdf/p1457.pdf]; the repo's `DataFiles/90CM/TableS-90CM.pdf` and `TableK.pdf` embed "Publication 1457 (7-1999)" as PDF Title, and `MortalityTable-90CM.pdf` embeds "Publication 1458 (7-1999)" |
| 5/1/2009 – 5/31/2023 | **2000CM** (2000 census) | §7520 rates | Pub 1457/1458/1459 (5-2009 release) | TD 9448, 74 FR 21438 (May 7, 2009), effective and applicable May 1, 2009 — verified in the text of the final rule [https://www.govinfo.gov/content/pkg/FR-2009-05-07/pdf/E9-10111.pdf]; also per Pub 1457 synopsis [https://www.irs.gov/pub/irs-pdf/p1457.pdf] |
| 6/1/2023 – present | **2010CM** (2010 census) | §7520 rates | Pub 1457/1458/1459 (6-2023 release) | TD 9974, 88 FR 37424 (June 7, 2023), effective June 1, 2023, applicable to valuations on or after June 1, 2023 — verified in the final rule [https://www.govinfo.gov/content/pkg/FR-2023-06-07/pdf/2023-11837.pdf] and Federal Register API metadata (document 2023-11837, effective_on 2023-06-01, 88 FR 37424) [https://www.federalregister.gov/api/v1/documents/2023-11837.json] |

Additional verified facts:

- **May 1, 2019 reliance window (why the "~2019" guess is half-right):** the 2023 final rule extends a transition rule to transactions with valuation dates **on or after May 1, 2019 and before June 2, 2023**, during which taxpayers may use either the 2000CM-based or the 2010CM-based factors (must be consistent across interests). The IRS landing page states it the same way: "For the period from May 1, 2019, to June 1, 2023, you may rely on either the prior or current tables" [https://www.irs.gov/retirement-plans/actuarial-tables]; also Pub 1457 (6-2023) footnote ** in the synopsis [https://www.irs.gov/pub/irs-pdf/p1457.pdf]. Commenters requested that date because it was the 10th anniversary of the 2000CM applicability date — per the final rule [https://www.govinfo.gov/content/pkg/FR-2023-06-07/pdf/2023-11837.pdf].
- **Rulemaking history of the 2010CM tables:** proposed regulations REG-122770-18, 87 FR 26806 (May 5, 2022), corrected 87 FR 34223 (June 6, 2022) — as cited inside TD 9974 [https://www.govinfo.gov/content/pkg/FR-2023-06-07/pdf/2023-11837.pdf]. There was no 2019 tax-ruling publication of these tables.
- **2010CM mortality basis:** "Table 2010CM, which is based on data compiled from the 2010 census" — final rule [https://www.govinfo.gov/content/pkg/FR-2023-06-07/pdf/2023-11837.pdf]; derived from the U.S. Decennial Life Tables 2009-2011 (NCHS), lx stated to seven digits — Pub 1457 and 1459 (6-2023) PDFs [https://www.irs.gov/pub/irs-pdf/p1457.pdf], [https://www.irs.gov/pub/irs-pdf/p1459.pdf]. **2000CM basis:** "data compiled from the 2000 census as set forth in Life Table 2000CM" — 2009 final rule [https://www.govinfo.gov/content/pkg/FR-2009-05-07/pdf/E9-10111.pdf].
- **The repo's "1990 series" naming:** the repo's tables are the 90CM tables from the July 1999 release (Pub 1457 Rev. 7-1999, "Actuarial Values Book Aleph"; mortality file is Pub 1458 7-1999 "Book Beth"), i.e. the edition that was in effect 1999–2009. `DataFiles/90CM/TableS-90CM.pdf` prints "Single Life Factors Based on Life Table 90CM" and the repo processors use the 2.2–22.0% interest grid (Section 4). [repo files; metadata read from the PDFs themselves]

---

## Section 2 — Current-edition (2010CM) official download URLs

All URLs verified live (HTTP 200) on 2026-09-06 unless noted.

**Publications (example/explanation booklets; also the authoritative edition labels):**

| Publication | Title / edition | Revision shown in PDF footer | Tables listed in it | URL | Last-modified (HTTP header) |
|---|---|---|---|---|---|
| Publication 1457 | Actuarial Values Version 4A (2023) | "Publication 1457 (Rev. 6-2023)", Catalog Number 63854M | Table S, Table B, Table H, Table K, Table J, Table R(2), Table 2010CM (per its "ACTUARIAL TABLES ASSOCIATED WITH PUBLICATION 1457" section) | [https://www.irs.gov/pub/irs-pdf/p1457.pdf](https://www.irs.gov/pub/irs-pdf/p1457.pdf) | 27 Jun 2023 |
| Publication 1458 | Actuarial Values Version 4B (2023) | "Publication 1458 (Rev. 6-2023)", Catalog Number 10719U | Table U(1), Table U(2), Table D, Table F, Table Z, Table 2010CM (per its associated-tables section) | [https://www.irs.gov/pub/irs-pdf/p1458.pdf](https://www.irs.gov/pub/irs-pdf/p1458.pdf) | 27 Jun 2023 |
| Publication 1459 | Actuarial Values Version 4C (2023) | "Publication 1459 (Rev. 6-2023)", Catalog Number 10720V | Table C, Table 2010CM | [https://www.irs.gov/pub/irs-pdf/p1459.pdf](https://www.irs.gov/pub/irs-pdf/p1459.pdf) | 27 Jun 2023 |

Important: these three PDFs are **short booklets (12, 12, and 6 pages)** of guidance and worked examples — they do **not** contain the full factor grids. They explicitly point to the tables "located at https://www.irs.gov/retirement-plans/actuarial-tables" [https://www.irs.gov/pub/irs-pdf/p1459.pdf, p.3]. The factor data of record is the spreadsheet set below.

**Official factor tables (XLS/XLSX), all linked from the IRS Actuarial tables page [https://www.irs.gov/retirement-plans/actuarial-tables] and all verified 200:**

| Table | Content | 2010CM URL |
|---|---|---|
| Table S | Single-life annuity/life estate/remainder factors | https://www.irs.gov/pub/irs-tege/table-s-2010cm-final.xlsx |
| Table R(2) | Two-life remainder factors | https://www.irs.gov/pub/irs-tege/table-r2-2010cm-final.xlsx |
| Table B | Term-certain annuity/income/remainder factors (not mortality-based) | https://www.irs.gov/pub/irs-tege/table-b-final.xlsx |
| Table H | Commutation factors | https://www.irs.gov/pub/irs-tege/table-h-2010cm-final.xlsx |
| Table K | Adjustment factors — annuities paid at end of period (not mortality-based) | https://www.irs.gov/pub/irs-tege/table-k-final.xlsx |
| Table J | Adjustment factors — term-certain annuities paid at beginning of period (not mortality-based; new in the 4A publication) | https://www.irs.gov/pub/irs-tege/table-j-final.xlsx |
| Table U(1) | One-life unitrust remainder factors | https://www.irs.gov/pub/irs-tege/table-u1-2010cm-final.xlsx |
| Table U(2) | Two-life unitrust remainder factors | https://www.irs.gov/pub/irs-tege/table-u2-2010cm-final.xlsx |
| Table D | Term-certain unitrust remainder factors (not mortality-based) | https://www.irs.gov/pub/irs-tege/table-d.xls |
| Table F | Unitrust payout-rate adjustment factors (not mortality-based) | https://www.irs.gov/pub/irs-tege/table-f.xls |
| Table Z | Unitrust commutation factors | https://www.irs.gov/pub/irs-tege/table-z-2010cm-final.xlsx |
| Table C | Depreciable-property remainder factors | https://www.irs.gov/pub/irs-tege/table-c-2010cm-final.xlsx |
| **Table 2010CM (lx)** | Mortality table (lx), 7-digit lx | https://www.irs.gov/pub/irs-tege/table-2010cm-final.xlsx |

**Mortality table (lx) publications for 2010CM** (three official places, all verified):
1. XLSX above: `table-2010cm-final.xlsx` (its workbook title reads "Table S - Based on Life Table 2010CM"; the lx file's shared strings call it Table 2010CM) [https://www.irs.gov/pub/irs-tege/table-2010cm-final.xlsx].
2. The final rule PDF (TD 9974) — the tables print there; in my downloaded copy the table pages rasterized as images, so I could verify the narrative text but not page-level table layouts [https://www.govinfo.gov/content/pkg/FR-2023-06-07/pdf/2023-11837.pdf].
3. Individual lx values are quoted in Pub 1457 (6-2023), e.g. l(21)=98,824.20, l(30)=97,989.90, l(60)=88,665.95, l(65)=84,221.59 [https://www.irs.gov/pub/irs-pdf/p1457.pdf].

**Not verified / dead links:**
- `https://www.irs.gov/forms-pubs/about-publication-1457`, `...1458`, `...1459` all return **404** (verified repeatedly). The "About" product pages for these publications no longer exist on irs.gov; the canonical page is the Actuarial tables landing page [https://www.irs.gov/retirement-plans/actuarial-tables].
- "Rev. Rul. 2019-31": `https://www.irs.gov/pub/irs-drop/rr-19-31.pdf` returns 404; no such ruling backs the 2010CM tables.

---

## Section 3 — 2000-series (2000CM) URLs

All verified 200 on 2026-09-06 unless noted.

**Factor tables (XLS/XLSX), same landing page [https://www.irs.gov/retirement-plans/actuarial-tables]:**

| Table | 2000CM URL |
|---|---|
| Table S | https://www.irs.gov/pub/irs-tege/table-s-2000cm.xlsx |
| Table R(2) | https://www.irs.gov/pub/irs-tege/table-r2-2009.xlsx |
| Table B | https://www.irs.gov/pub/irs-tege/table-b.xlsx |
| Table H | https://www.irs.gov/pub/irs-tege/table-h-2000cm.xls |
| Table U(1) | https://www.irs.gov/pub/irs-tege/table-u1-2000cm.xls |
| Table U(2) | https://www.irs.gov/pub/irs-tege/table-u2-2009cm.xlsx |
| Table Z | https://www.irs.gov/pub/irs-tege/table-z-2000cm.xls |
| Table C | https://www.irs.gov/pub/irs-tege/table-c-2000cm.xls |
| **Table 2000CM (lx)** | https://www.irs.gov/pub/irs-tege/table-2000cm.xls |

(Table D and Table F are the same non-mortality XLS files listed in Section 2 — https://www.irs.gov/pub/irs-tege/table-d.xls and https://www.irs.gov/pub/irs-tege/table-f.xls — the IRS page lists the identical file under both prior and current columns.)

**2000CM lx (mortality) official sources:**
1. XLS above: `table-2000cm.xls` [https://www.irs.gov/pub/irs-tege/table-2000cm.xls].
2. The 2009 final rule TD 9448 prints the tables and appears to include the mortality values (the PDF text I extracted contains the printed headings "TABLE S.—BASED ON LIFE TABLE 90CM … [Applicable After April 30, 1999 and Before May 1, 2009]", "TABLE U(1)—BASED ON LIFE TABLE 90CM …" and worked examples quoting "TABLE 2000CM value at age 60 … 87595" and "age 70 … 74794") [https://www.govinfo.gov/content/pkg/FR-2009-05-07/pdf/E9-10111.pdf]. Page-level layouts of those printed tables cannot be confirmed from my text extraction — requires opening the PDF.

**Archived prior publications:** `irs-prior` hosts the repo-era (90CM) editions, not 2000CM:
- https://www.irs.gov/pub/irs-prior/p1457--1999.pdf (200) — embedded PDF Title "Publication 1457 (7-1999)"; contains the 90CM mortality table ("TABLE 90CM Mortality Table") — i.e. this is the repo's series, not 2000CM.
- https://www.irs.gov/pub/irs-prior/p1458--1999.pdf (200), https://www.irs.gov/pub/irs-prior/p1459--1999.pdf (200).
- The 2000CM-era publications (5-2009 release, "Actuarial Values Version 3A/3B/3C") are NOT archived at any URL I could verify — **no verified URL; omitted rather than guessed**. (Version 3A is described as "forthcoming 2009" in TD 9448 [https://www.govinfo.gov/content/pkg/FR-2009-05-07/pdf/E9-10111.pdf]. Note also: the IRS landing page labels the prior pub "Actuarial Values 3A … (1999)" and links the 7-1999 PDF for it — that label conflicts with the 2009 final rule, which calls the 2009 release Version 3A [https://www.irs.gov/retirement-plans/actuarial-tables] vs [https://www.govinfo.gov/content/pkg/FR-2009-05-07/pdf/E9-10111.pdf]. Flagged as an IRS-page labeling inconsistency; the archived PDF itself is the 90CM 7-1999 edition.)

---

## Section 4 — What the repo's pipeline would need to change (INFERENCE, labeled as such)

This section is my inference from the verified facts above plus reading the repo (processors in `PythonDataApp/`, `DataProcessingApp.Core/Helpers/FilesHelper.cs`, `DataProcessingApp.Database/`). It is not peer-verified IRS fact.

Known constraints confirmed in the repo:
- `TableSProcessor.py`, `TableHProcessor.py`, `TableCProcessor.py` hardcode `interestRates = np.arange(2.2,22.1,0.2)`; `rowsPerPage = 57` (Table S/H), `55` (Table C), `38` (Table U(1)); pages computed from row counts [PythonDataApp/*.py].
- `FilesHelper.cs` maps `TableType` → a hardcoded filename, most with a `-1990` suffix ("TableC-1990-processed", "TableS-1990-processed", "TableH-1990-processed", "TableU1-1990-processed", "TableR(2)-full-1990", "TableU(2)-full-1990"), others without (TableB, TableD, TableF, TableJ, TableK, MortalityTable); no year/series dimension in the mapping [DataProcessingApp.Core/Helpers/FilesHelper.cs].
- JSON outputs carry the year in filenames only (e.g. `TableC-1990-processed.json`) [JSONFiles/]; a SQL Server project with publish profiles exists [DataProcessingApp.Database/].

Inferred change requirements to ingest the 2010CM (or 2000CM) series:

1. **The extraction stage becomes unnecessary — the source of record is spreadsheets, not PDFs.** The 4A/4B/4C publications are 6–12 page example booklets (verified, Section 2); the full factors ship as official XLSX/XLS with the rate grid as repeated section headers ("Interest at 0.2 Percent" … "Interest at 20.0 Percent"). The repo's Tabula→CSV→Python PDF-extraction workflow would be replaced by direct spreadsheet ingestion. Page-layout constants (rowsPerPage 57/55/38) would not transfer at all.
2. **The interest-rate grid changes**: 90CM (repo) grid is 2.2–22.0% in 0.2 steps (repo PDFs and processors), while BOTH the 2000CM and 2010CM Table S spreadsheets cover **0.2–20.0% in 0.2 steps (100 rates)** — verified from the files' own headers [https://www.irs.gov/pub/irs-tege/table-s-2000cm.xlsx], [https://www.irs.gov/pub/irs-tege/table-s-2010cm-final.xlsx]; likewise Table H, Table Z, Table C 2010CM show sections "of 0.2 Percent … of 20.0 Percent" (100 sections). Any hardcoded range or any downstream logic keyed to 2.2%–22.0% (or to the old adjustment-factor band "F(4.2) through F(14.0)" vs the new "Tables F(0.2) through F(20.0)" per the final rules [https://www.govinfo.gov/content/pkg/FR-2009-05-07/pdf/E9-10111.pdf], [https://www.govinfo.gov/content/pkg/FR-2023-06-07/pdf/2023-11837.pdf]) must be re-derived from the spreadsheet headers, not assumed.
3. **The year/series dimension is missing from the data model and file mapping.** `FilesHelper` maps TableType→single filename with hardcoded "-1990" suffixes and no series parameter; JSON filenames encode the year only as a suffix; the database/export paths follow. Adding 2000CM or 2010CM means parameterizing filenames, JSON, and DB rows by (series, table) — otherwise files overwrite each other. Also, the 2010CM series adds **Table J** (beginning-of-period adjustment factors) to the valued-table set [https://www.irs.gov/pub/irs-pdf/p1457.pdf], even though J was referenced by the regulations earlier (the 2009 final rule states Table J was "not changed" in 2009, i.e., it already existed in the 90CM-era regs [https://www.govinfo.gov/content/pkg/FR-2009-05-07/pdf/E9-10111.pdf]) — a TableType already exists for it in `FilesHelper.cs`, and a `TableJ.json` exists in the repo, but no TableJ processor or DataFiles PDF is present, so its provenance in the 1990 pipeline is unclear from the repo alone.