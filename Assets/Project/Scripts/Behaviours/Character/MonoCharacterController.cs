using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Unity.VisualScripting;
using UnityEngine;

public class MonoCharacterController : MonoBehaviour
{
    private MovementController _movementController;

    private AnimationController _animationController;

    private InteractionController _interactionController;

    // TODO: пока только с поддержкой анимаций и пряток. доработать
    private CharacterStateMachine _stateMachine = new(CharacterStateMachine.State.Idle);

    private Vector3 _lastMovementVector = Vector3.zero;

    public Vector3 LastMovementVector => _lastMovementVector;

    public Animator Animator => _animationController.Animator;

    public CharacterStateMachine.State State => _stateMachine.CurrentState;

    public event Action<CharacterStateMachine.State> StateChanged;

    public MovementController MovementController => _movementController;
    public AnimationController AnimationController => _animationController;
    public InteractionController InteractionController => _interactionController;

    public IUniTaskCoroutine MovementCoroutine => _movementController.MovementCoroutine;
    public IUniTaskCoroutine DirectionMonitoringCoroutine => _movementController.DirectionMonitoringCoroutine;
    public IUniTaskCoroutine IsMovingMonitoringCoroutine => _movementController.IsMovingMonitoringCoroutine;

    public bool IsAnimating => _animationController.IsAnimating;

    public CharacterController CharacterController => _movementController.CharacterController;
    public Vector3 VelocityVector => _movementController.VelocityVector;
    public float Velocity { get => _movementController.Velocity; set => _movementController.Velocity = value; }
    public bool IsMoving => _movementController.IsMoving;
    public bool CanMove => _movementController.CanMove;

    #region State Variables

    private bool isSprinting;

    private bool isMove;

    #endregion

    #region Govno

    public void Hide()
    {
        body.SetActive(false);
        unityCharacterController.enabled = false;
        _movementController.Disable();
    }

    public void UnHide()
    {
        body.SetActive(true);
        unityCharacterController.enabled = true;
        _movementController.Enable();
    }

    #endregion

    private void Configure()
    {
        _stateMachine.Subscribe(CharacterStateMachine.State.Moving,
        () =>
        {

        },
        () =>
        {

        });

        _stateMachine.Subscribe(CharacterStateMachine.State.Idle,
        () =>
        {
            animator.SetTrigger("Idle");
        },
        () =>
        {

        });

        _stateMachine.Subscribe(CharacterStateMachine.State.Walking,
        () =>
        {
            Velocity = 0.1f;
            animator.SetTrigger("Walk");
        },
        () =>
        {

        });

        _stateMachine.Subscribe(CharacterStateMachine.State.Running,
        () =>
        {
            Velocity = 0.2f;
            animator.SetTrigger("Run");
        },
        () =>
        {
            Velocity = 0.1f;
        });

        _stateMachine.Subscribe(CharacterStateMachine.State.Animation,
        () => _movementController.Disable(),
        () =>
        {
            _movementController.Enable();
            UpdateMoveState(isMove, isSprinting);
        });

        _stateMachine.Subscribe(CharacterStateMachine.State.Hiding,
        () =>
        {
            body.SetActive(false);
            unityCharacterController.enabled = false;
            _movementController.Disable();
            _interactionController.Disable();
        },
        () =>
        {
            body.SetActive(true);
            unityCharacterController.enabled = true;
            _movementController.Enable();
            _interactionController.Enable();
        });
    }

    #region Unity internal

    [Header("States")]
    [SerializeField] private CharacterStateMachine.State _state;

    [Button]
    private void Fire(int trigger)
    {
        _stateMachine.TryFire((CharacterStateMachine.Trigger)trigger);
    }

    [Header("Movement Settings")]
    [InspectorName("Velocity")]
    [SerializeField] private float MovementVelocity;
    [SerializeField] private bool BlockX;
    [SerializeField] private bool BlockY;

    [Header("Interaction Settings")]
    [SerializeField] private Vector3 interactionOffset;
    [SerializeField] private float interactionRadius;
    [SerializeField] private LayerMask FindingMask;
    [SerializeField] private LayerMask interactionObstacleLayerMask;

