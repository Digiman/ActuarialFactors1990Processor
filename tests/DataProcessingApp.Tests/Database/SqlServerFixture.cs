using Microsoft.Data.SqlClient;
using System;
using System.IO;
using Xunit;

namespace DataProcessingApp.Tests.Database;

/// <summary>
/// Marks the SQL Server integration tests. The fixture deploys the schema from
/// the db/ SSDT project files into a real SQL Server (docker compose or any
/// other instance) and is shared by all tests in the collection.
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

    private void DeploySchema()
    {
        var tableFiles = Directory.GetFiles(Path.Combine(RepositoryRoot, "db", "DataProcessingApp.Database", "dbo", "Tables"), "*.sql");
        var procedureFiles = Directory.GetFiles(
            Path.Combine(RepositoryRoot, "db", "DataProcessingApp.Database", "dbo", "Stored Procedures"),
            "*.sql", SearchOption.AllDirectories);

        using (var connection = new SqlConnection(ConnectionString))
        {
            connection.Open();

            // drop first, so reruns are idempotent; the tables have no foreign keys
            foreach (var file in tableFiles)
            {
                Execute(connection, $"DROP TABLE IF EXISTS dbo.{ObjectName(file)}");
            }
            foreach (var file in procedureFiles)
            {
                Execute(connection, $"DROP PROCEDURE IF EXISTS dbo.{ObjectName(file)}");
            }

            foreach (var file in tableFiles)
            {
                Execute(connection, ReadStatement(file));
            }
            foreach (var file in procedureFiles)
            {
                Execute(connection, ReadStatement(file));
            }
        }
    }

    private static string ObjectName(string sqlFile)
    {
        return Path.GetFileNameWithoutExtension(sqlFile);
    }

    private static string ReadStatement(string sqlFile)
    {
        return File.ReadAllText(sqlFile).TrimStart('\uFEFF');
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
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "db", "DataProcessingApp.Database")))
        {
            directory = directory.Parent;
        }
        if (directory == null)
        {
            throw new DirectoryNotFoundException("Repository root with db/DataProcessingApp.Database not found.");
        }
        return directory.FullName;
    }
}