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
            RobotConfig.RobotConfigTableRow jaguarSetting = null;
            DrRobotComm drRobotComm1 = null;
            RobotConfig robotConfig = new RobotConfig();
            const string COMM1_ID = "COMM1";

            robotConfig = InitCofig();
            jaguarSetting = (RobotConfig.RobotConfigTableRow)robotConfig.RobotConfigTable.Rows[0];

            double disableGPSDis = 1.5;      // Rover vel = 1.5m/s, so set 1.5m to target point to start disable gps to make sure turnning is right
            var MOTDIR = 1;
            var WHEEL_CNT = jaguarSetting.Wheel_CNT;
            var WHEEL_R = jaguarSetting.Wheel_R;
            var WHEELDIS = jaguarSetting.Wheel_DIS;
            var WHEEL_P = Math.PI * WHEEL_R * 2;
            drRobotComm1 = new DrRobotComm(COMM1_ID);
            bool res = drRobotComm1.StartClient(jaguarSetting.RobotIP, jaguarSetting.Port1);     //now use tcp first
            Console.WriteLine(res);                                                                               //   bool res = drRobotComm1.SerialOpen(1, 115200);     //now use tcp first


            /*if ((jaguarSetting.RobotType.ToLower() == "jaguar") || (jaguarSetting.RobotType.ToLower() == "jaguar_4arms"))
            {

            }
            else if (jaguarSetting.RobotType.ToLower() == "jaguar_lite")
            {

                disableGPSDis = 2.0;

            }
            else if (jaguarSetting.RobotType.ToLower() == "jaguar_4x4wheel")
            {

                disableGPSDis = 2.0;
            }
            else if (jaguarSetting.RobotType.ToLower() == "jaguar_4x4track")
            {
            }

            // set Encoder parameter
            DrRobotIMUGPSNavigationDemo.SetEncoderParameter(MOTDIR, WHEEL_CNT, WHEEL_R, WHEELDIS);
            // set driving parramter
            DrRobotIMUGPSNavigationDemo.SetDrvParameter(dis2TargetTH, dis2PathTH, drvAngleTH);
            // set some parameters
            DrRobotIMUGPSNavigationDemo.SetGPSPositionUpdateParameter(0.5, 2.0);//2.0*/

        }

        public static RobotConfig InitCofig()
        {
            const string configFile = "c:\\DrRobotAppFile\\OutDoorRobotConfig_4x4.xml";
            RobotConfig robotConfig = new RobotConfig();
            RobotConfig.RobotConfigTableRow row = null;


            robotConfig.ReadXml(configFile);
            row = (RobotConfig.RobotConfigTableRow)robotConfig.RobotConfigTable.Rows[0];


            row.RobotID = "DrRobot";
            row.RobotIP = "192.168.0.60";
            row.Port1 = 10001;
            row.Port2 = 10002;
            row.GPSIP = "192.168.0.230";
            row.GPSPort = 10001;
            row.CameraIP = "192.168.0.65";
            row.CameraPort = 8081;
            row.CameraPWD = "drrobot";
            row.CameraUser = "root";
            try
            {
                robotConfig.WriteXml(configFile);
            }
            catch
            {
            }
            return robotConfig;
        }
    }
}
