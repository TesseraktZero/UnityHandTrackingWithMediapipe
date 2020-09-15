using System;
using UnityEngine;

namespace LandmarkInterface
{
    public class LandmarkOrientation : MonoBehaviour
    {
        public LandmarkResultSet landmarkSet;

        void Update()
        {
            if (!landmarkSet.Connected)
                return;
            var orientation = landmarkSet.DeviceOrientation;
            updateOrientation(orientation);
        }

        private void updateOrientation(Vector3 orientation)
        {
            float x = 0, y = 0, z = 0;

            z = (orientation.y - 90);
            if (orientation.z < 0)
                z *= -1;
            z %= 360;

            x = Math.Abs(orientation.z) - 90;

            this.transform.localEulerAngles = new Vector3(x, y, z);
        }

    }
}
