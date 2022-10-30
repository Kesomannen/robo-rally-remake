using System;
using UnityEngine;

public class UIManager : Singleton<UIManager> {
    [SerializeField] GameObject _playerUIParent;

    public UIState CurrentState {
        get => _currentState;
        set {
            if (_currentState == value) return;
            Exit(_currentState);
            _currentState = value;
            Enter(_currentState);
            OnStateChange?.Invoke(_currentState);
        }
    }

    UIState _currentState = UIState.None;

    public static event Action<UIState> OnStateChange;

    void Enter(UIState state) {
        Action action = state switch {
            UIState.MainMenu => throw new NotImplementedException(),
            UIState.Lobby => throw new NotImplementedException(),
            UIState.GameHand => GameHand,
            UIState.GameMap => GameMap,
            UIState.GameShop => throw new NotImplementedException(),
            UIState.None => () => { },
            _ => throw new NotImplementedException()
        };

        action();

        void GameHand() {
            _playerUIParent.SetActive(true);
            UIMap.Instance.gameObject.SetActive(true);

            UIMap.Instance.CanFocus = true;
            UIMap.Instance.ZoomToDefault();
        }

        void GameMap() {
            UIMap.Instance.gameObject.SetActive(true);

            UIMap.Instance.CanFocus = false;
            UIMap.Instance.ZoomToFullscreen();
        }
    }

    void Exit(UIState state) {
        Action action = state switch {
            UIState.MainMenu => throw new NotImplementedException(),
            UIState.Lobby => throw new NotImplementedException(),
            UIState.GameHand => GameHand,
            UIState.GameMap => GameMap,
            UIState.GameShop => throw new NotImplementedException(),
            UIState.None => () => { },
            _ => throw new NotImplementedException()
        };

        action();

        void GameHand() {
            _playerUIParent.SetActive(false);
            UIMap.Instance.gameObject.SetActive(false);
        }

        void GameMap() {
            UIMap.Instance.gameObject.SetActive(false);
        }
    }
}

public enum UIState {
    MainMenu,
    Lobby,
    GameHand,
    GameMap,
    GameShop,
    None,
}