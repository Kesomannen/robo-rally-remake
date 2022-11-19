using System;
using System.Collections;
using UnityEngine;

public static class TweenHelper {
    public static IEnumerator DoUITween(UITween tween){
        foreach (var obj in tween.Objects){
            obj.SetActive(false);
        }
        
        var start = Vector3.one * tween.StartSize;
        foreach (var obj in tween.Objects){
            obj.transform.localScale = start;
            obj.SetActive(true);
            LeanTween.scale(obj, Vector3.one, tween.Duration).setEase(tween.TweenType);
            yield return Helpers.Wait(tween.Interval);
        }
    }
}

[Serializable]
public struct UITween {
    public GameObject[] Objects;
    public float StartSize;
    public float Duration;
    public float Interval;
    public LeanTweenType TweenType;
}