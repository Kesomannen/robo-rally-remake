// <author>
//   douduck08: https://github.com/douduck08
//   Use Reflection to get instance of Unity's SerializedProperty in Custom Editor.
//   Modified codes from 'Unity Answers', in order to apply on nested List<T> or Array. 
//   
//   Original author: HiddenMonk & Johannes Deml
//   Ref: http://answers.unity3d.com/questions/627090/convert-serializedproperty-to-custom-class.html
// </author>

using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

public static class SerializedPropertyExtensions {
    public static T GetValue<T> (this SerializedProperty property) where T : class {
        object obj = property.serializedObject.targetObject;
        var path = property.propertyPath.Replace (".Array.data", "");
        var fieldStructure = path.Split ('.');
        var rgx = new Regex (@"\[\d+\]");
        foreach (var structure in fieldStructure) {
            if (structure.Contains ("[")) {
                var index = System.Convert.ToInt32 (new string (structure.Where (char.IsDigit).ToArray()));
                obj = GetFieldValueWithIndex (rgx.Replace (structure, ""), obj, index);
            } else {
                obj = GetFieldValue (structure, obj);
            }
        }
        return (T) obj;
    }

    public static bool SetValue<T> (this SerializedProperty property, T value) where T : class {
        object obj = property.serializedObject.targetObject;
        var path = property.propertyPath.Replace (".Array.data", "");
        var fieldStructure = path.Split ('.');
        var rgx = new Regex (@"\[\d+\]");
        for (var i = 0; i < fieldStructure.Length - 1; i++) {
            if (fieldStructure[i].Contains ("[")) {
                var index = System.Convert.ToInt32 (new string (fieldStructure[i].Where (c => char.IsDigit (c)).ToArray ()));
                obj = GetFieldValueWithIndex (rgx.Replace (fieldStructure[i], ""), obj, index);
            } else {
                obj = GetFieldValue (fieldStructure[i], obj);
            }
        }

        var fieldName = fieldStructure.Last ();
        if (!fieldName.Contains("[")) {
            return SetFieldValue(fieldName, obj, value);
        }

        {
            var index = System.Convert.ToInt32 (new string (fieldName.Where (c => char.IsDigit (c)).ToArray ()));
            return SetFieldValueWithIndex (rgx.Replace (fieldName, ""), obj, index, value);
        }

    }

    static object GetFieldValue (string fieldName, object obj, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
        var field = obj.GetType ().GetField (fieldName, bindings);
        return field != null ? field.GetValue (obj) : default (object);
    }

    static object GetFieldValueWithIndex (string fieldName, object obj, int index, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
        var field = obj.GetType ().GetField (fieldName, bindings);
        if (field == null) {
            return default;
        }

        var list = field.GetValue (obj);
        if (list.GetType ().IsArray) {
            return ((object[]) list)[index];
        }

        return list is IEnumerable ? ((IList) list)[index] : default;
    }

    public static bool SetFieldValue (string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
        var field = obj.GetType ().GetField (fieldName, bindings);
        if (field == null) {
            return false;
        }

        field.SetValue (obj, value);
        return true;
    }

    public static bool SetFieldValueWithIndex (string fieldName, object obj, int index, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
        var field = obj.GetType ().GetField (fieldName, bindings);
        if (field == null) {
            return false;
        }

        var list = field.GetValue (obj);
        if (list.GetType ().IsArray) {
            ((object[]) list)[index] = value;
            return true;
        }

        if (list is not IEnumerable) {
            return false;
        }

        ((IList) list)[index] = value;
        return true;
    }
}