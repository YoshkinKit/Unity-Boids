using UnityEngine;

namespace Boids
{
    [CreateAssetMenu]
    public class BoidSettings : ScriptableObject
    {
        [Header("Boid Settings")]
        public float minSpeed = 2f;
        public float maxSpeed = 5f;
        public float perceptionRadius = 2.5f;
        public float avoidanceRadius = 1f;
        public float maxSteerForce = 3f;

        [Header("Weights")]
        public float alignWeight = 1f;
        public float cohesionWeight = 1f;
        public float separateWeight = 1f;
        public float targetWeight = 1f;

        [Header("Collisions")]
        public LayerMask obstacleMask;
        public float boundsRadius = 0.27f;
        public float avoidCollisionWeight = 10f;
        public float collisionAvoidDistance = 5f;
    }
}