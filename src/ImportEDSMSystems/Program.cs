using ImportEdsmSystems;
using Ionic.Zlib;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using System.Diagnostics;
using System.Globalization;
using System.Text;

// https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-implement-a-producer-consumer-dataflow-pattern

internal class Program
{
    private static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .AddCommandLine(args)
                .Build();

        var appSettings = new AppSettings(configuration["SystemsWithCoordinatesUrl"], configuration["SystemsWithCoordinatesFilename"], configuration["DbConnection"]);

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        logger.Information("SystemsWithCoordinatesUrl:{SystemsWithCoordinatesUrl}", appSettings.SystemsWithCoordinatesUrl);
        logger.Information("SystemsWithCoordinatesFilename:{SystemsWithCoordinatesFilename}", appSettings.SystemsWithCoordinatesFilename);
        logger.Information("ExpandedSystemsFileName:{ExpandedSystemsFileName}", appSettings.ExpandedSystemsFileName);
        logger.Information("DbConnection:{dbConnection}", appSettings.DbConnection);

        Downloader.DownloadFile(logger, appSettings.SystemsWithCoordinatesUrl, appSettings.SystemsWithCoordinatesFilename);
        UnzipFile(logger, appSettings.SystemsWithCoordinatesFilename, appSettings.ExpandedSystemsFileName);
        var systemsIds = await DB.GetAllIds(logger, appSettings.DbConnection);

        Stopwatch w = Stopwatch.StartNew();
        SortedDictionary<int, EDSystemJson> list = [];
        using DataReaderExample.JsonDataReader rdr = new(appSettings.ExpandedSystemsFileName);

        int rowsRead = 0;
        while (rdr.Read())
        {
            if (!systemsIds.Contains(rdr.CurrentElement.Id))
            {
                list.Add(rdr.CurrentElement.Id, rdr.CurrentElement);
                rowsRead++;
                if (rowsRead % 10000000 == 0)
                {
                    logger.Information("Rows read (million): {rowsRead}", rowsRead);
                    ExportToCsv(logger, list, appSettings.ExpandedSystemsFileName, rowsRead);
                    list.Clear();
                }
            }
        }

        if( list.Count > 0 )
        {
            ExportToCsv(logger, list, appSettings.ExpandedSystemsFileName, rowsRead);
            list.Clear();
        }

        w.Stop();
        logger.Information("Lines read: {rowsRead} in {elapsed} ms", rowsRead, w.ElapsedMilliseconds);
    }

    private static void ExportToCsv(Logger logger, SortedDictionary<int, EDSystemJson> list, string expandedSystemsFileName, int rowsRead)
    {
        var fileName = expandedSystemsFileName + "_" + rowsRead + ".csv";
        var fi = new FileInfo(fileName);
        if( fi.Exists) { fi.Delete(); }
        using StreamWriter sw = new(new FileStream(fileName, FileMode.Create), new UTF8Encoding(false));
        foreach( var row in list) 
        {
            var line = string.Format("{0},{1},\"{2}\",{3},{4},{5}",
                row.Value.Id,
                row.Value.Id64,
                row.Value.Name,
                row.Value.Coords.X.ToString(CultureInfo.InvariantCulture),
                row.Value.Coords.Y.ToString(CultureInfo.InvariantCulture),
                row.Value.Coords.Z.ToString(CultureInfo.InvariantCulture));
            sw.WriteLine(line);
        }

        sw.Close();
    }

    private static void UnzipFile(Logger logger, string fileName, string outputFile)
    {
        logger.Information("Unzip file: {fileName}", fileName);
        var sw = Stopwatch.StartNew();
        ArgumentNullException.ThrowIfNull(fileName);
        var fi = new FileInfo(fileName);
        if (!fi.Exists)
        {
            logger.Information("UnzipFile:File not found: {fileName}", fileName);
            return;
        }

        var outputFileinfo = new FileInfo(outputFile);
        if (outputFileinfo.Exists)
        {
            logger.Information("UnzipFile:File exists: {file}", outputFile);
            return;
        }

        logger.Information("UnzipFile:Outfile {file} was not found. Please wait, unzipping {file} to {outputFile} might take a few minutes", outputFile, fileName, outputFile);
        using (System.IO.Stream input = System.IO.File.OpenRead(fileName))
        using (Stream decompressor = new Ionic.Zlib.GZipStream(input, CompressionMode.Decompress, true))
        using (var output = System.IO.File.Create(outputFile))
            decompressor.CopyTo(output);

        sw.Stop();
        logger.Information("UnzipFile:Unzipped file {fileName} to {outputFile} in {elapsedMilliseconds} ms", fileName, outputFile, sw.ElapsedMilliseconds);
    }
}