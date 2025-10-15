using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class MonoCharacterController : MonoBehaviour
{
    private MovementController movementController;
    private AnimationController animationController;
    private InteractionController interactionController;

    public IUniTaskCoroutine MovementCoroutine => movementController.MovementCoroutine;
    public IUniTaskCoroutine DirectionMonitoringCoroutine => movementController.DirectionMonitoringCoroutine;
    public IUniTaskCoroutine IsMovingMonitoringCoroutine => movementController.IsMovingMonitoringCoroutine;

    public IUniTaskCoroutine InteractionMonitoringCoroutine => interactionController.MonitoringCoroutine;

    public bool IsAnimating => animationController.IsAnimating;

    public MovementStateMachine MovementStateMachine => movementController.MovementStateMachine;
    public MovementStateMachine.State MovementState => movementController.State;
    public Vector3 VelocityVector => movementController.VelocityVector;
    public float MaxVelocity { get => movementController.MaxVelocity; set => movementController.MaxVelocity = value; }
    public bool IsMoving => movementController.IsMoving;
    public bool CanMove => movementController.CanMove;
    public CharacterController CharacterController => movementController.CharacterController;

    public Animator Animator => animationController.Animator;

    [Header("Movement Settings")]
    [SerializeField] private float inspectorMaxVelocity;

    [Header("Interaction Settings")]
    [SerializeField] private float inspectorRadius;
    [SerializeField] private LayerMask inspectorMask;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Space]
    [SerializeField] private CharacterController unityCharacterController;
    [SerializeField] private Dictionary<string, IScriptableAnimation> animationLibrary;

    private void Awake()
    {
        animationController = new AnimationController(animationLibrary, animator);
        movementController = new MovementController(unityCharacterController);
        interactionController = new InteractionController(transform);

        // Передать значения из инспектора в контроллеры
        movementController.MaxVelocity = inspectorMaxVelocity;
        interactionController.Radius = inspectorRadius;
        interactionController.Mask = inspectorMask;
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
        if (movementController != null)
            movementController.MaxVelocity = inspectorMaxVelocity;

        if (interactionController != null)
        {
            interactionController.Radius = inspectorRadius;
            interactionController.Mask = inspectorMask;
        }

        if (animationController != null)
        {
            animationController.ValidateAnimator(animator);
        }
    }

    private void OnEnable()
    {
        movementController?.Enable();
        interactionController?.Enable();

        MovementStateMachine.OnStateChanged += OnMovementStateMachine_OnStateChanged;
    }

    private void OnMovementStateMachine_OnStateChanged(MovementStateMachine.State state)
    {
        switch (state)
        {
            case MovementStateMachine.State.Idle:
                animationController.FireAnimationTrigger("Idle");
                break;
            case MovementStateMachine.State.Run:
                throw new NotSupportedException();
                break;
            case MovementStateMachine.State.Move:
                animationController.FireAnimationTrigger("Walk");
                break;
        }
    }

    private void OnDisable()
    {
        movementController?.Disable();
        interactionController?.Disable();
    }

    private void OnDestroy()
    {
        movementController?.Dispose();
        interactionController?.Dispose();
    }

    // Методы управления анимацией
    public ScriptableAnimationScope PlayAnimation(string key)
    {
        return animationController.PlayAnimation(key);
    }

    public ScriptableAnimationScope PlayAnimation(IScriptableAnimation animation)
    {
        return animationController.PlayAnimation(animation);
    }

    // Методы движения
    public void MoveToDirection(Vector2 direction)
    {
        movementController.MoveToDirection(direction);
    }

    public void MoveFromOffset(Vector3 offset)
    {
        movementController.MoveFromOffset(offset);
    }

    public void Teleport(Vector3 point)
    {
        movementController.Teleport(point);
    }

    // Методы взаимодействия
    public void Interact()
    {
        interactionController.Interact();
    }

    public void InteractWith(InteractableBehaviour interactable)
    {
        interactionController.InteractWith(interactable);
    }

    public void FireAnimationTrigger(string key)
    {
        animationController.FireAnimationTrigger(key);
    }

    public void PlayerAnimationWithAnimatorAndTransform(IScriptableAnimation<(Animator, Transform)> animation, bool @override = false)
    {
        // TODO: Изменить или доработать отключение контроллера движения. Вызывает баги!
        // Он не должен отключаться либо должен обрабатывать другое поведение во время анимации
        if (animationController.IsAnimating) return;

        movementController.Disable();
        var wrapper = new SciptableAnimationWrapper<(Animator, Transform)>(animation, (this.animator, this.transform));
        var scope = animationController.PlayAnimation(wrapper, @override);
        if (scope != null)
            scope.Completed += () => movementController.Enable();
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


// -----------------------------------------------
#region  Animation Controller
public class AnimationController
{
    private ScriptableAnimationScope _currentScope;

    private readonly Dictionary<string, IScriptableAnimation> _animationLibrary;

    private Animator animator;

    public bool IsAnimating => _currentScope != null;

    public Animator Animator => animator;

    public AnimationController(Dictionary<string, IScriptableAnimation> animationLibrary, Animator animator)
    {
        _animationLibrary = animationLibrary;
        this.animator = animator;

        ValidateAnimator(animator);
    }

    public void ValidateAnimator(Animator animator)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is null.");
            return;
        }

        AnimatorControllerParameter[] parameters = animator.parameters;

        string[] requiredTriggers = { "Idle", "Walk", "Run" };
        foreach (string triggerName in requiredTriggers)
        {
            bool triggerFound = false;
            foreach (var param in parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger && param.name == triggerName)
                {
                    triggerFound = true;
                    break;
                }
            }
            if (!triggerFound)
            {
                Debug.LogWarning($"Animator is missing trigger parameter: '{triggerName}'");
            }
        }
    }

    public ScriptableAnimationScope PlayAnimation(string key, bool @override = false)
    {
        if (_animationLibrary.TryGetValue(key, out var animation))
        {
            var scope = PlayAnimation(animation, @override);
            return scope;
        }
        else
        {
            Debug.LogWarning($"Animation with key '{key}' not found.");
            return null;
        }
    }

    public ScriptableAnimationScope PlayAnimation(IScriptableAnimation animation, bool @override = false)
    {
        if (@override)
        {
            // При override отменяем текущий скоуп и создаём новый
            ClearAnimationScope();

            _currentScope = new ScriptableAnimationScope(animation);
            _currentScope.Completed += ClearAnimationScope;
        }
        else
        {
            if (IsAnimating)
            {
                Debug.LogWarning("Animation is already running");
                return null;
            }
            _currentScope = new ScriptableAnimationScope(animation);
            _currentScope.Completed += ClearAnimationScope;
        }

        return _currentScope;
    }

    private void ClearAnimationScope()
    {
        _currentScope?.Dispose();
        _currentScope = null;
    }

    public void FireAnimationTrigger(string trigger)
    {
        if (IsAnimating) return;

        if (animator == null)
        {
            Debug.LogWarning("Animator is null. Cannot set trigger.");
            return;
        }

        if (string.IsNullOrEmpty(trigger))
        {
            Debug.LogWarning("Trigger string is null or empty.");
            return;
        }

        animator.SetTrigger(trigger);
    }


    public void CancelCurrentAnimation()
    {
        _currentScope?.Cancel();
    }
}

