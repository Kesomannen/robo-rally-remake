using UnityEngine;

public interface ICanEnterHandler : IMapObject {
    bool CanEnter(Vector2Int enterDir);
    bool Movable { get; }
}