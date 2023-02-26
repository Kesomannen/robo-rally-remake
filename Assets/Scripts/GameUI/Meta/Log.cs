using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Log : NetworkSingleton<Log> {
    [SerializeField] GameObject _messagePrefab;
    [SerializeField] Transform _messageParent;
    [SerializeField] Color _upgradeColor;
    [SerializeField] Color _checkpointColor;
    [SerializeField] Color _defaultTextColor;
    [SerializeField] [ReadOnly] List<string> _messages = new();

    const string PlayerSpritePrefix = "Log_Player_";
    const string IconSpritePrefix = "Log_Icon_";

    public static event Action<string> OnMessageSent;
    
    public void UseUpgradeMessage(Player player, UpgradeCardData upgrade) => Publish(LogMessageType.UseUpgrade, args: new[] { IndexOf(player), upgrade.GetLookupId() });
    public void BuyUpgradeMessage(Player player, UpgradeCardData upgrade) => Publish(LogMessageType.BuyUpgrade, args: new[] { IndexOf(player), upgrade.GetLookupId() });
    public void CheckpointMessage(Player player, int checkpoint) => Publish(LogMessageType.Checkpoint, args: new[] { IndexOf(player), checkpoint });
    public void RebootMessage(Player player) => Publish(LogMessageType.Rebooted, args: new[] { IndexOf(player) });

    static int IndexOf(Player player) => PlayerSystem.Players.IndexOf(player);

    async void Publish(LogMessageType type, IEnumerable<int> args) {
        await Task.Delay(1000);
        
        var typeByte = (byte)type;
        var argsBytes = args.Select(i => (byte)i).ToArray();

        if (NetworkManager.Singleton == null) {
            CreateMessage(typeByte, argsBytes);
        } else {
            PublishMessageServerRpc(typeByte, argsBytes);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PublishMessageServerRpc(byte messageType, byte[] args) {
        CreateMessage(messageType, args);
        PublishMessageClientRpc(messageType, args);
    }

    [ClientRpc]
    void PublishMessageClientRpc(byte messageType, byte[] args) {
        if (IsServer) return;
        CreateMessage(messageType, args);
    }

    void CreateMessage(byte messageType, IReadOnlyList<byte> args) {
        var type = (LogMessageType)messageType;

        var message = type switch {
            LogMessageType.UseUpgrade => $"{PlayerString(args[0])}{ResetColor()} used {UpgradeString(args[1])}",
            LogMessageType.BuyUpgrade => $"{PlayerString(args[0])}{ResetColor()} bought {UpgradeString(args[1])}",
            LogMessageType.Checkpoint => $"{PlayerString(args[0])}{ResetColor()} reached {CheckpointString(args[1])}",
            LogMessageType.Rebooted => $"{PlayerString(args[0])}{RebootString()} rebooted",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        _messages.Add(message);
        var obj = Instantiate(_messagePrefab, _messageParent);
        obj.GetComponentInChildren<TMP_Text>().text = message;
        
        OnMessageSent?.Invoke(message);
    }

    static string PlayerString(byte playerIndex) {
        var player = PlayerSystem.Players[playerIndex];
        return $"{Sprite(PlayerSpritePrefix + player.RobotData.Name)}{SetColor(player.RobotData.Color)}{player}";
    }

    string UpgradeString(byte upgradeId) {
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
        Rebooted
    }
}