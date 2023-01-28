using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PassiveAnimation : MonoBehaviour {
    [Min(0)] public float _frameTime;
    public Sprite[] _sprites;
    
    SpriteRenderer _renderer;

    void Awake() {
        _renderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable() {
        StartCoroutine(AnimationRoutine());
    }

    IEnumerator AnimationRoutine() {
        var i = 0;
        while (enabled) {
            _renderer.sprite = _sprites[i];
            if (i++ >= _sprites.Length - 1) i = 0;
            yield return CoroutineUtils.Wait(_frameTime);
        }    
    }
}