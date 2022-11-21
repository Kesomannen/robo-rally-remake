using System.Linq;
using UnityEngine;

public class Wall : MapObject, ICanEnterExitHandler, ITooltipable {
    [SerializeField] Vector2Int[] _openDirections;

    public bool Pushable => false;
    
    public string Header => "Wall";
    public string Description => "A wall. You can't go through it.";

    protected override void Awake() {
        base.Awake();
        // Do initial rotation and subscribe to further changes
        _openDirections = _openDirections.Select(v => Rotator.Rotate(v)).ToArray();
        OnRotationChanged += s => {
            _openDirections = _openDirections.Select(v => v.Transform(s)).ToArray();
        };
    }

    public bool CanEnter(Vector2Int dir) => GetSide(dir);
    public bool CanExit(Vector2Int dir) => GetSide(dir);

    bool GetSide(Vector2Int dir) {
        return _openDirections.Contains(dir);
    }
}