namespace Cass.Character
{
    using UnityEngine;
    using UniRx;
    using System;
    using UnityEngine.Pool;
    using FMODUnity;
    using Unity.Netcode;

    /// <summary>
    /// Character Controller
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(Collider))]
    public class MainCharacterController : NetworkBehaviour
    {
        #region params

        private const float ZERO_ACCURACY = 0.005f;

        private const float SCALE_TO_LOCAL = 10f;

        private const float GRAVITY_MULTIPLY = 100f;

        /// <summary>
        /// For camera
        /// </summary>
        public static ReactiveProperty<Transform> TargetTransform = new ReactiveProperty<Transform>();

        public ReactiveProperty<int> DashAvailableCount => _dashAvailable;

        [SerializeField]
        private Transform _targetTransform = default;

        [SerializeField, Header("Root object to change scale/rotation")]
        private Transform _rootChangeTransform = default;
            
        [SerializeField]
        private bool _jumpEnable = false;

        [SerializeField]
        private bool _dashEnable = false;

        [SerializeField]
        private bool _isOtherSide = false;

        [Space(20)]

        [SerializeField, Range(0, 100)]
        private float _moveSpeed = 1f;

        [SerializeField, Range(0, 1)]
        private float _jumpForce = 1f;

        [SerializeField, Range(0, 10)]
        private float _distanceToGround = 0.5f;

        [SerializeField]
        private LayerMask _groundLayers = default;

        [SerializeField, Range(0, 2)]
        private float _gravityUpScale = 0.5f;

        [SerializeField, Range(0, 3)]
        private float _gravityDownScale = 1f;

        [SerializeField, Range(0, 2)]
        private float _gravity = 0.5f;

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

        [SerializeField, Range(0, 100)]
        private float _rotationCoefficient = 2f;

        [SerializeField, Range(1, 10)]
        private float _rotationRecoverySpeed = 2f;

        [Space(20)]

        [SerializeField, Range(0, 10)]
        private float _slidePower = 2f;

        [SerializeField, Range(0, 1)]
        private float _dashSeconds = 0.2f;

        [SerializeField, Range(0, 10)]
        private int _dashCount = 1;

        [SerializeField, Range(0, 10)]
        private float _dashRechargeTime = 2f;

        [SerializeField, Range(1, 4)]
        private float _dashForce = 1f;

        [SerializeField]
        private int _dashPoolCount = default;

        [SerializeField]
        private MultiParticlesPlayer _dashParticles = default;

        [SerializeField]
        private int _landPoolCount = default;

        [SerializeField]
        private MultiParticlesPlayer _landParticles = default;

        [SerializeField]
        private int _jumpPoolCount = default;

        [SerializeField]
        private MultiParticlesPlayer _jumpParticles = default;

        [Space(20)]

        [SerializeField]
        private StudioEventEmitter _dashSound = default;

        [SerializeField]
        private StudioEventEmitter _jumpSound = default;

        [SerializeField]
        private StudioEventEmitter _landSound = default;

        private ObjectPool<MultiParticlesPlayer> _dashPool = default;

        private ObjectPool<MultiParticlesPlayer> _landPool = default;

        private ObjectPool<MultiParticlesPlayer> _jumpPool = default;

        private CharacterController _chController = default;

        private MainCharacterInput _inputActions = default;

        private Vector3 _defaultScale = default;

        private ReactiveProperty<int> _dashAvailable = new ReactiveProperty<int>();

        private CompositeDisposable _disposables = new CompositeDisposable();

        private bool _isGrounded = true;

        private Vector3 _veloctityY = default;

        private Vector3 _velocityXZ = default;

        private bool _isUseDash = false;

        #endregion

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                Destroy(this);
                return;
            }

            TargetTransform.SetValueAndForceNotify(_targetTransform);
        }

        private void Awake()
        { 
            _chController = GetComponent<CharacterController>();

            if (_dashEnable)
            {
                CreateParticlesPool(ref _dashPool, _dashParticles, _dashPoolCount);
            }

            CreateParticlesPool(ref _landPool, _landParticles, _landPoolCount);

            if (_jumpEnable)
            {
                CreateParticlesPool(ref _jumpPool, _jumpParticles, _jumpPoolCount);
            }
        }
        private void Start()
        {
            _defaultScale = _rootChangeTransform.localScale;
            _dashAvailable.Value = _dashCount;

            _inputActions = new MainCharacterInput();
            _inputActions.Main.Enable();


            AddGroundedControl();
            AddGravity();
            AddMovement();
            AddScaleControl();
            AddJump();
            AddDash();


            AddVerticalVelocityControl();
            AddHorizontalVelocityControl();
        }

        private void AddHorizontalVelocityControl()
        {
            //TODO: ·Îˇ, ÌÛ Ì‡‰Ó Ó„‡ÌË˜ËÚÂÎ¸ 200% ÔÓÔÓÁÊÂ Ì‡ÍËÌÛ, Ì‡‰Â˛Ò¸))00000))))
            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (!isActiveAndEnabled)
                {
                    return;
                }

                _chController.Move(_velocityXZ);

                _velocityXZ = Vector3.zero;

            }).AddTo(_disposables);
        }

        private void AddVerticalVelocityControl()
        {
            Observable.EveryUpdate().Subscribe(_ => 
            {
                if(!isActiveAndEnabled)
                {
                    return;
                }

                _chController.Move(_veloctityY);

            }).AddTo(_disposables);
        }

        private void AddGroundedControl()
        {
            Observable.EveryFixedUpdate().Subscribe(_ =>
            {
                bool currentGrounded = _isGrounded;

                _isGrounded = Physics.Raycast(transform.position, -Vector3.up, _distanceToGround, _groundLayers, QueryTriggerInteraction.Ignore);

                if (!currentGrounded && _isGrounded)
                {
                    _landPool.Get(out MultiParticlesPlayer particle);
                    ReturnParticle(particle, _landPool);

                    _veloctityY = Vector3.zero;


                    //_landSound.Play();
                }

            }).AddTo(_disposables);
        }
        private void AddMovement()
        {
            //Movement
            Observable.EveryUpdate().Select(x => _inputActions.Main.Move.ReadValue<Vector2>()).Subscribe(moveInput =>
            {
                if (!isActiveAndEnabled)
                {
                    return;
                }

               Vector3 relativePosition = _springTransform.position - transform.position;

               float coef = _rotationCoefficient * SCALE_TO_LOCAL;

               Vector3 rotation = new Vector3(_rootChangeTransform.localRotation.eulerAngles.x, _rootChangeTransform.localRotation.eulerAngles.y, -relativePosition.z * coef);

               _rootChangeTransform.localRotation = Quaternion.Lerp(_rootChangeTransform.localRotation, Quaternion.Euler(rotation), Time.deltaTime * _rotationRecoverySpeed);

                if (moveInput == Vector2.zero)
                {
                    if (_springTransform != null)
                    {
                        relativePosition = new Vector3(relativePosition.x, 0, relativePosition.z);
                        _velocityXZ += relativePosition * _slidePower;
                    }
                    return;
                }

                Vector3 move = new Vector3(_isOtherSide ? -moveInput.y : moveInput.y, 0, _isOtherSide ? moveInput.x : -moveInput.x);

                float dashPower = _isUseDash ? _dashForce : 1;

                _velocityXZ += _moveSpeed * Time.deltaTime * move * dashPower;

                _rootChangeTransform.localRotation = Quaternion.Euler(_rootChangeTransform.localRotation.eulerAngles.x, Quaternion.LookRotation(move, _rootChangeTransform.forward).eulerAngles.y, _rootChangeTransform.localRotation.eulerAngles.z);

            }).AddTo(_disposables);
        }
        private void AddScaleControl()
        {
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
                    _rootChangeTransform.localScale = newScale;

                }).AddTo(_disposables);
            }
        }
        private void AddGravity()
        {
            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (!isActiveAndEnabled || _isGrounded)
                {
                    return;
                }

                float gravity = _gravity / GRAVITY_MULTIPLY;

                if (_chController.velocity.y > ZERO_ACCURACY)
                {
                    gravity *= _gravityUpScale / GRAVITY_MULTIPLY;
                }
                else if (_chController.velocity.y < -_gravityDownThreshold)
                {
                    gravity *= _gravityDownScale / GRAVITY_MULTIPLY;
                }

               _veloctityY -= Vector3.up * gravity;

            }).AddTo(_disposables);
        }
        private void AddDash()
        {
            if (_dashEnable)
            {
                CompositeDisposable rechargeTimer = new CompositeDisposable();
                //Dash
                Observable.EveryUpdate().Where(_ => _inputActions.Main.Dash.WasPressedThisFrame() && _dashAvailable.Value > 0 && !_isUseDash).Select(moveInput => _inputActions.Main.Move.ReadValue<Vector2>()).Subscribe(moveInput =>
                {
                    if (!isActiveAndEnabled || moveInput == Vector2.zero)
                    {
                        return;
                    }

                    //Set particle rotation
                    _dashPool.Get(out MultiParticlesPlayer particle);
                    var lookPos = new Vector3(transform.position.x - moveInput.y, transform.position.y, transform.position.z + moveInput.x);
                    particle.transform.LookAt(lookPos);
                    ReturnParticle(particle, _dashPool);

                    DashUse();

                    _isUseDash = true;

                    Observable.Timer(TimeSpan.FromSeconds(_dashSeconds)).Subscribe(_ =>
                    {
                        _isUseDash = false;
                        rechargeTimer.Clear();
                    }).AddTo(rechargeTimer);

                }).AddTo(_disposables);
            }
        }
        private void AddJump()
        {
            if (_jumpEnable)
            {
                
                //Jump
                Observable.EveryUpdate().Where(_ => _inputActions.Main.Jump.WasPressedThisFrame()).Subscribe(_ =>
                {
                    if (!isActiveAndEnabled || !_isGrounded)
                    {
                        return;
                    }


                    _jumpPool.Get(out MultiParticlesPlayer particle);
                    ReturnParticle(particle, _jumpPool);

                    _jumpSound.Play();

                    _veloctityY = Vector3.up * _jumpForce;
                   
                }).AddTo(_disposables);
            }
        }
        private void ReturnParticle(MultiParticlesPlayer particle, ObjectPool<MultiParticlesPlayer> pool)
        {
            CompositeDisposable disParticles = new CompositeDisposable();
            Observable.Timer(TimeSpan.FromSeconds(particle.Duration)).Subscribe(_ =>
            {
                pool.Release(particle);
                disParticles.Clear();
            }).AddTo(disParticles);
        }
        private void DashUse()
        {
            CompositeDisposable dis = new CompositeDisposable();
            _dashAvailable.Value--;

            _dashSound.Play();

            Observable.Timer(TimeSpan.FromSeconds(_dashRechargeTime)).Subscribe(_ =>
            {
                _dashAvailable.Value++;
                dis.Clear();
            }).AddTo(dis);
        }
        private void CreateParticlesPool(ref ObjectPool<MultiParticlesPlayer> pool, MultiParticlesPlayer particlePrefab, int poolCount) =>
             pool = new ObjectPool<MultiParticlesPlayer>(() => Instantiate(particlePrefab),
                particle =>
                {
                    particle.transform.position = transform.position;
                    particle.gameObject.SetActive(true);
                    particle.Play();
                },
                particle => particle.gameObject.SetActive(false),
                null,
                false,
                poolCount);
        public override void OnDestroy()
        {
            base.OnDestroy();

            if (!IsOwner)
            {
                return;
            }

            _inputActions.Main.Disable();

            _inputActions.Dispose();

            if (_dashEnable)
            {
                _dashPool.Clear();
            }

            if (_jumpEnable)
            {
                _jumpPool.Clear();
            }

            _landPool.Clear();

            _disposables.Clear();
        }
    }
}
