using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "GameEvent", order = 0)]
public class GameEvent : ScriptableObject {
    Action _event;

    public void Invoke() {
        _event?.Invoke();
    }

    public void AddListener(Action listener) {
        _event += listener;
    }

    public void RemoveListener(Action listener) {
        _event -= listener;
    }
}