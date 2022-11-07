using System.Collections;
using UnityEngine;

public abstract class DynamicObject : MapObject {
    const float _rotationSpeed = 2f;

    public override bool IsStatic => false;
    public override bool CanExit(Vector2Int dir) => false;
    public override bool CanEnter(Vector2Int dir) => false;

    public void RotateInstant(int rotation) {
        Rotation = Mathb.Mod(Rotation + rotation, 4);
        transform.rotation = Quaternion.Euler(0, 0, Rotation * 90);
    }

    public IEnumerator RotateRoutine(int rotation) {
        if (rotation == 0) yield break;

        var delta = rotation < 0 ? -1 : 1;
        var absRotation = Mathf.Abs(rotation);
        
        for (int i = 0; i < absRotation; i++) {
            Rotation = Mathb.Mod(Rotation + delta, 4);
            LeanTween.rotateZ(gameObject, Rotation * 90, 1 / _rotationSpeed);
            yield return Helpers.Wait(1 / _rotationSpeed);
        }
    }
}