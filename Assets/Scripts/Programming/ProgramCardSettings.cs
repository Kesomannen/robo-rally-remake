using UnityEngine;

[CreateAssetMenu(fileName = "ProgramCardSettings", menuName = "ScriptableObjects/Settings/ProgramCard", order = 0)]
public class ProgramCardSettings : SingletonSO<ProgramCardSettings> {
    [SerializeField] ProgramCard _programCardPrefab;
    
    public ProgramCard ProgramCardPrefab => _programCardPrefab;
}