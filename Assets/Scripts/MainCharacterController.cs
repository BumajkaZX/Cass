namespace Cass.Character
{
    using UnityEngine;
    using UniRx;
    using System;
    using UnityEngine.Pool;
    using FMODUnity;

    /// <summary>
    /// Character Controller
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class MainCharacterController : MonoBehaviour
    {
        #region params

        private const float ZERO_ACCURACY = 0.005f;

        private const float MOVE_MULTIPLY = 10f;

        private const float SCALE_TO_LOCAL = 10f;

        [SerializeField, Range(0, 10)]
        private float _moveSpeed = 1f;

        [SerializeField, Range(0, 1000)]
        private float _jumpForce = 1f;

        [SerializeField, Range(0, 10)]
        private float _distanceToGround = 0.5f;

        [SerializeField, Range(0, 1)]
        private float _gravityUpScale = 0.5f;

        [SerializeField, Range(0, 100)]
        private float _gravityDownScale = 2f;

        [SerializeField, Range(0, 10)]
        private float _gravity = 9.81f;

        [SerializeField, Range(0.001f, 0.2f)]
        private float _gravityDownThreshold = 0.1f;

        [Space(20)]

        [SerializeField]
        private Transform _springTransform = default;

        [SerializeField]
        private Vector3 _scaleDown = default;

        [SerializeField]
        private Vector3 _scaleUp = default;

        [SerializeField, Range(0, 5)]
        private float _scale—oefficient = 1f;

        [Space(20)]

        [SerializeField, Range(0, 10)]
        private float _slidePower = 2f;

        [SerializeField, Range(0, 10)]
        private int _dashCount = 1;

        [SerializeField, Range(0, 10)]
        private float _dashRechargeTime = 2f;

        [SerializeField, Range(0, 100)]
        private float _dashForce = 10f;

        [SerializeField, Range(0, 2)]
        private float _dashUp = 1f;

        [SerializeField]
        private int _dashPoolCount = default;

        [SerializeField]
        private ParticleSystem _dashParticles = default;

        [Space(20)]

        [SerializeField]
        private StudioEventEmitter _dashSound = default;

        private ObjectPool<ParticleSystem> _dashPool = default;

        private Rigidbody _rb = default;

        private MainCharacterInput _inputActions = default;

        private Vector3 _defaultScale = default;

        private int _dashAvailable = default;

        private bool _isGrounded = true;

        #endregion

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            CreateDashPool();
        }
        private void Start()
        {
            _defaultScale = transform.localScale;
            _dashAvailable = _dashCount;

            _inputActions = new MainCharacterInput();
            _inputActions.Main.Enable();

            //Grounded
            Observable.EveryUpdate().Subscribe(_ => _isGrounded = Physics.Raycast(transform.position, -Vector3.up, _distanceToGround)).AddTo(this);

            //Movement
            Observable.EveryUpdate().Select(x => _inputActions.Main.Move.ReadValue<Vector2>()).Subscribe(moveInput =>
            {
                if (!isActiveAndEnabled)
                {
                    return;
                }

                if (moveInput == Vector2.zero)
                {
                    if (_springTransform != null)
                    {
                        Vector3 relativePosition = _springTransform.position - transform.position;
                        relativePosition = new Vector3(relativePosition.x, 0, relativePosition.z);
                        _rb.AddForce(relativePosition * _slidePower, ForceMode.Acceleration);
                    }
                    return;
                }

                Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
                _rb.AddForce(_moveSpeed * MOVE_MULTIPLY * Time.deltaTime * move, ForceMode.VelocityChange);
            }).AddTo(this);

            //Jump
            Observable.EveryUpdate().Where(_ => _inputActions.Main.Jump.WasPressedThisFrame()).Subscribe(_ =>
            {
                if (!isActiveAndEnabled || !_isGrounded)
                {
                    return;
                }

                _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Acceleration);
            }).AddTo(this);

            //Scale control
            if (_springTransform != null)
            {
                Observable.EveryFixedUpdate().Subscribe(_ =>
                {
                    if (!isActiveAndEnabled)
                    {
                        return;
                    }

                    Vector3 relativePosition = _springTransform.position - transform.position;
                    float interpolator = Mathf.Abs(relativePosition.y * SCALE_TO_LOCAL * _scale—oefficient);
                    Vector3 newScale = _defaultScale;
                    if (relativePosition.y < 0)
                    {
                        newScale = Vector3.LerpUnclamped(_defaultScale, _scaleDown * _defaultScale.x, interpolator);
                    }
                    else if (relativePosition.y > 0)
                    {
                        newScale = Vector3.LerpUnclamped(_defaultScale, _scaleUp * _defaultScale.x, interpolator);
                    }
                    transform.localScale = newScale;
                }).AddTo(this);
            }

            //Custom gravity
            Observable.EveryFixedUpdate().Subscribe(_ =>
            {
                if (!isActiveAndEnabled)
                {
                    return;
                }

                float gravity = _gravity;

                if (_rb.velocity.y > ZERO_ACCURACY)
                {
                    gravity *= _gravityUpScale;
                }
                else if (_rb.velocity.y < -_gravityDownThreshold)
                {
                    gravity *= _gravityDownScale;
                }

                _rb.AddForce(-Vector3.up * gravity, ForceMode.Acceleration);
            }).AddTo(this);

            //Dash
            Observable.EveryUpdate().Where(_ => _inputActions.Main.Dash.WasPressedThisFrame() && _dashAvailable > 0 && !_isGrounded).Select(moveInput => _inputActions.Main.Move.ReadValue<Vector2>()).Subscribe(moveInput =>
            {
                if (!isActiveAndEnabled || moveInput == Vector2.zero)
                {
                    return;
                }

                //Set particle rotation
                _dashPool.Get(out ParticleSystem particle);
                var lookPos = new Vector3(transform.position.x - moveInput.x, transform.position.y, transform.position.z - moveInput.y);
                particle.transform.LookAt(lookPos);
                ReturnParticle(particle);

                DashUse();
       
                Vector3 dashDir = new Vector3(moveInput.x, _dashUp, moveInput.y) * _dashForce;
                _rb.AddForce(dashDir, ForceMode.VelocityChange);
            }).AddTo(this);
        }
        private void ReturnParticle(ParticleSystem particle)
        {
            CompositeDisposable disParticles = new CompositeDisposable();
            Observable.Timer(TimeSpan.FromSeconds(_dashParticles.main.duration)).Subscribe(_ =>
            {
                _dashPool.Release(particle);
                disParticles.Clear();
            }).AddTo(disParticles);
        }
        private void DashUse()
        {
            CompositeDisposable dis = new CompositeDisposable();
            _dashAvailable--;

            _dashSound.Play();

            Observable.Timer(TimeSpan.FromSeconds(_dashRechargeTime)).Subscribe(_ =>
            {
                _dashAvailable++;
                dis.Clear();
            }).AddTo(dis);
        }
        private void CreateDashPool() =>
                _dashPool = new ObjectPool<ParticleSystem>(() => Instantiate(_dashParticles),
                particle => {
                    particle.transform.position = transform.position;
                    particle.gameObject.SetActive(true);
                    particle.Play();
                },
                particle => particle.gameObject.SetActive(false),
                null,
                false,
                _dashPoolCount);
        private void OnDestroy()
        {
            _inputActions.Main.Disable();
            _dashPool.Clear();
        }
    }
}
