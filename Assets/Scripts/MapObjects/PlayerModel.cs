public class PlayerModel : DynamicObject {
    public Player Owner { get; private set; }

    public override void Fall() {
        Owner.Reboot();
    }

    public void Init(Player owner) {
        Owner = owner;
    }
}