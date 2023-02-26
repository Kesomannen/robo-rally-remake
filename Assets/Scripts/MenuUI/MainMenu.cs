using System.Collections;
using UnityEngine;

public class MainMenu : Menu {
    public void CreateGame() {
        var lobbyData = new LobbyData {
            MapID = 0,
            MaxPlayers = (byte) GameSettings.Instance.MaxPlayers,
            IsPrivate = false
        };
        
        StartCoroutine(CreateGameRoutine());

        IEnumerator CreateGameRoutine() {
            var task = LobbySystem.Instance.CreateLobby(lobbyData);
            using (new LoadScreen("Creating lobby...")) {
                while (!task.IsCompleted) yield return null;
            }
            
            if (task.IsFaulted) {
                Debug.LogError(task.Exception);
            }
            MenuSystem.Instance.PushMenu(MenuState.Room);
        }
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