using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LandmarkInterface
{
    public class HandLandmark : MonoBehaviour
    {
        public LandmarkResultSet landmarkSet;
        public List<GameObject> LandmarkObjects;
        public LandmarkType LandmarkType;
        public bool FlipXY = false;
        public bool NegateX = false;
        public bool NegateY = false;

        [SerializeField]
        private float thumbModelLength = 0.03f;
        private float scale;

        private DepthCalibrator depthCalibrator = new DepthCalibrator(-0.0719f, 0.439f);
        private TransformLink[] transformLinkers;

        private void Awake()
        {
            transformLinkers = this.transform.GetComponentsInChildren<TransformLink>();
        }

        private void Update()
        {
            var list = landmarkSet.GetLandmarks(this.LandmarkType);
            if (list != null && list.Count != 0)
            {
                updateLandmarkPosition(list);
                updateLandmarkScale(list);
            }

            updateWristRotation();
            foreach (var linker in transformLinkers)
            {
                linker.UpdateTransform();
            }

        }

        private void OnDrawGizmos()
        {
            //for debugging Positions
            Gizmos.color = Color.red;
            for (int i = 0; i < LandmarkObjects.Count; i++)
            {
                Gizmos.DrawSphere(LandmarkObjects[i].transform.position, 0.005f);
            }
        }

        //private void setupLandmarks(int count)
        //{
        //    for (int i = 0; i < count; i++)
        //    {
        //        var point = new GameObject("Point " + i);
        //        point.transform.SetParent(this.transform);
        //        LandmarkObjects.Add(point);
        //    }
        //}

        private void updateLandmarkPosition(List<Vector3> landmarks)
        {
            //if (LandmarkObjects.Count == 0)
            //{
            //    setupLandmarks(landmarks.Count);
            //}

            var offset = landmarks[0];
            for (int i = 1; i < landmarks.Count; i++)
            {
                var x = landmarks[i].x - offset.x;
                var y = landmarks[i].y - offset.y;
                var z = landmarks[i].z - offset.z;

                if (x == 0 && y == 0 && z == 0)
                    return;

                if (NegateX) x *= -1;
                if (NegateY) y *= -1;
                if (FlipXY) (x, y) = (y, x);

                LandmarkObjects[i].transform.localPosition = new Vector3(x, y, z);
            }

            float depth = depthCalibrator.GetDepthFromThumbLength(scale);
            this.transform.localPosition = new Vector3(offset.x, offset.y, depth * scale);
        }

        //correct landmark scale based on thumb length
        private void updateLandmarkScale(List<Vector3> list)
        {
            var pointA = new Vector3(list[0].x, list[0].y, list[0].z);
            var pointB = new Vector3(list[1].x, list[1].y, list[1].z);
            var thumbDetectedLength = Vector3.Distance(pointA, pointB);
            if (thumbDetectedLength == 0)
                return;
            scale = thumbModelLength / thumbDetectedLength;
            this.transform.localScale = new Vector3(scale, scale, scale);
        }

        private void updateWristRotation()
        {
            var wristTransform = LandmarkObjects[0].transform;
            var indexFinger = LandmarkObjects[5].transform.position;
            var middleFinger = LandmarkObjects[9].transform.position;

            var vectorToMiddle = middleFinger - wristTransform.position;
            var vectorToIndex = indexFinger - wristTransform.position;
            //to get ortho vector of middle finger from index finger
            Vector3.OrthoNormalize(ref vectorToMiddle, ref vectorToIndex);

            //vector normal to wrist
            Vector3 normalVector = Vector3.Cross(vectorToIndex, vectorToMiddle);

            //Debug.DrawRay(wristTransform.position, normalVector, Color.white);
            //Debug.DrawRay(wristTransform.position, vectorToIndex, Color.yellow);
            wristTransform.rotation = Quaternion.LookRotation(normalVector, vectorToIndex);
        }

    }

}