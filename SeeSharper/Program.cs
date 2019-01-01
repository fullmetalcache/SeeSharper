/*
 * File: Program.cs
 * Author: Brian Fehrman (fullmetalcache)
 * Date: 2018-12-27
 * Description:
 *      This is the main entroy point for the SeeSharper program. 
 */

using CommandLine;
using System.Collections.Generic;

namespace SeeSharper
{
    class Program
    {
        enum FileType { Nessus, HostFile };

        static void Main(string[] args)
        {
            int threads = 1;
            int timeout = 30;
            string fileName = "";
            FileType fileType = FileType.HostFile;
            bool appendPorts = false;
            bool prependHttps = false;
            List<string> hostList = null;
             
            //Parse command line arguments
            ParserResult<Options> argResults = Parser.Default.ParseArguments<Options>(args);
            ParserResult<Options> res = argResults.WithParsed<Options>(o =>
                {
                    appendPorts = o.AppendPorts;
                    prependHttps = o.PrependHTTPS;
                    threads = o.NumThreads;
                    timeout = o.Timeout;

                    if( o.FileName == "" )
                    {
                        System.Console.WriteLine("Please provide the name of a host file or a Nessus file");
                        System.Environment.Exit(1);
                    }

                    fileName = o.FileName;

                    //Automatically determine file type
                    try
                    {
                        System.IO.StreamReader file = new System.IO.StreamReader(fileName);
                        string line = file.ReadLine();

                        if (line != null)
                        {
                            if (line.Contains("xml version"))
                            {
                                fileType = FileType.Nessus;
                            }
                            else
                            {
                                fileType = FileType.HostFile;
                            }
                        }

                        file.Close();
                    }
                    catch
                    {
                        System.Console.WriteLine(string.Format("Could not open file: {0}", fileName));
                        System.Environment.Exit(1);
                    }

                });

            //This takes care of if the --help option was used
            if( argResults.Tag == CommandLine.ParserResultType.NotParsed )
            {
                System.Environment.Exit(1);
            }

            //Get hosts
            if(fileType == FileType.HostFile)
            {
                FileParser fParser = new FileParser();
                hostList = fParser.Parse(fileName, prependHttps, appendPorts);
            }
            else if(fileType == FileType.Nessus)
            {
                NessusParser nParser = new NessusParser();
                hostList = nParser.Parse(fileName);
            }

            if (hostList == null)
            {
                System.Environment.Exit(1);
            }

            foreach (string host in hostList)
            {
                System.Console.WriteLine(host);
            }

            //Create a new WebShot object for screenshotting.
            WebShot webshot = new WebShot(threads, timeout);


            //Some examples of screenshotting for now...
            //webshot.ScreenShot("https://google.com");
            //webshot.ScreenShot("https://www.blackhillsinfosec.com");
            //webshot.ScreenShot("http://www.reddit.com");


        }
    }

    //Class for command line arguments
    public class Options
    {
        [Option("appendports",
            Default = false,
            HelpText = "Appends ports specified in PortList.txt to each host that is provided; handles if port is already present")]
        public bool AppendPorts { get; set; }

        [Option('f', "file",
            Default = "",
            HelpText = "Path/Filename of hosts to screenshot")]
        public string FileName { get; set; }

        [Option("prependhttps",
            Default = false,
            HelpText = "Prepends HTTP and HTTPS to all addresses; handles if either prefix is already present")]
        public bool PrependHTTPS { get; set; }

        [Option("threads",
            Default = 1,
            HelpText = "Specify number of threads to use")]
        public int NumThreads { get; set; }

        [Option("timeout",
            Default = 30,
            HelpText = "Specify the timeout for web requests, in seconds")]
        public int Timeout { get; set; }
    }
}
