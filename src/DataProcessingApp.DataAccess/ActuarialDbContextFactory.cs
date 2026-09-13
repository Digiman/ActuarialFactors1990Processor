using DataProcessingApp.Core.Helpers;
using Microsoft.EntityFrameworkCore.Design;

namespace DataProcessingApp.DataAccess;

/// <summary>
/// Design-time factory for the EF Core CLI (migrations, database update):
/// resolves the connection string the same way the rest of the app does
/// (DPA_ConnectionStrings__Local / appsettings ConnectionStrings:Local),
/// with the local Docker SQL Server as the fallback. Commands can override
/// it with --connection.
/// </summary>
public class ActuarialDbContextFactory : IDesignTimeDbContextFactory<ActuarialDbContext>
{
    public ActuarialDbContext CreateDbContext(string[] args)
    {
        var connectionString = AppHelper.DatabaseConnectionString
            ?? "Server=localhost,1433;Database=DataProcessingAppDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=True";
        return new ActuarialDbContext(connectionString);
    }
}