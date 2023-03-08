using UnityEngine;

public class TransformRotator : IRotator {
    public Vector2Int Identity => Rotate(Vector2Int.right);
    
    public readonly bool FlipX;
    public readonly bool FlipY;
    
    public int RotZ {
        get => _rotZ;
        set => _rotZ = Mathb.Mod(value, 4);
    }

    int _rotZ;

    public TransformRotator(Vector3 angles) {
        _rotZ = VectorHelper.GetRotationSteps(angles.z);
        FlipX = Mathf.RoundToInt((angles.y / 180f) % 2f) != 0;
        FlipY = Mathf.RoundToInt((angles.x / 180f) % 2f) != 0;
    }

    public Vector2Int Rotate(Vector2Int v2) {
        v2 = VectorHelper.RotateCCW(v2, RotZ);
        if (FlipX) v2 = VectorHelper.FlipHorizontal(v2);
        if (FlipY) v2 = VectorHelper.FlipVertical(v2);

        return v2;
    }
}

public interface IRotator {
    Vector2Int Rotate(Vector2Int v2);
    Vector2Int Identity { get; }
}