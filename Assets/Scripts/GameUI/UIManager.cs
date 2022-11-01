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
            UIState.Hand => Hand,
            UIState.Map => Map,
            UIState.Shop => Shop,
            UIState.None => () => { },
            _ => throw new NotImplementedException()
        };

        action();

        void Hand() {
            _playerUIParent.SetActive(true);
            UIMap.Instance.gameObject.SetActive(true);

            UIMap.Instance.CanFocus = true;
            UIMap.Instance.ZoomToDefault();
        }

        void Map() {
            UIMap.Instance.gameObject.SetActive(true);

            UIMap.Instance.CanFocus = false;
            UIMap.Instance.ZoomToFullscreen();
        }

        void Shop() {

        }
    }

    void Exit(UIState state) {
        Action exitAction = state switch {
            UIState.Hand => Hand,
            UIState.Map => Map,
            UIState.Shop => Shop,
            UIState.None => () => { },
            _ => throw new NotImplementedException()
        };

        exitAction();

        void Hand() {
            _playerUIParent.SetActive(false);
            UIMap.Instance.gameObject.SetActive(false);
        }

        void Map() {
            UIMap.Instance.gameObject.SetActive(false);
        }

        void Shop() {
            throw new NotImplementedException();
        }
    }
}

public enum UIState {
    Hand,
    Map,
    Shop,
    None,
}