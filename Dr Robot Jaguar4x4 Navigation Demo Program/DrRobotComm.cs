using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
namespace DrRobot.IMUGPSNavigation
{
    class DrRobotComm
    {

        private DrRobotConnectionMethod commMethod = DrRobotConnectionMethod.WiFiComm;                 //1--for wifi 2 for serial

        private StreamReader readerReply = null;
        private StreamWriter writer = null;
        private NetworkStream replyStream = null;


        private static byte[] recBuffer;

        private Thread receiveThread = null;

        SerialPort serialPort = new System.IO.Ports.SerialPort();


        private static Socket clientSocket;
        private static int localPort = 0;

        private static string lastRecvErrorMsg = String.Empty;
        private static int lastRecvError = 0;
        private static string lastSendErrorMsg = String.Empty;
        private static int socketErrorCount = 0;



        private string processStr = "";
        private string recStr = "";

        private DrRobotIMUGPSNavigationDemo _form;
        private string comID = "MOT1";

        public DrRobotComm(string id)
        {
            comID = id;
        }

        //here is the WiFi connecting start
        internal bool StartClient(string addr, int portNum)
        {
            // Connect to remote server
            commMethod = DrRobotConnectionMethod.WiFiComm;
            try
            {
                // Establish the remote endpoint for the socket
                // GetHostEntry was an attempt to do a hostname lookup
                // so that you did not have to type an IP address.
                // However, it takes a LONG time for an IP address
                // (around 20 seconds).
                //                IPHostEntry ipHostInfo = Dns.GetHostEntry(addr);
                //                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress ipAddress;
                int remotePort = portNum;
                try
                {
                    ipAddress = IPAddress.Parse(addr);
                }
                catch (Exception e)
                {
                    lastSendErrorMsg = e.Message;
                    return false;
                }

                IPEndPoint remoteEP = new IPEndPoint(ipAddress, remotePort);

                // Create a TCP socket

                clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                //udp
                //clientSocket = new Socket(AddressFamily.InterNetwork,
                //   SocketType.Dgram, ProtocolType.Udp);
                //need send data to active the communication

                // Connect to the remote endpoint
                // We will wait for this to complete rather than do it
                // asynchronously

                receiveThread = new Thread((dataReceive));
                receiveThread.CurrentCulture = new CultureInfo("en-US");

                clientSocket.Connect(remoteEP);

                localPort = ((IPEndPoint)clientSocket.LocalEndPoint).Port;


                //for communication need to send a package first
                //Ping();



                receiveThread.Start();

                return true;

            }
            catch (SocketException e)
            {
                socketErrorCount++;
                lastRecvError = e.ErrorCode;
                lastRecvErrorMsg = e.ToString();
                return false;
            }
        }

        /// <summary>
        /// Open a serial port if in config we choose serial communication.
        /// </summary>
        /// <param name="comPort"></param>
        /// <param name="baudRate"></param>
        /// <returns>A Ccr Port for receiving serial port data</returns>
        internal bool SerialOpen(int comPort, int baudRate)
        {
            commMethod = DrRobotConnectionMethod.SerialComm;
            if (recBuffer == null)
                recBuffer = new byte[4095];

            if (baudRate < 1200)
                baudRate = 115200;

            serialPort.Close();

            string portName = "COM" + comPort.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            if (serialPort == null)
            {
                serialPort = new SerialPort(portName, baudRate);
            }
            else
            {
                serialPort.PortName = portName;
                serialPort.BaudRate = baudRate;
            }
            serialPort.Encoding = Encoding.Default;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;      // Handshake.RequestToSend;     //hardware flow control
            //   serialPort.WriteTimeout = 2000;
            serialPort.NewLine = "\r";
            serialPort.ReadTimeout = 0;
            serialPort.ReceivedBytesThreshold = 1;             //the shortest package is ping package
            serialPort.ReadBufferSize = 4096;


            //serialPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            //serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(portError);
            try
            {
                serialPort.Open();

                //start a thread to receive
                //       if (receiveThread == null)
                {
                    receiveThread = new Thread((dataReceive));
                    receiveThread.CurrentCulture = new CultureInfo("en-US");
                    receiveThread.Start();
                }

            }
            catch
            {

                return false;
            }
            return true;
        }

        /// <summary>
        /// to decode receive package here
        /// </summary>
        private void dataReceive()
        {
            if (commMethod == DrRobotConnectionMethod.WiFiComm)
            {
                if (readerReply != null)
                    readerReply.Close();
                if (replyStream != null)
                    readerReply.Close();
                replyStream = new NetworkStream(clientSocket);

                readerReply = new StreamReader(replyStream);

            }

            //receive data here
            bool StayConnected = true;

            byte[] recs = new byte[4095];
            int scount = 0;

            while (StayConnected)       //keep running
            {
                try
                {
                    // *serial
                    if (commMethod == DrRobotConnectionMethod.SerialComm)
                    {
                        //recStr = serialPort.ReadExisting();
                        if (serialPort.BytesToRead > 0)
                        {
                            recStr = serialPort.ReadLine();
                            processData();
                        }
                        else
                        {
                            Thread.Sleep(20);
                        }
                    }
                    else if (commMethod == DrRobotConnectionMethod.WiFiComm)
                    {
                        // wifi
                        if (!replyStream.DataAvailable)
                        {
                            Thread.Sleep(5);
                        }
                        else
                        {
                            recStr = readerReply.ReadLine();
                            processData();
                        }
                    }
                }
                catch (Exception ed)
                {
                    //need to handle some error here

                }

            }
        }


        void processData()
        {/*
            if (recStr.Length == 1)
            {
                //here is the ack message, "+", "-"
                if (recStr == "+")
                    _form.UpdateACkMsg(comID + "VALID");
                else if (recStr == "-")
                    _form.UpdateACkMsg(comID + "INVALID");
            }
            */

        }


        /// <summary>
        /// DO NOT USE THIS COMMAND DIRECTLY.
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        internal bool SendCommand(string Cmd)
        {

            /// <summary>
            Cmd += "\r\n";
            byte[] cmdData = ASCIIEncoding.UTF8.GetBytes(Cmd);

            if (commMethod == DrRobotConnectionMethod.SerialComm)
            {
                if (cmdData != null && serialPort != null && serialPort.IsOpen)
                {
                    int packetLength = cmdData.Length;// (2 + (cmdData.Length));
                    try
                    {
                        serialPort.Write(cmdData, 0, packetLength);
                    }
                    catch
                    {
                        return false;
                    }
                    return true;
                }
                else
                    return false;
            }

            else if (commMethod == DrRobotConnectionMethod.WiFiComm)
            {
                if ((cmdData != null) && (clientSocket != null))  //&& (clientSocket.Connected)
                {
                    try
                    {
                        if (clientSocket.Send(cmdData) == cmdData.Length)
                        {

                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }


                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Close the connection to a serial port.
        /// </summary>
        public void Close()
        {
            try
            {
                if (readerReply != null)
                    readerReply.Close();
                if (writer != null)
                    writer.Close();
            }
            catch
            {
            }


            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
            if ((clientSocket != null))
            {
                //if (clientSocket.Connected)
                clientSocket.Close();
            }
            if (receiveThread != null)
            {
                if (receiveThread.IsAlive)
                    receiveThread.Abort();      //terminate the receive thread
            }
        }

        /// <summary>
        /// True when the serial port connection is open.
        /// </summary>
        public bool IsOpen
        {
            get { return serialPort != null && serialPort.IsOpen; }
        }


    }

    public enum DrRobotConnectionMethod
    {
        SerialComm = 0x01,
        WiFiComm = 0x02,
    }


}
