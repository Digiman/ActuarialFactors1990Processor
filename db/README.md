# db/ reference archive

This folder holds the SQL Server **stored procedures** that existed before
Phase 5, kept verbatim as a reference. Nothing deploys or calls them anymore:

- The schema is owned by the EF Core migrations in
  [`src/DataProcessingApp.DataAccess/Migrations`](../src/DataProcessingApp.DataAccess/Migrations)
  (`make db-schema` applies them; `dotnet ef database update`).
- Application reads go through the EF entity queries in
  `DataProcessingApp.DataAccess` (`DbFactorData`).
- Seeding (`make db-fill`) bulk-copies the rows with the console `database`
  workflow; it does not use these procedures.

See `docs/adr/0001-ef-core-migrations-over-sqlproj.md` for the reasoning.
`Scripts/Dev/` are handy ad-hoc queries against a seeded database.