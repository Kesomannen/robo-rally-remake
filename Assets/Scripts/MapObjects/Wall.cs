using System.Linq;
using UnityEngine;

public class Wall : StaticObject {
    [SerializeField] Vector2Int[] _openDirections;

    protected override void Awake() {
        base.Awake();
        _openDirections = _openDirections.Select(v => RotateAsObject(v)).ToArray();
    }

    public override bool CanEnter(Vector2Int dir) {
        return GetSide(dir);
    }

    public override bool CanExit(Vector2Int dir) {
        return GetSide(dir);
    }

    bool GetSide(Vector2Int dir) {
        return _openDirections.Contains(dir);
    }
}