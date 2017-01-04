using System;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class TableKWorker
    {
        public static void LoadTableData()
        {
            // load data
            var filename = String.Format("{0}\\TableK.xml", AppHelper.BaseDataDir);

            var loader = new TableKLoader();
            loader.LoadFromXML(filename);
        }
    }
}
