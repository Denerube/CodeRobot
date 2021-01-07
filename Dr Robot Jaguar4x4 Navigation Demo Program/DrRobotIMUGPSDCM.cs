using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace DrRobot.IMUGPSNavigation
{
    public partial class DrRobotIMUGPSNavigationDemo : Form
    {

        /// </summary>
        /// <param name="dir">left motor direction, if left motor go forward, encoder value is increasing, dir = 1, otherwise, dir = -1</param>
        /// <param name="wheelCnt">wheel spin one circle, encoder count number</param>
        /// <param name="wheelR">wheel Radius</param>
        /// <param name="wheelDis"> distance between left/right wheel</param>
        /// <returns></returns>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
        public static extern int SetEncoderParameter(int dir, int wheelCnt, double wheelR, double wheelDis);    

         
        /// <summary>
        ///this function is for GPS sensor data update 
        /// </summary>
        /// <param name="cog">cog -- course over ground, radian, -pI ~ PI</param>
        /// <param name="vog">vog -- velocity over ground, unit: m/s</param>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
        public static extern void UpdateGPS(double cog, double vog);

        /// <summary>
        /// this function will estimate position based on latest encoder information
        /// </summary>
        /// <param name="leftEncoder">current left encoder reading</param>
        /// <param name="rightEncoder">current right encoder reading</param>
        /// <param name="estPos">pointer to current position estimation, estPos[0] -- E, estPos[1] -- N, estPos[2] -- Orientation(Yaw)</param>
        /// <returns></returns>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
        public static unsafe extern int EncoderEstimate(int leftEncoder, int rightEncoder, double* estPos);
        /// <summary>
        ///this function will use GPS latitude and longitude reading to update estimated position  
        /// </summary>
        /// <param name="latitude">unit: degree</param>
        /// <param name="longitude">unit : degree</param>
        /// <param name="estPos">pointer to current position estimation, estPos[0] -- E, estPos[1] -- N, estPos[2] -- Orientation(Yaw)</param>
        /// <returns></returns>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
	    public static unsafe extern int GPSUpdatePosition(double latitude, double longitude,double* estPos);

        
        /// <summary>
        /// these 5 functions are used for coordinate transform, this function must be called first
        /// GPS (latitude, longitude, altitude) <---> ECEF (XYZ) <----> Navigation ENU (E,N,U)
        /// we need first set reference latitude, longitude (ENU (0,0,0) point
        /// 
        /// </summary>
        /// <param name="latitude"> unit: degree</param>
        /// <param name="longitude"> unit: degree</param>
        /// <param name="altitude">0</param>
        /// <param name="xyzr">reference point in ECEF(XYZ)</param>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
	    public static unsafe extern void SetRefXYZ(double latitude, double longitude,double altitude,double* xyzr);

        
        /// <summary>
        ///this function is for LLH to XYZ 
        /// </summary>
        /// <param name="lat">unit: degree</param>
        /// <param name="lng">unit: degree</param>
        /// <param name="h">0</param>
        /// <param name="XYZ">position in ECEF</param>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
	    public static unsafe extern void LLH2XYZ(double lat,double lng,double h, double* XYZ);

        
        /// <summary>
        /// this function is for XYZ to ENU 
        /// </summary>
        /// <param name="XYZr">reference point XYZ</param>
        /// <param name="XYZ">point XYZ(ECEF)</param>
        /// <param name="ENU">point in ENU</param>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
        public static unsafe extern void XYZ2ENU(double* XYZr, double* XYZ, double* ENU);

        // 
        // 
        //
        // setENU[2] --U 0 forced as "0" in function
        /// <summary>
        /// this function is ENU 2 XYZ 
        /// setENU[0] - E    
        ///  setENU[1] -- N  
        ///  setENU[2] --U 0 forced as "0" 
        /// </summary>
        /// <param name="setENU"></param>
        /// <param name="calXYZ"></param>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
	    public static unsafe extern void ENU2XYZ(double* setENU, double* calXYZ);

        // 
        // 
        /// <summary>
        /// XYZ to LLH 
        /// latitude, longitude unit: degree, altitude = 0 
        /// </summary>
        /// <param name="XYZ"></param>
        /// <param name="calLLH"></param>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
	    public static unsafe extern void XYZ2LLH(double* XYZ, double* calLLH);

        
        /// <summary>
        /// this function will calculate the distance between two points 
        /// (latitude1, longitude1) to (latitude0, longitude0)
        /// </summary>
        /// <param name="latitude1"></param>
        /// <param name="longitude1"></param>
        /// <param name="latitude0"></param>
        /// <param name="longitude0"></param>
        /// <returns></returns>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
	    public static extern double DistanceLL(double latitude1, double longitude1, double latitude0, double longitude0);

         
        /// <summary>
        /// this function will calculate the heading direction from (latitude1, longitude1) to (latitude2, longitude2)
        /// the return value is in radian, -PI ~ PI, 0 is point to north, PI/2 is point to East  
        /// </summary>
        /// <param name="latitude1"></param>
        /// <param name="longitude1"></param>
        /// <param name="latitude2"></param>
        /// <param name="longitude2"></param>
        /// <returns></returns>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
	    public static extern double BearingToLL(double latitude1, double longitude1, double latitude2, double longitude2);
 
        /// <summary>
        /// this function will estimate driving direction, based on current estimated position and preset path 
        /// the preset path is defined by (startENU) ---> endENU
        /// the return value is robot driving direction 
        /// </summary>
        /// <param name="estPos"></param>
        /// <param name="distanceError"></param>
        /// <param name="startDir"></param>
        /// <param name="heading"></param>
        /// <param name="startENU"></param>
        /// <param name="endENU"></param>
        /// <returns></returns>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
	    public static unsafe extern double EstimateDrvDir(double* estPos, double distanceError,double startDir, double heading, double* startENU, double* endENU);

        /// <summary>
        /// this function will set driving parameters
        /// 
        /// </summary>
        /// <param name="targetTH">distance 2 target threshold</param>
        /// <param name="pathTH">distance 2 preset path threshold</param>
        /// <param name="angleTH">unit radian, set the minimum angle for robot adjustment</param>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
        public static extern void SetDrvParameter(double targetTH, double pathTH, double angleTH);

        // 
        /// <summary>
        /// this function will return distance between current XYZ and preset path(startXYZ--endXYZ) 
        /// </summary>
        /// <param name="curXYZ"></param>
        /// <param name="startXYZ"></param>
        /// <param name="endXYZ"></param>
        /// <returns></returns>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
        public static unsafe extern double Distance2Path(double* curXYZ, double* startXYZ, double* endXYZ);

        /// <summary>
        /// this function is setting GPS position estimate parameter
        /// position = encoder_position + kp *(gps_position - encoder_position)
        /// </summary>
        /// <param name="kp">now we set it as 0.5, if ground is very slip, you can trust GPS more, set as 0.8 or 1</param>
        /// <param name="diffTH"> if abs( distance(gps_position - pre_gps_position)- distance(encoder_position- pre_encoder_position) ) > diffTH, we did not use GPS estimation</param>
        [DllImport("DrRobotIMUGPSDCM.dll", SetLastError = true)]
        public static extern void SetGPSPositionUpdateParameter(double kp, double diffTH);



    }
}