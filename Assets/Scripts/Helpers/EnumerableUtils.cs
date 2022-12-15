using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableUtils {
    public static IEnumerable<TResult> For<TResult>(int count, Func<int, TResult> selector) {
        return Enumerable.Range(0, count).Select(selector);
    }
}