using SyncMapProject.Silkroad;
using Newtonsoft.Json;

using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace SyncMapProject
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "SyncMapProject v" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion + " - https://github.com/JellyBitz/SyncMapProject";
            Console.WriteLine(Console.Title + Environment.NewLine);

            // Load settings
            Settings settings = LoadOrCreateFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json"));

            // Abort if there is no path given
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("1. Set up your database connection at \"Settings.json\"");
                Console.WriteLine("2. Drag & drop your \"mapinfo.ifo\" file into the executable!");
                Console.ReadKey();
                return;
            }

            // Check file existence
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Error: file not found...");
                Console.ReadKey();
                return;
            }
            var filePath = args[0];

            // Try to load the given mapinfo file
            JMXVMFO mapInfo = new JMXVMFO();
            if (!mapInfo.Load(filePath))
            {
                Console.WriteLine("Error: MapInfo file cannot be loaded...");
                Console.ReadKey();
                return;
            }

            // Create database handler
            SQLDataDriver sql = new SQLDataDriver(settings.SQLConnection.Host, settings.SQLConnection.Username, settings.SQLConnection.Password, settings.SQLConnection.Database);

            // Try to connect
            try
            {
                // Get region ids at worldmap from database
                var results = sql.GetTableResult("SELECT wRegionID FROM _RefRegion WHERE wRegionID >= 0");

                // Disable all of them
                for (int i = 0; i < mapInfo.RegionData.Count; i++)
                    mapInfo.RegionData[i] = false;

                // Enable just the one found in database
                foreach (var result in results)
                {
                    short regionId = short.Parse(result[0]);
                    mapInfo.SetRegion(regionId, true);
                }

                // Save it
                File.Delete(filePath+".bak");
                File.Move(filePath, filePath + ".bak");
                mapInfo.Save(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.ReadKey();
            }
        }

        #region Private Helpers
        /// <summary>
        /// Creates a default settings if file doesn't exists
        /// </summary>
        private static Settings LoadOrCreateFile(string FilePath)
        {
            if (!File.Exists(FilePath))
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(new Settings(), Formatting.Indented));
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath));
        }
        #endregion
    }
}