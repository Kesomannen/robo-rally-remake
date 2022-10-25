using System.Collections;
using System.Collections.Generic;

public class Scheduler : Singleton<Scheduler> {
    static List<IEnumerator> _routines = new();
    static bool _isPlaying;

    public static void AddRoutine(IEnumerator routine) {
        _routines.Add(routine);
        if (!_isPlaying) {
            instance.StartPlaying();
        }
    }

    void StartPlaying() {
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine() {
        _isPlaying = true;
        while (_routines.Count > 0) {
            var routine = _routines[0];
            _routines.RemoveAt(0);
            yield return routine;
        }
        _isPlaying = false;
    }
}