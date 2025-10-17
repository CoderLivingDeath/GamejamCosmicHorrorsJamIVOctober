using DG.Tweening;
using UnityEngine;

public class ObjectSpinner : MonoBehaviour
{
    private void Start()
    {
        // Вращаем объект вокруг оси Y на 360 градусов бесконечно, с линейной скоростью
        transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }
}