using UnityEngine;

namespace LandmarkInterface.Socket
{
    [RequireComponent(typeof(LandmarkResultSet))]
    public class LandmarkSocketManager : MonoBehaviour
    {
        [SerializeField] private string serverIP = "127.0.0.1";
        [SerializeField] private int hostPort = 9500;
        [SerializeField] private int devicePort = 9500;
        [SerializeField] private string adbPath = @"D:\Unity\2019.2.19f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe";

        private LandmarkResultSet resultSet;
        private LandmarkSocket socket;

        private void Awake()
        {
            resultSet = this.GetComponent<LandmarkResultSet>();
            socket = new LandmarkSocket(serverIP, hostPort, devicePort, adbPath, resultSet);
        }

        private void OnEnable()
        {
            socket.Start();
        }

        private void OnDisable()
        {
            socket.Stop();
        }
    }

}