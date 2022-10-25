using UnityEngine;

public abstract class MapObject : MonoBehaviour {
    public abstract bool IsStatic { get; }
    public abstract bool CanEnter(Vector2Int dir);

    public virtual void OnEnter(MapObject dynamic) { }
    public virtual void OnExit(MapObject dynamic) { }

    public Vector2Int GetGridPos() {
        return MapSystem.instance.GetGridPos(this);
    }
}