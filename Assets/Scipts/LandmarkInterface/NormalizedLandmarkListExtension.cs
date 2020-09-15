using Mediapipe;
using System.Collections.Generic;
using UnityEngine;

namespace LandmarkInterface
{
	public static class NormalizedLandmarkListExtension
	{
		public static List<Vector3> ToList(this NormalizedLandmarkList input)
		{
			List<Vector3> output = new List<Vector3>();
			var landmarks = input.Landmark;
			for (int i = 0; i < landmarks.Count; i++)
			{
				var v = new Vector3(landmarks[i].X, landmarks[i].Y, landmarks[i].Z);
				output.Add(v);
			}
			return output;
		}
	}
}
