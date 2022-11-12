public class PlayerModel : DynamicObject {
    public Player Owner { get; private set; }

    public override void Fall(BaseBoard board) {
        board.Respawn(this);
        Owner.OnReboot();
    }

    public void Init(Player owner) {
        Owner = owner;
    }
}