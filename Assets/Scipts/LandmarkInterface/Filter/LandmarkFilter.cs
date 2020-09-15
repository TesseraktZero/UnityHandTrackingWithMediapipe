using Accord.Extensions.Statistics.Filters;
using Accord.Math;
using Mediapipe;
using System;
using UnityEngine;

namespace LandmarkInterface.Filter
{
    //https://www.codeproject.com/Articles/865935/Object-Tracking-Kalman-Filter-with-Ease
    using ModelState = ConstantVelocity3DModel;

    public class LandmarkFilter
    {
        private DiscreteKalmanFilter<ModelState, NormalizedLandmark> kalmanFilter;

        public LandmarkFilter(double timeInterval, double noise)
        {
            ModelState initialState = new ModelState 
            {
                Position = new Vector3(0, 0, 0),
                Velocity = new Vector3(0, 0, 0),
                //Acceleration = new Vector3(0, 0, 0),
            };

            double[,] initialStateError = ModelState.GetProcessNoise(noise, timeInterval);
            int measurementVectorDimension = 3;
            int controlVectorDimension = 0;
            Func<ModelState, double[]> stateConvertFunc = ModelState.ToArray;
            Func<double[], ModelState> stateConvertBackFunc = ModelState.FromArray;
            Func<NormalizedLandmark, double[]> measurementConvertFunc = v => { return new double[3] { v.X, v.Y, v.Z }; };

            kalmanFilter = new DiscreteKalmanFilter<ModelState, NormalizedLandmark>(
                initialState, 
                initialStateError, 
                measurementVectorDimension, 
                controlVectorDimension, 
                stateConvertFunc, 
                stateConvertBackFunc,
                measurementConvertFunc);

            kalmanFilter.ProcessNoise = ModelState.GetProcessNoise(noise, timeInterval);
            kalmanFilter.MeasurementNoise = Matrix.Diagonal<double>(kalmanFilter.MeasurementVectorDimension, 1);
            kalmanFilter.MeasurementMatrix = ModelState.GetPositionMeasurementMatrix();
            kalmanFilter.TransitionMatrix = ModelState.GetTransitionMatrix(timeInterval);
            kalmanFilter.Predict();
        }

        public void UpdateFilterParameter(double timeInterval, double noise)
        {
            kalmanFilter.ProcessNoise = ModelState.GetProcessNoise(noise, timeInterval);
            kalmanFilter.TransitionMatrix = ModelState.GetTransitionMatrix(timeInterval);
        }

        public void Correct(NormalizedLandmark landmark)
        {
            kalmanFilter.Correct(landmark);
        }

        public void Predict()
        {
            kalmanFilter.Predict();
        }

        public Vector3 GetPosition()
        {
            return kalmanFilter.State.Position;
        }

    }

}