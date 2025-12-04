using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BrickBreaker.Storage
{
    public sealed class StorageConfiguration
    {
        private readonly IConfiguration _config;
        private readonly string _root;

        public StorageConfiguration() //sets the directory for the json file that keeps connectionstring to db
        {
            _root = LocateSolutionRoot() ?? Directory.GetCurrentDirectory(); //defining _root
            var builder = new ConfigurationBuilder()
                .SetBasePath(_root)
                .AddJsonFile(
                    Path.Combine("BrickBreaker.Storage", "Properties", "appsettings.json"), //making a path to the json file
                    optional: true,
                    reloadOnChange: true);

            try
            {
                _config = builder.Build(); //if it finds the json file it builds the config
            }
            catch
            {
                // Fall back to an empty configuration so the app can still run without the file.
                _config = new ConfigurationBuilder().Build();
            }
        }
        //method that reads the connecction string and turns it into var ConnectionString
        public string? GetConnectionString()
        {
            var candidate = ReadFromEnvironment();
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }

            candidate = _config.GetConnectionString("Supabase")
                       ?? _config["Supabase"]
                       ?? _config["ConnectionString:Supabase"]
                       ?? _config["SupabaseConnection"];

            return string.IsNullOrWhiteSpace(candidate) ? null : candidate;
        }

        public static string? ReadFromEnvironment()
        {
            return Environment.GetEnvironmentVariable("SUPABASE_CONNECTION")
                   ?? Environment.GetEnvironmentVariable("SUPABASE_URL")
                   ?? Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING");
        }

        private static string? LocateSolutionRoot() //tries to find the root of the project by looking for .sln file and whoch folder it resides in
        {
            var dir = AppContext.BaseDirectory;
            while (!string.IsNullOrWhiteSpace(dir))
            {
                if (File.Exists(Path.Combine(dir, "BrickBreaker.sln")))
                {
                    return dir; //if it finds the file it returns the corrects directory (path)
                }

                dir = Directory.GetParent(dir)?.FullName;
            }

            return null;
        }
    }
}
