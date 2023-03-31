using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LockInButton : MonoBehaviour, IPointerClickHandler {
    [SerializeField] Image _image;
    [SerializeField] Selectable _selectable;
    [SerializeField] TMP_Text _errorText;
    [SerializeField] InputAction _submitAction;
    [Space]
    [SerializeField] Color _invalidColor;
    [SerializeField] float _shakeDuration;
    [SerializeField] float _shakeMagnitude;
    [Space]
    [SerializeField] SoundEffect _invalidSound;
    [SerializeField] SoundEffect _lockInSound;
    
    [Header("Sprites")] 
    [SerializeField] Sprite _availableSprite;
    [SerializeField] Sprite _availableHighlightedSprite;
    [Space]
    [SerializeField] Sprite _unavailableSprite;
    [SerializeField] Sprite _unavailableHighlightedSprite;
    [Space]
    [SerializeField] Sprite _lockedSprite;

    static Player Owner => PlayerSystem.LocalPlayer;

    enum State {
        Available,
        Locked,
        Unavailable
    }
    
    State _state;
    bool _isAnimating;
    bool _pressed;

    void Awake() {
        Owner.Program.RegisterChanged += RegisterChanged;
        ProgrammingPhase.PlayerLockedIn += PLayerLockedIn;
        ProgrammingPhase.PhaseStarted += PhaseStarted;
        
        UpdateState();
    }

    void OnDestroy() {
        Owner.Program.RegisterChanged -= RegisterChanged;
        ProgrammingPhase.PlayerLockedIn -= PLayerLockedIn;
        ProgrammingPhase.PhaseStarted -= PhaseStarted;
    }

    void OnEnable() {
        _submitAction.Enable();
        _submitAction.performed += OnSubmit;
    }
    
    void OnDisable() {
        _submitAction.Disable();
        _submitAction.performed -= OnSubmit;
    }

    void OnSubmit(InputAction.CallbackContext context) {
        OnPointerClick(null);
    }

    void PhaseStarted() {
        _pressed = false;
        UpdateState();
    }

    void PLayerLockedIn(Player player) => UpdateState();
    void RegisterChanged(int register, ProgramCardData prev, ProgramCardData next) => UpdateState();

    void UpdateState() {
        if (ProgrammingPhase.LocalPlayerLockedIn || _pressed) {
            _state = State.Locked;
        } else {
            _state = Owner.Program.Cards.Any(c => c == null) ? State.Unavailable : State.Available;
        }
        
        var spriteState = _selectable.spriteState;
        switch (_state) {
            case State.Available:
                spriteState.highlightedSprite = _availableHighlightedSprite;
                _image.sprite = _availableSprite;
                break;
            case State.Locked:
                _image.sprite = _lockedSprite;
                break;
            case State.Unavailable:
                spriteState.highlightedSprite = _unavailableHighlightedSprite;
                _image.sprite = _unavailableSprite;
                break;
            default: throw new ArgumentOutOfRangeException();
        }
        _selectable.spriteState = spriteState;
        
        _selectable.interactable = _state != State.Locked;
    }

    public void OnPointerClick(PointerEventData e) {
        if (_isAnimating || ProgrammingPhase.LocalPlayerLockedIn) return;
        
        switch (_state) {
            case State.Locked: return;
            case State.Unavailable:
                InvalidAnimation(); 
                return;
            case State.Available:
                Owner.SerializeRegisters(out var playerIndex, out var registers);
                ProgrammingPhase.Instance.LockRegisterServerRpc(playerIndex, registers);
                _lockInSound.Play();
                _pressed = true;
                UpdateState();
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    void InvalidAnimation() {
        _invalidSound.Play();
        _isAnimating = true;
        
        _image.color = _invalidColor;
        LeanTween.value(gameObject, _image.color, Color.white, _shakeDuration)
            .setOnUpdate(c => _image.color = c);
        
        LeanTween.value(gameObject, Color.white, Color.clear, _shakeDuration * 6)
            .setOnUpdate(c => _errorText.color = c);

        var pos = transform.position;
        LeanTween
            .moveX(gameObject, pos.x + _shakeMagnitude, _shakeDuration)
            .setFrom(pos.x - _shakeMagnitude)
            .setOnComplete(() => {
                LeanTween
                    .moveX(gameObject, pos.x, _shakeDuration / 5)
                    .setEase(LeanTweenType.easeOutBack)
                    .setOnComplete(() => _isAnimating = false);
            })
            .setEase(LeanTweenType.easeShake);
    }
}