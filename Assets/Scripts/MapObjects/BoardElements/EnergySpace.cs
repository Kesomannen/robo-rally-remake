using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnergySpace : BoardElement<EnergySpace, IPlayer> {
    [SerializeField] [Min(0)] int _reward = 1;
    [SerializeField] bool _hasEnergyCube = true;
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] Light2D _energyCubeLight;
    [SerializeField] Sprite _onSprite, _offSprite;

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
            }
            if (ExecutionPhase.CurrentRegister == ExecutionPhase.RegisterCount - 1) {
                RewardPlayer(player);
            }
        }

        void RewardPlayer(Player plr) => plr.Energy.Value += _reward;
    }
}