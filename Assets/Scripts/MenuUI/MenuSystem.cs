using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystem : Singleton<MenuSystem> {
    [SerializeField] MainMenu _mainMenu;
    [SerializeField] JoinGameMenu _joinGameMenu;
    [SerializeField] RoomMenu _roomMenu;
    [SerializeField] Menu _aboutMenu;

    readonly Stack<Menu> _menuStack = new();

    public static event Action MenuChanged;
    
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
        
        MenuChanged?.Invoke();
    }

    public void GoBack() {
        var oldMenu = _menuStack.Pop();
        var newMenu = _menuStack.Peek();
        
        oldMenu.Hide();
        newMenu.Show();
        
        MenuChanged?.Invoke();
    }

    Menu GetMenu(MenuState state) => state switch {
        MenuState.Main => _mainMenu,
        MenuState.JoinGame => _joinGameMenu,
        MenuState.Room => _roomMenu,
        MenuState.About => _aboutMenu,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };
}

public enum MenuState {
    Main,
    JoinGame,
    Room,
    About
}