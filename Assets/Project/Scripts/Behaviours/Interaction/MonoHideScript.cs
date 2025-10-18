using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    public class MonoHideScript : MonoBehaviour
    {
        public bool IsHiding = false;

        public UnityEvent OnHideStart;
        public UnityEvent OnHideEnd;

        public void SwitchHide(MonoCharacterController monoCharacterController)
        {
            if (IsHiding)
            {
                monoCharacterController.UnHide();
                OnHideEnd.Invoke();
                IsHiding = false;
            }
            else
            {
                monoCharacterController.Hide();
                OnHideStart.Invoke();
                IsHiding = true;
            }

        }
    }
}