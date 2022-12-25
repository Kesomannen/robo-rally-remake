using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateGameMenu : Menu {
    [SerializeField] Toggle _privateCheckbox;
    
    public async void Submit() {
        var lobbyData = new LobbyData {
            MapID = 0,
            MaxPlayers = (byte) GameSettings.Instance.MaxPlayers,
            IsPrivate = _privateCheckbox.isOn,
        };

        var success = await LobbySystem.Instance.CreateLobby(lobbyData);

        if (success) {
            MenuSystem.Instance.PushMenu(MenuState.Room);
        } else {
            Debug.LogWarning("Failed to create lobby");
        }
    }
}