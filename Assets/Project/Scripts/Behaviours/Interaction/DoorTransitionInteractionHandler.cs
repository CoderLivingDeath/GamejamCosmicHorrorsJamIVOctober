using System.Collections;
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
        public bool NeedPuzzle = false;

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

        [SerializeField]
        private GameObject Lamp;

        [SerializeField]
        private GameObject AlertColection;
        [SerializeField]
        private GameObject EmergencyLightCollection;
        [SerializeField]
        private AudioSource _audioSource;
        [SerializeField]
        private AudioClip _audioAccessDenied;
        [SerializeField]
        private AudioClip _audioAccessGranted;

        [SerializeField]
        private Material MaterialOpen;

        [Inject]
        private LocationController locationController;

        [Inject]
        private ViewManager viewManager;

        [Inject]
        private DoorPuzzlePopupView.Factory DoorPuzzlePopupViewfactory;

        public override void HandleInteract(InteractionContext context)
        {
            if (NeedKey)
            {
                if (PlayerPrefs.GetInt(ChackPlayerPrefsKey) != 1)
                {
                    Debug.Log("Key not taked.");
                    if (_audioSource.enabled)
                    {
                        _audioSource.clip = _audioAccessDenied;
                        _audioSource.Play();
                    }
                    return;
                }

                if (_audioSource.enabled)
                {
                    _audioSource.clip = _audioAccessGranted;
                    _audioSource.Play();
                    StartCoroutine(DisableAudioSourceAfterPlayback());
                }

                if (Lamp)
                {
                    Lamp.GetComponent<SpriteRenderer>().material = MaterialOpen;
                }
            }

            if (NeedPuzzle)
            {
                var ViewScope = viewManager.CreateView(DoorPuzzlePopupViewfactory);
                ViewScope.View.ViewModel.PuzzleSolved += (s, e) => NeedPuzzle = false;
                if (Lamp)
                {
                    ViewScope.View.ViewModel.PuzzleSolved += (s, e) => Lamp.GetComponent<SpriteRenderer>().material = MaterialOpen;
                    if (AlertColection && AlertColection.activeSelf)
                    {
                        if (AlertColection.activeSelf)
                        {
                            ViewScope.View.ViewModel.PuzzleSolved += (s, e) => AlertColection.SetActive(false);
                            ViewScope.View.ViewModel.PuzzleSolved += (s, e) => EmergencyLightCollection.SetActive(true);
                        }
                        if (_audioSource.enabled)
                        {
                            ViewScope.View.ViewModel.PuzzleSolved += (s, e) =>
                            {
                                _audioSource.clip = _audioAccessGranted;
                                _audioSource.Play();
                                StartCoroutine(DisableAudioSourceAfterPlayback());
                            };
                        }
                    }
                }
                return;
            }

            MonoCharacterController monoCharacterController = context.Interactor.GetComponent<MonoCharacterController>();

            var scope = doorTransitionAnimation.Animate(monoCharacterController, PathAnimationReverce);
            if (scope == null) return;
            scope.Completed += () => locationController.HideLocations(PrevId);

            locationController.ActivateRangeLocation(NextId);

            var wallMaterial = locationController.locationSettings.WallMaterial;
            var doorMaterial = locationController.locationSettings.DoorMaterial;
            var lampMaterial = locationController.locationSettings.DoorLampMasterMaterial;

            var MinDoorAlpha = locationController.locationSettings.MinDoorAlpha;
            var MaxDoorAlpha = locationController.locationSettings.MaxDoorAlpha;

            var MinLampEmission = locationController.locationSettings.MinDoorAlpha;
            var MaxDoorEmission = locationController.locationSettings.MaxDoorAlpha;

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
                AnimateShaderFloatParameter(lampMaterial, "_Alpha", MinDoorAlpha, duration);
                AnimateShaderFloatParameter(lampMaterial, "_GlowIntensity", MinDoorAlpha, duration);
            }
            else
            {
                AnimateShaderFloatParameter(doorMaterial, "_Alpha", MaxDoorAlpha, duration);
                AnimateShaderFloatParameter(lampMaterial, "_Alpha", MaxDoorAlpha, duration);
                AnimateShaderFloatParameter(lampMaterial, "_GlowIntensity", MaxDoorAlpha, duration);
            }

            void AnimateShaderFloatParameter(Material mat, string parameterName, float targetValue, float duration)
            {
                if (mat.HasProperty(parameterName))
                {
                    // TODO: костыль с задержкой перед стартом анимации.
                    DOTween.To(() => mat.GetFloat(parameterName), x =>
                    {
                        mat.SetFloat(parameterName, x);
                    }, targetValue, duration).SetDelay(duration * 2);
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

        IEnumerator DisableAudioSourceAfterPlayback()
        {
            yield return new WaitForSeconds(_audioAccessGranted.length);
            _audioSource.enabled = false;
        }
    }
}