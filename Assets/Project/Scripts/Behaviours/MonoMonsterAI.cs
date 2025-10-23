using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using Project.Scripts.behaviours.Interaction.InteractableHandlers;
using UnityEngine.UIElements;

public class MonoMonsterAI : MonoCharacter
{
    private InteractionController _interactionController;
    public InteractionController InteractionController => _interactionController;
    
    
    private const float RANDOM_MOVE_X_MIN = -10f;
    private const float RANDOM_MOVE_X_MAX = 10f;
    private const float STOP_DISTANCE = 1f;
    private const int IDLE_LOOP_DELAY_MS = 500;
    private const int IDLE_STAND_TALL_DELAY_MS = 5000;
    private const int RANDOM_IDLE_DELAY_MAX_MS = 10000;
    private const float ROTATE_INTERVAL_SECONDS = 1.5f;

    private MonsterAIStateMachine monsterAI;

    [SerializeField]
    private MonoCharacterController monoCharacterController;

    [SerializeField]
    private MonsterVisionTrigger AIVision;

    [SerializeField]
    private MonsterAIStateMachine.AIState aIState;

    #region Coroutines
    private UniTaskCoroutine MoveToPointCoroutine;
    private UniTaskCoroutine<int> DelayCoroutine;
    private UniTaskCoroutine IdleLoopCoroutine;
    private UniTaskCoroutine<Vector3> MoveToNodeCorutine;
    #endregion


    #region Player prediction system
    // История позиций игрока для предсказания
    private Queue<Vector3> playerPositionHistory = new Queue<Vector3>();
    private Vector3 smoothedPosition;
    private Vector3 smoothedVelocity;

    [Header("Target prediction")]
    public int historySize = 5;
    public float recordInterval = 0.2f;
    private float timeSinceLastRecord = 0f;

    public float predictionTimeStep = 0.1f;
    public int predictionCount = 10;
    public float smoothFactor = 0.5f;

    private List<Vector3> predictedPlayerPositions = new List<Vector3>();

    private void UpdatePlayerPrediction()
    {
        if (Target == null) return;

        timeSinceLastRecord += Time.deltaTime;
        if (timeSinceLastRecord >= recordInterval)
        {
            Vector3 currentPos = Target.position;

            if (playerPositionHistory.Count == 0)
            {
                smoothedPosition = currentPos;
                smoothedVelocity = Vector3.zero;
            }
            else
            {
                Vector3 currentVelocity = (currentPos - smoothedPosition) / recordInterval;

                smoothedPosition = Vector3.Lerp(smoothedPosition, currentPos, smoothFactor);
                smoothedVelocity = Vector3.Lerp(smoothedVelocity, currentVelocity, smoothFactor);
            }

            if (playerPositionHistory.Count >= historySize)
                playerPositionHistory.Dequeue();
            playerPositionHistory.Enqueue(currentPos);

            timeSinceLastRecord = 0f;

            UpdatePredictedPositions();
        }
    }

    private void UpdatePredictedPositions()
    {
        predictedPlayerPositions.Clear();
        for (int i = 1; i <= predictionCount; i++)
        {
            float t = predictionTimeStep * i;
            Vector3 predictedPos = smoothedPosition + smoothedVelocity * t;
            predictedPlayerPositions.Add(predictedPos);
        }
    }

    public IReadOnlyList<Vector3> GetPredictedPositions() => predictedPlayerPositions;
    #endregion

    #region Unity Internal

    public Transform Player;

    [Header("Path fiding")]
    [SerializeField]
    private MonoAIPath AIPath;

    [SerializeField]
    private List<MonoAIPathNode> path;

    [Space]
    [Header("State variables")]
    public Transform Target;
    public float SerachDuration = 10f;
    public float SearchTime;

    public Vector3 Input;
    [Button]
    public void MoveToNodeButton()
    {
        if (monsterAI.TryFire(MonsterAIStateMachine.AITrigger.GoMove))
        {
            MoveToNodeCorutine.Stop();
            MoveToNodeCorutine.Run(Input);
        }
    }

