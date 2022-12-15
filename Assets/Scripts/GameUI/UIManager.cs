using System;
using UnityEngine;

public class UIManager : Singleton<UIManager> {
    [SerializeField] Transform _playerUIParent, _executionUIParent, _shopUIParent;

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
            UIState.Execution => Execution,
            UIState.Shop => Shop,
            UIState.None => () => { },
            _ => throw new ArgumentOutOfRangeException()
        };

        action();

        void Hand() {
            _playerUIParent.gameObject.SetActive(true);
            UIMap.Instance.gameObject.SetActive(true);

            UIMap.Instance.CanFocus = true;
            UIMap.Instance.ZoomToDefault();
        }

        void Execution() {
            _executionUIParent.gameObject.SetActive(true);
            UIMap.Instance.gameObject.SetActive(true);

            UIMap.Instance.CanFocus = false;
            UIMap.Instance.ZoomToFullscreen();
        }

        void Shop() {
            _shopUIParent.gameObject.SetActive(true);
        }
    }

    void Exit(UIState state) {
        Action exitAction = state switch {
            UIState.Hand => Hand,
            UIState.Execution => Execution,
            UIState.Shop => Shop,
            UIState.None => () => { },
            _ => throw new ArgumentOutOfRangeException()
        };

        exitAction();

        void Hand() {
            _playerUIParent.gameObject.SetActive(false);
            UIMap.Instance.gameObject.SetActive(false);
        }

        void Execution() {
            _executionUIParent.gameObject.SetActive(false);
            UIMap.Instance.gameObject.SetActive(false);
        }

        void Shop() {
            _shopUIParent.gameObject.SetActive(false);
        }
    }
}

public enum UIState {
    Hand,
    Execution,
    Shop,
    None,
}