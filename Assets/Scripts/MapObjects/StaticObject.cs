public abstract class StaticObject : MapObject {
    public override bool IsStatic => true;

    protected DynamicObject CurrentDynamic { get; private set; }

    public override void OnEnter(DynamicObject dynamic) {
        CurrentDynamic = dynamic;
    }

    public override void OnExit(DynamicObject dynamic) {
        CurrentDynamic = null;
    }
}