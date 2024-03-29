using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerPanel : MonoBehaviour {
    [SerializeField] TMP_Text _nameText;
    [SerializeField] Image _readyImage;
    [SerializeField] Sprite _readySprite, _notReadySprite;
    [SerializeField] Image _hostIcon;
    [SerializeField] Image _robotIcon;

    public ulong PlayerId { get; private set; }
    public LobbyPlayerData PlayerData { get; private set; }

    public void SetContent(ulong playerID, LobbyPlayerData playerData) {
        PlayerId = playerID;
        PlayerData = playerData;

        var isLocalPlayer = playerID == NetworkManager.Singleton.LocalClientId;
        _nameText.text = isLocalPlayer ? $"{playerData.Name} (You)" : playerData.Name;

        _readyImage.sprite = playerData.IsReady ? _readySprite : _notReadySprite;
        _robotIcon.sprite = RobotData.GetById(playerData.RobotId).Icon;
        
        _hostIcon.gameObject.SetActive(playerData.IsHost);
    }
}