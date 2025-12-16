using System;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Services.LevelRepository
{
    [Serializable]
    public class LevelMetadata
    {
        [SerializeField] private int levelNumber;
        [SerializeField] private string resourcePath;
        [SerializeField] private int rows;
        [SerializeField] private int columns;

        public int LevelNumber
        {
            get => levelNumber;
            set => levelNumber = value;
        }

        public string ResourcePath
        {
            get => resourcePath;
            set => resourcePath = value;
        }

        public int Rows
        {
            get => rows;
            set => rows = value;
        }

        public int Columns
        {
            get => columns;
            set => columns = value;
        }

        public LevelMetadata()
        {
        }
    }
}