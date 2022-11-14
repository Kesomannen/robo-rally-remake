using UnityEngine;

public class StandardBoard : MonoBehaviour, IBoard {
    [SerializeField] RebootToken _rebootToken;

    public void Parent(Transform child) {
        child.SetParent(transform);
    }

    public void Respawn(MapObject obj) {
        if (obj is PlayerModel player) {
            Scheduler.Push(_rebootToken.RespawnRoutine(player), $"StandardBoard Respawn {player}");
        } else {
            Debug.LogWarning($"Respawn not implemented for {obj.name}");
        }
    }
}