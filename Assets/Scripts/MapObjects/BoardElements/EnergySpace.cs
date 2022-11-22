using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnergySpace : BoardElement<EnergySpace, IPlayer>, ITooltipable {
    [Header("Stats")]
    [SerializeField] [Min(0)] int _reward = 1;
    [SerializeField] bool _hasEnergyCube = true;
    
    [Header("References")]
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] Light2D _energyCubeLight;
    [SerializeField] Sprite _onSprite, _offSprite;

    public string Header => "Energy Space";
    public string Description => _hasEnergyCube ? 
        $"Has an energy cube, end the register here to receive {_reward} energy!"
        : $"End the fifth register here to receive {_reward} energy.";
    
    void Start() {
        _renderer.sprite = _hasEnergyCube ? _onSprite : _offSprite;
        _energyCubeLight.enabled = _hasEnergyCube;
    }

    protected override void Activate(IPlayer[] targets) {
        foreach (var playerModel in targets) {
            var player = playerModel.Owner;

            if (_hasEnergyCube) {
                RewardPlayer(player);
                _hasEnergyCube = false;

                _renderer.sprite = _offSprite;
                _energyCubeLight.enabled = false;
            } else if (ExecutionPhase.CurrentRegister == ExecutionPhase.RegisterCount - 1) {
                RewardPlayer(player);
            }
        }

        void RewardPlayer(Player plr) => plr.Energy.Value += _reward;
    }
}