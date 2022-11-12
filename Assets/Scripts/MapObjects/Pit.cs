public class Pit : StaticObject {
    public override void OnEnter(DynamicObject dynamic) {
        base.OnEnter(dynamic);
        dynamic.Fall(MapSystem.Instance.GetBoard(this));
    }
}