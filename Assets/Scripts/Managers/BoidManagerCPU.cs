namespace Boids
{
    public class BoidManagerCPU : BoidManager
    {
        private void Update()
        {
            if (Boids is null)
                return;

            var boidData = new BoidData[Boids.Length];

            for (var i = 0; i < Boids.Length; i++)
            {
                boidData[i].Position = Boids[i].Position;
                boidData[i].Direction = Boids[i].Forward;
            }

            CalculateFlockBehavior(boidData);

            for (var i = 0; i < Boids.Length; i++)
            {
                Boids[i].AvgFlockHeading = boidData[i].FlockHeading;
                Boids[i].CenterOfFlockmates = boidData[i].FlockCenter;
                Boids[i].AvgAvoidanceHeading = boidData[i].AvoidanceHeading;
                Boids[i].PerceivedFlockmatesCount = boidData[i].FlockmatesCount;

                Boids[i].UpdateBoid();
            }
        }

        private void CalculateFlockBehavior(BoidData[] boidData)
        {
            for (int i = 0; i < boidData.Length; i++)
            {
                for (int j = 0; j < boidData.Length; j++)
                {
                    if (i == j)
                        continue;

                    var boidB = boidData[j];
                    var offset = boidB.Position - boidData[i].Position;
                    var sqrDst = offset.sqrMagnitude;

                    if (sqrDst > settings.perceptionRadius * settings.perceptionRadius)
                        continue;

                    boidData[i].FlockmatesCount += 1;
                    boidData[i].FlockHeading += boidB.Direction;
                    boidData[i].FlockCenter += boidB.Position;

                    if (sqrDst < settings.avoidanceRadius * settings.avoidanceRadius)
                        boidData[i].AvoidanceHeading -= offset / sqrDst;
                }
            }
        }
    }
}