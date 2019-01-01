/*
 * File: FileParser.cs
 * Author: Brian Fehrman (fullmetalcache)
 * Date: 2018-12-31
 * Description:
 *      This class can be used to parse text files that contain a host on each line
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace SeeSharper
{
    class FileParser
    {
        private List<string> _ports;

        public FileParser()
        {
            //Create ports list
            try
            {
                StreamReader portsFile = new StreamReader("PortList.txt");
                _ports = new List<string>();
                string currPort = "";

                while ((currPort = portsFile.ReadLine()) != null)
                {
                    currPort = currPort.Trim();

                    if (!_ports.Contains(currPort))
                    {
                        _ports.Add(currPort);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Warning: PortList.txt file could not be found in this directory");
            }
        }

        /// <summary>
        /// Parses a text file containing a host on each line. Can do additionaly processing,
        /// such as prepending HTTP:// and HTTPS:// to each host and/or appending common ports
        /// to the end of each host
        /// </summary>
        /// <param name="fileName">Absolute or Relative Path of Host File to Parse</param>
        /// <returns>List of Hosts to Screenshot</returns>
        public List<string> Parse(string fileName, bool prependHTTPS, bool appendPorts)
        {
            List<string> hostList = new List<string>();
            StreamReader file;
            string line;

            //Load the file
            try
            {
                file = new StreamReader(fileName);
            }
            catch
            {
                Console.WriteLine(String.Format("Error Opening Host File: {0}", fileName));
                return null;
            }

            //Read in each host
            while ((line = file.ReadLine()) != null)
            {
                List<String> currHosts = new List<String>();

                //Trim whitespace
                line = line.Trim();

                //Handle option to prepend HTTP and HTTPS
                if (prependHTTPS)
                {
                    //Remove current prefix, if present, to help ensure an invalid
                    //prefix doesn't result from this processing
                    line = line.Replace("http://", "");
                    line = line.Replace("https://", "");

                    //Add http:// and https:// formats of host to temporary host list
                    currHosts.Add(String.Format("http://{0}", line));
                    currHosts.Add(String.Format("https://{0}", line));
                }
                else
                {
                    //If prependHTTPS option is not used, add host as-is to temporary host list
                    currHosts.Add(line);
                }

                if (appendPorts)
                {
                    //Create a temporary copy of the original hosts
                    List<String> origHosts = new List<String>(currHosts);
                    foreach (string currHost in origHosts)
                    {
                        string host = currHost;
                        string prefix = "";

                        //Remove and store prefix, if present. This makes it easier to
                        //remove any port suffixes that are present
                        if(host.Contains("://"))
                        {
                            prefix = host.Split("://".ToCharArray())[0] + "://";
                            host = host.Split(new string[] { "://" }, StringSplitOptions.None)[1];
                        }

                        //Remove port suffix, if present
                        host = host.Split(':')[0];

                        //Reconstruct host
                        host = String.Format("{0}{1}", prefix, host);

                        //Construct host:port formatted hosts and add them to the current host list
                        foreach (string port in _ports)
                        {
                            string currHostPort = String.Format("{0}:{1}", host, port);
                            currHosts.Add(currHostPort);
                        }
                    }
                }

                //Add the current host list to the overall host list while avoiding duplicates
                foreach( string host in currHosts)
                {
                    if(!hostList.Contains(host))
                    {
                        hostList.Add(host);
                    }
                }
            }

            return hostList;
        }
    }
}
