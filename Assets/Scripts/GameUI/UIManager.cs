using System;
using System.Collections;
using UnityEngine;

public class UIManager : Singleton<UIManager> {
    [SerializeField] UITransition _transition;
    [SerializeField] Transform _playerUIParent, _executionUIParent, _shopUIParent;

    public IEnumerator ChangeState(UIState newState) {
        if (_currentState == newState) yield break;
        
        Exit(_currentState);
        _currentState = newState;
        yield return Enter(_currentState);
        
        OnStateChange?.Invoke(_currentState);
    }

    UIState _currentState = UIState.None;

    public static event Action<UIState> OnStateChange;

    IEnumerator Enter(UIState state) {
        yield return state switch {
            UIState.Programming => Programming(),
            UIState.Execution => Execution(),
            UIState.Shop => Shop(),
            UIState.None => None(),
            _ => throw new ArgumentOutOfRangeException()
        };

        IEnumerator None() { yield break; }
        
        IEnumerator Programming() {
            yield return _transition.DoTransition("Programming Phase", () => {
                _playerUIParent.gameObject.SetActive(true);
                UIMap.Instance.gameObject.SetActive(true);

                UIMap.Instance.CanFocus = true;
                UIMap.Instance.ZoomToDefault();
            });
        }

        IEnumerator Execution() {
            yield return _transition.DoTransition("Execution Phase", () => {
                _executionUIParent.gameObject.SetActive(true);
                UIMap.Instance.gameObject.SetActive(true);

                UIMap.Instance.CanFocus = false;
                UIMap.Instance.ZoomToFullscreen(); 
            });
        }

        IEnumerator Shop() {
            yield return _transition.DoTransition("Shop Phase", () => {
                _shopUIParent.gameObject.SetActive(true); 
            });
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