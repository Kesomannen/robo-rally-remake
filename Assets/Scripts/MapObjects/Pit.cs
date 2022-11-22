public class Pit : MapObject, IOnEnterHandler, ITooltipable {
    public string Header => "Pit";
    public string Description => "Falling in here reboots your robot.";
    
    public void OnEnter(MapObject mapObject) {
        MapSystem.Instance.TryGetBoard(GridPos, out var board);
        mapObject.Fall(board);
    }
}