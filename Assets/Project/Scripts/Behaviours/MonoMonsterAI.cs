using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using EditorAttributes;
using System;
using Stateless;

/// <summary>
/// Класс управления ИИ монстра, расширяющий MonoCharacter.
/// Использует MonoCharacterController для управления движением и анимацией.
/// </summary>
public class MonoMonsterAI : MonoCharacter
{
    private MonsterAI monsterAI;

    private CancellationTokenSource _moveCancellation;

    private Transform playerTransform; // Ссылка на цель для погони

    private float searchDuration = 5f; // Время поиска
    private float searchTimer;

    [SerializeField]
    private MonoCharacterController monoCharacterController;

    [SerializeField]
    private MonsterVisionTrigger AIVision;

    [SerializeField]
    private MonsterAI.AIState aIState;

    public async UniTask ChaseTask(CancellationToken token)
    {
        Transform targetTransform = playerTransform;

        while (!token.IsCancellationRequested)
        {
            if (targetTransform == null)
            {
                // Цель отсутствует — прерываем задачу
                break;
            }

            Vector3 direction = targetTransform.position - transform.position;
            direction.y = 0; // Игнорируем ось Y для движения по земле

            Vector2 moveDir = new Vector2(direction.x, direction.z).normalized;
            monoCharacterController.MoveToDirection(moveDir);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
    }


    private void Awake()
    {
        monsterAI = new MonsterAI();

        if (AIVision != null)
        {
            AIVision.TargetEntered += OnPlayerSpotted;
            AIVision.TargetExited += OnPlayerLost;
        }
    }

    private void OnDestroy()
    {
        if (AIVision != null)
        {
            AIVision.TargetEntered -= OnPlayerSpotted;
            AIVision.TargetExited -= OnPlayerLost;
        }
    }

    private void Update()
    {
        aIState = monsterAI.CurrentState;
        switch (monsterAI.CurrentState)
        {
            case MonsterAI.AIState.Idle:
                StopMovement();
                break;

            case MonsterAI.AIState.Search:
                searchTimer += Time.deltaTime;
                if (TryFindPlayer(out playerTransform))
                {
                    monsterAI.SpotTarget();
                    searchTimer = 0f;
                }
                else if (searchTimer > searchDuration)
                {
                    monsterAI.SearchTimedOut();
                    searchTimer = 0f;
                }
                else
                {
                    PatrolAround();
                }
                break;

            case MonsterAI.AIState.Chase:
                if (playerTransform == null || !IsPlayerVisible(playerTransform))
                {
                    monsterAI.LoseTarget();
                }
                else
                {
                    ChaseTarget(playerTransform.position);
                }
                break;
        }
    }


    // Обработчики событий от обзора
    private void OnPlayerSpotted(Transform player)
    {
        playerTransform = player;
        monsterAI.SpotTarget();
    }

    private void OnPlayerLost(Transform player)
    {
        playerTransform = null;
        monsterAI.LoseTarget();
    }


    private bool TryFindPlayer(out Transform player)
    {
        // Реализуйте кастомную логику поиска игрока, если нужно
        player = null;
        return false;
    }


    private bool IsPlayerVisible(Transform player)
    {
        return true;
    }


    private void PatrolAround()
    {
        SetMoveDirection(UnityEngine.Random.insideUnitCircle.normalized);
    }


    private void ChaseTarget(Vector3 targetPos)
    {

        Vector3 direction = targetPos - transform.position;
        direction.y = 0; // Игнорируем ось Y для движения по земле

        Vector2 moveDir = new Vector2(direction.x, direction.z).normalized;
        monoCharacterController.MoveToDirection(moveDir);
    }

    /// <summary>
    /// Устанавливает направление движения монстра через MonoCharacterController.
    /// </summary>
    /// <param name="direction">Направление движения в 2D (X,Z).</param>
    public void SetMoveDirection(Vector2 direction)
    {
        CancelMoveToPoint();
        monoCharacterController.MoveToDirection(direction);
    }


    /// <summary>
    /// Останавливает движение монстра.
    /// </summary>
    public void StopMovement()
    {
        CancelMoveToPoint();
        monoCharacterController.MoveToDirection(Vector2.zero);
    }


    /// <summary>
    /// Проигрывает анимацию по ключу.
    /// </summary>
    /// <param name="animationKey">Ключ анимации.</param>
    public void PlayAnimation(string animationKey)
    {
        monoCharacterController.PlayAnimation(animationKey);
    }


    /// <summary>
    /// Выполняет взаимодействие с ближайшим интерактивным объектом.
    /// </summary>
    public void Interact()
    {
        monoCharacterController.Interact();
    }


    /// <summary>
    /// Асинхронное перемещение к целевой точке с возможностью отмены.
    /// </summary>
    /// <param name="targetPoint">Целевая позиция в мире.</param>
    /// <param name="stopDistance">Расстояние остановки от точки (по умолчанию 0.1f).</param>
    public async UniTask MoveToPointAsync(Vector3 targetPoint, float stopDistance = 0.1f)
    {
        CancelMoveToPoint();
        _moveCancellation = new CancellationTokenSource();
        var token = _moveCancellation.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                Vector3 direction = (targetPoint - transform.position);
                direction.y = 0;

                float distance = direction.magnitude;
                if (distance <= stopDistance)
                {
                    StopMovement();
                    break;
                }

                Vector2 moveDir = new Vector2(direction.x, direction.z).normalized;
                SetMoveDirection(moveDir);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
            StopMovement();
        }
        finally
        {
            _moveCancellation.Dispose();
            _moveCancellation = null;
        }
    }


