using UnityEngine;
using UnityEngine.EventSystems;

public class RegisterUI : MonoBehaviour, IPointerClickHandler {
    [SerializeField] int _index;
    [SerializeField] ProgramCard _card;

    bool _isEmpty = true;
    Player _owner => PlayerManager.LocalPlayer;

    static RegisterUI[] _registers { get; } = new RegisterUI[ExecutionPhase.RegisterCount];
    public static RegisterUI GetRegister(int index) => _registers[index];

    void Awake() {
        _registers[_index] = this;
    }

    void OnEnable() {
        _card.gameObject.SetActive(false);
    }

    public bool Place(ProgramCard item) {
        if (!item.Data.CanPlace(_owner, _index)) return false;
        if (!_isEmpty) Remove();

        _isEmpty = false;

        _card.SetData(item.Data);
        _card.gameObject.SetActive(true);
        _owner.Registers[_index] = item.Data;

        return true;
    }

    public void Remove() {
        if (_isEmpty) return;
        _isEmpty = true;

        _owner.Hand.AddCard(_card.Data, CardPlacement.Top);
        _card.gameObject.SetActive(false);
        _owner.Registers[_index] = null;
    }

    public void OnPointerClick(PointerEventData e) {
        Remove();
    }
}