#endregion

#region Movement Controller
public class MovementController : IDisposable
{
    private float _velocity;
    public float Velocty => Velocty;
    private Vector3 _directionVector;
    private Vector2 _inputVector;

    private MovementStateMachine _movementStateMachine;

    private CharacterController _characterController;

    private Vector3 Position => _characterController.transform.position;
    private Vector3 _velocityVector;

    private float _maxVelocity = 0.2f;

    private PlayerLoopTiming _timing = PlayerLoopTiming.FixedUpdate;

    private UniTaskCoroutine _movementCoroutine;
    private UniTaskCoroutine _directionMonitoringCoroutine;
    private UniTaskCoroutine _isMovingMonitoringCoroutine;

    public MovementStateMachine MovementStateMachine => _movementStateMachine;
    public MovementStateMachine.State State => _movementStateMachine.CurrentState;

    // Публичный доступ к корутинам
    public UniTaskCoroutine MovementCoroutine => _movementCoroutine;
    public UniTaskCoroutine DirectionMonitoringCoroutine => _directionMonitoringCoroutine;
    public UniTaskCoroutine IsMovingMonitoringCoroutine => _isMovingMonitoringCoroutine;

    public float Velocity
    {
        get => _velocity;
        private set => _velocity = value;
    }

    public Vector3 VelocityVector
    {
        get => _velocityVector;
        set => _velocityVector = value;
    }

