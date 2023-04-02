using UnityEngine;

public interface ICanExitHandler : IMapObject {
    bool CanExit(Vector2Int exitDir);
}