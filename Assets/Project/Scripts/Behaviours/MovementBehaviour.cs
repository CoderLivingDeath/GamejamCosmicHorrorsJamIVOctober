using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementBehaviour : MonoBehaviour
{
    [SerializeField]
    private bool _blockX;

    [SerializeField]
    private bool _blockY;

    public MovementStateMachine.State State => _movementStateMachine.CurrentState;

    [SerializeField]
    private MovementStateMachine.State _state;

    [SerializeField]
    private float _maxVelocity = 0.2f;

    [SerializeField]
    private float _velocity;
    public float Velocity => _velocity;

    [SerializeField]
    private Vector3 _directionVector;
    public Vector3 DirectionVector => _directionVector;

    [SerializeField]
    private PlayerLoopTiming _timing = PlayerLoopTiming.FixedUpdate;

    private CharacterController _characterController;

    private Vector2 _inputVector;

    public MovementStateMachine MovementStateMachine => _movementStateMachine;
    private MovementStateMachine _movementStateMachine;

    private UniTaskCoroutine _movementCoroutine;
    private UniTaskCoroutine _directionMonitoringCoroutine;
    private UniTaskCoroutine _isMovingMonitoringCoroutine;

    private DisposerContainer _disposerContainer = new();

    public event Action<MovementStateMachine.State> OnStateChanged;
    public event Action<Vector3> OnPositionChanged;

    private Vector3 _velocityVector; // для гравитации

    public void Teleport(Vector3 position)
    {
        _characterController.enabled = false;
        transform.position = position;
        _characterController.enabled = true;
    }

    private bool CanMove()
    {
        return _characterController.isGrounded;
    }

    public void Move(Vector2 direction)
    {
        if (_blockX) direction.x = 0;
        if (_blockY) direction.y = 0;
        _inputVector = direction;
    }

    public bool IsMoving()
    {
        return _velocity != 0;
    }

    private void MovementStateMachine_OnStateChanged(MovementStateMachine.State state)
    {
        OnStateChanged?.Invoke(state);
    }

    private Vector3 CalculateDirectionVector(Vector2 input)
    {
        // Преобразуем 2D-ввод в 3D (XZ-плоскость)
        Vector3 direction = new Vector3(input.x, 0, input.y).normalized;
        return direction;
    }

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

    private void InternalMove()
    {
        if (CanMove())
        {
            Vector3 motion = _directionVector * _velocity + _velocityVector;
            _characterController.Move(motion);

            OnPositionChanged?.Invoke(transform.position);
        }
        else
        {
            Vector3 motion = new Vector3(0, -0.5f, 0);
            _characterController.Move(motion);

            OnPositionChanged?.Invoke(transform.position);
        }
    }

    public void MoveTo(Vector3 point, float distance)
    {
        if (point == Vector3.zero || distance == 0) return;

        Vector3 direction = point - transform.position;

        InternalMoveTo(direction, distance);
    }

    private void InternalMoveTo(Vector3 direction, float distance)
    {
        Vector3 motion = direction.normalized * distance;
        _characterController.Move(motion);

        OnPositionChanged?.Invoke(transform.position);
    }

    public async UniTask MovePathAsync(Vector3 point, float duration, CancellationToken token = default)
    {
        float elapsed = 0f;

        Vector3 startPosition = transform.position;
        float totalDistance = Vector3.Distance(startPosition, point);

        _movementCoroutine.Stop();
        _movementStateMachine.UpdateState(true);

        if (totalDistance < 0.01f)
        {
            // Обработка случая, если уже на месте
            Teleport(point);
            MoveTo(Vector3.zero, 0);
            return;
        }

        while (elapsed < duration && !token.IsCancellationRequested)
        {
            elapsed += Time.fixedDeltaTime;

            // Рассчитываем прогресс перемещения
            float t = Mathf.Clamp01(elapsed / duration);

            // Целевая позиция по Lerp от начала к цели
            Vector3 targetPosition = Vector3.Lerp(startPosition, point, t);

            // Рассчитываем направление и шаг для движения
            Vector3 direction = targetPosition - transform.position;
            float distance = direction.magnitude;

            // Вычисляем скорость движения, чтобы дойти ровно за duration
            float maxStep = totalDistance / duration * Time.fixedDeltaTime;

            // Ограничиваем шаг текущей длиной до цели
            float step = Mathf.Min(maxStep, distance);

            if (step < 0.001f)
            {
                break; // Достигли цели или близко к ней
            }

            MoveTo(point, step);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }

        Teleport(point);
        MoveTo(Vector3.zero, 0);

        _movementStateMachine.UpdateState(false);
        _movementCoroutine.Run();
    }

    public void SnapToGround()
    {
        if (_characterController.isGrounded) return;

        Bounds bounds = _characterController.bounds;

        float offsetUp = 0f;

        float maxSnapDistance = 15.0f;

        Vector3 rayStart = bounds.center;

        Vector3 rayDirection = Vector3.down;

        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, maxSnapDistance + offsetUp, 1))
        {
            Vector3 targetPosition = transform.position;
            targetPosition.y = hit.point.y + bounds.extents.y;

            Teleport(targetPosition);
        }
    }

    #region Coroutines

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
        bool prevMove = IsMoving();
        while (!token.IsCancellationRequested)
        {
            await UniTask.WaitUntil(() => prevMove != IsMoving(), _timing, token);
            _movementStateMachine.UpdateState(IsMoving());
            prevMove = IsMoving();
        }
    }
    #endregion

    #region Unity Internal
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        _movementCoroutine = new UniTaskCoroutine(MovementTask);
        _directionMonitoringCoroutine = new UniTaskCoroutine(DirectionMonitoringTask);
        _isMovingMonitoringCoroutine = new UniTaskCoroutine(IsMovingMonitoringTask);

        _disposerContainer.Add(_movementCoroutine);
        _disposerContainer.Add(_directionMonitoringCoroutine);
        _disposerContainer.Add(_isMovingMonitoringCoroutine);

        _movementStateMachine = new();
    }

    private void Update()
    {
        _state = State;
    }

    private void OnEnable()
    {
        _movementCoroutine.Run();
        _directionMonitoringCoroutine.Run();
        _isMovingMonitoringCoroutine.Run();

        _movementStateMachine.OnStateChanged += MovementStateMachine_OnStateChanged;
    }

    private void OnDisable()
    {
        _movementCoroutine.Stop();
        _directionMonitoringCoroutine.Stop();
        _isMovingMonitoringCoroutine.Stop();

        _movementStateMachine.OnStateChanged -= MovementStateMachine_OnStateChanged;
    }

    private void OnDestroy()
    {
        _disposerContainer.Dispose();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        DrawLineFromPosition(new Vector3(_inputVector.x, 0, _inputVector.y));
        Gizmos.color = Color.red;
        DrawLineFromPosition(_directionVector);
        DrawLineFromPosition(transform.up);

        void DrawLineFromPosition(Vector3 to)
        {
            Vector3 Position = transform.position;
            float scale = 3f;

            Gizmos.DrawLine(Position, Position + to * scale);
        }
    }
    #endregion
}