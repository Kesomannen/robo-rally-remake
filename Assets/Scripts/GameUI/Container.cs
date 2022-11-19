using UnityEngine;

public abstract class Container<T> : MonoBehaviour {
    public T Data { get; private set; }

    protected abstract void Serialize(T player);

    public void SetContent(T data) {
        if (data.Equals(Data)) return;
        Data = data;
        Serialize(Data);
    }
}