using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystem : Singleton<MenuSystem> {
    [SerializeField] MainMenu _mainMenu;
    [SerializeField] OptionsMenu _optionsMenu;
    [SerializeField] CreateGameMenu _createGameMenu;
    [SerializeField] JoinGameMenu _joinGameMenu;
    [SerializeField] RoomMenu _roomMenu;

    readonly Stack<Menu> _menuStack = new();

    void Start() {
        PushMenu(MenuState.Main);
    }

    public void PushMenu(MenuState newState) {
        if (_menuStack.Count > 0) {
            _menuStack.Peek().Hide();
        }
        var newMenu = GetMenu(newState);
        newMenu.Show();
        _menuStack.Push(newMenu);
    }

    public void GoBack() {
        var oldMenu = _menuStack.Pop();
        var newMenu = _menuStack.Peek();
        
        oldMenu.Hide();
        newMenu.Show();
    }

    Menu GetMenu(MenuState state) => state switch {
        MenuState.Main => _mainMenu,
        MenuState.Options => _optionsMenu,
        MenuState.CreateGame => _createGameMenu,
        MenuState.JoinGame => _joinGameMenu,
        MenuState.Room => _roomMenu,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };
}

public enum MenuState {
    None,
    Main,
    Options,
    CreateGame,
    JoinGame,
    Room,
}