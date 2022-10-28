using UnityEngine;

public class Antenna : DynamicObject {
    static Antenna _instance;

    protected override void Awake() {
        base.Awake();
        _instance = this;
    }

    public static float GetDistance(Vector2Int pos) {
        return Vector2Int.Distance(_instance.GridPos, pos);
    }
}