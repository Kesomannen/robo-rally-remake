using UnityEngine;

public static class VectorHelper {
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

    public static int GetRotationSteps(Transform transform) {
        return Mathf.RoundToInt((transform.eulerAngles.z / 90) % 4);
    }

    public static Vector2Int RotateAsTransform(Vector2Int v2, Transform transform) {
        return RotateCCW(v2, GetRotationSteps(transform));
    }

    public static int Range(this Vector2Int v2) => Random.Range(v2.x, v2.y);
}