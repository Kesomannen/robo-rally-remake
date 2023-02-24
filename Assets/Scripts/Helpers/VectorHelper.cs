using UnityEngine;

public static class VectorHelper {
    public static Vector2Int ToVec2Int(this Vector3Int v3) => new(v3.x, v3.y);
    public static Vector3Int ToVec3Int(this Vector2Int v2) => new(v2.x, v2.y);

    static Vector2Int RotateCW(Vector2Int v2) => new(v2.y, -v2.x);
    static Vector2Int RotateCCW(Vector2Int v2) => -RotateCW(v2);

    public static Vector2Int RotateCCW(Vector2Int v2, int steps) {
        for (var i = 0; i < steps; i++) { v2 = RotateCCW(v2); }
        return v2;
    }

    public static Vector2Int Transform(this Vector2Int v2, int steps) => RotateCCW(v2, steps);

    public static Vector2Int FlipHorizontal(Vector2Int v2) => new(-v2.x, v2.y);
    public static Vector2Int FlipVertical(Vector2Int v2) => new(v2.x, -v2.y);
    public static int GetRotationSteps(float zRot) => Mathf.RoundToInt(zRot / 90 % 4);
}