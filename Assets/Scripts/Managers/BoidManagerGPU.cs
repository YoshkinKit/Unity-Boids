using UnityEngine;

namespace Boids
{
    public class BoidManagerGPU : BoidManager
    {
        private static readonly int BoidsArray = Shader.PropertyToID("boids");
        private static readonly int BoidsCount = Shader.PropertyToID("num_boids");
        private static readonly int ViewRadius = Shader.PropertyToID("view_radius");
        private static readonly int AvoidRadius = Shader.PropertyToID("avoid_radius");

        private const int ThreadGroupSize = 1024;

        [SerializeField] private ComputeShader compute;

        private void Update()
        {
            if (Boids == null)
                return;

            var boidData = new BoidData[Boids.Length];

            for (var i = 0; i < Boids.Length; i++)
            {
                boidData[i].Position = Boids[i].Position;
                boidData[i].Direction = Boids[i].Forward;
            }

            var boidBuffer = new ComputeBuffer(Boids.Length, BoidData.Size);
            boidBuffer.SetData(boidData);

            compute.SetBuffer(0, BoidsArray, boidBuffer);
            compute.SetInt(BoidsCount, Boids.Length);
            compute.SetFloat(ViewRadius, settings.perceptionRadius);
            compute.SetFloat(AvoidRadius, settings.avoidanceRadius);

            var threadGroups = Mathf.CeilToInt(Boids.Length / (float)ThreadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boidData);

            for (var i = 0; i < Boids.Length; i++)
            {
                Boids[i].AvgFlockHeading = boidData[i].FlockHeading;
                Boids[i].CenterOfFlockmates = boidData[i].FlockCenter;
                Boids[i].AvgAvoidanceHeading = boidData[i].AvoidanceHeading;
                Boids[i].PerceivedFlockmatesCount = boidData[i].FlockmatesCount;

                Boids[i].UpdateBoid();
            }

            boidBuffer.Release();
        }
    }
}