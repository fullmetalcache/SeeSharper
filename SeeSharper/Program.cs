/*
 * File: Program.cs
 * Author: Brian Fehrman (fullmetalcache)
 * Date: 2018-12-27
 * Description:
 *      This is the main entroy point for the SeeSharper program. 
 * 
 */

namespace SeeSharper
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a new WebShot object for screenshotting.
            WebShot webshot = new WebShot(10);

            //Some examples of screenshotting for now...
            //webshot.ScreenShot("https://google.com");
            //webshot.ScreenShot("https://www.blackhillsinfosec.com");
            //webshot.ScreenShot("http://www.reddit.com");
            NessusParser nParser = new NessusParser();


        }
    }
}
