namespace Cass.Items.Guns
{
    using System.Collections;
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public class BulletLifetimeController : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody _rb = default;

        [SerializeField]
        private Transform _bulletTransform = default;

        [SerializeField]
        private ParticleSystem _endParticles = default;

        [SerializeField]
        private Vector3 _endScale = default;

        private Vector3 _defaultScale = default;

        private Collider _collider = default;

        public void Init()
        {
            _defaultScale = transform.localScale;
            _collider = GetComponent<Collider>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            StopAllCoroutines();
            _bulletTransform.gameObject.SetActive(false);
            _rb.velocity = Vector3.zero;
            _endParticles.Play();
        }

        public void Shoot(Vector3 velocity, float lifetime)
        {
            _rb.velocity = velocity;
            _bulletTransform.gameObject.SetActive(true);
            StartCoroutine(BulletLife(lifetime));
        }

        private IEnumerator BulletLife(float lifetime)
        {
            float time = 0;

            while(time < lifetime)
            {
                transform.localScale = Vector3.Lerp(_defaultScale, _endScale, time / lifetime);

                time += Time.deltaTime;

                yield return null;
            }

            _bulletTransform.gameObject.SetActive(false);
            _rb.velocity = Vector3.zero;
            _endParticles.Play();  
        }

        public void Stop()
        {
            StopAllCoroutines();
            _endParticles.Stop();
            gameObject.SetActive(false);
            _rb.velocity = Vector3.zero;
        }
    }
}
