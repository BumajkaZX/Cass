namespace Cass.Items.Guns
{
    using UnityEngine.Pool;
    using UnityEngine;
    using System.Threading.Tasks;
    using UniRx;
    using System;

    public class GunController : MonoBehaviour
    {
        [SerializeField]
        private Transform _shootPosition = default;

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

            if (_bulletsPool != null)
            {
                _bulletsPool.Clear();
            }
            CreateBulletPool();
            _disposable.Clear();

            return Task.CompletedTask;
        }

        /// <summary>
        /// return recoil
        /// </summary>
        /// <returns></returns>
        public float Shoot()
        {
            if (_disposable.Count != 0)
            {
                return 0;
            }

            var velocities = _gun.Shoot(transform.forward);
            for (int i = 0; i < _gun.BulletsPerShoot; i++)
            {
                var bullet = _bulletsPool.Get();
                bullet.velocity = velocities[i];
                ReturnParticle(bullet, _gun.Range / _gun.BulletSpeed);
            }

            Observable.Timer(TimeSpan.FromSeconds(_gun.ShootSpeed)).Subscribe(_ => _disposable.Clear()).AddTo(_disposable);

            return _gun.Recoil;
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
            _bulletsPool = 
            new ObjectPool<Rigidbody>(() => 
            {
               return Instantiate(_gun.PrefabTransform).GetComponent<Rigidbody>();
            },
                bullet => 
                {
                    bullet.transform.position = _shootPosition.position;
                    bullet.velocity = Vector3.zero;
                    bullet.gameObject.SetActive(true);
                },
                bullet => bullet.gameObject.SetActive(false),
                null,
                false,
                _gun.BulletsPerShoot * 10);
    }
}
