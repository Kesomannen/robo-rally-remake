using System;
using static System.Convert;

public static class EnumUtils {
    public static TBase ModifyValue<TBase, TSet>(TBase baseVal, TSet setVal) where TBase : Enum where TSet : Enum {
        var setValInt = ToInt32(setVal);
        if (setValInt == 0) return baseVal;
        return (TBase) Enum.ToObject(typeof(TBase), setValInt + 1);
    }

    public static T Shift<T>(this T e, int shift) where T : Enum =>
        (T)Enum.ToObject(typeof(T), ToInt32(e) + shift);

    public static TConvert Convert<TBase, TConvert>(this TBase baseVal, int shift = 0) where TBase : Enum where TConvert : Enum {
        var baseValInt = ToInt32(baseVal);
        return (TConvert)Enum.ToObject(typeof(TConvert), baseValInt + shift);
    }
}