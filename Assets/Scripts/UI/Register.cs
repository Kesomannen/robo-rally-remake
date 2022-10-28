using UnityEngine;
using UnityEngine.EventSystems;

public class Register : MonoBehaviour, IPointerClickHandler {
    [SerializeField] int _index;

    ProgramCard _card;
    Player _owner = NetworkSystem.LocalPlayer;
    bool _isEmpty = true;

    public void OnPointerClick(PointerEventData eventData) {
        if (_isEmpty) return;
        if (eventData.button == PointerEventData.InputButton.Left) {
            _isEmpty = true;
            var data = _card.Data;

            // give card back to hand
            _card.gameObject.SetActive(false);
            _owner.Registers[_index] = null;
            _owner.Hand.AddCard(data, CardPlacement.Top);
        }
    }

    public bool SetCard(ProgramCardData card) {
        if (!_isEmpty) return false;
        _isEmpty = false;
        _card.gameObject.SetActive(true);
        _card.SetData(card);
        _owner.Registers[_index] = card;
        return true;
    }
}