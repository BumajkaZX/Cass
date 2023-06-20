namespace Cass.Items.Guns
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine.Pool;
    using UnityEngine;
    using System.Threading.Tasks;
    using UniRx;
    using System;

    public class GunController : MonoBehaviour
    {
        [SerializeField]
        private Transform _gunPosition = default;

        private Transform _gunTransform = default;

        private AbstractGun _gun = default;

        private ObjectPool<Rigidbody> _bulletsPool = default;

        private CompositeDisposable _disposable = new CompositeDisposable();

        public Task Init(string gunId)
        {
            if(_gunTransform != null)
            {
                Destroy(_gunTransform.gameObject);
            }
            _gun = ItemsContainer.Instance.Items.Find(_ => _.ItemId == gunId) as AbstractGun;
            _gunTransform = Instantiate(_gun.PrefabTransform, transform);
            _gunTransform.position = _gunPosition.position;
            _bulletsPool.Clear();
            CreateBulletPool();
            _disposable.Clear();

            return Task.CompletedTask;
        }

        public void Shoot()
        {
            var velocities = _gun.Shoot();
            for (int i = 0; i < _gun.BulletsPerShoot; i++)
            {
                var bullet = _bulletsPool.Get();
                bullet.velocity = velocities[i];
                ReturnParticle(bullet, _gun.Range * velocities[i].magnitude * 2);
            }
        }

        private void ReturnParticle(Rigidbody bullet, float returnTime)
        {
            CompositeDisposable disParticles = new CompositeDisposable();
            Observable.Timer(TimeSpan.FromSeconds(returnTime)).Subscribe(_ =>
            {
                if(_bulletsPool == null)
                {
                    disParticles.Clear();
                    return;
                }
                _bulletsPool.Release(bullet);
                disParticles.Clear();
            }).AddTo(disParticles);
        }
        private void CreateBulletPool() =>
            _bulletsPool = new ObjectPool<Rigidbody>(() => Instantiate(_gun.BulletPrefab),
                bullet => 
                {
                    bullet.velocity = Vector3.zero;
                    bullet.gameObject.SetActive(true);
                },
                bullet => bullet.gameObject.SetActive(false),
                null,
                false,
                _gun.BulletsPerShoot * 10);
    }
}
