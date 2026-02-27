using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class CommTween
{
    public static Tween To(float currentValue, float targetValue, float duration, Action<float> onUpdate = null)
    {
        return DOTween.To(() => currentValue, x => currentValue = x, targetValue, duration).OnUpdate(() =>
        {
            onUpdate?.Invoke(currentValue);
        });
    }
}
