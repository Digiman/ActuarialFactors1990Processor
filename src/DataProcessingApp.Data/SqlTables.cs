using DataProcessingApp.Core.Helpers;

namespace DataProcessingApp.Data
{
    /// <summary>
    /// Maps actuarial table types to SQL Server destination tables.
    /// </summary>
    public static class SqlTables
    {
        private static readonly System.Collections.Generic.Dictionary<TableType, string> DestinationTables =
            new System.Collections.Generic.Dictionary<TableType, string>
            {
                { TableType.TableB, "[dbo].[tblB]" },
                { TableType.TableC, "[dbo].[tblC]" },
                { TableType.TableD, "[dbo].[tblD]" },
                { TableType.TableF, "[dbo].[tblF]" },
                { TableType.TableH, "[dbo].[tblH]" },
                { TableType.TableJ, "[dbo].[tblJ]" },
                { TableType.TableK, "[dbo].[tblK]" },
                { TableType.TableR2, "[dbo].[tblR2]" },
                { TableType.TableS, "[dbo].[tblS]" },
                { TableType.TableU1, "[dbo].[tblU1]" },
                { TableType.TableU2, "[dbo].[tblU2]" },
                { TableType.MortalityTable, "[dbo].[tblMortality]" }
            };

        public static string DestinationTable(TableType tableType)
        {
            return DestinationTables[tableType];
        }
    }
}
