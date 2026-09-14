using DataProcessingApp.Core.DataObjects;
using System.Collections.Generic;

namespace DataProcessingApp.Calculator;

/// <summary>
/// A source for the actuarial table rows the scenarios compute on. The file
/// implementation (FactorData over the committed JSON) is the offline/test
/// path; the database implementation (DbFactorData in
/// DataProcessingApp.DataAccess) is the web runtime path.
/// </summary>
public interface FactorDataSource
{
    List<TableSRow> TableS(string series);

    List<TableU1Row> TableU1(string series);

    List<TableU2Row> TableU2(string series);

    List<TableR2Row> TableR2(string series);

    List<TableBRow> TableB();

    List<TableDRow> TableD();

    List<TableFRow> TableF();

    List<TableJRow> TableJ();

    List<TableKRow> TableK();

    List<MortalityTableRow> Mortality();

    /// <summary>lx values for one census year (1990 / 2000 / 2010).</summary>
    List<MortalityTableRow> Mortality(int year);

    /// <summary>Census year behind a series (1990 / 2000 / 2010).</summary>
    int CensusYear(string series);
}