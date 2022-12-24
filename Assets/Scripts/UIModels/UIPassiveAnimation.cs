using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIPassiveAnimation : MonoBehaviour {
    public float _frameTime = 0.1f;
    public Sprite[] _sprites;
    
    Image _image;
    
    void Awake() {
        _image = GetComponent<Image>();
    }
    
    void OnEnable() {
        StartCoroutine(AnimationRoutine());
    }

    WaitUntil _waitUntil;
    
    IEnumerator AnimationRoutine() {
        var i = 0;
        while (true) {
            if (_sprites == null || _sprites.Length == 0) {
                yield return _waitUntil ??= new WaitUntil(() => _sprites is { Length: > 0 });
            }
            
            if (i++ >= _sprites.Length - 1) i = 0;
            _image.sprite = _sprites[i];
            yield return CoroutineUtils.Wait(_frameTime);
        }    
    }
}