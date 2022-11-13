using UnityEngine;

public interface IMapObject {
    MapObject Object { get; }
}

public interface IOnEnterHandler : IMapObject  {
    void OnEnter(MapObject mapObject);
}

public interface IOnExitHandler : IMapObject {
    void OnExit(MapObject mapObject);
}

public interface IOnEnterExitHandler : IOnEnterHandler, IOnExitHandler { }

public interface ICanEnterHandler : IMapObject {
    bool CanEnter(Vector2Int enterDir);
}

public interface ICanExitHandler : IMapObject {
    bool CanExit(Vector2Int exitDir);
}

public interface ICanEnterExitHandler : ICanEnterHandler, ICanExitHandler { }

public interface IPushable : IMapObject {
    void OnPush(Vector2Int dir, MapObject source);
}

public interface IPlayer : IMapObject {
    Player Owner { get; }
}