using UnityEngine;

public class Mono_ChangeVentPosidionEvent : MonoBehaviour
{
    [SerializeField]
    private Vector3 NewPos;
    [SerializeField]
    private Vector3 NewRotation;

   
    public void ChangeTransformPositionAndRotation()
   {
        transform.localPosition = NewPos;
        transform.localRotation = Quaternion.Euler(NewRotation);
   }
}
