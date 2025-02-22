using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private Boid prefab;
        [SerializeField] private float spawnRadius = 10;
        [SerializeField] private int spawnCount = 10;
        [SerializeField] private Color color;
        [SerializeField] private GizmoType showSpawnRegion;
        
        public List<Boid> Boids { get; private set; }

        private void Awake()
        {
            Boids = new List<Boid>();
            
            for (var i = 0; i < spawnCount; i++)
            {
                var pos = transform.position + Random.insideUnitSphere * spawnRadius;
                var boid = Instantiate(prefab);
                boid.transform.position = pos;
                boid.transform.forward = Random.insideUnitSphere;
                boid.SetColor(color);
                
                Boids.Add(boid);
            }
        }

        private void OnDrawGizmos()
        {
            if (showSpawnRegion == GizmoType.Always)
                DrawGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (showSpawnRegion == GizmoType.SelectedOnly)
                DrawGizmos();
        }

        private void DrawGizmos()
        {
            Gizmos.color = new Color(color.r, color.g, color.b, 0.3f);
            Gizmos.DrawSphere(transform.position, spawnRadius);
        }
    }

    public enum GizmoType
    {
        Never,
        SelectedOnly,
        Always
    }
}