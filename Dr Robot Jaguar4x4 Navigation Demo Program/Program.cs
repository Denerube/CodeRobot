using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
namespace DrRobot.IMUGPSNavigation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 


        [STAThread]
        static void Main()
        {
            Console.WriteLine(DrRobotIMUGPSNavigationDemo.SetEncoderParameter(1, 1, 1, 1));
            
        }
    }
}
