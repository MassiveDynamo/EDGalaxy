using System.Text.RegularExpressions;

namespace ImportEdsmSystems
{
    internal class AppSettings(string? systemsWithCoordinatesUrl, string? systemsWithCoordinatesFilename, string? dbConnection)
    {
        private Regex re = new(@"\.gz$", RegexOptions.IgnoreCase);

        public string? SystemsWithCoordinatesUrl { get; set; } = systemsWithCoordinatesUrl;
        public string? SystemsWithCoordinatesFilename { get; set; } = systemsWithCoordinatesFilename;
        public string? DbConnection { get; set; } = dbConnection;

        public string ExpandedSystemsFileName
        {
            get
            {
                if (!SystemsWithCoordinatesFilename.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                {
                    return SystemsWithCoordinatesFilename + ".json";
                }

                
                return re.Replace(SystemsWithCoordinatesFilename, "");
            }
        }
    }
}
