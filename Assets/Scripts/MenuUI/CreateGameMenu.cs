using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateGameMenu : Menu {
    [SerializeField] Toggle _privateCheckbox;

    protected override MenuState PreviousMenuState => MenuState.Main;

    public async void Submit() {
        var lobbyData = new LobbyData {
            MapID = 1,
            MaxPlayers = (byte) GameSettings.Instance.MaxPlayers,
            IsPrivate = _privateCheckbox.isOn,
        };

        var success = await LobbySystem.Instance.CreateLobby(lobbyData);

        if (success) {
            MenuSystem.Instance.ChangeMenu(MenuState.Room);
        } else {
            Debug.LogWarning("Failed to create lobby");
        }
    }
}