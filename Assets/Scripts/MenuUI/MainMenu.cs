using UnityEngine;

public class MainMenu : Menu {
    public async void CreateGame() {
        var lobbyData = new LobbyData {
            MapID = 0,
            MaxPlayers = (byte) GameSettings.Instance.MaxPlayers,
            IsPrivate = false
        };

        await LobbySystem.Instance.CreateLobby(lobbyData);
        MenuSystem.Instance.PushMenu(MenuState.Room);
    }

    public void JoinGame() {
        MenuSystem.Instance.PushMenu(MenuState.JoinGame);
    }

    public void Options() {
        MenuSystem.Instance.PushMenu(MenuState.Options);
    }
    
    public void About() {
        MenuSystem.Instance.PushMenu(MenuState.About);
    }
    
    public void Quit() {
        Application.Quit();
    }

    public override void Back() {
        Quit();
    }
}