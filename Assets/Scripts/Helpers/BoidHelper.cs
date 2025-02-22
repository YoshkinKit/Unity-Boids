using UnityEngine;

namespace Boids
{
    public static class BoidHelper
    {
        private const int ViewDirectionsCount = 500;
        public static readonly Vector3[] Directions;
        
        static BoidHelper()
        {
            Directions = new Vector3[ViewDirectionsCount];
            
            var goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
            var angleIncrement = 2 * Mathf.PI * goldenRatio;
            
            for (var i = 0; i < ViewDirectionsCount; i++)
            {
                var t = (float) i / (ViewDirectionsCount - 1);
                var inclination = Mathf.Acos(1 - 2 * t);
                var azimuth = angleIncrement * i;
                
                var x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                var y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                var z = Mathf.Cos(inclination);
                Directions[i] = new Vector3(x, y, z);
            }
        }
    }
}