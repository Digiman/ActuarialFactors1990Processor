using System;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableBWorker
    {
        public static void LoadTableData()
        {
            // load data
            var filename = String.Format("{0}\\TableB.xml", AppHelper.BaseDataDir);

            var loader = new TableBLoader();
            loader.LoadFromXML(filename);
        }
    }
}
