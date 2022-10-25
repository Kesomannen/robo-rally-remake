using System;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
    public static T instance {
        get {
            if (_instaceExists) return _instance;
            else {
                Debug.LogWarning("A script is trying to access the singleton instance, but it doesn't exist yet.");
                return null;
            }
        }
    }

    static T _instance;
    static bool _instaceExists;

    public static event Action<T> OnInstanceCreated;

    protected virtual void Awake() {
        if (_instance == null) {
            _instance = (T) this;
            _instaceExists = true;
            OnInstanceCreated?.Invoke(_instance);
        } else {
            Debug.LogWarning($"An singleton instance of {nameof(T)} was already found, destroying this instance!", gameObject);
            Destroy(this);
        }
    }

    protected virtual void OnDestroy() {
        if (_instance == this) {
            _instance = null;
            _instaceExists = false;
        }
    }
}