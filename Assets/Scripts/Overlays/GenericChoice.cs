using UnityEngine;

public class GenericChoice<T> : Choice<T> where T : IContainable<T> {
    [SerializeField] Transform _optionContainer;
    [SerializeField] [Min(0)] int _max, _min;

    public override int MaxOptions => _max;
    public override int MinOptions => _min;

    public override void Init(T[] options, ChoiceCallbackReciever callback) {
        base.Init(options, callback);
        foreach (var option in options) {
            var optionInstance = Instantiate(option.ContainerPrefab, _optionContainer);
            optionInstance.SetData(option);
        }
    }
}