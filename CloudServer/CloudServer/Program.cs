using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Xml;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace CloudServer
{
    public struct File_Log
    {
        public string File_Path { get; set; }
        public DateTime Recent_Change { get; set; }
        public int File_Size { get; set; }
        public string Sha256 { get; set; }

        public override string ToString()
        {
            string temp = "";

            temp += "File_Path\t: " + File_Path + "\n";
            temp += "Recent_Change\t: " + Recent_Change + "\n";
            temp += "File_Size\t: " + File_Size + "\n";
            temp += "Sha256\t\t: " + Sha256;

            return temp;
        }
    }

    class Program
    {
        /// <summary>
        /// Stores the config information to be saved and read when needed in the program.
        /// </summary>
        static Dictionary<string, string> config_dictionary;

        /// <summary>
        /// Keeps track of what files are in the 
        /// </summary>
        static List<File_Log> file_catalog;

        /// <summary>
        /// The last time the local file catalog was changed.
        /// </summary>
        static DateTime file_catalog_time;

        /// <summary>
        /// The passphrase that the client and server file_catalogs need to match on or there will be no update/connection.
        /// </summary>
        static string file_catalog_passphrase;

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
            file_catalog = new List<File_Log>();

            // TESTING STUFF

            // TESTING SAVING

            File_Log test = new File_Log
            {
                File_Path = "TEST",
                Recent_Change = new DateTime(),
                File_Size = 100,
                Sha256 = "TESTINGSHA256"

            };
            File_Log test2 = new File_Log
            {
                File_Path = "TEST2",
                Recent_Change = new DateTime(),
                File_Size = 1002,
                Sha256 = "TESTINGSHA256"
            };

            file_catalog.Add(test);
            file_catalog.Add(test2);

            SaveCatalog();
            

            // END OF TESTING STUFF

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\config.ini"))
            {
                
                
                //FileInfo FI = new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\config.ini");
                //FI.Length;
            }

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\file_manifest.xml"))
            {
                LoadCatalog();
            }
            //If no arguments are passed, the default location of the files will be a new folder in the same path as the executable.
            if (args.Length == 0)
            {
                config_dictionary["location"] = "\\Files\\";

                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Files\\"))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Files\\");
                }
            }

            while (true)
            {
                string input = Console.ReadLine();

                ConsoleCommand(input);
            }
        }

        static void CreateCrashLog(string issue)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\crash_logs\\"))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\crash_logs\\");

            FileStream crashLog = File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\crash_logs\\" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString().Replace(':', '-').Replace(' ', '-') + ".txt");
            byte[] encoded_log = Encoding.UTF8.GetBytes(issue);
            crashLog.Write(encoded_log, 0, encoded_log.Length);
        }

        static void LoadCatalog()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "\\file_catalog.xml");

            file_catalog = new List<File_Log>();

            foreach (XmlNode node in doc.SelectNodes("/FileCatalog/File_Catalog"))
            {
                File_Log file = new File_Log
                {
                    File_Path = node.Attributes["File_Path"].Value,
                    Recent_Change = DateTime.Parse(node.Attributes["Recent_Change"].Value),
                    File_Size = int.Parse(node.Attributes["File_Size"].Value),
                    Sha256 = node.Attributes["Sha256"].Value
                };

                file_catalog.Add(file);
            }
        }

        static void SaveCatalog()
        {
            XmlWriter writer = XmlWriter.Create(AppDomain.CurrentDomain.BaseDirectory + "\\file_catalog.xml");
            XElement xml = new XElement("FileCatalog", file_catalog.Select(x => new XElement("File_Catalog", new XAttribute("File_Path", x.File_Path), new XAttribute("Recent_Change", x.Recent_Change), new XAttribute("File_Size", x.File_Size), new XAttribute("Sha256", x.Sha256))));

            xml.WriteTo(writer);
            writer.Flush();
            writer.Close();
        }

        static void LoadConfig()
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

        static void SaveConfig()
        {
            string text_to_save = "";
            foreach (KeyValuePair<string, string> p in config_dictionary)
            {
                text_to_save += p.Key + "=" + p.Value + "\n";
            }
            FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\config.ini", FileMode.Create);
            byte[] to_write = Encoding.UTF8.GetBytes(text_to_save);
            fs.Write(to_write, 0, to_write.Length);
        }

        static void ConsoleCommand(string input)
        {
            string[] inputs = input.Split(' ');
            switch (inputs[0].ToLower())
            {
                case "list":
                    if (inputs.Length < 2)
                    {
                        Console.WriteLine("Lists of Items\n>files");
                        break;
                    }
                        
                    switch (inputs[1].ToLower())
                    {
                        case "files":
                            WriteList(file_catalog);
                            break;
                        default:
                            Console.WriteLine("No Existing List Of Item : " + inputs[1]);
                            break;
                    }
                    break;
                default:
                    Console.WriteLine("Console Command Not Recognized.");
                    break;
            }
        }

        static void WriteList<T>(List<T> list)
        {
            bool first = true;

            Console.WriteLine("-----Writing-List-Contents-----");
            foreach (T x in list)
            {
                if (!first)
                    Console.Write('\n');
                Console.WriteLine(x.ToString());
                first = false;
            }
        }
    }
}
