using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ProjectileScriptableObject", order = 1)]
    public class ProjectileScriptableObject: HitscanScriptableObject
    {
        public GameObject bulletPrefab;
        public float bulletForce;
        public float explosionRadius;
        public float bulletLifeTime;
    }
}