    public float MaxVelocity
    {
        get => _maxVelocity;
        set => _maxVelocity = value;
    }

    public bool IsMoving
    {
        get => _velocity != 0;
    }

    public bool CanMove
    {
        get => _characterController.isGrounded;
    }


    public CharacterController CharacterController => _characterController;

    public event Action<MovementStateMachine.State> OnStateChanged;
    public event Action<Vector3> OnPositionChanged;

    public MovementController(CharacterController characterController)
    {
        _characterController = characterController;

        _directionMonitoringCoroutine = new UniTaskCoroutine(DirectionMonitoringTask);
        _movementCoroutine = new UniTaskCoroutine(MovementTask);
        _isMovingMonitoringCoroutine = new UniTaskCoroutine(IsMovingMonitoringTask);

        _movementStateMachine = new MovementStateMachine();
    }

    private void InternalMove()
    {
        if (CanMove)
        {
            Vector3 motion = _directionVector * _velocity + _velocityVector;
            _characterController.Move(motion);

            OnPositionChanged?.Invoke(Position);
        }
        else
        {
            Vector3 motion = new Vector3(0, -0.5f, 0);
            _characterController.Move(motion);

            OnPositionChanged?.Invoke(Position);
        }
    }

    private void InternalMoveTo(Vector3 direction, float distance)
    {
        Vector3 motion = direction.normalized * distance;
        _characterController.Move(motion);

        OnPositionChanged?.Invoke(Position);
    }

    private Vector3 CalculateDirectionVector(Vector2 input)
    {
        return new Vector3(input.x, 0, input.y).normalized;
    }

    #region Coroutines

    private void InternalDirectionCalculation()
    {
        if (_inputVector != Vector2.zero)
        {
            _velocity = _maxVelocity;
            _directionVector = CalculateDirectionVector(_inputVector);
        }
        else
        {
            _velocity = 0;
        }
    }

