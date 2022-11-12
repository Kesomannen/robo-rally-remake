using UnityEngine;

public abstract class StaticObject : MapObject {
    public override bool IsStatic => true;

    public override bool CanEnter(Vector2Int dir) => true;
    public override bool CanExit(Vector2Int dir) => true;
}