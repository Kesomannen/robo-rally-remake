using System;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
    public static T instance {
        get {
            if (_instanceExists) return _instance;
            else {
                Debug.LogWarning($"A script is trying to access the singleton instance {typeof(T)}, but it doesn't exist yet.");
                return null;
            }
        }
    }

    static T _instance;
    static bool _instanceExists;

    public static event Action<T> OnInstanceCreated;

    protected virtual void Awake() {
        if (!_instanceExists) {
            _instanceExists = true;
            _instance = (T) this;
            OnInstanceCreated?.Invoke(_instance);
        } else {
            Debug.LogWarning($"An singleton instance of {typeof(T)} was already found, destroying this instance!", gameObject);
            Destroy(this);
        }
    }

    protected virtual void OnDestroy() {
        if (_instance == this) {
            _instance = null;
            _instanceExists = false;
        }
    }
}