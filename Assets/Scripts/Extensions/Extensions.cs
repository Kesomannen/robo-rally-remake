using UnityEngine;

public static class Extensions {
    public static Vector3Int ToVec3Int(this Vector2Int vector2) {
        return new Vector3Int(vector2.x, vector2.y);
    }

    public static Vector3 ToVec3(this Vector2Int vector2) {
        return new Vector3(vector2.x, vector2.y);
    }

    public static Vector2Int ToVec2Int(this Vector3Int vector3) {
        return new Vector2Int(vector3.x, vector3.y);
    }

    public static Vector2Int RotateClockwise(this Vector2Int vector2) {
        return new Vector2Int(vector2.y, -vector2.x);
    }

    public static Vector2Int RotateClockwise(this Vector2Int vector2, int steps) {
        var v = vector2;
        for (int i = 0; i < steps; i++) {
            v = vector2.RotateClockwise();
        }
        return v;
    }

    public static Vector2Int RotateCounterClockwise(this Vector2Int vector2) {
        return new Vector2Int(-vector2.y, vector2.x);
    }

    public static Vector2Int RotateCounterClockwise(this Vector2Int vector2, int steps) {
        var v = vector2;
        for (int i = 0; i < steps; i++) {
            v = vector2.RotateCounterClockwise();
        }
        return v;
    }
}