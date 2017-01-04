using System;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableDWorker
    {
        public static void LoadTableData()
        {
            // load data
            var filename = String.Format("{0}\\TableD.xml", AppHelper.BaseDataDir);

            var loader = new TableDLoader();
            loader.LoadFromXML(filename);
        }
    }
}
