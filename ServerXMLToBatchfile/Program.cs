using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ServerXMLToBatchfile
{

    public static class ArrayExtensions
    {

        public static String Combine<T>(this IEnumerable<T> array, String afterInnerText = " ")
        {
            String str = "";

            foreach (T obj in array)
            {
                str += obj.ToString() + afterInnerText;
            }

            return str;
        }

    }

    class Program
    {

        public static KeyValuePair<string, string>[] GAME_MOD = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("Counter-Strike: Global Offensive", "csgo"),
            new KeyValuePair<string, string>("Counter-Strike: Source", "css"),
        };

        private static String GetModByGameName(String gname)
        {
            foreach (KeyValuePair<string, string> kvp in GAME_MOD)
            {
                if (gname == kvp.Key)
                {
                    return kvp.Value;
                }
            }
            return String.Empty;
        }

        static MemoryStream ConvertUTF8ToStream(string xml)
        {
            // Encode the XML string in a UTF-8 byte array
            byte[] encodedString = Encoding.UTF8.GetBytes(xml);

            // Put the byte array into a stream and rewind it to the beginning
            MemoryStream ms = new MemoryStream(encodedString);
            ms.Flush();
            ms.Position = 0;

            return ms;
        }

        static XmlDocument LoadXML(string path)
        {
            StreamReader reader = new StreamReader(path);
            XmlDocument doc = new XmlDocument();

            doc.Load(ConvertUTF8ToStream(reader.ReadToEnd()));
            reader.Close();
            return doc;
        }

        //Parameters & "-game " & GameMod & " -port " & UDPPort & " +hostname " & Chr(34) & ServerName & Chr(34) & " +map " & ServerMap & " +maxplayers " & MaxPlayers & " +sv_lan " & NetworkComboBox.SelectedIndex & " " & AdditionalCommands

        static void Main(string[] args)
        {
            if (args.Count() < 1)
            {
                Console.WriteLine("You must specify a file.");
                Console.Read();
                return;
            }

            String srcdsparams = "";

            XmlDocument doc = LoadXML(args.Combine());

            XmlNode config = doc.SelectSingleNode("/Config/Server-Config");
            String sPath = doc.SelectSingleNode("/Config/Srcds-Config/Path").InnerText;

            StreamWriter writer = new StreamWriter(sPath + "/start.bat");
#if DEBUG
            Console.WriteLine("HostName: "+config["HostName"].InnerText);
#endif
            srcdsparams += "-console -game " + GetModByGameName(config["Mod"].InnerText) + " -port " + config["Port"].InnerText + " +hostname \"" + config["HostName"].InnerText + "\" +map \"" + config["Map"].InnerText + "\" +rcon_password " + config["RCON"].InnerText + " +maxplayers " + config["Players"].InnerText + " +sv_lan " + config["Network"].InnerText;

            try
            {
                srcdsparams += " " + config["AdditionalCommands"].InnerText;
            }
            catch { }

#if DEBUG
            Console.WriteLine("Server Paramaters: "+srcdsparams);
#endif
            writer.WriteLine("@ECHO OFF");
            writer.WriteLine("srcds.exe "+srcdsparams);
            writer.WriteLine("pause");

            writer.Flush();
            writer.Close();
            Console.WriteLine("\"start.bat\" is done being created!");
            Console.ReadKey();
        }
    }
}
