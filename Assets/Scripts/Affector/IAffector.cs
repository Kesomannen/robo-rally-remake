public interface IAffector<in T> : IPermanentAffector<T> {
    void Remove(T target);
}