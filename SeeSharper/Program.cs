using System;
using System.Threading;


namespace SeeSharper
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            WebShot webshot = new WebShot();

            webshot.ScreenShot(args[0]);
        }
    }
}
