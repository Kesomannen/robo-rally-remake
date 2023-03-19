using UnityEngine;

public class MainMenu : Menu {
    public async void CreateGame() {
        var lobbyData = new LobbyData {
            MapID = (byte) MapData.GetRandom().GetLookupId(),
            MaxPlayers = (byte) GameSettings.Instance.MaxPlayers,
            IsPrivate = false
        };
        
        bool successful;
        using (new LoadingScreen("Creating lobby...")) {
             successful = await LobbySystem.Instance.CreateLobby(lobbyData);
        }
        if (!successful) return;
        MenuSystem.Instance.PushMenu(MenuState.Room);
    }

    public void JoinGame() {
        MenuSystem.Instance.PushMenu(MenuState.JoinGame);
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