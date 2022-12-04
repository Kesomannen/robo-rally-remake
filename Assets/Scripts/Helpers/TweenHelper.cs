using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public static class TweenHelper {
    public static IEnumerator DoUITween(StaticUITween tween){
        if (!tween.Simultaneous){
            foreach (var obj in tween.Objects){
                obj.SetActive(false);
            }
        }

        foreach (var obj in tween.Objects){
            var originalSize = obj.transform.localScale;
            var start = originalSize * tween.StartSize;
            obj.transform.localScale = start;
            
            obj.SetActive(true);
            LeanTween.scale(obj, originalSize, tween.Duration).setEase(tween.TweenType);
            if (!tween.Simultaneous) yield return CoroutineUtils.Wait(tween.Interval);
        }
    }

    public static IEnumerator DoUITween(DynamicUITween dynamicTween, IEnumerable<GameObject> objects) {
        yield return DoUITween(dynamicTween.ToTween(objects));
    }
    
    public static IEnumerator DoUITween(DynamicUITween dynamicTween, params GameObject[] objects) {
        yield return DoUITween(dynamicTween.ToTween(objects));
    }
}

[Serializable]
public struct StaticUITween {
    [SerializeField] GameObject[] _objects;
    [SerializeField] [Range(0, 1)] float _startSize; 
    [SerializeField] [Min(0)] float _duration;
    [SerializeField] [Min(0)] float _interval;
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] bool _simultaneous;
    
    public IEnumerable<GameObject> Objects => _objects;
    public float StartSize => _startSize;
    public float Duration => _duration;
    public float Interval => _interval;
    public LeanTweenType TweenType => _tweenType;
    public bool Simultaneous => _simultaneous;
    
    public StaticUITween(
        IEnumerable<GameObject> objects, 
        float startSize,
        float duration,
        float interval, 
        LeanTweenType tweenType,
        bool simultaneous = false
        ){
        _objects = objects.ToArray();
        _startSize = startSize;
        _duration = duration;
        _interval = interval;
        _tweenType = tweenType;
        _simultaneous = simultaneous;
    }
}

[Serializable]
public struct DynamicUITween {
    [SerializeField] float _startSize; 
    [SerializeField] [Min(0)] float _duration;
    [SerializeField] [Min(0)] float _interval;
    [SerializeField] LeanTweenType _tweenType;
    [SerializeField] bool _simultaneous;
    
    public StaticUITween ToTween(IEnumerable<GameObject> objects){
        return new StaticUITween(objects, _startSize, _duration, _interval, _tweenType, _simultaneous);
    }
}