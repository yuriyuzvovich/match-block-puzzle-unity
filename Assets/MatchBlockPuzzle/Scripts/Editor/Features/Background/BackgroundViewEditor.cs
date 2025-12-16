using MatchPuzzle.Features.Background;
using UnityEditor;
using UnityEngine;

namespace MatchPuzzle.Editor
{
    /// <summary>
    /// Custom editor for BackgroundView that visualizes the ground line in the scene view
    /// </summary>
    [CustomEditor(typeof(BackgroundView))]
    public class BackgroundViewEditor : UnityEditor.Editor
    {
        private static readonly Color GroundLineColor = new Color(0f, 1f, 0f, 0.8f);
        private static readonly Color GroundLineFillColor = new Color(0f, 1f, 0f, 0.1f);
        private const float GroundLineThickness = 3f;
        private const float GroundLineExtent = 100f;

        private BackgroundView _backgroundView;

        private void OnEnable()
        {
            _backgroundView = (BackgroundView)target;
        }

        private void OnSceneGUI()
        {
            if (_backgroundView == null || _backgroundView.GroundLine == null)
                return;

            // Get the ground line position
            Vector3 groundPosition = _backgroundView.GroundLine.position;

            // Draw ground line
            Handles.color = GroundLineColor;
            
            // Draw a horizontal line
            Vector3 leftPoint = new Vector3(groundPosition.x - GroundLineExtent, groundPosition.y, groundPosition.z);
            Vector3 rightPoint = new Vector3(groundPosition.x + GroundLineExtent, groundPosition.y, groundPosition.z);
            
            Handles.DrawLine(leftPoint, rightPoint, GroundLineThickness);

            // Draw label
            Handles.Label(groundPosition + Vector3.right * 0.5f, "Ground Line", new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = GroundLineColor },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            });

            // Draw position handle to allow moving the ground line in the scene
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(groundPosition, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_backgroundView.GroundLine, "Move Ground Line");
                _backgroundView.GroundLine.position = newPosition;
            }

            // Draw a filled rectangle below the ground line to visualize the ground area
            Handles.color = GroundLineFillColor;
            Vector3[] fillPoints = new Vector3[4]
            {
                new Vector3(groundPosition.x - GroundLineExtent, groundPosition.y, groundPosition.z),
                new Vector3(groundPosition.x + GroundLineExtent, groundPosition.y, groundPosition.z),
                new Vector3(groundPosition.x + GroundLineExtent, groundPosition.y - 10f, groundPosition.z),
                new Vector3(groundPosition.x - GroundLineExtent, groundPosition.y - 10f, groundPosition.z)
            };
            Handles.DrawSolidRectangleWithOutline(fillPoints, GroundLineFillColor, Color.clear);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (_backgroundView.GroundLine == null)
            {
                EditorGUILayout.HelpBox("Ground Line is not assigned. Please assign a Transform to visualize it in the Scene view.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"Ground Line Position: {_backgroundView.GroundLine.position}", MessageType.Info);
            }
        }
    }
}
