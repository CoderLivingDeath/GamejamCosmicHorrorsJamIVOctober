using DG.Tweening;
using UnityEngine;

public class ObjectSpinner : MonoBehaviour
{
    private void Start()
    {
        // ������� ������ ������ ��� Y �� 360 �������� ����������, � �������� ���������
        transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }
}