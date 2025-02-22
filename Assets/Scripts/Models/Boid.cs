using UnityEngine;

namespace Boids
{
    public class Boid : MonoBehaviour
    {
        public Vector3 Position { get; private set; }
        public Vector3 Forward { get; private set; }
        public Vector3 AvgFlockHeading { get; set; }
        public Vector3 AvgAvoidanceHeading { get; set; }
        public Vector3 CenterOfFlockmates { get; set; }
        public int PerceivedFlockmatesCount { get; set; }

        private Vector3 _velocity;
        private BoidSettings _settings;
        private Transform _target;
        private Material _material;
        private Transform _cachedTransform;

        private void Awake()
        {
            _material = GetComponentInChildren<MeshRenderer>().material;
            _cachedTransform = transform;
        }

        public void Initialize(BoidSettings settings, Transform target)
        {
            _settings = settings;
            _target = target;

            Position = _cachedTransform.position;
            Forward = _cachedTransform.forward;

            var startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
            _velocity = transform.forward * startSpeed;
        }

        public void UpdateBoid()
        {
            var acceleration = Vector3.zero;

            if (_target)
            {
                var offsetToTarget = _target.position - Position;
                acceleration = SteerTowards(offsetToTarget) * _settings.targetWeight;
            }

            if (PerceivedFlockmatesCount != 0)
            {
                CenterOfFlockmates /= PerceivedFlockmatesCount;

                var offsetToFlockCenter = CenterOfFlockmates - Position;

                var alignmentForce = SteerTowards(AvgFlockHeading) * _settings.alignWeight;
                var cohesionForce = SteerTowards(offsetToFlockCenter) * _settings.cohesionWeight;
                var separationForce = SteerTowards(AvgAvoidanceHeading) * _settings.separateWeight;

                acceleration += alignmentForce;
                acceleration += cohesionForce;
                acceleration += separationForce;
            }

            if (IsHeadingForCollision())
            {
                var collisionAvoidDirection = ObstacleRays();
                var collisionAvoidForce = SteerTowards(collisionAvoidDirection) * _settings.avoidCollisionWeight;
                acceleration += collisionAvoidForce;
            }

            _velocity += acceleration * Time.deltaTime;
            var speed = _velocity.magnitude;
            var direction = _velocity / speed;
            speed = Mathf.Clamp(speed, _settings.minSpeed, _settings.maxSpeed);
            _velocity = direction * speed;

            _cachedTransform.position += _velocity * Time.deltaTime;
            _cachedTransform.forward = direction;

            Position = _cachedTransform.position;
            Forward = direction;
        }

        private Vector3 SteerTowards(Vector3 vector)
        {
            var v = vector.normalized * _settings.maxSpeed - _velocity;
            return Vector3.ClampMagnitude(v, _settings.maxSteerForce);
        }

        private bool IsHeadingForCollision()
        {
            return Physics.SphereCast(Position, _settings.boundsRadius, Forward, out _,
                _settings.collisionAvoidDistance,
                _settings.obstacleMask);
        }

        private Vector3 ObstacleRays()
        {
            var rayDirections = BoidHelper.Directions;

            foreach (var rayDirection in rayDirections)
            {
                var dir = _cachedTransform.TransformDirection(rayDirection);
                var ray = new Ray(Position, dir);

                if (!Physics.SphereCast(ray, _settings.boundsRadius, _settings.collisionAvoidDistance,
                        _settings.obstacleMask))
                    return dir;
            }

            return Forward;
        }

        public void SetColor(Color color)
        {
            if (_material)
                _material.color = color;
        }
    }
}