    /// <summary>
    /// Отмена текущего перемещения к точке.
    /// </summary>
    public void CancelMoveToPoint()
    {
        if (_moveCancellation != null && !_moveCancellation.IsCancellationRequested)
        {
            _moveCancellation.Cancel();
        }
    }


    [Button]
    public void Fire(int id)
    {
        switch (id)
        {
            case 0:
                monsterAI.TryFire(MonsterAI.AITrigger.StartSearching);
                break;
            case 1:
                monsterAI.TryFire(MonsterAI.AITrigger.TargetSpotted);
                break;
            case 2:
                monsterAI.TryFire(MonsterAI.AITrigger.LostTarget);
                break;
            case 3:
                monsterAI.TryFire(MonsterAI.AITrigger.SearchTimeout);
                break;
        }
    }
}

public class MonsterAI
{
    public enum AIState
    {
        Idle,
        Search,
        Chase
    }

    public enum AITrigger
    {
        StartSearching,
        TargetSpotted,
        LostTarget,
        SearchTimeout
    }

    private readonly StateMachine<AIState, AITrigger> _stateMachine;

    public MonsterAI()
    {
        _stateMachine = new StateMachine<AIState, AITrigger>(AIState.Idle);

        _stateMachine.Configure(AIState.Idle)
          .Permit(AITrigger.StartSearching, AIState.Search)
          .Permit(AITrigger.TargetSpotted, AIState.Chase)
          .OnEntry(() => OnIdleEnter());

        _stateMachine.Configure(AIState.Search)
            .Permit(AITrigger.TargetSpotted, AIState.Chase)
            .Permit(AITrigger.SearchTimeout, AIState.Idle)
            .Permit(AITrigger.LostTarget, AIState.Idle)  // Добавлено разрешение для LostTarget
            .OnEntry(() => OnSearchEnter());


        _stateMachine.Configure(AIState.Chase)
          .Permit(AITrigger.LostTarget, AIState.Search)
          .OnEntry(() => OnChaseEnter());
    }

    public AIState CurrentState => _stateMachine.State;

    // Методы вызова триггеров
    public void StartSearching() => _stateMachine.Fire(AITrigger.StartSearching);
    public void SpotTarget() => _stateMachine.Fire(AITrigger.TargetSpotted);
    public void LoseTarget() => _stateMachine.Fire(AITrigger.LostTarget);
    public void SearchTimedOut() => _stateMachine.Fire(AITrigger.SearchTimeout);

    public bool TryFire(AITrigger trigger)
    {
        if (_stateMachine.CanFire(trigger))
        {
            _stateMachine.Fire(trigger);
            return true;
        }
        return false;
    }

    // Колбеки при входе в состояние
    private void OnIdleEnter()
    {
        // Логика для бездействия
    }

    private void OnSearchEnter()
    {
        // Логика для поиска
    }

    private void OnChaseEnter()
    {
        // Логика для погони
    }
}
