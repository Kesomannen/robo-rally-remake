public abstract class StaticObject : MapObject {
    public override bool IsStatic => true;

    protected DynamicObject DynamicObject { get; private set; }

    public override void OnEnter(DynamicObject dynamic) {
        DynamicObject = dynamic;
    }

    public override void OnExit(DynamicObject dynamic) {
        DynamicObject = null;
    }
}