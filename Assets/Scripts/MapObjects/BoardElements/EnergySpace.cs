using UnityEngine;

public class EnergySpace : BoardElement<EnergySpace> {
    [SerializeField] [Min(0)] int _reward = 1;
    [SerializeField] bool _hasEnergyCube = true;

    protected override void Activate(DynamicObject dynamic) {
        if (dynamic is PlayerModel plrModel) {
            var player = plrModel.Owner;
            if (_hasEnergyCube) {
                RewardPlayer(player);
                _hasEnergyCube = false;
            }
            if (ExecutionPhase.CurrentRegister == 4) {
                RewardPlayer(player);
            }
        }

        void RewardPlayer(Player plr) => plr.Energy.Value += _reward;
    }
}