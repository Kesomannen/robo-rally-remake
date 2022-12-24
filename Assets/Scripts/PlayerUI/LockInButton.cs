using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LockInButton : MonoBehaviour, IPointerClickHandler {
    [SerializeField] Image _image;
    [SerializeField] Selectable _selectable;
    [SerializeField] Color _invalidColor;
    [SerializeField] float _shakeDuration;
    [SerializeField] float _shakeMagnitude;

    bool _canClick = true;
    
    public bool CanClick {
        get => _canClick;
        set {
            _canClick = value;
            _selectable.interactable = value;
        }
    }
    
    static Player Owner => PlayerSystem.LocalPlayer;

    public void OnPointerClick(PointerEventData e) {
        if (!CanClick) return;
        CanClick = false;
        
        if (Owner.Program.Cards.All(r => r != null)) {
            Owner.SerializeRegisters(out var playerIndex, out var registerCardIds);
            ProgrammingPhase.Instance.LockRegisterServerRpc(playerIndex, registerCardIds);
        } else {
            InvalidAnimation();
        }
    }
    
    void InvalidAnimation() {
        _image.color = _invalidColor;
        LeanTween.value(gameObject, _image.color, Color.white, _shakeDuration)
            .setOnUpdate(c => _image.color = c);

        var pos = transform.position;
        LeanTween
            .moveX(gameObject, pos.x + _shakeMagnitude, _shakeDuration)
            .setFrom(pos.x - _shakeMagnitude)
            .setOnComplete(() => {
                LeanTween
                    .moveX(gameObject, pos.x, _shakeDuration / 5)
                    .setEase(LeanTweenType.easeOutBack)
                    .setOnComplete(() => CanClick = true);
            })
            .setEase(LeanTweenType.easeShake);
    }
}