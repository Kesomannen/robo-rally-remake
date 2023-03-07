using UnityEngine;
using UnityEngine.EventSystems;

public class ToggleButton : MonoBehaviour, IPointerClickHandler {
    [SerializeField] GameObject _target;
    
    public void OnPointerClick(PointerEventData eventData) {
        _target.SetActive(!_target.activeSelf);
    }
}