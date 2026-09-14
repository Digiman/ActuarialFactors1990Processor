using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace DataProcessingApp.Tests.Database;

/// <summary>
/// Marks the SQL Server integration tests. The fixture applies the EF Core
/// migrations from DataProcessingApp.DataAccess to a real SQL Server (docker
/// compose or any other instance) and is shared by all tests in the
/// collection.
/// </summary>
[CollectionDefinition("sqlserver")]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
}

public class SqlServerFixture : IDisposable
{
    public const string DefaultConnectionString =
        "Server=localhost,1433;Database=DataProcessingAppDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=True";

    public SqlServerFixture()
    {
        ConnectionString = Environment.GetEnvironmentVariable("DPA_TEST_CONNECTION_STRING") ?? DefaultConnectionString;
        try
        {
            PrepareDatabase();
            DeploySchema();
            Available = true;
        }
        catch (Exception exception)
        {
            Available = false;
            UnavailableReason = string.Format(
                "SQL Server is not available ({0}). Start it with: docker compose up -d --wait sqlserver",
                exception.Message);
        }
    }

    public string ConnectionString { get; }

    public string RepositoryRoot { get; } = FindRepositoryRoot();

    public bool Available { get; }

    public string UnavailableReason { get; }

    public void Dispose()
    {
    }

    private void PrepareDatabase()
    {
        var master = new SqlConnectionStringBuilder(ConnectionString)
        {
            InitialCatalog = "master",
            ConnectTimeout = 5
        }.ConnectionString;

        using (var connection = new SqlConnection(master))
        {
            connection.Open();
            Execute(connection, "IF DB_ID(N'DataProcessingAppDB') IS NULL CREATE DATABASE [DataProcessingAppDB]");
        }
    }

    /// <summary>
    /// Applies the EF Core migrations (idempotent), the same way
    /// `make db-schema` does. Requires `dotnet tool restore` to have run
    /// (the dotnet-ef CLI comes from .config/dotnet-tools.json).
    /// </summary>
    private void DeploySchema()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = RepositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add("ef");
        startInfo.ArgumentList.Add("database");
        startInfo.ArgumentList.Add("update");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add("src/DataProcessingApp.DataAccess/DataProcessingApp.DataAccess.csproj");
        startInfo.ArgumentList.Add("--connection");
        startInfo.ArgumentList.Add(ConnectionString);

        using (var process = Process.Start(startInfo))
        {
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    "EF migrations failed: " + error + output);
            }
        }
    }

    private static void Execute(SqlConnection connection, string statement)
    {
        using var command = connection.CreateCommand();
        command.CommandText = statement;
        command.ExecuteNonQuery();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null &&
               !File.Exists(Path.Combine(directory.FullName, "src", "DataProcessingApp.DataAccess", "DataProcessingApp.DataAccess.csproj")))
        {
            directory = directory.Parent;
        }
        if (directory == null)
        {
            throw new DirectoryNotFoundException("Repository root with src/DataProcessingApp.DataAccess not found.");
        }
        return directory.FullName;
    }
}