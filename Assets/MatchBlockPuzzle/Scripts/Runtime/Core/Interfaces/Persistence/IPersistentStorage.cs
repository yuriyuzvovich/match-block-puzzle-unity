namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Low-level storage abstraction (PlayerPrefs, file system, etc.)
    /// </summary>
    public interface IPersistentStorage
    {
        void SetString(string key, string value);
        string GetString(string key, string defaultValue = "");
        void SetInt(string key, int value);
        int GetInt(string key, int defaultValue = 0);
        bool HasKey(string key);
        void DeleteKey(string key);
        void DeleteAll();
        void Save();
    }
}
