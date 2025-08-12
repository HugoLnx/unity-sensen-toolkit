using UnityEngine;

namespace SensenToolkit
{
    public struct RaycastHitx
    {
        public Vector3 point;
        public Vector3 normal;
        public float distance;
        public Collider collider;
        public Ray ray;

        public static RaycastHitx FromRaycastHit(RaycastHit hit)
        {
            return new RaycastHitx
            {
                point = hit.point,
                normal = hit.normal,
                distance = hit.distance,
                collider = hit.collider
            };
        }
    }
}
