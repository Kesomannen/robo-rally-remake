using System;
using UnityEngine;

public class MainMenu : Menu {
    protected override MenuState PreviousMenuState => MenuState.None;

    public void CreateGame() {
        MenuSystem.Instance.ChangeMenu(MenuState.CreateGame);
    }

    public void JoinGame() {
        MenuSystem.Instance.ChangeMenu(MenuState.JoinGame);
    }

    public void Options() {
        MenuSystem.Instance.ChangeMenu(MenuState.Options);
    }
    
    public void Quit() {
        Application.Quit();
    }

    public override void Back() {
        Quit();
    }
}