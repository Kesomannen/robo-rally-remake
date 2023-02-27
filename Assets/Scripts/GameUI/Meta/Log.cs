using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Log : Singleton<Log> {
    [SerializeField] GameObject _messagePrefab;
    [SerializeField] Transform _messageParent;
    [SerializeField] Color _upgradeColor;
    [SerializeField] Color _checkpointColor;
    [SerializeField] Color _rebootColor;
    [SerializeField] Color _defaultTextColor;
    [SerializeField] [ReadOnly] List<string> _messages = new();

    const string PlayerSpritePrefix = "Log_Player_";
    const string IconSpritePrefix = "Log_Icon_";

    public static event Action<string> OnMessageSent;
    
    public void UseUpgradeMessage(Player player, UpgradeCardData upgrade) => Publish(LogMessageType.UseUpgrade, args: new[] { IndexOf(player), upgrade.GetLookupId() });
    public void BuyUpgradeMessage(Player player, UpgradeCardData upgrade) => Publish(LogMessageType.BuyUpgrade, args: new[] { IndexOf(player), upgrade.GetLookupId() });
    public void CheckpointMessage(Player player, int checkpoint) => Publish(LogMessageType.Checkpoint, args: new[] { IndexOf(player), checkpoint });
    public void RebootMessage(Player player) => Publish(LogMessageType.Rebooted, args: new[] { IndexOf(player) });
    public void SkipMessage(Player player) => Publish(LogMessageType.Skip, args: new[] { IndexOf(player) });

    static int IndexOf(Player player) => PlayerSystem.Players.IndexOf(player);

    void Publish(LogMessageType type, IReadOnlyList<int> args, string message = null) {
        var text = type switch {
            LogMessageType.UseUpgrade => $"{PlayerString(args[0])}{ResetColor()} used {UpgradeString(args[1])}",
            LogMessageType.BuyUpgrade => $"{PlayerString(args[0])}{ResetColor()} bought {UpgradeString(args[1])}",
            LogMessageType.Checkpoint => $"{PlayerString(args[0])}{ResetColor()} reached {CheckpointString(args[1])}",
            LogMessageType.Rebooted => $"{PlayerString(args[0])} {RebootString()}{SetColor(_rebootColor)}rebooted",
            LogMessageType.Skip => $"{PlayerString(args[0])}{ResetColor()} skipped buying an upgrade",
            _ => throw new ArgumentOutOfRangeException()
        };
        text += $"{message}";

        _messages.Add(text);
        var obj = Instantiate(_messagePrefab, _messageParent);
        obj.GetComponentInChildren<TMP_Text>().text = text;
        
        OnMessageSent?.Invoke(text);
    }

    static string PlayerString(int playerIndex) {
        var player = PlayerSystem.Players[playerIndex];
        return $"{Sprite(PlayerSpritePrefix + player.RobotData.Name)}{SetColor(player.RobotData.Color)}{player}";
    }

    string UpgradeString(int upgradeId) {
        return $"{Sprite(IconSpritePrefix + "Card")}{SetColor(_upgradeColor)}{UpgradeCardData.GetById(upgradeId).Name}";
    }

    string CheckpointString(int checkpointIndex) {
        return $"{Sprite(IconSpritePrefix + "Checkpoint")}{SetColor(_checkpointColor)} #{checkpointIndex}";
    }

    static string RebootString() {
        return $"{Sprite(IconSpritePrefix + "Reboot")}";
    }

    static string Sprite(string name) {
        return $"<sprite name=\"{name}\" color=#{ColorUtility.ToHtmlStringRGB(Color.white)}>";
    }

    static string SetColor(Color color) {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
    }

    string ResetColor() {
        return SetColor(_defaultTextColor);
    }

    enum LogMessageType {
        UseUpgrade,
        BuyUpgrade,
        Checkpoint,
        Rebooted,
        Skip
    }
}