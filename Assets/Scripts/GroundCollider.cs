using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GroundCollider : MonoBehaviour
{
    //may unhide later
    [HideInInspector] public UnityEvent OnTouchGround;
    [HideInInspector] public UnityEvent OnLeaveGround;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Terrain"))
            OnTouchGround.Invoke();
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Terrain"))
            OnLeaveGround.Invoke();
    }
}
