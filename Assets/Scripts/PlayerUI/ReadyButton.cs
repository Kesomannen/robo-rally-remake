using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReadyButton : MonoBehaviour, IPointerClickHandler {
    Player Owner => PlayerManager.LocalPlayer;

    public void OnPointerClick(PointerEventData e) {
        if (Owner.Program.Cards.All(r => r != null)) {
            Owner.SerializeRegisters(out var playerIndex, out var registerCardIds);
            ProgrammingPhase.Instance.LockRegisterServerRpc(playerIndex, registerCardIds);
        } else {
            Debug.Log("You must fill all registers before you can ready up.");
        }
    }
}