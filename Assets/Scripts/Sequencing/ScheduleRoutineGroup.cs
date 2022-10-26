using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScheduleRoutineGroup : IScheduleItem {
    List<IEnumerator> _routines;
    float _interval;
    float _startDelay;

    public ScheduleRoutineGroup (
        IEnumerable<IEnumerator> routines,
        float interval = Scheduler.DefaultInterval,
        float startDelay = Scheduler.DefaultDelay) 
    {
        _routines = routines.ToList();
        _interval = interval;
        _startDelay = startDelay;
    }

    public void AddRoutine(IEnumerator routine) {
        _routines.Add(routine);
    }

    public IEnumerator Play() {
        yield return new WaitForSeconds(_startDelay);
        foreach (var routine in _routines) {
            yield return routine;
        }
    }
}