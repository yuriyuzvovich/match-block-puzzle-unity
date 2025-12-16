using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services
{
    /// <summary>
    /// Storage implementation using Unity PlayerPrefs
    /// </summary>
    public class PlayerPrefsStorage : IPersistentStorage
    {
        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
