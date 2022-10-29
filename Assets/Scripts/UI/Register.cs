using UnityEngine;
using UnityEngine.EventSystems;

public class Register : MonoBehaviour, IPointerClickHandler {
    [SerializeField] int _index;
    [SerializeField] ProgramCard _card;

    bool _isEmpty = true;
    Player _owner => NetworkSystem.LocalPlayer;

    public ProgramCard Card => _card;
    public bool IsEmpty => _isEmpty;

    void Awake() {
        _card.gameObject.SetActive(false);
    }

    void Start() {
        _owner.Registers[_index] = this;    
    }

    public bool Place(ProgramCard item) {
        if (!_isEmpty) Remove();
        if (!item.Data.CanPlace(_owner, _index)) return false;

        _isEmpty = false;

        _card.SetData(item.Data);
        _card.gameObject.SetActive(true);

        ProgrammingPhase.RefreshRegisterState();

        return true;
    }

    public void Remove() {
        if (_isEmpty) return;
        _isEmpty = true;

        _owner.Hand.AddCard(_card.Data, CardPlacement.Top);
        _card.gameObject.SetActive(false);
    }

    public void Discard() {
        if (_isEmpty) return;
        _isEmpty = true;

        _owner.DiscardPile.AddCard(_card.Data, CardPlacement.Top);
        _card.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData e) {
        Remove();
    }
}