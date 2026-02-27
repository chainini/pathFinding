using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public void CancleAttack()
    {
        SendMessageUpwards("OnCancleAttack", SendMessageOptions.DontRequireReceiver);
    }

    public void ApplyAttack()
    {
        SendMessageUpwards("OnApplyAttack", SendMessageOptions.DontRequireReceiver);
    }
}