    [SerializeField]
    private LayerMask FindingMask;
    void Awake()
    {
        _interactionController = new InteractionController(transform);
        _interactionController.Radius = 1f;
        _interactionController.Mask = FindingMask;
        
        AIVision.TargetEntered += TargetSpoted;
        AIVision.TargetExited += TargetLost;
        ConfigureStates();

        MoveToPointCoroutine = new(MoveToPoint);
        DelayCoroutine = new(DelayTask);
        IdleLoopCoroutine = new(IdleLoopTask);
        MoveToNodeCorutine = new(MoveToNode);
    }

    void Start()
    {
        monsterAI.EnterState(monsterAI.CurrentState);
    }

    private void Update()
    {
        aIState = monsterAI.CurrentState;

        UpdatePlayerPrediction();

        InternalUpdate();
    }

    public void InternalUpdate()
    {
        switch (aIState)
        {
            case MonsterAIStateMachine.AIState.Idle:
                UpdateIdle();
                break;
            case MonsterAIStateMachine.AIState.IdleWalk:
                UpdateIdleWalk();
                break;
            case MonsterAIStateMachine.AIState.IdleStandTall:
                UpdateIdleStandTall();
                break;
            case MonsterAIStateMachine.AIState.Move:
                break;

            case MonsterAIStateMachine.AIState.Search:
                UpdateSearch();
                break;
            case MonsterAIStateMachine.AIState.Chase:
                UpdateChase();
                break;
            case MonsterAIStateMachine.AIState.Agressive:
                UpdateAgressive();
                break;
        }
    }

    public void MoveStateUpdate()
    {

    }

    void OnDestroy()
    {
        MoveToPointCoroutine?.Dispose();
        DelayCoroutine?.Dispose();
        IdleLoopCoroutine?.Dispose();
        MoveToNodeCorutine?.Dispose();

        AIVision.TargetEntered -= TargetSpoted;
        AIVision.TargetExited -= TargetLost;
    }

    private void DrawPathGizmos(List<MonoAIPathNode> path, Vector3 gizmoOffset, Color color)
    {
        if (path == null || path.Count == 0)
            return;

        Gizmos.color = color;

        for (int i = 0; i < path.Count - 1; i++)
        {
            if (path[i] == null || path[i + 1] == null)
                continue;

            Vector3 posA = path[i].transform.position + gizmoOffset;
            Vector3 posB = path[i + 1].transform.position + gizmoOffset;

            Gizmos.DrawCube(posA, Vector3.one * 0.4f);
            Gizmos.DrawLine(posA, posB);
        }

        // Нарисовать последнюю точку
        if (path[path.Count - 1] != null)
        {
            Vector3 posLast = path[path.Count - 1].transform.position + gizmoOffset;
            Gizmos.DrawCube(posLast, Vector3.one * 0.4f);
        }
    }

    private void DrawPredictionGizmos()
    {
        // История позиций игрока - синие сферы и линии
        if (playerPositionHistory != null && playerPositionHistory.Count > 0)
        {
            Gizmos.color = Color.blue;
            Vector3 prevPoint = Vector3.zero;
            bool first = true;
            foreach (var pos in playerPositionHistory)
            {
                Gizmos.DrawSphere(pos, 0.2f);
                if (!first)
                    Gizmos.DrawLine(prevPoint, pos);
                else
                    first = false;
                prevPoint = pos;
            }
        }

        // Предсказанные позиции - красные кубы и линии
        if (predictedPlayerPositions != null && predictedPlayerPositions.Count > 0)
        {
            Gizmos.color = Color.red;
            Vector3 lastPos = smoothedPosition;
            foreach (var predPos in predictedPlayerPositions)
            {
                Gizmos.DrawCube(predPos, Vector3.one * 0.25f);
                Gizmos.DrawLine(lastPos, predPos);
                lastPos = predPos;
            }
        }
    }
    private new void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.green;
        Gizmos.DrawCube(Input, Vector3.one * 0.5f);

