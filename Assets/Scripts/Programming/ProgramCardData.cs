using UnityEngine;

public abstract class ProgramCardData : ScriptableObject {
    [SerializeField] string _name;
    [SerializeField] string _description;
    [SerializeField] Sprite _artwork;
    [SerializeField] bool _isDamage;

    public string Name => _name;
    public string Description => _description;
    public Sprite Artwork => _artwork;
    public bool IsDamage => _isDamage;

    public abstract IScheduleItem Execute(Player player, int positionInRegister);
    public abstract bool CanPlace(Player player, int positionInRegister);

    public override string ToString() => _name;
}