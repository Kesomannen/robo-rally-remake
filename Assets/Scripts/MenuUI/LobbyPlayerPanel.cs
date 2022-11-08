using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerPanel : MonoBehaviour {
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _readyText;
    [SerializeField] Image _hostIcon;

    public ulong PlayerId { get; private set; }
    public LobbyPlayerData PlayerData { get; private set; }

    public void SetData(ulong playerID, LobbyPlayerData playerData) {
        PlayerId = playerID;
        PlayerData = playerData;

        _nameText.text = playerID == NetworkManager.Singleton.LocalClientId ? $"{playerID} (You)" : playerID.ToString();
        _readyText.text = playerData.IsReady ? "Ready" : "Not Ready";
        _hostIcon.gameObject.SetActive(playerData.IsHost);
    }
}