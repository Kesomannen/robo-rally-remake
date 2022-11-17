using System;
using UnityEngine;

public class UIManager : Singleton<UIManager> {
    [SerializeField] GameObject _playerUIParent;

    public void ChangeState(UIState newState) {
        if (_currentState == newState) return;
        
        Exit(_currentState);
        _currentState = newState;
        Enter(_currentState);
        
        OnStateChange?.Invoke(_currentState);
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
            throw new NotImplementedException();
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