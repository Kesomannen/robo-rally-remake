using System;
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

    public void UseUpgradeMessage(Player player, UpgradeCardData upgrade) => Publish(LogMessageType.UseUpgrade, args: new[] { IndexOf(player), upgrade.GetLookupId() });
    public void BuyUpgradeMessage(Player player, UpgradeCardData upgrade) => Publish(LogMessageType.BuyUpgrade, args: new[] { IndexOf(player), upgrade.GetLookupId() });
    public void CheckpointMessage(Player player, int checkpoint) => Publish(LogMessageType.Checkpoint, args: new[] { IndexOf(player), checkpoint });
    public void RebootMessage(Player player) => Publish(LogMessageType.Rebooted, args: new[] { IndexOf(player) });
    public void SkipMessage(Player player) => Publish(LogMessageType.Skip, args: new[] { IndexOf(player) });
    public void RawMessage(string message) => Publish(LogMessageType.Raw, args: Array.Empty<int>(), message: message);

    static int IndexOf(Player player) => PlayerSystem.Players.IndexOf(player);

    void OnEnable() {
        StartCoroutine(_scrollRect.ScrollToBottom());
    }

    void Publish(LogMessageType type, IReadOnlyList<int> args, string message = null) {
        var text = type switch {
            LogMessageType.UseUpgrade => $"{PlayerString(args[0])} used {UpgradeString(args[1])}",
            LogMessageType.BuyUpgrade => $"{PlayerString(args[0])} bought {UpgradeString(args[1])} for {EnergyString(UpgradeCardData.GetById(args[1]).Cost)}",
            LogMessageType.Checkpoint => $"{PlayerString(args[0])} reached {CheckpointString(args[1])}",
            LogMessageType.Rebooted => $"{PlayerString(args[0])} {RebootString()}{SetColor(_rebootColor)}rebooted",
            LogMessageType.Skip => $"{PlayerString(args[0])} skipped buying an upgrade",
            LogMessageType.Raw => "",
            _ => throw new ArgumentOutOfRangeException()
        };
        text += $"{message}";

        _messages.Add(text);
        var obj = Instantiate(_messagePrefab, _messageParent);
        obj.GetComponentInChildren<TMP_Text>().text = text;

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
    
    string ResetColor() => SetColor(_defaultTextColor);
    string CheckpointString(int checkpointIndex) => $"{Sprite(IconSpritePrefix + "Checkpoint")}{SetColor(_checkpointColor)}#{checkpointIndex}{ResetColor()}";
    public static string EnergyString(int energy) => $"{Sprite(IconSpritePrefix + "Energy")}{SetColor(Instance._energyColor)}{energy}{Instance.ResetColor()}";
    static string RebootString() => $"{Sprite(IconSpritePrefix + "Reboot")}";
    static string Sprite(string name) => $"<sprite name=\"{name}\" color=#{ColorUtility.ToHtmlStringRGB(Color.white)}>";
    static string SetColor(Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";

    enum LogMessageType {
        UseUpgrade,
        BuyUpgrade,
        Checkpoint,
        Rebooted,
        Skip,
        Raw
    }
}