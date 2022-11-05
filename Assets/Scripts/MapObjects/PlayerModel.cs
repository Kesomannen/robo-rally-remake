public class PlayerModel : DynamicObject {
    public Player Owner { get; private set; }

    public void Init(Player owner) {
        Owner = owner;
    }
}