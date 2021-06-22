using UnityEngine;

namespace Core
{
    public static class Helper
    {
        public static Ray ForwardCollisionRay(Transform _transform, float _speed, out float _nextTravelDistance, out Vector3 _nextPosition)
        {
            var forward = _transform.forward;
            
            _nextPosition = forward * (_speed * Time.deltaTime);
            
            var currentPosition = _transform.position;
            
            _nextTravelDistance = Vector3.Distance(currentPosition, currentPosition + _nextPosition);
            
            return new Ray(currentPosition, forward);
        }
    }
}