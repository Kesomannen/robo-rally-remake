using System.Collections;
using UnityEngine;

public class Antenna : MapObject, ICanEnterHandler, ITooltipable {
    [SerializeField] Transform _beamPrefab;
    [SerializeField] float _beamSpeed;
    [SerializeField] LeanTweenType _beamTweenType;
    
    static Antenna _instance;
    public static Antenna Instance => _instance;

    public bool Pushable => false;

    public bool CanEnter(Vector2Int enterDir) => false;
    
    public string Header => "Antenna";
    public string Description => "Priority is decided depending on distance to this antenna.";

    protected override void Awake() {
        if (_instance == null) {
            _instance = this;
        } else {
            Debug.LogWarning("Duplicate antenna found!", this);
        }
    }

    public static float GetDistance(Vector2Int pos) {
        return Vector2Int.Distance(_instance.GridPos, pos);
    }

    public IEnumerator BeamAnimation(Player player) {
        var target = player.Model.transform.position;
        
        var beam = Instantiate(_beamPrefab, transform);
        var dir = transform.position - target;
        var duration = dir.magnitude / _beamSpeed;
        
        LeanTween.move(beam.gameObject, target, duration).setEase(_beamTweenType);
        yield return new WaitForSeconds(duration);
        Destroy(beam.gameObject);
    }
}