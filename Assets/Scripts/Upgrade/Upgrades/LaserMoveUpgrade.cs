using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Laser Move")]
public class LaserMoveUpgrade : UpgradeCardData {
    [SerializeField] int _moveDistance;
    [SerializeField] int _minDistance;
    [Space]
    [SerializeField] float _moveSpeed = 2.5f;
    [SerializeField] LeanTweenType _moveType = LeanTweenType.easeOutBounce;
    [Space]
    [SerializeField] bool _countAsPush;
    [SerializeField] bool _optional;
    [ShowIf("_optional")]
    [SerializeField] OverlayData<Choice<bool>> _overlay;
    
    public override void OnAdd(Player player) {
        player.Model.OnShoot += OnShoot;
    }
    
    public override void OnRemove(Player player) {
        player.Model.OnShoot -= OnShoot;
    }
    
    void OnShoot(PlayerModel.CallbackContext context) {
        var attacker = context.Attacker;
        var target = context.Target;

        var attackerPos = attacker.Model.GridPos;
        var targetPos = target.Model.GridPos;
        
        var distance = attackerPos.GridDistance(targetPos);
        if (distance < _minDistance) return;

        TaskScheduler.PushRoutine(_optional ? OptionalChoice() : MoveTarget());

        IEnumerator OptionalChoice() {
            var result = new bool[1];
            yield return ChoiceSystem.DoChoice(new ChoiceData<bool> {
                Overlay = _overlay,
                Player = context.Attacker,
                Options = new[] { true, false },
                OutputArray = result,
                Message = $"choosing whether to {(_moveDistance < 0 ? "pull" : "push")} {context.Target}",
                MinChoices = 1
            }); 
            if (result[0]) {
                TaskScheduler.PushRoutine(MoveTarget());
            }
        }
        
        IEnumerator MoveTarget() {
            Log.Message($"{Log.PlayerString(attacker)} {(_moveDistance < 0 ? "pulled" : "pushed")} {Log.PlayerString(target)} with {Log.UpgradeString(this)}");
            
            var delta = (int) Mathf.Sign(_moveDistance);
            for (var i = 0; i < Mathf.Abs(_moveDistance); i++) {
                if (distance < _minDistance) yield break;
                distance += delta;
            
                var canPush = Interaction.Push(target.Model, context.OutgoingDirection * delta, out var mapEvent);
                if (!canPush) yield break;
                if (_countAsPush) attacker.Model.RegisterPush(mapEvent);
                TaskScheduler.PushRoutine(Interaction.EaseEvent(mapEvent, _moveType, _moveSpeed));
            }
        }
    }
}