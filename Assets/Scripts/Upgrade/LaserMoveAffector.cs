using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "LaserMoveAffector", menuName = "ScriptableObjects/Affectors/Laser Move")]
public class LaserMoveAffector : ScriptableAffector<IPlayer> {
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
    
    public override void Apply(IPlayer player) {
        player.Owner.Model.OnShoot += OnShoot;
    }
    
    public override void Remove(IPlayer player) {
        player.Owner.Model.OnShoot -= OnShoot;
    }
    
    void OnShoot(PlayerModel.CallbackContext context) {
        var attacker = context.Attacker;
        var target = context.Target;

        var attackerPos = attacker.Model.GridPos;
        var targetPos = target.Model.GridPos;
        
        var distance = attackerPos.GridDistance(targetPos);
        if (distance < _minDistance) return;

        TaskScheduler.PushRoutine(_optional ? OptionalChoice() : PushTarget());

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
                TaskScheduler.PushRoutine(PushTarget());
            }
        }
        
        IEnumerator PushTarget() {
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