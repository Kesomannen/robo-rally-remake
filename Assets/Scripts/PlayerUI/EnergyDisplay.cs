using TMPro;
using UnityEngine;

public class EnergyDisplay : MonoBehaviour, ITooltipable {
    [SerializeField] TMP_Text _text;

    public string Header => "Energy";
    public string Description => $"You have {Owner.Energy.Value} energy.";
    
    static Player Owner => PlayerSystem.LocalPlayer;

    void Start() {
        Owner.Energy.ValueChanged += UpdateText;
        UpdateText(0, Owner.Energy.Value);
        gameObject.SetActive(GameSystem.Settings.EnergyEnabled);
    }
    
    void UpdateText(int prev, int next){
        _text.text = next.ToString();
    }
}