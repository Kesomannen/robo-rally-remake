using UnityEngine;

public static class VectorHelper {
    public static int Range(this Vector2Int v2) => Random.Range(v2.x, v2.y);

    public static Vector2Int ToVec2Int(this Vector3Int v3) => new(v3.x, v3.y);
    public static Vector3Int ToVec3Int(this Vector2Int v2) => new(v2.x, v2.y);
    public static Vector3 ToVec3(this Vector2Int v2) => new(v2.x, v2.y);

    public static Vector2Int RotateCW(Vector2Int v2) => new(v2.y, -v2.x);
    public static Vector2Int RotateCCW(Vector2Int v2) => -RotateCW(v2);

    public static Vector2Int RotateCW(Vector2Int v2, int steps) {
        for (int i = 0; i < steps; i++) { v2 = RotateCW(v2); }
        return v2;
    }

    public static Vector2Int RotateCCW(Vector2Int v2, int steps) {
        for (int i = 0; i < steps; i++) { v2 = RotateCCW(v2); }
        return v2;
    }

    public static Vector2Int Transform(this Vector2Int v2, int steps) => RotateCCW(v2, steps);

    public static Vector2Int FlipHorizontal(Vector2Int v2) => new(-v2.x, v2.y);
    public static Vector2Int FlipVertical(Vector2Int v2) => new(v2.x, -v2.y);

    public static int GetRotationSteps(float zRot) {
        return Mathf.RoundToInt((zRot / 90) % 4);
    }

    public static TransformRotator GetRotator(Vector3 angles) {
        var flipX = Mathf.RoundToInt((angles.x / 180f) % 2f) != 0;
        var flipY = Mathf.RoundToInt((angles.y / 180f) % 2f) != 0;
        var zRot = GetRotationSteps(angles.z);

        return new TransformRotator(flipX, flipY, zRot);
    }
}

public interface IRotator {
    Vector2Int Rotate(Vector2Int v2);
}

public struct TransformRotator : IRotator {
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
    public int RotZ { get; set; }

    public TransformRotator(bool flipX, bool flipY, int rot) {
        FlipX = flipX;
        FlipY = flipY;
        RotZ = rot;
    }

    public Vector2Int Rotate(Vector2Int v2) {
        if (FlipX) v2 = VectorHelper.FlipHorizontal(v2);
        if (FlipY) v2 = VectorHelper.FlipVertical(v2);
        v2 = VectorHelper.RotateCCW(v2, RotZ);

        return v2;
    }
}