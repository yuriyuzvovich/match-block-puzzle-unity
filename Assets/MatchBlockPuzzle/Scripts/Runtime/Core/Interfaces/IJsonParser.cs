namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Abstracts JSON serialization and deserialization.
    /// </summary>
    public interface IJsonParser
    {
        string Serialize<T>(T data);
        T Deserialize<T>(string json);
    }
}
