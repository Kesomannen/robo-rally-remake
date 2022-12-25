using UnityEngine;

public class RobotTooltip : MonoBehaviour, ITooltipable {
    [SerializeField] LobbyPlayerPanel _playerPanel;

    RobotData Robot => RobotData.GetById(_playerPanel.PlayerData.RobotId);
    
    public string Header => Robot.Name;
    public string Description => Robot.Description;
}