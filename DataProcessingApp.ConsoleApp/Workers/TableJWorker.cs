using System;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableJWorker
    {
        public static void LoadTableData()
        {
            // load data
            var filename = String.Format("{0}\\TableJ.xml", AppHelper.BaseDataDir);

            var loader = new TableJLoader();
            loader.LoadFromXML(filename);
        }
    }
}
