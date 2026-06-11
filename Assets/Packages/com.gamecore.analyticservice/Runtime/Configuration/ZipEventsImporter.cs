using System.IO.Compression;

namespace GameCore.AnalyticService
{
    public class ZipEventsImporter
    {
        public ZipEventsContainer Import(string path)
        {
            ZipArchive zip = ZipFile.OpenRead(path);
            ZipEventsContainer container = new ZipEventsContainer(zip);

            return container;
        }
    }
}