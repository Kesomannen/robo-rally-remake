using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : Menu {
    const string HasSeenTutorialKey = "HasSeenTutorial";
    
    void Awake() {
        var hasSeenTutorial = PlayerPrefs.GetInt(HasSeenTutorialKey, 0);
        
        if (hasSeenTutorial > 0) return;
        PlayerPrefs.SetInt(HasSeenTutorialKey, 1);
        PlayerPrefs.Save();
            
        Tutorial();
    }

    public async void CreateGame() {
        var lobbyData = new LobbyData {
            MapID = (byte) LobbySystem.Instance.AvailableMaps.GetRandom().GetLookupId(),
            MaxPlayers = LobbySystem.MaxPlayers,
            IsPrivate = false
        };
        
        bool successful;
        using (new LoadingScreen("Creating lobby...")) {
             successful = await LobbySystem.Instance.CreateLobby(lobbyData);
        }
        if (!successful) return;
        MenuSystem.Instance.PushMenu(MenuState.Room);
    }

    public void Tutorial() {
        StartCoroutine(Routine());
        
        IEnumerator Routine() {
            using (new LoadingScreen("Loading tutorial...")) {
                NetworkSystem.CurrentGameType = NetworkSystem.GameType.Tutorial;
                yield return SceneManager.LoadSceneAsync(NetworkSystem.GameScene);
            }
        }
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