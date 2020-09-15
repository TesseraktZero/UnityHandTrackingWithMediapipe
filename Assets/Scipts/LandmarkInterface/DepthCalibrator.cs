namespace LandmarkInterface
{
    //predict actual distance based on measured length
    //y = m/x + c
    //measured values: m:-0.0719f, c:0.439f
    public class DepthCalibrator
    {
        //todo convert to MonoBehaviour and add SerializeField private
        private float m;
        private float c;

        public DepthCalibrator(float m, float c)
        {
            this.m = m;
            this.c = c;
        }

        public float GetDepthFromThumbLength(float length)
        {
            if (length == 0)
                return 0;
            return m / length + c;
        }
    }

}
