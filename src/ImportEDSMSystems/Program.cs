// -SystemsZipfile "D:\data\EDSM-Dumps\systemsWithCoordinates.json.gz" -ExpandSystemName "D:\data\EDSM-Dumps\systemsWithCoordinates.json" -Force
// & "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe" /Sourcefile:D:\prj\Galaxy.Net7\src\eddb\bin\Release\eddb.dacpac /TargetDatabaseName:eddb /TargetServerName:"(localdb)\MSSQLLocalDB" /Action:Publish

using ImportEdsmSystems;
using Ionic.Zlib;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

// https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-implement-a-producer-consumer-dataflow-pattern

internal class Program
{
    private static void Main(string[] args)
    {

        // string connectionString = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={initialCatalog};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        // string destinationTableName = "[dbo].[tblEDSystemsWithCoordinates]";

        var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .AddCommandLine(args)
                .Build();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        logger.Information("SystemsWithCoordinatesUrl:{SystemsWithCoordinatesUrl}", configuration["SystemsWithCoordinatesUrl"]);
        logger.Information("SystemsWithCoordinatesFilename:{SystemsWithCoordinatesFilename}", configuration["SystemsWithCoordinatesFilename"]);
        logger.Information("DbConnection:{dbConnection}", configuration["DbConnection"]);

        Downloader.DownloadFile(logger, configuration["SystemsWithCoordinatesUrl"], configuration["SystemsWithCoordinatesFilename"]);
        string outputFile = CreateOutputFileName(configuration["SystemsWithCoordinatesFilename"]);
        UnzipFile(logger, configuration["SystemsWithCoordinatesFilename"], outputFile);

        Stopwatch w = Stopwatch.StartNew();
        SortedDictionary<int, EDSystemJson> list = [];
        using DataReaderExample.JsonDataReader rdr = new(configuration["SystemsWithCoordinatesFilename"]);

        int rowsRead = 0;
        //while (rdr.Read())
        //{
        //    list.Add(rdr.CurrentElement.Id, rdr.CurrentElement);
        //    rowsRead++;
        //    if (rowsRead % 1000000 == 0)
        //    {
        //        Console.WriteLine($"Rows read (million): {rowsRead / 1000000}", rowsRead);

        //    }
        //}

        w.Stop();
        Console.WriteLine($"Lines read: {list.Count}");
        Console.WriteLine($"Time elapsed: {w.Elapsed}");
    }

    private static string CreateOutputFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        if (!fileName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
        {
            return fileName + ".json";
        }

        var re = new Regex(@"\.gz$", RegexOptions.IgnoreCase);
        return re.Replace(fileName, "");
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