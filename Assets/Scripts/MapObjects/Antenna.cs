using UnityEngine;

public class Antenna : MapObject, ICanEnterHandler {
    static Antenna _instance;

    public bool Pushable => false;

    public bool CanEnter(Vector2Int enterDir) => false;

    protected override void Awake() {
        if (_instance == null) {
            _instance = this;
        } else {
            Debug.LogWarning($"Duplicate antenna found!", this);
        }
    }

    public static float GetDistance(Vector2Int pos) {
        return Vector2Int.Distance(_instance.GridPos, pos);
    }
}