    private async UniTask DirectionMonitoringTask(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            InternalDirectionCalculation();
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);
        }
    }

    private async UniTask MovementTask(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            InternalMove();
            await UniTask.Yield(_timing, token);
        }
    }

    private async UniTask IsMovingMonitoringTask(CancellationToken token)
    {
        bool prevMove = IsMoving;
        while (!token.IsCancellationRequested)
        {
            await UniTask.WaitUntil(() => prevMove != IsMoving, _timing, token);
            _movementStateMachine.UpdateState(IsMoving);
            prevMove = IsMoving;
        }
    }

    #endregion

    // Методы для внешнего управления

    public void MoveToDirection(Vector2 direction)
    {
        _inputVector = direction;
    }

    public void MoveTo(Vector3 point, float distance)
    {
        if (point == Vector3.zero || distance == 0) return;

        Vector3 direction = point - Position;

        InternalMoveTo(direction, distance);
    }

    public void MoveFromOffset(Vector3 offset)
    {
        Vector3 targetPosition = Position + offset;
        MoveTo(targetPosition, offset.magnitude);
    }

    public void Teleport(Vector3 position)
    {
        _characterController.enabled = false;
        _characterController.transform.position = position;
        _characterController.enabled = true;
        OnPositionChanged?.Invoke(Position);
    }

    public void Enable()
    {
        _directionMonitoringCoroutine.Run();
        _movementCoroutine.Run();
        _isMovingMonitoringCoroutine.Run();
    }

    public void Disable()
    {
        _directionMonitoringCoroutine?.Stop();
        _movementCoroutine?.Stop();
        _isMovingMonitoringCoroutine?.Stop();
    }

    public void Dispose()
    {
        _directionMonitoringCoroutine?.Stop();
        _movementCoroutine?.Stop();
        _isMovingMonitoringCoroutine?.Stop();

        _directionMonitoringCoroutine?.Dispose();
        _movementCoroutine?.Dispose();
        _isMovingMonitoringCoroutine?.Dispose();

        _directionMonitoringCoroutine = null;
        _movementCoroutine = null;
        _isMovingMonitoringCoroutine = null;
    }
}
#endregion
// ---------------------------------------
#region  Interaction Controller
public class InteractionController : IDisposable
{
    public Transform Transform;

    private float _radius;
    public InteractableBehaviour SelectedInteractable => Interactables.FirstOrDefault();
    public IEnumerable<InteractableBehaviour> Interactables;
    private LayerMask _mask;
    private PlayerLoopTiming _timing = PlayerLoopTiming.Update;

    public float Radius { get => _radius; set => _radius = value; }
    public LayerMask Mask { get => _mask; set => _mask = value; }
    public PlayerLoopTiming Timing { get => _timing; set => _timing = value; }

    private UniTaskCoroutine _monitoringCoroutine;
    public UniTaskCoroutine MonitoringCoroutine => _monitoringCoroutine;

    public InteractionController(Transform transform)
    {
        Transform = transform;
        // Создаём корутину мониторинга в конструкторе, но пока не запускаем
        _monitoringCoroutine = new UniTaskCoroutine(ct => InteractableMonitoring(ct));
    }

    private async UniTask InteractableMonitoring(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Interactables = GetInteractables(Transform.position);
            await UniTask.Yield(_timing, cancellationToken);
        }
    }

    public IEnumerable<InteractableBehaviour> FindInteractables(Vector3 origin, float radius, LayerMask mask)
    {
        Collider[] hits = Physics.OverlapSphere(origin, radius, mask);
        HashSet<InteractableBehaviour> result = new HashSet<InteractableBehaviour>();
        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<InteractableBehaviour>();
            if (interactable != null)
            {
                result.Add(interactable);
            }
        }
        return result;
    }

    public IEnumerable<InteractableBehaviour> GetInteractables(Vector3 position)
    {
        return FindInteractables(position, _radius, _mask)
            .OrderBy(item => Vector3.Distance(position, item.transform.position));
    }

    public void Interact()
    {
        if (SelectedInteractable != null)
        {
            SelectedInteractable.Interact(Transform.gameObject);
        }
    }

    public void InteractWith(InteractableBehaviour interactableBehaviour)
    {
        if (interactableBehaviour == null) return;
        interactableBehaviour.Interact(Transform.gameObject);
    }

    // Запуск мониторинга (если ещё не запущена)
    public void Enable()
    {
        if (!_monitoringCoroutine.IsRunning)
        {
            _monitoringCoroutine.Run();
        }
    }

    // Остановка мониторинга
    public void Disable()
    {
        if (_monitoringCoroutine.IsRunning)
        {
            _monitoringCoroutine.Stop();
        }
    }

    // Реализация IDisposable
    public void Dispose()
    {
        Disable();
        _monitoringCoroutine?.Dispose();
        _monitoringCoroutine = null;
    }
}
#endregion