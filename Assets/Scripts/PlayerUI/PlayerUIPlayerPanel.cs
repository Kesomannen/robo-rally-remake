using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIPlayerPanel : Container<Player> {
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _energyText;
    [SerializeField] Image _robotIcon;
    
    protected override void Serialize(Player player){
        _nameText.text = player.ToString();
        _energyText.text = player.Energy.ToString();
        _robotIcon.sprite = player.RobotData.Icon;
    }
}