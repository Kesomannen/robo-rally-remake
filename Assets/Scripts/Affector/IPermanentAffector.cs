public interface IPermanentAffector<in T> {
    void Apply(T target);
}