namespace Cass.Guns
{
    using UnityEngine;
    using System.Collections.Generic;

    public abstract class AbstractGun : ScriptableObject
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id => _id;

        [SerializeField]
        protected string _id = default;

        [SerializeField]
        protected Transform _gunPrefab = default;

        [SerializeField]
        protected Transform _bulletPrefab = default;

        [SerializeField, Min(0)]
        protected float _shootSpeed = default;

        [SerializeField, Min(0)]
        protected int _bulletPerShoot = default;

        [SerializeField, Min(0)]
        protected float _range = default;

        [SerializeField, Min(0)]
        protected float _damage = default;

        /// <summary>
        /// Shoot
        /// </summary>
        /// <returns>bullet velocity/ies </returns>
        protected abstract List<Vector3> Shoot();

    }
}