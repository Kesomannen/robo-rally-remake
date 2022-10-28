using System.Collections;
using System.Collections.Generic;

public class ScheduleGroup : IScheduleItem {
    readonly List<IScheduleItem> _items;

    public ScheduleGroup(IScheduleItem[] items) {
        _items = new(items);
    }

    public ScheduleGroup(IScheduleItem item) {
        _items = new() { item };
    }

    public ScheduleGroup() {
        _items = new();
    }

    public void AddItem(IScheduleItem routine) {
        _items.Add(routine);
    }

    public IEnumerator Play() {
        foreach (var routine in _items) {
            Scheduler.AddItem(routine);
        }
        yield break;
    }
}