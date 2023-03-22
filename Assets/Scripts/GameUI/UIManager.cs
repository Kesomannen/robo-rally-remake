using System;
using System.Collections;
using UnityEngine;

public class UIManager : Singleton<UIManager> {
    [SerializeField] Transition _transition;
    [SerializeField] Transform _playerUIParent, _executionUIParent, _shopUIParent;

    public IEnumerator ChangeState(UIState newState) {
        if (_currentState == newState) yield break;
        
        var text = newState switch {
            UIState.Programming => "Programming Phase",
            UIState.Execution => "Execution Phase",
            UIState.Shop => "Shop Phase",
            UIState.None => "None",
            _ => throw new ArgumentOutOfRangeException()
        };
        yield return _transition.DoTransition(text, () => {
            Exit(_currentState);
            Enter(newState);
            _currentState = newState;
        });

        OnStateChange?.Invoke(_currentState);
    }

    UIState _currentState = UIState.None;

    public static event Action<UIState> OnStateChange;

    void Enter(UIState state) {
        Action action = state switch {
            UIState.Programming => Programming,
            UIState.Execution => Execution,
            UIState.Shop => Shop,
            UIState.None => None,
            _ => throw new ArgumentOutOfRangeException()
        };
        action();

        void None() { }
        
        void Programming() {
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
            UIMap.Instance.gameObject.SetActive(false);
            _shopUIParent.gameObject.SetActive(true); 
        }
    }

    void Exit(UIState state) {
        Action exitAction = state switch {
            UIState.Programming => Programming,
            UIState.Execution => Execution,
            UIState.Shop => Shop,
            UIState.None => () => { },
            _ => throw new ArgumentOutOfRangeException()
        };

        exitAction();

        void Programming() {
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
    Programming,
    Execution,
    Shop,
    None,
}