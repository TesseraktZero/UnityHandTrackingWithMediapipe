using LandmarkInterface.Filter;
using Mediapipe;
using System.Collections.Generic;
using UnityEngine;

namespace LandmarkInterface
{
    public class LandmarkResultSet : MonoBehaviour
    {
        public bool Connected;
        public Vector3 DeviceOrientation { get; set; }

        public bool UseFilter = true;

        [Range(0.0f, 1.0f)] public double FilterTimeInterval = 0.4;
        [Range(0.0f, 1.0f)] public double FilterNoise = 0.1;
        [Header("Filter extreme data exceeding the limit")]
        [Range(0.0f, 1.0f)] public double DisplacementLimit = 0.4;

        private LandmarkListFilter LeftHandFilter;

        private LandmarkListFilter RightHandFilter;

        private NormalizedLandmarkList leftHandLandmarks;
        public NormalizedLandmarkList LeftHandLandmarks
        {
            get => leftHandLandmarks;
            set
            {
                if (UseFilter)
                {
                    LeftHandFilter.CorrectAndPredict(value);
                }
                leftHandLandmarks = value;
            }
        }

        private NormalizedLandmarkList rightHandLandmarks;

        public NormalizedLandmarkList RightHandLandmarks
        {
            get => rightHandLandmarks;
            set
            {
                if (UseFilter)
                {
                    RightHandFilter.CorrectAndPredict(value);
                }
                rightHandLandmarks = value;
            }
        }

        private void Awake()
        {
            LeftHandFilter = new LandmarkListFilter(FilterTimeInterval, FilterNoise, DisplacementLimit);
            RightHandFilter = new LandmarkListFilter(FilterTimeInterval, FilterNoise, DisplacementLimit);
        }

        private void Update()
        {
            LeftHandFilter.UpdateFilterParameter(FilterTimeInterval, FilterNoise, DisplacementLimit);
            RightHandFilter.UpdateFilterParameter(FilterTimeInterval, FilterNoise, DisplacementLimit);
        }

        public void UpdateLandmark(LandmarkType landmarkType, NormalizedLandmarkList landmarkList)
        {
            if (landmarkType == LandmarkType.LeftHand)
            {
                LeftHandLandmarks = landmarkList;
                //Debug.Log($"left hand: {landmarkList.Landmark[0].X}, {landmarkList.Landmark[0].Y}, {landmarkList.Landmark[0].Z}");
            }
            else if (landmarkType == LandmarkType.RightHand)
            {
                RightHandLandmarks = landmarkList;
                //Debug.Log($"right hand: {landmarkList.Landmark[0].X}, {landmarkList.Landmark[0].Y}, {landmarkList.Landmark[0].Z}");
            }
            else
            {
                Debug.LogError("Not Implemented");
            }
        }

        public List<Vector3> GetLandmarks(LandmarkType landmarkType)
        {
            if (UseFilter)
            {
                if (landmarkType == LandmarkType.LeftHand)
                {
                    return LeftHandFilter.GetPositions();
                }
                else if (landmarkType == LandmarkType.RightHand)
                {
                    return RightHandFilter.GetPositions();
                }
            }
            else
            {
                if (landmarkType == LandmarkType.LeftHand)
                {
                    return LeftHandLandmarks != null ? LeftHandLandmarks.ToList() : null;
                }
                else if (landmarkType == LandmarkType.RightHand)
                {
                    return RightHandLandmarks != null ? RightHandLandmarks.ToList() : null;
                }
            }
            return null;
        }

    }

}