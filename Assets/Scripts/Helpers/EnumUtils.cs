using System;

public static class EnumUtils {
    public static TBase ModifyValue<TBase, TSet>(TBase baseVal, TSet setVal) where TBase : Enum where TSet : Enum {
        var setValInt = Convert.ToInt32(setVal);
        if (setValInt == 0) return baseVal;
        return (TBase) Enum.ToObject(typeof(TBase), setValInt + 1);
    }
}