using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class MovementController : IDisposable
{
    private float _velocity = 0.2f;

    private Vector3 _directionVector;
    private Vector2 _inputVector;
    private Vector3 _velocityVector;
    private Vector3 Position => _characterController.transform.position;

    private CharacterController _characterController;

    private PlayerLoopTiming _timing = PlayerLoopTiming.FixedUpdate;

    private UniTaskCoroutine _movementCoroutine;
    private UniTaskCoroutine _directionMonitoringCoroutine;
    private UniTaskCoroutine _isMovingMonitoringCoroutine;

    // Публичный доступ к корутинам
    public UniTaskCoroutine MovementCoroutine => _movementCoroutine;
    public UniTaskCoroutine DirectionMonitoringCoroutine => _directionMonitoringCoroutine;
    public UniTaskCoroutine IsMovingMonitoringCoroutine => _isMovingMonitoringCoroutine;
    
    public float Velocity
    {
        get => _velocity;
        set => _velocity = value;
    }

    public Vector3 VelocityVector
    {
        get => _velocityVector;
        set => _velocityVector = value;
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

    public event Action<Vector3> OnPositionChanged;

    public MovementController(CharacterController characterController)
    {
        _characterController = characterController;

        _directionMonitoringCoroutine = new UniTaskCoroutine(DirectionMonitoringTask);
        _movementCoroutine = new UniTaskCoroutine(MovementTask);
        _isMovingMonitoringCoroutine = new UniTaskCoroutine(IsMovingMonitoringTask);

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

    #region Coroutines

    private void InternalDirectionCalculation()
    {
        // TODO: необходим расчет относительно поверхности
        _directionVector = new Vector3(_inputVector.x, 0, _inputVector.y).normalized;
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
            prevMove = IsMoving;
        }
    }

    #endregion

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
    public void DrawInputAndDirection(Vector3 origin, Color inputColor, Color directionColor)
    {
        Gizmos.color = inputColor;
        // Входной вектор (на основе _inputVector) — отображаем от центра
        Vector3 inputEnd = origin + new Vector3(_inputVector.x, 0, _inputVector.y);
        Gizmos.DrawLine(origin, inputEnd);
        Gizmos.DrawSphere(inputEnd, 0.05f); // маркер конца входного вектора

        Gizmos.color = directionColor;
        // Направление движения (_directionVector)
        Vector3 directionEnd = origin + _directionVector;
        Gizmos.DrawLine(origin, directionEnd);
        Gizmos.DrawSphere(directionEnd, 0.05f);
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