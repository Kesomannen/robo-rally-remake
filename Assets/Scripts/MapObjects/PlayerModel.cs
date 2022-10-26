using UnityEngine;

public class PlayerModel : MapObject {
    public override bool IsStatic => false;
    public override bool CanEnter(Vector2Int dir) => false;

    void Update() {
        if (Input.GetKeyDown(KeyCode.W)) {
            Scheduler.AddRoutine(InteractionSystem.Push(this, Vector2Int.up, out var success));
        } else if (Input.GetKeyDown(KeyCode.S)) {
            Scheduler.AddRoutine(InteractionSystem.Push(this, Vector2Int.down, out var success));
        } else if (Input.GetKeyDown(KeyCode.A)) {
            Scheduler.AddRoutine(InteractionSystem.Push(this, Vector2Int.left, out var success));
        } else if (Input.GetKeyDown(KeyCode.D)) {
            Scheduler.AddRoutine(InteractionSystem.Push(this, Vector2Int.right, out var success));
        }    
    }
}