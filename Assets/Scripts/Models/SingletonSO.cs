using UnityEngine;

public abstract class SingletonSO<T> : ScriptableObject where T : SingletonSO<T> {
    public static T Instance { get; private set; }

    protected virtual void OnEnable() {
        if (Instance == null) {
            Instance = (T) this;
        } else {
            Debug.LogWarning($"An singleton SO of {typeof(T)} was already found!", this);
        }
    }

    protected virtual void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }
}