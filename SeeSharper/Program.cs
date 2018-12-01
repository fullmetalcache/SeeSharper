using System;
using System.Threading;
using System.Windows.Forms;

namespace SeeSharper
{
    class Program
    {
        static void Main(string[] args)
        {
            WebShot webshot = new WebShot();
            webshot.ScreenShot(args[0]);
        }
    }
}
