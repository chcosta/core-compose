﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UpdateNuGetConfig
{
    class Program
    {
        static void Main(string[] args)
        {
            string nuGetConfigFilename = args[0];
            string[] feeds = new string[args.Length - 1];
            Array.Copy(args, 1, feeds, 0, args.Length - 1);

            var feedDictionary = new Dictionary<string, string>();
            for(int i = 0; i < feeds.Length; i++)
            {
                feedDictionary.Add("autogenerated_privatefeed_" + i, feeds[i]);
            }
            ReadNuGetConfigFile(nuGetConfigFilename, feedDictionary);
            WriteNuGetConfigFile(nuGetConfigFilename, feedDictionary);

        }

        static void WriteNuGetConfigFile(string nuGetConfigFilename, Dictionary<string,string> feedDictionary)
        {
            using (FileStream stream = File.OpenWrite(nuGetConfigFilename))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.WriteLine("<configuration>");
                writer.WriteLine("  <packageSources>");
                writer.WriteLine("    <!--To inherit the global NuGet package sources remove the <clear/> line below -->");
                writer.WriteLine("    <clear />");
                foreach(var feed in feedDictionary)
                {
                    writer.WriteLine("    <add key=\"{0}\" value=\"{1}\" />", feed.Key, feed.Value);
                }
                writer.WriteLine("  </packageSources>");
                writer.WriteLine("</configuration>");
            }
        }

        static Dictionary<string, string> ReadNuGetConfigFile(string nuGetConfigFilename, Dictionary<string,string> feedDictionary)
        {
            Regex feedRegex = new Regex("add key=\"(?<key>[^\"]+)\" value=\"(?<value>[^\"]+)\"");

            using (FileStream stream = File.OpenRead(nuGetConfigFilename))
            using (StreamReader reader = new StreamReader(stream))
            {
                while(!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    Match m = feedRegex.Match(line);
                    if(m.Success)
                    {
                        string key = m.Groups["key"].Value;
                        string value = m.Groups["value"].Value;
                        if (!feedDictionary.ContainsValue(value))
                        {
                            feedDictionary.Add(key, value);
                        }
                    }
                }
            }
            return feedDictionary;
        }
    }
}
