using System;
using Sirenix.OdinInspector;

namespace PathCreation
{
    [Serializable]
    public class PathInfo
    {
        [ReadOnly]
        public int BezierIndex;

        public float ActionRadius;

        public float WaitingTime;
    }
}