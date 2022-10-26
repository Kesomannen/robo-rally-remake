using System;
using System.Collections.Generic;
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

    public static Vector2Int RotateCW(this Vector2Int vector2) {
        return new Vector2Int(vector2.y, -vector2.x);
    }

    public static Vector2Int RotateCW(this Vector2Int vector2, int steps) {
        for (int i = 0; i < steps; i++) {
            vector2 = vector2.RotateCW();
        }
        return vector2;
    }

    public static Vector2Int RotateCCW(this Vector2Int vector2) {
        return new Vector2Int(-vector2.y, vector2.x);
    }

    public static Vector2Int RotateCCW(this Vector2Int vector2, int steps) {
        for (int i = 0; i < steps; i++) {
            vector2 = vector2.RotateCCW();
        }
        return vector2;
    }

    public static Vector2Int RotateAsTransform(this Vector2Int vector2, Transform transform) {
        return vector2.RotateCCW(GetRotationSteps(transform));
    }

    public static int GetRotationSteps(this Transform transform) {
        return Mathf.RoundToInt((transform.eulerAngles.z / 90) % 4);
    }

    public static TValue EnforceKey<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
    where TValue : struct {
        if (!dict.ContainsKey(key)) {
            dict.Add(key, defaultValue);
        }
        return dict[key];
    }

    public static TValue EnforceKey<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> getDefault)
    where TValue : class {
        if (!dict.ContainsKey(key)) {
            dict.Add(key, getDefault());
        }
        return dict[key];
    }
}