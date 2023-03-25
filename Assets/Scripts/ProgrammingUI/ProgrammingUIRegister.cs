using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProgrammingUIRegister : MonoBehaviour, IPointerClickHandler {
    [SerializeField] int _index;
    [SerializeField] Container<ProgramCardData> _cardContainer;
    [SerializeField] SoundEffect _onPlaceSound, _onRemoveSound;
    [SerializeField] GameObject _lockedOverlay;

    static Player Owner => PlayerSystem.LocalPlayer;

    static readonly ProgrammingUIRegister[] _registers = new ProgrammingUIRegister[ExecutionPhase.RegisterCount];
    public static ProgrammingUIRegister GetRegister(int index) => _registers[index];

    static bool _locked;
    public static bool Locked {
        get => _locked;
        set {
            _locked = value;
            LockedChanged?.Invoke(value);
        }
    }

    public bool IsEmpty => Owner.Program[_index] == null;

    static event Action<bool> LockedChanged; 

    void Awake() {
        _registers[_index] = this;
    }

    void OnEnable() {
        _cardContainer.gameObject.SetActive(false);
    }

    void Start() {
        Owner.Program.RegisterChanged += OnRegisterChanged;
        LockedChanged += OnLockedChanged;
    }
    
    void OnDestroy() {
        Owner.Program.RegisterChanged -= OnRegisterChanged;
        LockedChanged -= OnLockedChanged;
    }
    
    void OnLockedChanged(bool locked) {
        _lockedOverlay.SetActive(locked);
    }

    void OnRegisterChanged(int index, ProgramCardData prev, ProgramCardData next) {
        if (!enabled) return;
        if (index != _index) return;

        if (next == null) {
            _cardContainer.gameObject.SetActive(false);
        } else {
            _cardContainer.SetContent(next);
            _cardContainer.gameObject.SetActive(true);
        }
    }


    public bool Place(Container<ProgramCardData> item) {
        if (Locked || !item.Content.CanPlace(Owner, _index)) return false;
        if (!IsEmpty) return false;

        _cardContainer.SetContent(item.Content);
        _cardContainer.gameObject.SetActive(true);
        Owner.Program.SetRegister(_index, item.Content);
        
        _onPlaceSound.Play();
        return true;
    }

    void Remove() {
        if (Locked || IsEmpty || !Owner.Hand.AddCard(_cardContainer.Content, CardPlacement.Top)) return;

        _cardContainer.gameObject.SetActive(false);
        Owner.Program.SetRegister(_index, null);
        
        _onRemoveSound.Play();
    }

    public void OnPointerClick(PointerEventData e) {
        Remove();
    }
}