using UnityEngine;

namespace Boids
{
    public struct BoidData
    {
        public Vector3 Position;
        public Vector3 Direction;

        public Vector3 FlockHeading;
        public Vector3 FlockCenter;
        public Vector3 AvoidanceHeading;

        public int FlockmatesCount;

        public static int Size => sizeof(float) * 3 * 5 + sizeof(int);
    }
}