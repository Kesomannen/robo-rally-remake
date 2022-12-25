using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
    public static T Instance {
        get{
            if (_instanceExists) return _instance;
            Debug.LogWarning($"A script is trying to access the singleton instance {typeof(T)}, but it doesn't exist yet.");
            return null;
        }
    }

    static T _instance;
    static bool _instanceExists;

    protected virtual void Awake() {
        if (!_instanceExists) {
            _instanceExists = true;
            _instance = (T) this;
        } else {
            Debug.LogWarning($"An singleton instance of {typeof(T)} was already found, destroying this instance!", gameObject);
            Destroy(this);
        }
    }

    protected virtual void OnDestroy() {
        if (_instance != this) return;
        _instance = null;
        _instanceExists = false;
    }
}