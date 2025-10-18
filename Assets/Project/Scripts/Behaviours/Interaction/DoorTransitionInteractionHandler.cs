using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class DoorTransitionInteractionHandler : MonoInteractableHandlerBase
    {
        [SerializeField]
        private bool PathAnimationReverce;

        [SerializeField]
        public bool NeedKey = false;

        [SerializeField]
        public bool HideWall;

        [SerializeField]
        public bool HideDoor;

        [SerializeField]
        private string ChackPlayerPrefsKey = "Key_taked";

        [SerializeField]
        private MonoDoorTransition doorTransitionAnimation;

        [SerializeField]
        private string[] NextId;

        [SerializeField]
        private string[] PrevId;

        [Inject]
        private LocationController locationController;

        public override void HandleInteract(InteractionContext context)
        {
            if (NeedKey)
            {
                if (PlayerPrefs.GetInt(ChackPlayerPrefsKey) != 1)
                {
                    Debug.Log("Key not taked.");
                    return;
                }
            }

            MonoCharacterController monoCharacterController = context.Interactor.GetComponent<MonoCharacterController>();

            var scope = doorTransitionAnimation.Animate(monoCharacterController, PathAnimationReverce);
            scope.Completed += () => locationController.HideLocations(PrevId);

            locationController.ActivateRangeLocation(NextId);

            var wallMaterial = locationController.locationSettings.WallMaterial;
            var doorMaterial = locationController.locationSettings.DoorMaterial;

            var MinDoorAlpha = locationController.locationSettings.MinDoorAlpha;
            var MaxDoorAlpha = locationController.locationSettings.MaxDoorAlpha;

            var MinWallAlpha = locationController.locationSettings.MinWallAlpha;
            var MaxWallAlpha = locationController.locationSettings.MaxWallAlpha;

            float duration = 1f; // время анимации в секундах

            if (HideWall)
            {
                AnimateShaderFloatParameter(wallMaterial, "_Alpha", MinWallAlpha, duration);
            }
            else
            {
                AnimateShaderFloatParameter(wallMaterial, "_Alpha", MaxWallAlpha, duration);
            }

            if (HideDoor)
            {
                AnimateShaderFloatParameter(doorMaterial, "_Alpha", MinDoorAlpha, duration);
            }
            else
            {
                AnimateShaderFloatParameter(doorMaterial, "_Alpha", MaxDoorAlpha, duration);
            }

            void AnimateShaderFloatParameter(Material mat, string parameterName, float targetValue, float duration)
            {
                if (mat.HasProperty(parameterName))
                {
                    DOTween.To(() => mat.GetFloat(parameterName), x =>
                    {
                        mat.SetFloat(parameterName, x);
                    }, targetValue, duration);
                }
                else
                {
                    Debug.LogWarning("parameterName not exist");
                }
            }
        }

        void OnDestroy()
        {
            // Проверяем, что материалы проинициализированы
            if (locationController != null && locationController.locationSettings != null)
            {
                var wallMaterial = locationController.locationSettings.WallMaterial;
                var doorMaterial = locationController.locationSettings.DoorMaterial;

                // Установка параметров обратно в 1
                if (wallMaterial != null && wallMaterial.HasProperty("_Alpha"))
                {
                    wallMaterial.SetFloat("_Alpha", 1f);
                }

                if (doorMaterial != null && doorMaterial.HasProperty("_Alpha"))
                {
                    doorMaterial.SetFloat("_Alpha", 1f);
                }
            }
        }

    }
}