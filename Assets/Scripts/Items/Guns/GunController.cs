namespace Cass.Items.Guns
{
    using UnityEngine.Pool;
    using UnityEngine;
    using System.Threading.Tasks;
    using UniRx;
    using System;
    using Cass.VibrationManager;
    using FMODUnity;

    public class GunController : MonoBehaviour
    {
        private const float PARTICLE_TIME = 0.4f;

        [SerializeField]
        private Transform _shootPosition = default;

        private Transform _gunTransform = default;

        private StudioEventEmitter _shotPlayer = default;

        private AbstractGun _gun = default;

        private ObjectPool<BulletLifetimeController> _bulletsPool = default;

        private CompositeDisposable _disposable = new CompositeDisposable();

        public Task Init(string gunId, StudioEventEmitter shotPlayer)
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

            _shotPlayer = shotPlayer;

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
                bullet.Shoot(velocities[i], _gun.Range / _gun.BulletSpeed);
                ReturnParticle(bullet, (_gun.Range / _gun.BulletSpeed) + PARTICLE_TIME);
            }

            _shotPlayer.EventReference = _gun.ShotSound[UnityEngine.Random.Range(0, _gun.ShotSound.Length)];

            _shotPlayer.Play();

            VibrationManager.Vibrate(VibrationManager.VibrationPower.Easy, VibrationManager.VibrationType.Shot);

            Observable.Timer(TimeSpan.FromSeconds(_gun.ShootSpeed)).Subscribe(_ => _disposable.Clear()).AddTo(_disposable);

            return _gun.Recoil;
        }

        private void ReturnParticle(BulletLifetimeController bullet, float returnTime)
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
            new ObjectPool<BulletLifetimeController>(() => 
            {
               var bullet = Instantiate(_gun.PrefabTransform).GetComponent<BulletLifetimeController>();
                bullet.Init();
                return bullet;
            },
                bullet => 
                {
                    bullet.transform.position = _shootPosition.position;
                    bullet.gameObject.SetActive(true);
                },
                bullet => bullet.Stop(),
                null,
                false,
                _gun.BulletsPerShoot * 10);
    }
}
