public class Pit : MapObject, IOnEnterHandler {
    public void OnEnter(MapObject mapObject) {
        MapSystem.Instance.TryGetBoard(GridPos, out var board);
        mapObject.Fall(board);
    }
}