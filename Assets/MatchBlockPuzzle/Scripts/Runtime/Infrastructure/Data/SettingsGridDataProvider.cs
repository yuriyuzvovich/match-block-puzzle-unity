using System;
using MatchPuzzle.Core.Interfaces;
using UnityEngine;

namespace MatchPuzzle.Infrastructure.Data
{
    /// <summary>
    /// Default grid data provider that wraps a GridSettings asset.
    /// </summary>
    public class SettingsGridDataProvider : IGridDataProvider
    {
        private readonly GridSettings _gridSettings;

        public SettingsGridDataProvider(GridSettings gridSettings)
        {
            if (!gridSettings) throw new ArgumentNullException(nameof(gridSettings), "GridSettings asset cannot be null.");
            _gridSettings = gridSettings;
        }

        public float CellSize => _gridSettings.CellSize;
        public Vector2 GridOffset => _gridSettings.GridOffset;
    }
}