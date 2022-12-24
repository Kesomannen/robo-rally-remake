using UnityEngine;

public abstract class Container<T> : MonoBehaviour {
    public T Content { get; private set; }

    protected abstract void Serialize(T data);

    public Container<T> SetContent(T data) {
        if (data.Equals(Content)) return this;
        Content = data;
        Serialize(Content);
        return this;
    }
}