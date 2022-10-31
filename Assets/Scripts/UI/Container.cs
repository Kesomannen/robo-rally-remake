using UnityEngine;

public abstract class Container<T> : MonoBehaviour {
    public T Data { get; private set; }

    protected abstract void Serialize(T data);

    public void SetData(T data) {
        Data = data;
        Serialize(Data);
    }
}