using UnityEngine;
using UnityEngine.UI;

public class CreateGameMenu : Menu {
    [SerializeField] Toggle _privateCheckbox;
    
    public async void Submit() {
        var lobbyData = new LobbyData {
            MapID = 0,
            MaxPlayers = (byte) GameSettings.Instance.MaxPlayers,
            IsPrivate = _privateCheckbox.isOn,
        };

        await LobbySystem.Instance.CreateLobby(lobbyData);
        MenuSystem.Instance.PushMenu(MenuState.Room);
    }
}