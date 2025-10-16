using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    public UnityEvent Action;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Action.Invoke();
        }

    }
}
