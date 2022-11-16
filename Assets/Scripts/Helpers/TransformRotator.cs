using UnityEngine;

public struct TransformRotator : IRotator {
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
    public int RotZ {
        get => _rotZ;
        set => _rotZ = Mathb.Mod(value, 4);
    }

    int _rotZ;

    public TransformRotator(bool flipX, bool flipY, int rot) {
        FlipX = flipX;
        FlipY = flipY;
        _rotZ = rot;
    }

    public Vector2Int Rotate(Vector2Int v2) {
        if (FlipX) v2 = VectorHelper.FlipHorizontal(v2);
        if (FlipY) v2 = VectorHelper.FlipVertical(v2);
        v2 = VectorHelper.RotateCCW(v2, RotZ);

        return v2;
    }

    public static TransformRotator GetRotator(Vector3 angles) {
        var flipX = Mathf.RoundToInt((angles.x / 180f) % 2f) != 0;
        var flipY = Mathf.RoundToInt((angles.y / 180f) % 2f) != 0;
        var zRot = VectorHelper.GetRotationSteps(angles.z);

        return new TransformRotator(flipX, flipY, zRot);
    }
}

public interface IRotator {
    Vector2Int Rotate(Vector2Int v2);
}