using System;
using UnityEngine;

namespace Bullets
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        public float damage = 10f;
        public float explosionRadius = 2.5f;
        public float force = 10f;
        [Tooltip("Life time of the bullet")]
        public float lifeTime = 15f;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = transform.GetComponent<Rigidbody>();
        }

        private void Start()
        {
            Destroy(gameObject, lifeTime);
            
            // Apply force
            _rigidbody.AddForce(transform.forward * force, ForceMode.Impulse);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.TryGetComponent(out Health health))
            {
                health.GetComponent<Health>().TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}