using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace CloudServer
{
    class Program
    {
        static Dictionary<string, string> config_dictionary;

        /// <summary>
        /// Tries to get a config file, if that does not exist, then it will create one with the parameters given.
        /// If no parameters are given, it will create one itself with default information.
        /// </summary>
        /// <param name="args">
        /// The first value is the location of the files.
        /// The second value is the port that it will be using.
        /// </param>
        static void Main(string[] args)
        {
            config_dictionary = new Dictionary<string, string>();

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\config.ini"))
            {
                string[] config = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\config.ini");
                foreach (string s in config)
                {
                    config_dictionary[s.Split('=')[0]] = s.Split('=')[1];
                }
                if (!config_dictionary.ContainsKey("location"))
                {
                    CreateCrashLog("Config did not contain a location variable!");
                }

            }
            //If no arguments are passed, the default location of the files will be a new folder in the same path as the executable.
            if (args.Length == 0)
            {
                
            }
        }

        static void CreateCrashLog(string issue)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\crash_logs\\"))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\crash_logs\\");

            FileStream crashLog = File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\crash_logs\\" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString().Replace(':', '-') + ".txt");
            byte[] encoded_log = Encoding.UTF8.GetBytes(issue);
            crashLog.Write(encoded_log, 0, encoded_log.Length);
        }
    }
}
