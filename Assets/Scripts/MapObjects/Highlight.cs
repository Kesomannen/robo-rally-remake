using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Highlight : MonoBehaviour {
    [SerializeField] Material _highlightMaterial;
    
    SpriteRenderer _renderer;
    Material _defaultMaterial;

    void Awake() {
        _renderer = GetComponent<SpriteRenderer>();
        _defaultMaterial = _renderer.material;
    }

    void OnEnable() {
        _renderer.material = _highlightMaterial;
    }
    
    void OnDisable() {
        _renderer.material = _defaultMaterial;
    }
}