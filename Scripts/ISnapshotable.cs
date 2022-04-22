public interface ISnapshotable<T>
{
    T MakeSnapshot();
    void ApplySnapshot(T data);
}