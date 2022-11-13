using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RegisterUI : MonoBehaviour, IPointerClickHandler {
    [SerializeField] int _index;
    [SerializeField] Container<ProgramCardData> _cardContainer;

    Player _owner => PlayerManager.LocalPlayer;

    static RegisterUI[] _registers { get; } = new RegisterUI[ExecutionPhase.RegisterCount];
    public static RegisterUI GetRegisterUI(int index) => _registers[index];

    public bool IsEmpty => _owner.Program[_index] == null;

    void Awake() {
        _registers[_index] = this;
    }

    void OnEnable() {
        _cardContainer.gameObject.SetActive(false);
        _owner.Program.OnRegisterChanged += OnRegisterChanged;
    }
    void OnDisable() {
        _owner.Program.OnRegisterChanged -= OnRegisterChanged;
    }

    void OnRegisterChanged(int index, ProgramCardData prev, ProgramCardData next) {
        if (index != _index) return;

        if (next == null) {
            _cardContainer.gameObject.SetActive(false);
        } else {
            _cardContainer.SetData(next);
            _cardContainer.gameObject.SetActive(true);
        }
    }


    public bool Place(Container<ProgramCardData> item) {
        if (!IsEmpty) return false;
        if (!item.Data.CanPlace(_owner, _index)) return false;

        _cardContainer.SetData(item.Data);
        _cardContainer.gameObject.SetActive(true);
        _owner.Program.SetCard(_index, item.Data);

        return true;
    }

    public void Remove() {
        if (IsEmpty) return;

        if (_owner.Hand.AddCard(_cardContainer.Data, CardPlacement.Top)) {
            _cardContainer.gameObject.SetActive(false);
            _owner.Program.SetCard(_index, null);
        }
    }

    public void OnPointerClick(PointerEventData e) {
        Remove();
    }
}