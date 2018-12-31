/*
 * File: Program.cs
 * Author: Brian Fehrman (fullmetalcache)
 * Date: 2018-12-27
 * Description:
 *      This is the main entroy point for the SeeSharper program. 
 * 
 */

using CommandLine;

namespace SeeSharper
{
    class Program
    {
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

            [Option('x', "nessus",
                Default = "",
                HelpText = "Path/Filename of Nessus XML .nessus file to screenshot")]
            public string Nessus { get; set; }
        }

        static void Main(string[] args)
        {
            int threads = 1;
            int timeout = 30;
            string fileName = "";
            string fileType = "";
            bool appendPorts = false;
            bool prependHttps = false;

            //Parse command line arguments
            ParserResult<Options> argResults = Parser.Default.ParseArguments<Options>(args);
            argResults.WithParsed<Options>(o =>
                {
                    appendPorts = o.AppendPorts;
                    prependHttps = o.PrependHTTPS;
                    threads = o.NumThreads;
                    timeout = o.Timeout;

                    if( o.FileName == "" && o.Nessus == "")
                    {
                        System.Console.WriteLine("Please provide the name of a host file or a Nessus file");
                        return;
                    }

                    if (o.FileName != "")
                    {
                        fileName = o.FileName;
                        fileType = "hostFile";
                    }
                    else if(o.Nessus != "")
                    {
                        fileName = o.FileName;
                        fileType = "nessusFile";
                    }

                });

            //Create a new WebShot object for screenshotting.
            WebShot webshot = new WebShot(threads, timeout);

            //Some examples of screenshotting for now...
            //webshot.ScreenShot("https://google.com");
            //webshot.ScreenShot("https://www.blackhillsinfosec.com");
            //webshot.ScreenShot("http://www.reddit.com");
            NessusParser nParser = new NessusParser();


        }
    }
}
