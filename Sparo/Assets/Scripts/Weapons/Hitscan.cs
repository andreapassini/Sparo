using System.Collections;
using UnityEngine;

namespace Weapons
{
    public class Hitscan : MonoBehaviour
    {
        private Animator _animator;
        private bool _shooting = false;
        private IEnumerator _shootingCoroutine;

        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject vfxShootEffect;
        
        [Space]
        [SerializeField] private float cooldown = 1.5f;
        public float damage = 75f;
    
        [Space]
        public LayerMask hitMask;

        private void Awake()
        {
            _animator = transform.GetComponentInChildren<Animator>();
        }

        void Start()
        {
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && !_shooting)
            {
                ShootRay(cooldown);
            }
        }

        private void ShootRay(float cooldown)
        {
            _shooting = true;
        
            // ShootRay Ray
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(firePoint.position, firePoint.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, hitMask))
            {
                if (hit.transform.TryGetComponent(out Health health))
                {
                    health.GetComponent<Health>().TakeDamage(damage);
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
            if (vfxShootEffect)
            {
                Instantiate(vfxShootEffect, firePoint.forward, firePoint.rotation);
            }

            // Start Coroutine
            _shootingCoroutine = WaitForCooldown(cooldown);
            StartCoroutine(_shootingCoroutine);
        }

        private IEnumerator WaitForCooldown(float cooldwon)
        {
            yield return new WaitForSeconds(cooldwon);

            _shooting = false;
            _animator.SetTrigger("stopShooting");
        }
    }
}
