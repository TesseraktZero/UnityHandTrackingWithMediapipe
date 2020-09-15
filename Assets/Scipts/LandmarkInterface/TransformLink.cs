using UnityEngine;

namespace LandmarkInterface
{
    public class TransformLink : MonoBehaviour
    {
        public Transform Target;

        public void UpdateTransform()
        {
            if (Target == null)
                return;
            Target.SetPositionAndRotation(this.transform.position, this.transform.rotation);
        }
    }

}