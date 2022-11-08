using UnityEngine;
using UnityEngine.EventSystems;

public class RegisterUI : MonoBehaviour, IPointerClickHandler {
    [SerializeField] int _index;
    [SerializeField] Container<ProgramCardData> _cardContainer;

    Player _owner => PlayerManager.LocalPlayer;

    static RegisterUI[] _registers { get; } = new RegisterUI[ExecutionPhase.RegisterCount];
    public static RegisterUI GetRegisterUI(int index) => _registers[index];

    public bool IsEmpty => _owner.Registers[_index] == null;

    void Awake() {
        _registers[_index] = this;
    }

    void OnEnable() {
        _cardContainer.gameObject.SetActive(false);
    }

    public bool Place(Container<ProgramCardData> item) {
        if (!item.Data.CanPlace(_owner, _index)) return false;
        if (!IsEmpty) Remove();

        _cardContainer.SetData(item.Data);
        _cardContainer.gameObject.SetActive(true);
        _owner.Registers[_index] = item.Data;

        return true;
    }

    public void Remove() {
        if (IsEmpty) return;

        if (_owner.Hand.AddCard(_cardContainer.Data, CardPlacement.Top)) {
            _cardContainer.gameObject.SetActive(false);
            _owner.Registers[_index] = null;
        }
    }

    public void OnPointerClick(PointerEventData e) {
        Remove();
    }
}