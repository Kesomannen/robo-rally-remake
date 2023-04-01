using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler {
    [SerializeField] bool _disableWhenUnInteractable = true;
    [SerializeField] Optional<SoundEffect> _onHover, _onClick;

    Selectable _selectable;
    bool Enabled => !_disableWhenUnInteractable || _selectable == null || _selectable.interactable;
    
    void Awake() {
        _selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData e) {
        if (_onHover.Enabled && Enabled) _onHover.Value.Play(AudioTrack.UI);
    }
    
    public void OnPointerClick(PointerEventData e) {
        if (_onClick.Enabled && Enabled) _onClick.Value.Play(AudioTrack.UI);
    }
}