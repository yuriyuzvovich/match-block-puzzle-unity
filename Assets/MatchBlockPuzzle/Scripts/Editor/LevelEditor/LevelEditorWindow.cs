using MatchPuzzle.Core.Domain;
using MatchPuzzle.Infrastructure.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MatchPuzzle.Infrastructure.Services;
using MatchPuzzle.Infrastructure.Services.LevelRepository;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace MatchPuzzle.Editor.LevelEditor
{
    /// <summary>
    /// Editor window for creating and editing levels
    /// </summary>
    public class LevelEditorWindow : EditorWindow
    {
        private const string LEVELS_FOLDER = "Assets/Resources/Levels";
        private const string LEVEL_DATA_FOLDER = "Assets/Resources/Levels/Data";
        private const string LEVEL_RESOURCE_PREFIX = "Levels/Data/";
        private const string LEVEL_CONFIG_PATH = "Assets/Resources/Levels/LevelConfiguration.asset";

        [MenuItem("MatchPuzzle/Level Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(600, 400);
        }

        private LevelConfiguration _levelConfiguration;
        private Level _currentLevel;
        private int _selectedLevelIndex = -1;
        private GameSettings _gameSettings;

        // Grid editing
        private int _gridRows = 6;
        private int _gridColumns = 6;
        private BlockTypeId _selectedBlockType = BlockTypeId.None;
        private BlockTypeId[,] _gridData;

        // UI
        private Vector2 _levelListScrollPosition;
        private Vector2 _gridScrollPosition;

        private void OnEnable()
        {
            LoadLevelConfiguration();
            EnsureDataFolderExists();
            LoadGameSettings();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            // Left panel: Level list
            DrawLevelListPanel();

            // Right panel: Level editor
            DrawLevelEditorPanel();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLevelListPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));

            GUILayout.Label("Levels", EditorStyles.boldLabel);

            if (_levelConfiguration == null)
            {
                if (GUILayout.Button("Create Level Configuration"))
                {
                    CreateLevelConfiguration();
                }
            }
            else
            {
                _levelListScrollPosition = EditorGUILayout.BeginScrollView(_levelListScrollPosition);

                for (int i = 0; i < _levelConfiguration.Levels.Count; i++)
                {
                    var level = _levelConfiguration.Levels[i];
                    var isSelected = i == _selectedLevelIndex;

                    var style = isSelected ? new GUIStyle(GUI.skin.button) { normal = { background = Texture2D.grayTexture } } : GUI.skin.button;

                    if (GUILayout.Button($"Level {level.LevelNumber} ({level.Columns}x{level.Rows})", style))
                    {
                        SelectLevel(i);
                    }
                }

                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("New Level"))
                {
                    CreateNewLevel();
                }

                if (_selectedLevelIndex >= 0 && GUILayout.Button("Delete Level"))
                {
                    DeleteLevel(_selectedLevelIndex);
                }

                if (GUILayout.Button("Save All"))
                {
                    SaveLevelConfiguration();
                }

                if (GUILayout.Button("Export to JSON"))
                {
                    ExportLevelsToJSON();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawLevelEditorPanel()
        {
            EditorGUILayout.BeginVertical();

            if (_currentLevel == null)
            {
                GUILayout.Label("Select or create a level to edit", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
                return;
            }

            GUILayout.Label($"Editing Level {_currentLevel.LevelNumber}", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");
            _gameSettings = (GameSettings) EditorGUILayout.ObjectField("Game Settings", _gameSettings, typeof(GameSettings), false);

            if (_gameSettings == null)
            {
                EditorGUILayout.HelpBox("Assign a GameSettings asset to pick block types from ScriptableObjects.", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Grid size
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Grid Size", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rows:", GUILayout.Width(60));
            var newRows = EditorGUILayout.IntSlider(_gridRows, 1, 20, GUILayout.ExpandWidth(true));
            GUILayout.Label(_gridRows.ToString(), GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Columns:", GUILayout.Width(60));
            var newColumns = EditorGUILayout.IntSlider(_gridColumns, 1, 20, GUILayout.ExpandWidth(true));
            GUILayout.Label(_gridColumns.ToString(), GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            if (newRows != _gridRows || newColumns != _gridColumns)
            {
                _gridRows = newRows;
                _gridColumns = newColumns;
                ResizeGrid();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Block type selector
            DrawBlockTypeSelector();

            EditorGUILayout.Space();

            // Grid editor
            _gridScrollPosition = EditorGUILayout.BeginScrollView(_gridScrollPosition);
            DrawGrid();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear Grid"))
            {
                ClearGrid();
            }

            if (GUILayout.Button("Apply Changes"))
            {
                ApplyChangesToLevel();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawGrid()
        {
            if (_gridData == null)
                return;

            var cellSize = 40f;

            // Draw from top to bottom (row 0 is bottom in game, but we show it at bottom of editor too)
            for (int row = _gridRows - 1; row >= 0; row--)
            {
                EditorGUILayout.BeginHorizontal();

                for (int col = 0; col < _gridColumns; col++)
                {
                    var blockType = _gridData[row, col];
                    var color = GetBlockTypeColor(blockType);

                    var originalColor = GUI.backgroundColor;
                    GUI.backgroundColor = color;

                    if (GUILayout.Button($"{GetBlockTypeLabel(blockType)}", GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
                    {
                        // Toggle: clicking a cell with the same type clears it, otherwise paint the selected type.
                        var newType = blockType == _selectedBlockType ? BlockTypeId.None : _selectedBlockType;
                        _gridData[row, col] = newType;
                    }

                    GUI.backgroundColor = originalColor;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawBlockTypeSelector()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Block Type:", GUILayout.Width(100));

            if (_gameSettings == null)
            {
                GUILayout.Label("No GameSettings assigned", EditorStyles.miniLabel);
            }
            else
            {
                var blockTypes = _gameSettings.GetAllBlockTypes()
                    .Where(t => t != null && !t.TypeId.IsNone)
                    .ToList();

                if (blockTypes.Count == 0)
                {
                    GUILayout.Label("No BlockTypeData assets in GameSettings", EditorStyles.miniLabel);
                }
                else
                {
                    var labels = blockTypes.Select(t => string.IsNullOrEmpty(t.DisplayName) ? t.Id : t.DisplayName).ToArray();
                    var currentIndex = Mathf.Max(0, blockTypes.FindIndex(t => t.TypeId == _selectedBlockType));
                    currentIndex = Mathf.Clamp(currentIndex, 0, labels.Length - 1);

                    var newIndex = EditorGUILayout.Popup(currentIndex, labels);
                    _selectedBlockType = blockTypes[newIndex].TypeId;
                }
            }

            if (GUILayout.Button("None", GUILayout.Width(60)))
            {
                _selectedBlockType = BlockTypeId.None;
            }

            EditorGUILayout.EndHorizontal();
        }

        private Color GetBlockTypeColor(BlockTypeId type)
        {
            var data = _gameSettings != null ? _gameSettings.GetBlockTypeData(type) : null;
            if (data != null)
                return data.EditorColor;

            return type.IsNone ? Color.gray : Color.white;
        }

        private string GetBlockTypeLabel(BlockTypeId type)
        {
            var data = _gameSettings != null ? _gameSettings.GetBlockTypeData(type) : null;

            if (data != null && !string.IsNullOrEmpty(data.ShortLabel))
                return data.ShortLabel;

            if (!type.IsNone && !string.IsNullOrEmpty(type.Value))
                return type.Value.Substring(0, 1).ToUpperInvariant();

            return "-";
        }

        private void SelectLevel(int index)
        {
            _selectedLevelIndex = index;
            var metadata = GetSelectedMetadata();

            if (metadata == null)
            {
                _currentLevel = null;
                _gridData = null;
                return;
            }

            _currentLevel = LoadLevelFromDisk(metadata);

            if (_currentLevel == null)
            {
                _gridData = null;
                return;
            }

            // Prefer live data, fall back to stored metadata.
            _gridRows = _currentLevel.Rows > 0 ? _currentLevel.Rows : metadata.Rows;
            _gridColumns = _currentLevel.Columns > 0 ? _currentLevel.Columns : metadata.Columns;

            metadata.Rows = _gridRows;
            metadata.Columns = _gridColumns;

            InitializeGrid();
            LoadLevelIntoGrid();
        }

        private void CreateNewLevel()
        {
            var newLevelNumber = _levelConfiguration.Levels.Count > 0
                ? _levelConfiguration.Levels.Max(l => l.LevelNumber) + 1
                : 1;

            var metadata = CreateMetadata(newLevelNumber, _gridRows, _gridColumns);
            var newLevel = new Level(newLevelNumber, _gridRows, _gridColumns);

            SaveLevelToDisk(metadata, newLevel);

            var entries = _levelConfiguration.Levels.ToList();
            entries.Add(metadata);
            entries = entries.OrderBy(x => x.LevelNumber).ToList();
            _levelConfiguration.SetLevels(entries);

            _selectedLevelIndex = entries.FindIndex(x => x.LevelNumber == newLevelNumber);
            SelectLevel(_selectedLevelIndex);

            SaveLevelConfiguration();
        }

        private void DeleteLevel(int index)
        {
            var metadata = _levelConfiguration.Levels[index];
            if (EditorUtility.DisplayDialog(
                    "Delete Level",
                    $"Are you sure you want to delete Level {metadata.LevelNumber}?",
                    "Yes", "No"
                ))
            {
                var entries = _levelConfiguration.Levels.ToList();
                entries.RemoveAt(index);
                _levelConfiguration.SetLevels(entries);

                DeleteLevelAsset(metadata);
                _selectedLevelIndex = -1;
                _currentLevel = null;
                _gridData = null;

                SaveLevelConfiguration();
            }
        }

        private void InitializeGrid()
        {
            _gridData = new BlockTypeId[_gridRows, _gridColumns];
        }

        private void ResizeGrid()
        {
            var oldGrid = _gridData;
            _gridData = new BlockTypeId[_gridRows, _gridColumns];

            if (oldGrid != null)
            {
                // Copy old data
                var minRows = Mathf.Min(_gridRows, oldGrid.GetLength(0));
                var minCols = Mathf.Min(_gridColumns, oldGrid.GetLength(1));

                for (int row = 0; row < minRows; row++)
                {
                    for (int col = 0; col < minCols; col++)
                    {
                        _gridData[row, col] = oldGrid[row, col];
                    }
                }
            }
        }

        private void ClearGrid()
        {
            InitializeGrid();
        }

        private void LoadLevelIntoGrid()
        {
            if (_currentLevel == null || _currentLevel.Blocks == null)
                return;

            foreach (var blockData in _currentLevel.Blocks)
            {
                if (blockData.Row >= 0 && blockData.Row < _gridRows &&
                    blockData.Column >= 0 && blockData.Column < _gridColumns)
                {
                    _gridData[blockData.Row, blockData.Column] = blockData.Type;
                }
            }
        }

        private void ApplyChangesToLevel()
        {
            if (_currentLevel == null)
            {
                EditorUtility.DisplayDialog("Error", "No level selected!", "OK");
                return;
            }

            var metadata = GetSelectedMetadata();
            if (metadata == null)
            {
                EditorUtility.DisplayDialog("Error", "No level metadata found!", "OK");
                return;
            }

            Debug.Log($"Before apply - Level {_currentLevel.LevelNumber}: {_currentLevel.Blocks.Count} blocks");

            _currentLevel.Rows = _gridRows;
            _currentLevel.Columns = _gridColumns;
            metadata.Rows = _gridRows;
            metadata.Columns = _gridColumns;

            // Create new list instead of clearing to ensure proper serialization
            var newBlocks = new List<BlockData>();

            for (int row = 0; row < _gridRows; row++)
            {
                for (int col = 0; col < _gridColumns; col++)
                {
                    var blockType = _gridData[row, col];
                    if (blockType != BlockTypeId.None)
                    {
                        newBlocks.Add(new BlockData(blockType, row, col));
                        Debug.Log($"Adding block: {blockType} at ({row}, {col})");
                    }
                }
            }

            _currentLevel.Blocks = newBlocks;
            Debug.Log($"After apply - Level {_currentLevel.LevelNumber}: {_currentLevel.Blocks.Count} blocks");

            SaveLevelToDisk(metadata, _currentLevel);

            // Mark as dirty BEFORE saving
            EditorUtility.SetDirty(_levelConfiguration);

            SaveLevelConfiguration();

            // Verify after save
            Debug.Log($"After save - Level {_currentLevel.LevelNumber}: {_currentLevel.Blocks.Count} blocks");

            EditorUtility.DisplayDialog("Success", $"Level changes applied! {newBlocks.Count} blocks saved.", "OK");
        }

        private void LoadLevelConfiguration()
        {
            _levelConfiguration = AssetDatabase.LoadAssetAtPath<LevelConfiguration>(LEVEL_CONFIG_PATH);
        }

        private void LoadGameSettings()
        {
            if (_gameSettings != null)
                return;

            var guids = AssetDatabase.FindAssets("t:GameSettings");
            if (guids.Length == 0)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _gameSettings = AssetDatabase.LoadAssetAtPath<GameSettings>(path);
        }

        private void CreateLevelConfiguration()
        {
            if (!Directory.Exists(LEVELS_FOLDER))
            {
                Directory.CreateDirectory(LEVELS_FOLDER);
            }
            EnsureDataFolderExists();

            _levelConfiguration = CreateInstance<LevelConfiguration>();
            AssetDatabase.CreateAsset(_levelConfiguration, LEVEL_CONFIG_PATH);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Success", "Level Configuration created!", "OK");
        }

        private void SaveLevelConfiguration()
        {
            if (_levelConfiguration != null)
            {
                _levelConfiguration.Sort();
                // Mark the ScriptableObject as modified
                EditorUtility.SetDirty(_levelConfiguration);

                // Force save to disk
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Level configuration saved. Total levels: {_levelConfiguration.Levels.Count}");
            }
        }

        private void ExportLevelsToJSON()
        {
            var path = EditorUtility.SaveFilePanel("Export Levels to JSON", "", "levels.json", "json");

            if (string.IsNullOrEmpty(path))
                return;

            var levels = new List<Level>();

            foreach (var metadata in _levelConfiguration.Levels)
            {
                var level = LoadLevelFromDisk(metadata);
                if (level != null)
                {
                    levels.Add(level);
                }
            }

            var json = JsonConvert.SerializeObject(new LevelListWrapper(levels), Formatting.Indented);
            File.WriteAllText(path, json);

            EditorUtility.DisplayDialog("Success", $"Levels exported to {path}", "OK");
        }

        private LevelMetadata GetSelectedMetadata()
        {
            if (_levelConfiguration == null)
                return null;

            if (_selectedLevelIndex < 0 || _selectedLevelIndex >= _levelConfiguration.Levels.Count)
                return null;

            return _levelConfiguration.Levels[_selectedLevelIndex];
        }

        private LevelMetadata CreateMetadata(int levelNumber, int rows, int columns)
        {
            return new LevelMetadata {
                LevelNumber = levelNumber,
                Rows = rows,
                Columns = columns,
                ResourcePath = $"{LEVEL_RESOURCE_PREFIX}level_{levelNumber:D4}"
            };
        }

        private string GetLevelAssetPath(LevelMetadata metadata)
        {
            var resourcePath = metadata.ResourcePath;
            if (string.IsNullOrEmpty(resourcePath))
            {
                resourcePath = $"{LEVEL_RESOURCE_PREFIX}level_{metadata.LevelNumber:D4}";
                metadata.ResourcePath = resourcePath;
            }

            return Path.Combine("Assets/Resources", resourcePath + ".json").Replace("\\", "/");
        }

        private Level LoadLevelFromDisk(LevelMetadata metadata)
        {
            var assetPath = GetLevelAssetPath(metadata);

            if (!File.Exists(assetPath))
            {
                Debug.LogWarning($"Level file missing at {assetPath}");
                return null;
            }

            try
            {
                var json = File.ReadAllText(assetPath);
                return JsonConvert.DeserializeObject<Level>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load level {metadata.LevelNumber}: {ex.Message}");
                return null;
            }
        }

        private void SaveLevelToDisk(LevelMetadata metadata, Level level)
        {
            EnsureDataFolderExists();

            var assetPath = GetLevelAssetPath(metadata);
            var directory = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(level, Formatting.Indented);
            File.WriteAllText(assetPath, json);
            AssetDatabase.ImportAsset(assetPath);
        }

        private void DeleteLevelAsset(LevelMetadata metadata)
        {
            var assetPath = GetLevelAssetPath(metadata);
            if (File.Exists(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        private void EnsureDataFolderExists()
        {
            if (!Directory.Exists(LEVEL_DATA_FOLDER))
            {
                Directory.CreateDirectory(LEVEL_DATA_FOLDER);
            }
        }

        [System.Serializable]
        private class LevelListWrapper
        {
            [SerializeField, JsonProperty] private List<Level> levels;

            [JsonIgnore] public List<Level> Levels => levels;

            public LevelListWrapper()
            {
            }

            public LevelListWrapper(List<Level> levels)
            {
                this.levels = levels;
            }
        }
    }
}