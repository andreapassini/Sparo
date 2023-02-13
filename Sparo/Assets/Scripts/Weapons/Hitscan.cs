using System.Collections;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Weapons
{
    public class Hitscan : MonoBehaviour
    {
        private Animator _animator;
        private bool _shooting = false;
        private IEnumerator _shootingCoroutine;

        [Tooltip("Spawn point of the VFX, from Z axis")]
        [SerializeField] private Transform firePointVFX;
        private GameObject _vfxShootEffect;
        
        [Space]
        [Tooltip("Fire point of the Ray, from Z axis")]
        [SerializeField] private Transform firePoint;

        [Space] 
        public HitscanScriptableObject hitscanScriptableObject;

        private bool _stopWithAnimation;
        private float _cooldown = 1.5f;
        private float _damage = 75f;
        
        private LayerMask _hitMask;

        private void Awake()
        {
            _animator = transform.GetComponentInChildren<Animator>();
        }
        void Start()
        {
            SetUpFields();
        }
        void Update()
        {
            if (Input.GetMouseButton(0) && !_shooting)
            {
                ShootRay(_cooldown);
            }
        }
        private void ShootRay(float cooldown)
        {
            _shooting = true;
        
            // ShootRay Ray
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(firePoint.position, firePoint.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, _hitMask))
            {
                if (hit.transform.TryGetComponent(out Health health))
                {
                    health.GetComponent<Health>().TakeDamage(_damage);
                }
                Debug.DrawRay(firePoint.position, firePoint.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                Debug.Log("Did Hit");
            }
            else
            {
                Debug.DrawRay(firePoint.position, firePoint.TransformDirection(Vector3.forward) * 1000, Color.white);
                Debug.Log("Did not Hit");
            }
        
            // Animation and VFX
            _animator.SetTrigger("shoot");
            if (_vfxShootEffect)
            {
                Instantiate(_vfxShootEffect, firePointVFX.forward, firePointVFX.rotation);
            }

            if(_stopWithAnimation)
                return;
            
            // Start Coroutine
            _shootingCoroutine = WaitForCooldown(cooldown);
            StartCoroutine(_shootingCoroutine);
        }
        
        // Called by the animation
        public void StopShooting()
        {
            if (_stopWithAnimation)
            {
                _shooting = false;
            }
        }
        private IEnumerator WaitForCooldown(float cooldwon)
        {
            yield return new WaitForSeconds(cooldwon);

            _shooting = false;
        }
        private void SetUpFields()
        {
            _cooldown = hitscanScriptableObject.cooldown;
            _damage = hitscanScriptableObject.damage;
            _vfxShootEffect = hitscanScriptableObject.vfxShootEffect;
            _hitMask = hitscanScriptableObject.hitMask;
            _stopWithAnimation = hitscanScriptableObject.stopWithCooldown;
        }
    }
}
