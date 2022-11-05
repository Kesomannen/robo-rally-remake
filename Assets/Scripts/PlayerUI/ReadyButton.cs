using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReadyButton : MonoBehaviour, IPointerClickHandler {
    Player _owner => PlayerManager.LocalPlayer;

    public void OnPointerClick(PointerEventData e) {
        if (_owner.Registers.All(r => r != null)) {
            var playerIndex = (byte) PlayerManager.Players.IndexOf(_owner);
            var registers = _owner.Registers.Select(c => (byte) c.GetLookupId()).ToArray();
            ProgrammingPhase.Instance.LockRegisterServerRpc(playerIndex, registers);
        } else {
            Debug.Log("You must fill all registers before you can ready up.");
        }
    }
}