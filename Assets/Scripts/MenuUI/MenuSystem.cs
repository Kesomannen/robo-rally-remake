using System;
using UnityEngine;

public class MenuSystem : Singleton<MenuSystem> {
    [SerializeField] MainMenu _mainMenu;
    [SerializeField] OptionsMenu _optionsMenu;
    [SerializeField] CreateGameMenu _createGameMenu;
    [SerializeField] JoinGameMenu _joinGameMenu;
    [SerializeField] RoomMenu _roomMenu;

    public MenuState CurrentState { get; private set; } = MenuState.Main;
    public Menu CurrentMenu { get; private set; }

    public event Action<MenuState, MenuState> OnMenuStateChanged;

    public void ChangeMenu(MenuState newState) {
        var oldState = CurrentState;
        var oldMenu = CurrentMenu;

        CurrentState = newState;
        CurrentMenu = GetMenu(newState);

        oldMenu?.Hide();
        CurrentMenu.Show();

        OnMenuStateChanged?.Invoke(oldState, newState);
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
    Main,
    Options,
    CreateGame,
    JoinGame,
    Room,
    None,
}