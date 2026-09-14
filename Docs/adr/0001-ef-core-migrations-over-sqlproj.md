# EF Core migrations own the schema; reads go through entity queries

The database schema used to live in the `db/DataProcessingApp.Database`
SQLProj files, deployed by a make target shell loop, with every read path
going through hand-written stored procedures. As of Phase 5, the schema is
created and evolved only through EF Core migrations in
`DataProcessingApp.DataAccess`, and application reads use EF entity queries.
The SQLProj, its table DDL and the stored procedures were removed entirely:
EF Core covers schema management and reads, and none of the procedures was
called anymore after the migration (the archive briefly kept around proved
unnecessary and was dropped again; `git history` retains the files).

We chose this because the stored procedures never covered all 13 tables the
calculator needs (no `GetAll` for D/F/J/K/U1/U2/R2), EF Core tooling
(`dotnet ef migrations`, scaffolding, migration bundles) is now first-class
on .NET 10, and one schema source of truth beats two that must be kept in
sync by hand.

## Considered options

- **Keep SQLProj + stored procedures, use EF only for reads**: two schema
  descriptions to keep in sync for no benefit.
- **Add the missing `GetAll` procedures and route every read through them
  via EF raw SQL**: keeps the procedure layer but duplicates what a typed
  query gives for free, and the procedures carry no logic worth preserving.
- **Switch to SQLite/Postgres**: the entire pipeline (bulk writers,
  integration tests, compose, CI) already targets SQL Server 2022; no reason
  to pay that cost.

## Consequences

- Baseline EF migration must reproduce the old table shapes exactly
  (including `float` columns and the int `MortalityTable` census-year
  column), so existing databases adopt seamlessly via `dotnet ef database
  update`.
- Seeding is unchanged: the console `database` workflow bulk-copies into
  the same table names, now created by migrations.
- SQL Server integration tests moved from stored-procedure assertions to
  EF entity round-trips.