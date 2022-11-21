using UnityEngine;

public class Antenna : MapObject, ICanEnterHandler, ITooltipable {
    static Antenna _instance;

    public bool Pushable => false;

    public bool CanEnter(Vector2Int enterDir) => false;
    
    public string Header => "Antenna";
    public string Description => "Robot priority is decided depending on distance to this antenna. " +
                                 $"Current distance to your robot: {(int) GetDistance(PlayerManager.LocalPlayer.Model.GridPos)} tiles.";

    protected override void Awake() {
        if (_instance == null) {
            _instance = this;
        } else {
            Debug.LogWarning($"Duplicate antenna found!", this);
        }
    }

    public static float GetDistance(Vector2Int pos) {
        return Vector2Int.Distance(_instance.GridPos, pos);
    }
}