    [InspectorName("Draw Gizmos")]
    [SerializeField]
    private bool interactionDrawGizmos = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Space]
    [SerializeField] private CharacterController unityCharacterController;
    [SerializeField] private Dictionary<string, IScriptableAnimation> animationLibrary;

    [SerializeField] private GameObject body;

    private void Awake()
    {
        _animationController = new AnimationController(animationLibrary, animator);
        _movementController = new MovementController(unityCharacterController);
        _interactionController = new InteractionController(transform);

        // Передать значения из инспектора в контроллеры
        _movementController.Velocity = Velocity;
        _interactionController.Radius = interactionRadius;
        _interactionController.Mask = FindingMask;

        _stateMachine.StateChanged += (newState) => StateChanged?.Invoke(newState);
        Configure();
    }

    private void Update()
    {
        _state = _stateMachine.CurrentState;
    }

    private void OnValidate()
    {
        if (unityCharacterController == null)
        {
            unityCharacterController = GetComponent<CharacterController>();
        }

        if (animationLibrary == null)
        {
            animationLibrary = new Dictionary<string, IScriptableAnimation>();
        }

        // Обновить значения в контроллерах при изменении в инспекторе
        if (_movementController != null)
            _movementController.Velocity = MovementVelocity;

        if (_interactionController != null)
        {
            _interactionController.Radius = interactionRadius;
            _interactionController.Mask = FindingMask;
        }

        if (_animationController != null)
        {
            _animationController.ValidateAnimator(animator);
        }
    }

    private void OnEnable()
    {
        _movementController?.Enable();
        _interactionController?.Enable();

        _movementController.OnPositionChanged += (pos) => _interactionController.UpdateInteractables(pos + interactionOffset, obstacleLayerMask: interactionObstacleLayerMask);
    }

    private void OnDisable()
    {
        _movementController?.Disable();
        _interactionController?.Disable();
    }

    private void OnDestroy()
    {
        _movementController?.Dispose();
        _interactionController?.Dispose();
    }
    #endregion

    private void UpdateScaleByMovementVector(Vector3 movementVector)
    {
        if (movementVector == Vector3.zero)
            return;

        Vector3 scale = body.transform.localScale;
        scale.x = movementVector.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        body.transform.localScale = scale;
    }

    private void UpdateMoveState(bool isMoving, bool isSprinting)
    {
        if (isMoving)
        {
            if (isSprinting)
                _stateMachine.TryFire(CharacterStateMachine.Trigger.StartRunning);
            else
                _stateMachine.TryFire(CharacterStateMachine.Trigger.StartWalking);
        }
        else
        {
            _stateMachine.TryFire(CharacterStateMachine.Trigger.StopMoving);
        }
    }

    public void Sprint(bool state)
    {
        isSprinting = state;
        UpdateMoveState(isMove, isSprinting);
    }

    public void MoveToDirection(Vector2 direction)
    {
        if (BlockX) direction.x = 0;
        if (BlockY) direction.y = 0;

        _movementController.MoveToDirection(direction);

        Vector3 movementVector3 = new Vector3(direction.x, 0, direction.y);
        _lastMovementVector = movementVector3;

        // TODO: исправить на более подходящую реализацию поворта
        UpdateScaleByMovementVector(_lastMovementVector);

        isMove = direction != Vector2.zero;
        UpdateMoveState(isMove, isSprinting);
    }

    public void MoveFromOffset(Vector3 offset)
    {
        _movementController.MoveFromOffset(offset);

        _lastMovementVector = offset.normalized;

        // TODO: исправить на более подходящую реализацию поворта
        UpdateScaleByMovementVector(_lastMovementVector);
    }

    public void Teleport(Vector3 point)
    {
        _movementController.Teleport(point);
    }

    public void Interact()
    {
        _interactionController.Interact();
    }

    public void InteractWith(InteractableBehaviour interactable)
    {
        if (interactable == null) throw new ArgumentNullException();

        _interactionController.InteractWith(interactable);
    }

    public void FireAnimationTrigger(string key)
    {
        _animationController.FireAnimationTrigger(key);
    }

    public ScriptableAnimationScope PlayAnimation(IScriptableAnimation animation)
    {
        var scope = _animationController.PlayAnimation(animation);
        if (scope != null)
        {
            scope.OnStart += () => _stateMachine.TryFire(CharacterStateMachine.Trigger.StartAnimation);
            scope.Completed += () => _stateMachine.TryFire(CharacterStateMachine.Trigger.StopAnimation);
            scope.Start();
        }

        return scope;
    }

    public IScriptableAnimationScope PlayAnimation(string key)
    {
        var scope = _animationController.PlayAnimation(key);
        if (scope != null)
        {
            scope.OnStart += () => _stateMachine.TryFire(CharacterStateMachine.Trigger.StartAnimation);
            scope.Completed += () => _stateMachine.TryFire(CharacterStateMachine.Trigger.StopAnimation);
            scope.Start();
        }

        return scope;
    }

    public IScriptableAnimationScope PlayerAnimationWithTransform(IScriptableAnimation<Transform> animation, bool @override = false)
    {
        if (@override == false && _animationController.IsAnimating) throw new Exception("In animating.");

        var wrapper = new SciptableAnimationWrapper<Transform>(animation, this.transform);
        var scope = _animationController.PlayAnimation(wrapper, @override);
        if (scope != null)
        {
            scope.OnStart += () => _stateMachine.TryFire(CharacterStateMachine.Trigger.StartAnimation);
            scope.Completed += () => _stateMachine.TryFire(CharacterStateMachine.Trigger.StopAnimation);
            scope.Start();
        }

        return scope;
    }

    public IScriptableAnimationScope PlayerAnimationWithAnimatorAndTransform(IScriptableAnimation<(Animator, Transform)> animation, bool @override = false)
    {
        if (@override == false && _animationController.IsAnimating) throw new Exception("In animating.");

        var wrapper = new SciptableAnimationWrapper<(Animator, Transform)>(animation, (this.animator, this.transform));
        var scope = _animationController.PlayAnimation(wrapper, @override);
        if (scope != null)
        {
            scope.OnStart += () => _stateMachine.TryFire(CharacterStateMachine.Trigger.StartAnimation);
            scope.Completed += () => _stateMachine.TryFire(CharacterStateMachine.Trigger.StopAnimation);
            scope.Start();
        }

        return scope;
    }

    public bool TryFireTrigger(CharacterStateMachine.Trigger trigger) => _stateMachine.TryFire(trigger);

    private void OnDrawGizmos()
    {
        if (_interactionController != null && interactionDrawGizmos)
            _interactionController.DrawGizmos(transform.position + interactionOffset, Color.yellow);

        if (_movementController != null)
        {
            _movementController.DrawInputAndDirection(transform.position, Color.blue, Color.red);
        }
    }
}

public class SciptableAnimationWrapper<TContext> : IScriptableAnimation
{
    public IScriptableAnimation<TContext> Animation { get; }
    public TContext Context { get; }
    public float Duration => Animation.Duration;

    public SciptableAnimationWrapper(IScriptableAnimation<TContext> animation, TContext context)
    {
        Animation = animation ?? throw new ArgumentNullException(nameof(animation));
        Context = context;
    }

    public async UniTask Run(CancellationToken token = default)
    {
        await Animation.Run(Context, token);
    }
}