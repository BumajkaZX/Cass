namespace Cass.Items.Guns
{
    using UnityEngine;
    using System.Collections.Generic;
    using FMODUnity;

    public abstract class AbstractGun : PlayerItem
    {
        public float ShootSpeed => _shootSpeed;

        public float BulletSpeed => _bulletSpeed;

        public float Recoil => _recoil;

        public int BulletsPerShoot => _bulletsPerShoot;

        public float Range => _range;

        public float Damage => _damage;

        public EventReference[] ShotSound => _shotSound;

        [SerializeField, Min(0)]
        protected float _shootSpeed = default;

        [SerializeField, Min(0)]
        protected int _bulletsPerShoot = default;

        [SerializeField, Min(0)]
        protected float _range = default;

        [SerializeField, Min(0)]
        protected float _damage = default;

        [SerializeField, Min(0)]
        protected float _bulletSpeed = default;

        [SerializeField, Min(0.01f)]
        protected float _scatter = default;

        [SerializeField, Min(0)]
        protected float _recoil = default;

        [SerializeField]
        protected EventReference[] _shotSound = default;

        /// <summary>
        /// Shoot
        /// </summary>
        /// <returns>bullet velocity/ies </returns>
        public abstract List<Vector3> Shoot(Vector3 forward);

    }
}