        DrawPredictionGizmos();
        DrawPathGizmos(path, new Vector3(0, 0.2f, 0), Color.white);
    }
    #endregion


    private async void UpdateIdleWalk()
    {
        //Debug.Log("Updating IdleWalk state");
    }

    private void UpdateIdleStandTall()
    {
        //Debug.Log("Updating StandTall state");
    }

    private void UpdateIdle()
    {
        //Debug.Log("Updating Idle state");
    }
    [SerializeField]
    private Vector3 _pointToMove = Vector3.zero;

    private void UpdateChase()
    {
        if (Target)
        {
            if (Target.gameObject.GetComponent<CharacterController>().enabled == false)
            {
                TargetLost(Target);
                if (_pointToMove.x != 0f)
                {
                    MoveToPointCoroutine.RunAsync().ContinueWith(() =>
                    {
                        Interact();
                        
                    }).ContinueWith(() =>
                    {
                        _pointToMove = new Vector3(transform.position.x - 50f, transform.position.y, transform.position.z);
                        MoveToPointCoroutine.RunAsync().ContinueWith(() =>
                        {
                            _pointToMove = Vector3.zero;
                            Destroy(gameObject);
                        });
                    });
                }
                
                return;
            }
        }

        if (Target)
        {
            if (Mathf.Abs(Target.position.z - transform.position.z) > 1)
            {
                Interact();
            }
        }
           
        if (predictedPlayerPositions.Count == 0)
        {
            var direction = Target.position - transform.position;
            monoCharacterController.MoveToDirection(direction.normalized);
            return;
        }

        Vector3 predictedTargetPosition = predictedPlayerPositions[0];
        Vector3 directionToPredicted = predictedTargetPosition - transform.position;

        if (directionToPredicted.magnitude > STOP_DISTANCE)
        {
            monoCharacterController.MoveToDirection(directionToPredicted.normalized);
        }
        else
        {
            StopMovement();
        }

        // if (Player.position.z > this.transform.position.z)
        // {
        //         monoCharacterController.InteractionController.Interact();
        // }
        // if (Mathf.Abs(Target.position.z - transform.position.z) > 1)
        // {
        //     Interact();
        // }
    }

    private void Interact()
    {
        _interactionController?.Enable();
        _interactionController.UpdateInteractables(
            transform.position + new Vector3(1f, 1f, 1f)
        );
        _pointToMove = _interactionController.Interactables.FirstOrDefault().transform.position;
        _interactionController.Interact();
    }

    private void UpdateAgressive()
    {
        // Debug.Log("Updating Agressive state");
    }

    private float RotateInterval = ROTATE_INTERVAL_SECONDS;
    private float RotateTimer = 0f;
    private bool RotateRightNext = true;

    private void UpdateSearch()
    {
        SearchTime += Time.deltaTime;
        RotateTimer += Time.deltaTime;

        if (RotateTimer >= RotateInterval)
        {
            if (RotateRightNext)
                monoCharacterController.RotateRight();
            else
                monoCharacterController.RotateLeft();

            RotateRightNext = !RotateRightNext;
            RotateTimer = 0f;
        }

        if (SearchTime >= SerachDuration)
        {
            SearchTimeout();
            SearchTime = 0f;
        }
    }

    private void SearchTimeout()
    {
        monsterAI.TryFire(MonsterAIStateMachine.AITrigger.SearchTimeout);
    }

    private void TargetSpoted(Transform target)
    {
        Target = target;
        monsterAI.TryFire(MonsterAIStateMachine.AITrigger.TargetSpotted);
    }

    private void TargetLost(Transform target)
    {
        Target = null;
    }

    private void StopMovement()
    {
        monoCharacterController.MoveToDirection(Vector3.zero);
    }

    private void ConfigureStates()
    {
        monsterAI = new();

        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.Idle,
            onEnter: () => { IdleLoopCoroutine.Run(); },
            onExit: () => { IdleLoopCoroutine.Stop(); }
        );

        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.IdleWalk,
            onEnter: () =>
            {
                if (MoveToNodeCorutine.IsRunning)
                    MoveToNodeCorutine.Stop();

                var area = new Bounds(transform.position, new Vector3(10, 1, 3));
                List<MonoAIPathNode> points = PathFinder.FindNodesInBounds(AIPath.Nodes, area);

                if (points == null || points.Count == 0)
                {
                    Debug.LogWarning("No nodes found in area for IdleWalk.");
                    return;
                }

                int randomIndex = UnityEngine.Random.Range(0, points.Count);
                Vector3 targetPosition = points[randomIndex].transform.position;

                MoveToNodeCorutine.Run(targetPosition);
            },
            onExit: () =>
            {
                MoveToNodeCorutine.Stop();
            }
        );

        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.Move,
            onEnter: () =>
            {
                if (MoveToNodeCorutine.IsRunning)
                    MoveToNodeCorutine.Stop();

                List<MonoAIPathNode> points = PathFinder.FindShortestPath(AIPath.Nodes, transform.position, Input);

                if (points == null || points.Count == 0)
                {
                    Debug.LogWarning("No nodes found in area for IdleWalk.");
                    return;
                }

                int randomIndex = UnityEngine.Random.Range(0, points.Count);
                Vector3 targetPosition = points[randomIndex].transform.position;

                MoveToNodeCorutine.Run(targetPosition);
            },
            onExit: () =>
            {
                MoveToNodeCorutine.Stop();
            }
        );

        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.IdleStandTall,
            onEnter: () =>
            {
                DelayCoroutine.RunAsync(IDLE_STAND_TALL_DELAY_MS).ContinueWith(() =>
                    monsterAI.TryFire(MonsterAIStateMachine.AITrigger.GoIdle)).Forget();
                monoCharacterController.Animator.SetBool("IsStand", true);
            },
            onExit: () =>
            {
                monoCharacterController.Animator.SetBool("IsStand", false);
                DelayCoroutine.Stop();
            }
        );

        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.Search,
            onEnter: () => { SearchTime = 0f; },
            onExit: () => { SearchTime = 0f; }
        );

        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.Chase,
            onEnter: () => { },
            onExit: () => { StopMovement(); }
        );

        monsterAI.Subscribe(
            MonsterAIStateMachine.AIState.Agressive,
            onEnter: () => { },
            onExit: () => { }
        );
    }

    public async UniTask MoveToPoint(CancellationToken token = default)
    {
        Vector3 point = _pointToMove;

        while (!token.IsCancellationRequested)
        {
            if (Vector3.Distance(point, transform.position) < STOP_DISTANCE)
                break;

            Vector3 direction = (point - transform.position).normalized;
            monoCharacterController.MoveToDirection(new Vector2(point.x, transform.position.z));
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        StopMovement();
        monsterAI.TryFire(MonsterAIStateMachine.AITrigger.GoIdle);
    }

    private async UniTask DelayTask(int milliseconds, CancellationToken token = default)
    {
        await UniTask.Delay(milliseconds, cancellationToken: token);
    }

    private async UniTask IdleLoopTask(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(IDLE_LOOP_DELAY_MS, cancellationToken: token);
            int choice = UnityEngine.Random.Range(0, 3);
            switch (choice)
            {
                case 0:
                    monsterAI.TryFire(MonsterAIStateMachine.AITrigger.GoIdleWalk);
                    break;
                case 1:
                    monsterAI.TryFire(MonsterAIStateMachine.AITrigger.GoIdleStandTall);
                    break;
                case 2:
                    await UniTask.Delay(UnityEngine.Random.Range(0, RANDOM_IDLE_DELAY_MAX_MS), cancellationToken: token);
                    break;
            }
        }
    }

    public async UniTask MoveToNode(Vector3 point, CancellationToken token = default)
    {
        Debug.Log("MoveToNode started");

        path = PathFinder.FindShortestPath(AIPath.Nodes, transform.position, point);
        if (path == null || path.Count == 0)
        {
            return;
        }

        Queue<MonoAIPathNode> queue = new Queue<MonoAIPathNode>(path);
        const float stopThreshold = STOP_DISTANCE * 0.1f;

        while (!token.IsCancellationRequested && queue.Count > 0)
        {

            var currentNode = queue.Peek();
            if (currentNode is MonoAIPathNodeWithInteraction)
            {
                var withInteraction = (MonoAIPathNodeWithInteraction)currentNode;
                monoCharacterController.InteractWith(withInteraction.Interactable);
                continue;
            }
            Vector3 targetPos = currentNode.transform.position;

            Vector3 flatTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            Vector3 flatPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            float distance = Vector3.Distance(flatPosition, flatTarget);


            if (distance < stopThreshold)
            {
                queue.Dequeue();

                // monoCharacterController.MoveToDirection(Vector2.zero);

                if (queue.Count == 0)
                {
                    StopMovement();
                    path.Clear();
                    return;
                }
                continue;
            }

            Vector3 direction = (flatTarget - flatPosition).normalized;

            monoCharacterController.MoveToDirection(new Vector2(direction.x, direction.z));
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        StopMovement();
        path.Clear();
        monsterAI.TryFire(MonsterAIStateMachine.AITrigger.GoIdle);
    }
}