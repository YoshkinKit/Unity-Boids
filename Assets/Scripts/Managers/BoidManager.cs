using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public abstract class BoidManager : MonoBehaviour
    {
        [SerializeField] public BoidSettings settings;
        [SerializeField] protected Transform target;
        public Boid[] Boids { get; private set; }

        protected void Start()
        {
            var spawners = FindObjectsOfType<Spawner>();
            if (spawners.Length == 0)
                return;

            var boidList = new List<Boid>();
            foreach (var spawner in spawners)
                boidList.AddRange(spawner.Boids);

            Boids = boidList.ToArray();

            foreach (var boid in Boids)
            {
                boid.Initialize(settings, target);
            }
        }
    }
}