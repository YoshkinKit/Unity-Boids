using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class BoidVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private bool showDirectionVectors = true;
        [SerializeField] private bool showFlockConnections = true;
        [SerializeField] private bool showAvoidanceVectors = true;
        [SerializeField] private bool showPerceptionRadius;
        [SerializeField] private bool showAvoidanceRadius;

        [Header("Appearance")]
        [SerializeField] private float vectorScale = 1f;
        [SerializeField] private Color directionColor = Color.blue;
        [SerializeField] private Color alignmentColor = Color.green;
        [SerializeField] private Color cohesionColor = Color.yellow;
        [SerializeField] private Color separationColor = Color.red;
        [SerializeField] private Color collisionAvoidanceColor = Color.magenta;
        [SerializeField] private Color connectionColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        [Header("Directions Visualization")]
        [SerializeField] private bool showDirectionPoints;
        [SerializeField] [Range(0f, 1f)] private float pointSize = 0.05f;
        [SerializeField] private Gradient directionGradient = new Gradient();
        [SerializeField] private float pointVisualizationRadius = 1.5f;
        [SerializeField] [Range(1, 500)] private int maxDirectionPoints = 500;

        private BoidManagerCPU _boidManager;
        private LineRenderer[] _lineRenderers;
        private const int MaxLineCount = 1000;

        private readonly Dictionary<Boid, DirectionPointsVisualizer> _directionVisualizers = new();

        private void Awake()
        {
            _boidManager = GetComponent<BoidManagerCPU>();
            InitializeLineRenderers();
            SetupDefaultGradient();
        }

        private void InitializeLineRenderers()
        {
            _lineRenderers = new LineRenderer[MaxLineCount];

            for (int i = 0; i < MaxLineCount; i++)
            {
                var lineObj = new GameObject($"BoidConnection_{i}");
                lineObj.transform.SetParent(transform);

                var lr = lineObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = connectionColor;
                lr.endColor = connectionColor;
                lr.enabled = false;

                _lineRenderers[i] = lr;
            }
        }

        private void SetupDefaultGradient()
        {
            if (directionGradient.colorKeys.Length > 1)
                return;

            var colorKeys = new GradientColorKey[4];
            colorKeys[0] = new GradientColorKey(new Color(0.8f, 0.2f, 0.2f), 0.0f);
            colorKeys[1] = new GradientColorKey(new Color(0.8f, 0.8f, 0.2f), 0.33f);
            colorKeys[2] = new GradientColorKey(new Color(0.2f, 0.8f, 0.2f), 0.66f);
            colorKeys[3] = new GradientColorKey(new Color(0.2f, 0.2f, 0.8f), 1.0f);

            var alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);

            directionGradient.SetKeys(colorKeys, alphaKeys);
        }

        private void OnEnable()
        {
            if (!_boidManager || _boidManager.Boids == null)
                return;

            foreach (var boid in _boidManager.Boids)
            {
                EnsureDirectionVisualizerExists(boid);
            }
        }

        private void EnsureDirectionVisualizerExists(Boid boid)
        {
            if (_directionVisualizers.ContainsKey(boid))
                return;

            var visualizerObj = new GameObject($"DirectionVisualizer_{boid.GetInstanceID()}");
            visualizerObj.transform.SetParent(boid.transform);
            visualizerObj.transform.localPosition = Vector3.zero;

            var visualizer = visualizerObj.AddComponent<DirectionPointsVisualizer>();
            visualizer.Initialize(BoidHelper.Directions, maxDirectionPoints, pointSize, directionGradient);
            _directionVisualizers[boid] = visualizer;
        }

        private void LateUpdate()
        {
            if (_boidManager.Boids == null)
                return;

            foreach (var line in _lineRenderers)
            {
                line.enabled = false;
            }

            foreach (var boid in _boidManager.Boids)
            {
                if (!_directionVisualizers.ContainsKey(boid))
                    EnsureDirectionVisualizerExists(boid);

                var visualizer = _directionVisualizers[boid];
                visualizer.SetVisible(showDirectionPoints);

                if (showDirectionPoints)
                    visualizer.UpdatePoints(pointVisualizationRadius, pointSize);
            }

            DrawBoidVisualizations();
        }

        private void DrawBoidVisualizations()
        {
            int connectionIndex = 0;

            for (int i = 0; i < _boidManager.Boids.Length; i++)
            {
                var boid = _boidManager.Boids[i];
                var position = boid.Position;

                if (showDirectionVectors)
                {
                    Debug.DrawRay(position, boid.Forward * vectorScale, directionColor);

                    if (boid.PerceivedFlockmatesCount > 0)
                    {
                        Debug.DrawRay(position, boid.AvgFlockHeading.normalized * (vectorScale * 0.8f), alignmentColor);

                        var cohesionVector = boid.CenterOfFlockmates - position;
                        Debug.DrawRay(position, cohesionVector.normalized * (vectorScale * 0.8f), cohesionColor);

                        if (showAvoidanceVectors)
                            Debug.DrawRay(position, boid.AvgAvoidanceHeading.normalized * (vectorScale * 0.8f),
                                separationColor);

                        if (boid.IsHeadingForCollision())
                            Debug.DrawRay(position, boid.CollisionAvoidanceHeading.normalized * (vectorScale * 0.8f),
                                collisionAvoidanceColor);
                    }
                }

                if (showFlockConnections && _boidManager)
                {
                    for (int j = i + 1; j < _boidManager.Boids.Length; j++)
                    {
                        var otherBoid = _boidManager.Boids[j];
                        var offset = otherBoid.Position - position;
                        var sqrDst = offset.sqrMagnitude;

                        if (!(sqrDst <= _boidManager.settings.perceptionRadius *
                                _boidManager.settings.perceptionRadius))
                            continue;

                        if (connectionIndex >= MaxLineCount)
                            continue;

                        var line = _lineRenderers[connectionIndex];
                        line.enabled = true;
                        line.SetPosition(0, position);
                        line.SetPosition(1, otherBoid.Position);

                        if (sqrDst <= _boidManager.settings.avoidanceRadius * _boidManager.settings.avoidanceRadius)
                        {
                            line.startColor = separationColor;
                            line.endColor = separationColor;
                        }
                        else
                        {
                            line.startColor = connectionColor;
                            line.endColor = connectionColor;
                        }

                        connectionIndex++;
                    }
                }

                if (!_boidManager)
                    continue;

                if (showPerceptionRadius)
                    DrawSphere(position, _boidManager.settings.perceptionRadius, Color.cyan, 16);

                if (showAvoidanceRadius)
                    DrawSphere(position, _boidManager.settings.avoidanceRadius, separationColor, 16);
            }
        }

        private void DrawSphere(Vector3 center, float radius, Color color, int segments)
        {
            for (int latitude = 0; latitude <= segments; latitude++)
            {
                var phi = latitude * Mathf.PI / segments;
                var y = radius * Mathf.Cos(phi);
                var r = radius * Mathf.Sin(phi);

                var lastPoint = Vector3.zero;
                var isFirstPoint = true;

                for (int lon = 0; lon <= segments; lon++)
                {
                    var theta = lon * 2 * Mathf.PI / segments;
                    var x = r * Mathf.Sin(theta);
                    var z = r * Mathf.Cos(theta);

                    var newPoint = center + new Vector3(x, y, z);

                    if (!isFirstPoint)
                        Debug.DrawLine(lastPoint, newPoint, color);

                    lastPoint = newPoint;
                    isFirstPoint = false;
                }
            }

            for (int longitude = 0; longitude < segments; longitude++)
            {
                var theta = longitude * 2 * Mathf.PI / segments;
                var sinTheta = Mathf.Sin(theta);
                var cosTheta = Mathf.Cos(theta);

                var lastPoint = Vector3.zero;
                var isFirstPoint = true;

                for (int lat = 0; lat <= segments; lat++)
                {
                    var phi = lat * Mathf.PI / segments;
                    var y = radius * Mathf.Cos(phi);
                    var r = radius * Mathf.Sin(phi);

                    var x = r * sinTheta;
                    var z = r * cosTheta;

                    var newPoint = center + new Vector3(x, y, z);

                    if (!isFirstPoint)
                        Debug.DrawLine(lastPoint, newPoint, color);

                    lastPoint = newPoint;
                    isFirstPoint = false;
                }
            }
        }
    }
}