using System.Collections;
using ScriptableObjects;
using UnityEngine;

namespace Weapons
{
    public class TrackingHitscan : MonoBehaviour
    {
        private IEnumerator _shootEnumerator;
        private bool _shooting = false;
        private Animator _animator;

        [Tooltip("Spawn point of the VFX, from Z axis")]
        [SerializeField] private Transform firePointVFX;
        private GameObject _vfxShootEffect;
        
        [Space]
        [Tooltip("Fire point of the Ray, from Z axis")]
        [SerializeField] private Transform firePoint;
        
        [Space] 
        public HitscanScriptableObject hitscanScriptableObject;

        private float _fireRate = .01f;
        private float _damage = 6f;
        
        private LayerMask _hitMask;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        void Start()
        {
            SetUpFields();
        }
        void Update()
        {
            // On mouse 0 hold, keep shooting
            if (Input.GetMouseButton(0) && !_shooting)
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
                _shootEnumerator = Shoot(_fireRate);
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
                if (Physics.Raycast(firePoint.position, firePoint.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, _hitMask))
                {
                    if (hit.transform.TryGetComponent(out Health health))
                    {
                        health.GetComponent<Health>().TakeDamage(_damage);
                    }                    Debug.DrawRay(firePoint.position, firePoint.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                    Debug.Log("Did Hit");
                }
                else
                {
                    Debug.DrawRay(firePoint.position, firePoint.TransformDirection(Vector3.forward) * 1000, Color.white);
                    Debug.Log("Did not Hit");
                }
            
                // VFX
                if (_vfxShootEffect)
                {
                    Instantiate(_vfxShootEffect, firePointVFX.forward, firePointVFX.rotation);
                }
                
                // wait
                yield return new WaitForSeconds(shootingRate);
            }
        }
        private void SetUpFields()
        {
            _fireRate = hitscanScriptableObject.cooldown;
            _damage = hitscanScriptableObject.damage;
            _vfxShootEffect = hitscanScriptableObject.vfxShootEffect;
            _hitMask = hitscanScriptableObject.hitMask;
        }
    }
}
