using UnityEngine;

namespace GameJamLvl5.Project.Infrastructure.EventBus.Subscribers
{
    public interface IUI_MousePositionEventHandler : IGlobalSubscriber
    {
        void HandleMousePosition(Vector2 position);
    }
}