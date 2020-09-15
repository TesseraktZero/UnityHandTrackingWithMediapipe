using Mediapipe;
using SharpAdbClient;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace LandmarkInterface.Socket
{
    //socket for receiving data from device, refresh rate may not be same as Monobehaviour
    public class LandmarkSocket
    {
        private string serverIP;
        private int hostPort;
        private int devicePort;
        private string adbPath;
        private LandmarkResultSet resultSet;

        private Thread landmarkThread;
        private TcpListener tcpServer;
        private DeviceData device;
        private AdbClient adbClient;
        private bool connected;
        private int maxRetryCount = 10;

        public LandmarkSocket(string serverIP, int hostPort, int devicePort, string adbPath, LandmarkResultSet resultSet)
        {
            this.serverIP   = serverIP;
            this.hostPort   = hostPort;
            this.devicePort = devicePort;
            this.adbPath    = adbPath;
            this.resultSet  = resultSet;
        }

        public void Start()
        {
            if (landmarkThread != null && landmarkThread.IsAlive)
            {
                Debug.LogError("Previous thread is still alive");
            }
            landmarkThread = new Thread(new ThreadStart(() => run()));
            landmarkThread.IsBackground = true;
            landmarkThread.Start();
        }

        public void Stop()
        {
            connected = false;
            resultSet.Connected = false;
        }

        private void run()
        {
            try
            {
                using (var client = tryStartConnection())
                {
                    if(client != null)
                    {
                        Debug.Log("Connection Success");
                        fetchResults(client);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Connection Failed");
                Debug.LogError(e);
            }
            finally
            {
                Stop();
                closeApp();
                if (tcpServer != null)
                {
                    tcpServer.Stop();
                    Debug.Log("server terminated");
                }

                if (landmarkThread != null)
                {
                    landmarkThread.Abort();
                    Debug.Log("socket thread stopped");
                }
            }
        }

        private TcpClient tryStartConnection()
        {
            for (int retryCount = 0; retryCount < maxRetryCount; retryCount++)
            {
                try
                {
                    var tcpClient = startConnection();
                    tcpClient.ReceiveTimeout = 1000 * 60 * 30;
                    connected = true;
                    resultSet.Connected = true;
                    return tcpClient;
                }
                catch (SocketException e)
                {
                    Debug.LogError(e);
                    this.hostPort++;
                    Debug.Log("Error connecting to device. Retrying with new port: " + hostPort);
                }
            }
            return null;
        }

        private TcpClient startConnection()
        {
            Debug.Log("starting socket");
            tcpServer = new TcpListener(IPAddress.Parse(serverIP), hostPort);
            Debug.Log("waiting for device connection");
            tcpServer.Start();
            var acceptClientTask = tcpServer.AcceptTcpClientAsync();

            Debug.Log("starting android connection");
            //https://github.com/quamotion/madb
            var adbServer = new AdbServer();
            adbServer.StartServer(adbPath, true);
            adbClient = new AdbClient();
            var devices = adbClient.GetDevices();
            if (devices.Count == 0)
            {
                Debug.LogError("Error: Device not connected");
                return null;
            }
            device = devices.Last();

            string command1 = @"am force-stop com.example.mediapipemultihandstrackingapp";
            var receiver1 = new ConsoleOutputReceiver();
            adbClient.ExecuteRemoteCommand(command1, device, receiver1);
            Debug.Log("Stop remote app result: " + receiver1.ToString());

            Debug.Log("Resetting reverse connection");
            adbClient.RemoveAllReverseForwards(device);
            var reverseResult = adbClient.CreateReverseForward(
                device, "tcp:" + devicePort.ToString(), "tcp:" + hostPort.ToString(), true);

            string command2 = @"monkey -p com.example.mediapipemultihandstrackingapp -c android.intent.category.LAUNCHER 1";
            var receiver2 = new ConsoleOutputReceiver();
            adbClient.ExecuteRemoteCommand(command2, device, receiver2);
            Debug.Log("Start remote app result: " + receiver2.ToString());

            return acceptClientTask.Result;            
        }

        private void fetchResults(TcpClient client)
        {
            using (var stream = client.GetStream())
            {
                while (connected)
                {
                    var landmarkType = (LandmarkType)stream.ReadByte();

                    if (landmarkType == LandmarkType.Orientation)
                    {
                        var buffer = new byte[4];
                        stream.Read(buffer, 0, 4);
                        float x = BitConverter.ToSingle(buffer, 0) * 180 / (float) Math.PI;
                        stream.Read(buffer, 0, 4);
                        float y = BitConverter.ToSingle(buffer, 0) * 180 / (float) Math.PI;
                        stream.Read(buffer, 0, 4);
                        float z = BitConverter.ToSingle(buffer, 0) * 180 / (float) Math.PI;
                        //Debug.Log($"Device Orientation: ({x}, {y}, {z})");
                        resultSet.DeviceOrientation = new Vector3(x, y, z);

                    }
                    else
                    {
                        var landmarkList = NormalizedLandmarkList.Parser.ParseDelimitedFrom(stream);
                        resultSet.UpdateLandmark(landmarkType, landmarkList);
                    }
                }
            }
        }

        private void closeApp()
        {
            if (device == null)
                return;
            string command1 = @"am force-stop com.example.mediapipemultihandstrackingapp";
            var receiver1 = new ConsoleOutputReceiver();
            adbClient.ExecuteRemoteCommand(command1, device, receiver1);
            Debug.Log("Stop remote app result: " + receiver1.ToString());
        }



    }
}