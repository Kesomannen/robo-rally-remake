using UnityEngine;

public class StandardBoard : MonoBehaviour, IBoard {
    [SerializeField] RebootToken _rebootToken;

    public void Parent(Transform child) {
        child.SetParent(transform);
    }

    public void Respawn(IPlayer player) {
        TaskScheduler.PushRoutine(_rebootToken.RespawnRoutine(player));
    }
}