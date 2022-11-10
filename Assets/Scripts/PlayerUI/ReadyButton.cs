using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReadyButton : MonoBehaviour, IPointerClickHandler {
    Player _owner => PlayerManager.LocalPlayer;

    public void OnPointerClick(PointerEventData e) {
        if (_owner.Program.Cards.All(r => r != null)) {
            _owner.SerializeRegisters(out var playerIndex, out var registerCardIds);
            ProgrammingPhase.Instance.LockRegisterServerRpc(playerIndex, registerCardIds);
        } else {
            Debug.Log("You must fill all registers before you can ready up.");
        }
    }
}