public interface IBlueprint
{
    string ID { get; }
#if UNITY_EDITOR
    void GenerateID();
#endif
}
