using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Scheduler))]
public class SchedulerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (Scheduler.CurrentRoutine != null) {
            var routine = (Scheduler.ScheduleItem) Scheduler.CurrentRoutine;
            EditorGUILayout.LabelField("Currently Playing ", routine.Label);
            
            foreach (var scheduleItem in Scheduler.RoutineList) {
                GUILayout.Label(scheduleItem.Label);
            }
        } else {
            EditorGUILayout.LabelField("Scheduler is idle");
        }
    }
}