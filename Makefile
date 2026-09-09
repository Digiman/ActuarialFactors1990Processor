# Task runner for the IRS Actuarial Factors processor.
# Run `make` or `make help` to list all targets.

SHELL := /bin/bash
.DEFAULT_GOAL := help

PYTHON := python3
DOTNET := dotnet
SCRIPTS := src/PythonDataApp

# SQL Server container credentials and connection string.
# Override e.g. with: make db-fill SA_PASSWORD='My!Password1' or CONN='Server=...'
SA_PASSWORD ?= YourStrong!Passw0rd
CONN ?= Server=localhost,1433;Database=DataProcessingAppDB;User Id=sa;Password=$(SA_PASSWORD);TrustServerCertificate=True;Encrypt=True
DPA_ENV := DPA_BaseDataDir=JSONFiles DPA_XmlDataDir=XMLFiles DPA_ConnectionStrings__Local="$(CONN)"

.PHONY: help install build test test-integration format-check \
        db-up db-down db-schema db-fill \
        data-extract data-json data-xml data-verify \
        run web clean

help: ## Show this help
	@grep -E '^[a-z0-9_-]+:.*##' $(MAKEFILE_LIST) \
		| awk -F':.*## ?' '{printf "  \033[1;36m%-20s\033[0m %s\n", $$1, $$2}'

install: ## Install Python dependencies (numpy, openpyxl, xlrd, pdfplumber, pytest)
	$(PYTHON) -m pip install -r $(SCRIPTS)/requirements.txt -r tests/python/requirements.txt

build: ## Build the .NET solution
	$(DOTNET) build DataProcessingApp.sln

test: ## Run the fast tests: C# unit/data-invariant tests + Python tests
	$(DOTNET) test tests/DataProcessingApp.Tests
	$(PYTHON) -m pytest tests/python -q

test-integration: db-up ## Run the SQL Server integration tests (starts the container first)
	$(DOTNET) test tests/DataProcessingApp.Tests --filter Category=Integration

format-check: ## Verify the dotnet format rules (same check as CI)
	$(DOTNET) format DataProcessingApp.sln --verify-no-changes

db-up: ## Start the SQL Server 2022 container and wait until healthy
	@MSSQL_SA_PASSWORD="$(SA_PASSWORD)" docker compose up --detach --wait sqlserver

db-down: ## Stop the SQL Server container
	@docker compose down

db-schema: db-up ## Deploy the schema from the db/ project files (drops existing objects first)
	@docker exec dpa-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa \
		-P "$(SA_PASSWORD)" \
		-Q "IF DB_ID(N'DataProcessingAppDB') IS NULL CREATE DATABASE [DataProcessingAppDB]"
	@for f in db/DataProcessingApp.Database/dbo/Tables/*.sql \
	          "db/DataProcessingApp.Database/dbo/Stored Procedures"/*/*.sql; do \
		o=$$(basename "$$f" .sql); \
		if [ "$$(dirname "$$f")" = "db/DataProcessingApp.Database/dbo/Tables" ]; then \
			docker exec dpa-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa \
				-P "$(SA_PASSWORD)" -d DataProcessingAppDB \
				-Q "DROP TABLE IF EXISTS dbo.[$$o]" || exit 1; \
		else \
			docker exec dpa-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa \
				-P "$(SA_PASSWORD)" -d DataProcessingAppDB \
				-Q "DROP PROCEDURE IF EXISTS dbo.[$$o]" || exit 1; \
		fi; \
	done
	@for f in db/DataProcessingApp.Database/dbo/Tables/*.sql \
	          "db/DataProcessingApp.Database/dbo/Stored Procedures"/*/*.sql; do \
		echo "  $$f"; \
		sed $$'1s/^\xEF\xBB\xBF//' "$$f" | docker exec -i dpa-sqlserver /opt/mssql-tools18/bin/sqlcmd \
			-C -S localhost -U sa -P "$(SA_PASSWORD)" -d DataProcessingAppDB -b || exit 1; \
	done

db-fill: db-schema data-xml ## Fill the database from all sources via the C# database workflow
	$(DPA_ENV) $(DOTNET) run --project src/DataProcessingApp.ConsoleApp -- database

data-extract: ## Extract the 90CM tables from the source PDFs into CSVs (verifies against the committed reference)
	$(PYTHON) $(SCRIPTS)/Extract90CMFromPdf.py

data-json: ## Convert every source into JSON: 90CM CSVs, root tables, 2000CM, 2010CM
	$(PYTHON) $(SCRIPTS)/Process90CMTables.py
	$(PYTHON) $(SCRIPTS)/ConvertRootTablesToJson.py
	$(PYTHON) $(SCRIPTS)/Convert2000CMToJson.py
	$(PYTHON) $(SCRIPTS)/Convert2010CMToJson.py

data-xml: ## Generate XMLFiles/ (SQL export XML) from JSONFiles/ - required by the C# workflows
	$(PYTHON) $(SCRIPTS)/JsonToXml.py

data-verify: ## Verify the source data files against DataFiles/manifest.json
	$(PYTHON) $(SCRIPTS)/VerifyManifest.py

run: data-xml ## Run a console app workflow. ARGS: "load", "database", "excel --dry-run", "factor --scenario life-estate --age 65 --rate 5.2" ...
	$(DPA_ENV) $(DOTNET) run --project src/DataProcessingApp.ConsoleApp -- $(ARGS)

web: ## Start the factor web UI (http://localhost:5000)
	DPA_BaseDataDir=$(CURDIR)/JSONFiles $(DOTNET) run --project src/DataProcessingApp.WebApi

clean: ## Remove build outputs and Python caches
	$(DOTNET) clean DataProcessingApp.sln
	rm -rf .pytest_cache tests/python/__pycache__ src/PythonDataApp/__pycache__
