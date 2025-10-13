using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[InteractableComponent]
public class TransitionInteractionHandler : MonoInteractableHandlerBase
{
    public Vector3 StartPoint => transform.position;
    public Vector3 EndPoint => _endPoint.position;

    [SerializeField]
    public Transform _endPoint;

    public override void HandleInteract(InteractionContext context)
    {
        var behaiour = context.Interactor.GetComponent<MovementBehaviour>();
        behaiour.Teleport(StartPoint);
        behaiour.SnapToGround();
        behaiour.MovePathAsync(EndPoint, 1f).Forget();
    }
}
