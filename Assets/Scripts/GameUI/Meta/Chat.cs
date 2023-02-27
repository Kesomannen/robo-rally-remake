using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static System.Text.Encoding;

public class Chat : NetworkSingleton<Chat> {
    [SerializeField] Transform _messageParent;
    [SerializeField] ChatMessage _messagePrefab;
    [SerializeField] TMP_InputField _inputField;

    public static event Action<string> OnMessageSent;

    void OnEnable() {
        _inputField.onSubmit.AddListener(_ => Send());
    }

    public void Send() {
        var text = _inputField.text.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        var playerIndex = (byte)PlayerSystem.Players.IndexOf(PlayerSystem.LocalPlayer);
        var bytes = UTF8.GetBytes(text);
        
        if (NetworkManager.Singleton == null) {
            CreateMessage(bytes, playerIndex);
        } else {
            SendServerRpc(bytes, playerIndex);
        }
        
        _inputField.text = "";
    }

    [ServerRpc(RequireOwnership = false)]
    void SendServerRpc(byte[] bytes, byte playerIndex) {
        CreateMessage(bytes, playerIndex);
        SendClientRpc(bytes, playerIndex);
    }
    
    [ClientRpc]
    void SendClientRpc(byte[] bytes, byte playerIndex) {
        if (IsServer) return;
        CreateMessage(bytes, playerIndex);
    }

    void CreateMessage(byte[] bytes, byte playerIndex) {
        var message = Instantiate(_messagePrefab, _messageParent);
        var str = UTF8.GetString(bytes);
        message.SetContent(
            PlayerSystem.Players[playerIndex],
            DateTime.Now,
            str
            );
        
        OnMessageSent?.Invoke(str);
    } 
}