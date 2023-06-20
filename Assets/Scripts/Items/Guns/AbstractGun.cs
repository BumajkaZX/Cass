namespace Cass.Items.Guns
{
    using UnityEngine;
    using System.Collections.Generic;

    public abstract class AbstractGun : PlayerItem
    {
        public Rigidbody BulletPrefab => _bulletPrefab;

        public Transform GunSpawnPosition => _gunSpawnPos;

        public float ShootSpeed => _shootSpeed;

        public int BulletsPerShoot => _bulletsPerShoot;

        public float Range => _range;

        public float Damage => _damage;

        [SerializeField]
        protected Rigidbody _bulletPrefab = default;

        [SerializeField]
        protected Transform _gunSpawnPos = default;

        [SerializeField, Min(0)]
        protected float _shootSpeed = default;

        [SerializeField, Min(0)]
        protected int _bulletsPerShoot = default;

        [SerializeField, Min(0)]
        protected float _range = default;

        [SerializeField, Min(0)]
        protected float _damage = default;

        /// <summary>
        /// Shoot
        /// </summary>
        /// <returns>bullet velocity/ies </returns>
        public abstract List<Vector3> Shoot();

    }
}
