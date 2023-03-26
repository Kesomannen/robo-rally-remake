using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Log : Singleton<Log> {
    [SerializeField] GameObject _messagePrefab;
    [SerializeField] Transform _messageParent;
    [FormerlySerializedAs("_upgradeColor")]
    [SerializeField] Color _cardColor;
    [SerializeField] Color _checkpointColor;
    [SerializeField] Color _rebootColor;
    [SerializeField] Color _energyColor;
    [SerializeField] Color _defaultTextColor;
    [SerializeField] ScrollRect _scrollRect;
    [SerializeField] [ReadOnly] List<string> _messages = new();

    const string PlayerSpritePrefix = "Log_Player_";
    const string IconSpritePrefix = "Log_Icon_";

    void OnEnable() {
        StartCoroutine(_scrollRect.ScrollToBottom());
    }

    public static void Message(string message) => Instance.MessageInternal(message);
    
    void MessageInternal(string message) {
        _messages.Add(message);
        Instantiate(_messagePrefab, _messageParent).GetComponentInChildren<TMP_Text>().text = message;
        
        if (!enabled) return;
        StartCoroutine(_scrollRect.ScrollToBottom());
    }

    public static string PlayerString(Player player) => Instance.PlayerString(PlayerSystem.Players.IndexOf(player));
    string PlayerString(int playerIndex) {
        var player = PlayerSystem.Players[playerIndex];
        return $"{Sprite(PlayerSpritePrefix + player.RobotData.Name)}{SetColor(player.RobotData.Color)}{player}{ResetColor()}";
    }

    public static string UpgradeString(UpgradeCardData upgrade) => Instance.UpgradeString(upgrade.GetLookupId());
    string UpgradeString(int upgradeId) => $"{Sprite(IconSpritePrefix + "Card")}{SetColor(_cardColor)}{UpgradeCardData.GetById(upgradeId).Name}{ResetColor()}";

    public static string ProgramString(ProgramCardData upgrade) => Instance.ProgramString(upgrade.GetLookupId());
    string ProgramString(int programId) => $"{Sprite(IconSpritePrefix + "Card")}{SetColor(_cardColor)}{ProgramCardData.GetById(programId).Name}{ResetColor()}";

    public static string DirectionString(Vector2Int direction) {
        if (direction == Vector2Int.up) return "upwards";
        if (direction == Vector2Int.down) return "downwards";
        if (direction == Vector2Int.left) return "to the left";
        return direction == Vector2Int.right ? "to the right" : direction.ToString();
    }
    
    public static string CheckpointString(int checkpointIndex) => $"{Sprite(IconSpritePrefix + "Checkpoint")}{SetColor(Instance._checkpointColor)}#{checkpointIndex}{Instance.ResetColor()}";
    public static string EnergyString(int energy) => $"{Sprite(IconSpritePrefix + "Energy")}{SetColor(Instance._energyColor)}{energy}{Instance.ResetColor()}";
    public static string RebootString() => $"{Sprite(IconSpritePrefix + "Reboot")}{SetColor(Instance._rebootColor)}rebooted{Instance.ResetColor()}";
    
    static string Sprite(string name) => $"<sprite name=\"{name}\" color=#{ColorUtility.ToHtmlStringRGB(Color.white)}>";
    static string SetColor(Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
    string ResetColor() => SetColor(_defaultTextColor);
}