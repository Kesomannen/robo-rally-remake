using System.Collections;
using UnityEngine;

public class ScheduleRoutine : IScheduleItem {
    IEnumerator _routine;
    float _delay;

    public ScheduleRoutine(IEnumerator routine, float delay = Scheduler.DefaultDelay) {
        _routine = routine;
        _delay = delay;
    }

    public IEnumerator Play() {
        yield return new WaitForSeconds(_delay);
        yield return _routine;
    }
}