using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerUIPlayerPanel : Container<Player>, IPointerClickHandler {
    [SerializeField] TMP_Text _nameText;
    [SerializeField] Image _robotIcon;
    [SerializeField] OverlayData<PlayerOverlay> _overlayData;

    Player _player;
    
    protected override void Serialize(Player player) {
        if (_player != null){
            _player.Energy.OnValueChanged -= OnEnergyChanged;
        }
        _player = player;
        player.Energy.OnValueChanged += OnEnergyChanged;
        
        _nameText.text = player.ToString();
        _robotIcon.sprite = player.RobotData.Icon;
    }
    
    void OnEnergyChanged(int prev, int next) {
        
    }
    
    public void OnPointerClick(PointerEventData e) {
        OverlaySystem.Instance.PushOverlay(_overlayData).Init(_player);
    }
}