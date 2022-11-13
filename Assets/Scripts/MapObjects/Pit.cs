public class Pit : MapObject, IOnEnterHandler {
    public void OnEnter(MapObject mapObject) {
        if (mapObject is PlayerModel player) {
            MapSystem.Instance.TryGetBoard(GridPos, out var board);
            player.Owner.Reboot(board);
        }
    }
}