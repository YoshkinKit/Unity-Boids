using UnityEngine;

namespace Boids
{
    public class DirectionPointsVisualizer : MonoBehaviour
    {
        private Vector3[] _directions;
        private GameObject[] _pointObjects;
        private MeshRenderer[] _renderers;
        private Gradient _gradient;
        
        public void Initialize(Vector3[] directions, int maxPoints, float pointSize, Gradient gradient)
        {
            _gradient = gradient;
            var pointCount = Mathf.Min(directions.Length, maxPoints);
            
            _directions = new Vector3[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                _directions[i] = directions[i];
            }
            
            CreatePointObjects(pointSize);
        }
        
        private void CreatePointObjects(float pointSize)
        {
            _pointObjects = new GameObject[_directions.Length];
            _renderers = new MeshRenderer[_directions.Length];
            
            for (int i = 0; i < _directions.Length; i++)
            {
                _pointObjects[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _pointObjects[i].transform.localScale = Vector3.one * pointSize;
                _pointObjects[i].transform.SetParent(transform);
                
                Destroy(_pointObjects[i].GetComponent<Collider>());
                
                _renderers[i] = _pointObjects[i].GetComponent<MeshRenderer>();
                
                var gradientPosition = (float)i / (_directions.Length - 1);
                _renderers[i].material = new Material(Shader.Find("Standard"))
                {
                    color = _gradient.Evaluate(gradientPosition)
                };

                _renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _renderers[i].receiveShadows = false;
            }
        }
        
        public void UpdatePoints(float radius, float pointSize)
        {
            for (int i = 0; i < _directions.Length; i++)
            {
                _pointObjects[i].transform.localPosition = _directions[i] * radius;
                
                _pointObjects[i].transform.localScale = Vector3.one * pointSize;
            }
        }
        
        public void SetVisible(bool visible)
        {
            foreach (var point in _pointObjects)
            {
                point.SetActive(visible);
            }
        }
        
        private void OnDestroy()
        {
            foreach (var meshRenderer in _renderers)
            {
                if (meshRenderer && meshRenderer.material)
                    Destroy(meshRenderer.material);
            }
        }
    }
}