using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessage : MonoBehaviour {
    [SerializeField] TMP_Text _playerName;
    [SerializeField] Image _playerIcon;
    [SerializeField] TMP_Text _time;
    [SerializeField] TMP_Text _message;

    public void SetContent(Player player, DateTime time, string message) {
        _playerName.text = player.ToString();
        _playerName.color = player.RobotData.Color;
        _playerIcon.sprite = player.RobotData.Icon;
        _time.text = time.ToString("HH:mm");
        _message.text = message;
    }
}