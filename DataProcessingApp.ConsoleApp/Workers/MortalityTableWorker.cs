using System;
using DataProcessingApp.Core.Helpers;
using DataProcessingApp.Logic.Loaders;

namespace DataProcessingApp.ConsoleApp.Workers
{
    public static class MortalityTableWorker
    {
        public static void LoadTableData()
        {
            // load data
            var filename = String.Format("{0}\\MortalityTable.xml", AppHelper.BaseDataDir);

            var loader = new MortalityTableLoader();
            loader.LoadFromXML(filename);
        }
    }
}
