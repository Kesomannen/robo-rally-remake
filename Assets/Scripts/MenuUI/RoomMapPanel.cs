using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoomMapPanel : Container<MapData> {
    [SerializeField] Selectable _nextButton, _prevButton;
    [Space]
    [SerializeField] Image _mapImage;
    [SerializeField] TMP_Text _mapNameText;
    [Space]
    [SerializeField] Image _difficultyImage;
    [SerializeField] TMP_Text _difficultyText;
    [SerializeField] Sprite[] _difficultySprites;
    [Space]
    [SerializeField] Image _lengthImage;
    [SerializeField] TMP_Text _lengthText;
    [SerializeField] Sprite[] _lengthSprites;

    MapData[] _maps;

    static int MapID => LobbySystem.LobbyMapId;

    void OnEnable() {
        LobbySystem.OnLobbyMapChanged += Serialize;
    }

    void OnDisable() {
        LobbySystem.OnLobbyMapChanged -= Serialize;
    }

    void Start() {
        _maps = MapData.GetAll().ToArray();
        Serialize(MapID);
        
        var isServer = NetworkManager.Singleton.IsServer;
        _nextButton.interactable = isServer;
        _prevButton.interactable = isServer;
    }

    public void GoNext() {
        var next = MapID + 1;
        if (next >= _maps.Length) next = 0;
        LobbySystem.Instance.SetLobbyMap(next);
    }
    
    public void GoPrevious() {
        LobbySystem.Instance.SetLobbyMap(MapID == 0 ? _maps.Length - 1 : MapID - 1);
    }

    void Serialize(int id) => Serialize(MapData.GetById(id));

    protected override void Serialize(MapData data) {
        // TODO: Map thumbnail
        _mapNameText.text = data.Name;
        
        _difficultyImage.sprite = _difficultySprites[(int) data.Difficulty];
        _difficultyText.text = data.Difficulty.ToString();
        
        _lengthImage.sprite = _lengthSprites[(int) data.Length];
        _lengthText.text = data.Length.ToString();
    }
}