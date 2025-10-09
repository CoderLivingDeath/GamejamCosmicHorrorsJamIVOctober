using UnityEngine;

namespace GameJamLvl5.Project.Infrastructure.EventBus.Subscribers
{
    public interface IGameplay_MovementEventHandler : IGlobalSubscriber
    {
        void HandleMovement(Vector2 direction);
    }
}