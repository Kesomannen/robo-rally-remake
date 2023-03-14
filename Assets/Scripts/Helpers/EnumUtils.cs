using System;
using static System.Convert;

public static class EnumUtils {
    public static TBase ModifyValue<TBase, TSet>(TBase baseVal, TSet setVal) where TBase : Enum where TSet : Enum {
        var setValInt = ToInt32(setVal);
        if (setValInt == 0) return baseVal;
        return (TBase) Enum.ToObject(typeof(TBase), setValInt + 1);
    }
    
    public static TResult Convert<TBase, TResult>(this TBase baseVal, int shift = 0) where TBase : Enum where TResult : Enum {
        var baseValInt = ToInt32(baseVal);
        return (TResult)Enum.ToObject(typeof(TResult), baseValInt + shift);
    }
}