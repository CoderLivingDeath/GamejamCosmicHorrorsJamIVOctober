using System;
using System.Linq;
using System.Threading;
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

    #region Coroutines

    private async UniTask DirectionMonitoringTask(CancellationToken token)
    {
        await UniTask.WaitForSeconds(1);
        while (!token.IsCancellationRequested)
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
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);
        }
    }

    private Vector3 CalculateDirectionVector(Vector2 input)
    {
        // Преобразуем 2D-ввод в 3D (XZ-плоскость)
        Vector3 direction = new Vector3(input.x, 0, input.y).normalized;
        return direction;
    }

    private async UniTask MovementTask(CancellationToken token)
    {
        await UniTask.WaitForSeconds(1);
        while (!token.IsCancellationRequested)
        {
            if (CanMove())
            {
                Vector3 motion = _directionVector * _velocity + _velocityVector;
                _characterController.Move(motion);

                OnPositionChanged?.Invoke(transform.position);
            }
            else
            {
                Vector3 motion = new Vector3(0, -0.05f, 0);
                _characterController.Move(motion);

                OnPositionChanged?.Invoke(transform.position);
            }
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
