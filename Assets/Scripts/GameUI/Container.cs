using UnityEngine;

public abstract class Container<T> : MonoBehaviour {
    public T Data { get; private set; }

    protected abstract void Serialize(T player);

    public Container<T> SetContent(T data) {
        if (data.Equals(Data)) return this;
        Data = data;
        Serialize(Data);
        return this;
    }
}