using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    public class Projectile : MonoBehaviour
    {
        private IEnumerator _shootEnumerator;
        private bool _shooting = false;
        private Animator _animator;

        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject vfxShootEffect;

        [Space] 
        [SerializeField] private GameObject bullet;
        [SerializeField] private float cooldown = 1.5f;
        public float damage = 6f;
    
        [Space]
        public LayerMask hitMask;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        void Update()
        {
            if (Input.GetMouseButtonDown(0) && !_shooting)
            {
                ShootProjectile(cooldown);
            }
        }
        
        private void ShootProjectile(float cooldown)
        {
            _shooting = true;
        
            // ShootProjectile
            Instantiate(bullet, firePoint.position, firePoint.rotation);
        
            // Animation and VFX
            _animator.SetTrigger("shoot");
            if (vfxShootEffect)
            {
                Instantiate(vfxShootEffect, firePoint.forward, firePoint.rotation);
            }

            // Start Coroutine
            _shootEnumerator = WaitForCooldown(cooldown);
            StartCoroutine(_shootEnumerator);
        }

        private IEnumerator WaitForCooldown(float cooldwon)
        {
            yield return new WaitForSeconds(cooldwon);

            _shooting = false;
            _animator.SetTrigger("stopShooting");
        }
    }
}