using UnityEngine;

public abstract class MapObject : MonoBehaviour {
    public abstract bool IsStatic { get; }
    public abstract bool CanEnter(Vector2Int dir);

    public virtual void OnEnter(DynamicObject dynamic) { }
    public virtual void OnExit(DynamicObject dynamic) { }

    public int Rotation { get; protected set; }

    public Vector2Int GridPos => MapSystem.Instance.GetGridPos(this);

    public Vector2Int RotateVector(Vector2Int v2) {
        return VectorHelper.RotateCCW(v2, Rotation);
    }

    protected virtual void Awake() {
        Rotation = VectorHelper.GetRotationSteps(transform);
    }
}