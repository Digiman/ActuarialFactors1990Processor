using System;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableFWorker
    {
        public static void LoadTableData()
        {
            // load data
            var filename = String.Format("{0}\\TableF.xml", AppHelper.BaseDataDir);

            var loader = new TableFLoader();
            loader.LoadFromXML(filename);
        }
    }
}
