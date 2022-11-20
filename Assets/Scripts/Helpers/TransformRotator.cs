using UnityEngine;

public struct TransformRotator : IRotator {
    public Vector2Int Identity => Rotate(Vector2Int.right);
    
    readonly bool _flipX;
    readonly bool _flipY;
    
    public int RotZ {
        get => _rotZ;
        set => _rotZ = Mathb.Mod(value, 4);
    }

    int _rotZ;

    public TransformRotator(Vector3 angles){
        _rotZ = VectorHelper.GetRotationSteps(angles.z);
        _flipX = Mathf.RoundToInt((angles.y / 180f) % 2f) != 0;
        _flipY = Mathf.RoundToInt((angles.x / 180f) % 2f) != 0;
    }

    public Vector2Int Rotate(Vector2Int v2) {
        v2 = VectorHelper.RotateCCW(v2, RotZ);
        if (_flipX) v2 = VectorHelper.FlipHorizontal(v2);
        if (_flipY) v2 = VectorHelper.FlipVertical(v2);

        return v2;
    }
}

public interface IRotator {
    Vector2Int Rotate(Vector2Int v2);
    Vector2Int Identity { get; }
}