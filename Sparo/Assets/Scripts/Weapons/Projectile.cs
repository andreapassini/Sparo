using System.Collections;
using System.Collections.Generic;
using Bullets;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Weapons
{
    public class Projectile : MonoBehaviour
    {
        private IEnumerator _shootEnumerator;
        private bool _shooting = false;
        private Animator _animator;
        
        [Tooltip("Spawn point of the VFX, from Z axis")]
        [SerializeField] private Transform firePointVFX;
        private GameObject _vfxShootEffect;
        
        [Space]
        [Tooltip("Fire point of the Projectile, from Z axis")]
        [SerializeField] private Transform firePoint;

        [Space] 
        public ProjectileScriptableObject projectileScriptableObject;
        
        private bool _stopWithAnimation;
        private float _cooldown = 1.5f;
        
        private GameObject _bulletPrefab;
        private float _damage = 6f;
        private float _force;
        private float _explosionRadius;
        private float _bulletLifetime;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        void Update()
        {
            if (Input.GetMouseButton(0) && !_shooting)
            {
                ShootProjectile(_cooldown);
            }
        }
        
        private void ShootProjectile(float cooldown)
        {
            _shooting = true;
        
            // ShootProjectile
            Bullet bulletScript = _bulletPrefab.GetComponent<Bullet>();
            bulletScript.damage = _damage;
            bulletScript.force = _force;
            bulletScript.explosionRadius = _explosionRadius;
            bulletScript.lifeTime = _bulletLifetime;
            Instantiate(_bulletPrefab, firePoint.position, firePoint.rotation);
        
            // Animation and VFX
            _animator.SetTrigger("shoot");
            if (_vfxShootEffect)
            {
                Instantiate(_vfxShootEffect, firePointVFX.forward, firePointVFX.rotation);
            }
            
            if(_stopWithAnimation)
                return;

            // Start Coroutine
            _shootEnumerator = WaitForCooldown(cooldown);
            StartCoroutine(_shootEnumerator);
        }
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
            _damage = projectileScriptableObject.damage;
            _force = projectileScriptableObject.bulletForce;
            _vfxShootEffect = projectileScriptableObject.vfxShootEffect;
            _explosionRadius = projectileScriptableObject.explosionRadius;
            _bulletLifetime = projectileScriptableObject.bulletLifeTime;
            _bulletPrefab = projectileScriptableObject.bulletPrefab;
            _stopWithAnimation = projectileScriptableObject.stopWithCooldown;
        }
    }
}