using UnityEngine;

public class Antenna : StaticObject {
    static Antenna _instance;

    public override bool CanEnter(Vector2Int dir) => false;

    protected override void Awake() {
        base.Awake();
        _instance = this;
    }

    public static float GetDistance(Vector2Int pos) {
        return Vector2Int.Distance(_instance.GridPos, pos);
    }
}