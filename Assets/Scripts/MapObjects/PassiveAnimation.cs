using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PassiveAnimation : MonoBehaviour {
    [SerializeField] [Min(0)] float _frameTime;
    [SerializeField] Sprite[] _sprites;
    [SerializeField] bool _looping = true;
    
    SpriteRenderer _renderer;

    void Awake() {
        _renderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable() {
        if (_looping) {
            StartCoroutine(Loop());
        }
    }

    public void PlayOnce() {
        if (_looping) return;
        StartCoroutine(Play());
    }

    IEnumerator Loop() {
        while (_looping && enabled) {
            yield return Play();
        }    
    }

    IEnumerator Play() {
        foreach (var sprite in _sprites) {
            _renderer.sprite = sprite;
            yield return CoroutineUtils.Wait(_frameTime);
        }
    }
}