using UnityEngine;

namespace GameJamLvl5.Project.Infrastructure.EventBus.Subscribers
{
    public interface IGameplay_Mouse_PositionEventHandler : IGlobalSubscriber
    {
        void HandleMousePosition(Vector2 position);
    }
}