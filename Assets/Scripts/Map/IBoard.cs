using UnityEngine;

public interface IBoard {
    void Respawn(PlayerModel playerModel);
    Transform transform { get; }
}