using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace MatchPuzzle.Editor.Tools
{
    /// <summary>
    /// Editor utilities to inspect and clear the persistent game state.
    /// </summary>
    public static class PersistenceDebugMenu
    {
        private const string GAME_STATE_KEY = "MatchPuzzle_GameState";

        [MenuItem("MatchPuzzle/Tools/Print Persistent State")]
        public static void PrintPersistentState()
        {
            if (!PlayerPrefs.HasKey(GAME_STATE_KEY))
            {
                EditorUtility.DisplayDialog("Persistence", "No saved game state found.", "OK");
                Debug.Log("[Persistence Debug] No saved game state found.");
                return;
            }

            var json = PlayerPrefs.GetString(GAME_STATE_KEY);
            var pretty = TryPrettyPrintJson(json);

            Debug.Log($"[Persistence Debug] Saved game state ({json?.Length ?? 0} chars):\n{pretty}");
            EditorGUIUtility.systemCopyBuffer = pretty;
            EditorUtility.DisplayDialog("Persistence", "Saved game state printed to Console and copied to clipboard.", "OK");
        }

        [MenuItem("MatchPuzzle/Tools/Clear Persistent State")]
        public static void ClearPersistentState()
        {
            if (!PlayerPrefs.HasKey(GAME_STATE_KEY))
            {
                EditorUtility.DisplayDialog("Persistence", "No saved game state found.", "OK");
                Debug.Log("[Persistence Debug] No saved game state found.");
                return;
            }

            var json = PlayerPrefs.GetString(GAME_STATE_KEY);
            var pretty = TryPrettyPrintJson(json);

            Debug.Log($"[Persistence Debug] Saved game state ({json?.Length ?? 0} chars):\n{pretty}");

            if (EditorUtility.DisplayDialog("Clear Saved State",
                "Are you sure you want to clear the saved game state? This cannot be undone.",
                "Clear", "Cancel"))
            {
                PlayerPrefs.DeleteKey(GAME_STATE_KEY);
                PlayerPrefs.Save();
                Debug.Log("[Persistence Debug] Cleared saved game state.");
                EditorUtility.DisplayDialog("Persistence", "Saved game state cleared.", "OK");
            }
            else
            {
                Debug.Log("[Persistence Debug] Clear canceled by user.");
            }
        }

        // Menu validation to disable menu items when no saved state exists
        [MenuItem("MatchPuzzle/Tools/Print Persistent State", true)]
        private static bool ValidatePrintPersistentState()
        {
            return PlayerPrefs.HasKey(GAME_STATE_KEY);
        }

        [MenuItem("MatchPuzzle/Tools/Print And Clear Persistent State", true)]
        private static bool ValidatePrintAndClear()
        {
            return PlayerPrefs.HasKey(GAME_STATE_KEY);
        }

        private static string TryPrettyPrintJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return "(empty)";

            try
            {
                var token = JToken.Parse(json);
                return token.ToString(Formatting.Indented);
            }
            catch (Exception)
            {
                // Fall back to raw JSON if parsing fails
                return json;
            }
        }
    }
}
