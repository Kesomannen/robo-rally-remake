using UnityEngine;

public abstract class ProgramCardData : ScriptableObject {
    [SerializeField] bool _isDamage;

    public bool IsDamage => _isDamage;

    public abstract void Execute(Player player, int positionInRegister);
}