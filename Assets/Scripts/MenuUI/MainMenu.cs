using System;
using UnityEngine;

public class MainMenu : Menu {
    public void CreateGame() {
        MenuSystem.Instance.PushMenu(MenuState.CreateGame);
    }

    public void JoinGame() {
        MenuSystem.Instance.PushMenu(MenuState.JoinGame);
    }

    public void Options() {
        MenuSystem.Instance.PushMenu(MenuState.Options);
    }
    
    public void Quit() {
        Application.Quit();
    }

    public override void Back() {
        Quit();
    }
}