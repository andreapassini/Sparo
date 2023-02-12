using System.Collections;
using UnityEngine;

namespace Weapons
{
    public class TrackingHitscan : MonoBehaviour
    {
        private IEnumerator _shootEnumerator;
        private bool _shooting = false;
        private Animator _animator;

        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject vfxShootEffect;
        
        [Space]
        [SerializeField] private float fireRate = .01f;
        public float damage = 6f;
    
        [Space]
        public LayerMask hitMask;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            // On mouse 0 hold, keep shooting
            if (Input.GetMouseButton(0))
            {
                StartShooting();
            } else if (_shooting)
            {
                StopShooting();
            }
        }

        private void StartShooting()
        {
            _shooting = true;
            if (_shootEnumerator == null)
            {
                _shootEnumerator = Shoot(fireRate);
                StartCoroutine(_shootEnumerator);
                _animator.SetTrigger("shoot");
            }
        }

        private void StopShooting()
        {
            StopCoroutine(_shootEnumerator);
            _shootEnumerator = null;
            _shooting = false;
            _animator.SetTrigger("stopShooting");
        }

        private IEnumerator Shoot(float shootingRate)
        {
            while (_shooting)
            {
                RaycastHit hit;
                // Does the ray intersect any objects excluding the player layer
                if (Physics.Raycast(firePoint.position, firePoint.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, hitMask))
                {
                    if (hit.transform.TryGetComponent(out Health health))
                    {
                        health.GetComponent<Health>().TakeDamage(damage);
                    }                    Debug.DrawRay(firePoint.position, firePoint.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                    Debug.Log("Did Hit");
                }
                else
                {
                    Debug.DrawRay(firePoint.position, firePoint.TransformDirection(Vector3.forward) * 1000, Color.white);
                    Debug.Log("Did not Hit");
                }
            
                // VFX
                if (vfxShootEffect)
                {
                    Instantiate(vfxShootEffect, firePoint.forward, firePoint.rotation);
                }
                
                // wait
                yield return new WaitForSeconds(shootingRate);
            }
        }
    }
}
