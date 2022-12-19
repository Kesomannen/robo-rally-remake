using UnityEngine;
using UnityEngine.EventSystems;

public class UISounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler {
    [SerializeField] Optional<SoundEffect> _onHover, _onClick;

    public void OnPointerEnter(PointerEventData e) {
        if (_onHover.Enabled) _onHover.Value.Play(AudioTrack.UI);
    }
    
    public void OnPointerClick(PointerEventData e) {
        if (_onClick.Enabled) _onClick.Value.Play(AudioTrack.UI);
    }
}