using UnityEngine;
using UnityEditor;
using Connect.Common;
using System.Collections.Generic;

namespace Connect.Generator
{
    [ExecuteAlways]
    public class CellBrushEditor : MonoBehaviour
    {
        public LevelGenerator level;
        public static BlockType CurrentType = BlockType.None;

        // Các ô brush đã đi qua
        private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();

        private int lastX = -1;
        private int lastY = -1;

        // ================= REGISTER EDITOR INPUT =================
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        // ================= TRACK POSITION =================
        private void Update()
        {
            if (level == null) return;

            TrackBrushPosition();
        }

        private void TrackBrushPosition()
        {
            if (!level.WorldToGrid(transform.position, out int x, out int y))
                return;

            if (x == lastX && y == lastY)
                return;

            Vector2Int cell = new Vector2Int(x, y);
            visitedCells.Add(cell);

            lastX = x;
            lastY = y;

            // Snap brush vào tâm ô
            transform.position = level.GridToWorldCenter(x, y);
        }

        // ================= HOTKEY (EDITOR) =================
        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            if (e == null) return;

            if (!e.alt) return;

            // ALT + Q → Paint
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Q)
            {
                PaintAll();
                e.Use();
            }

            // ALT + Z → Clear
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Z)
            {
                ClearList();
                e.Use();
            }
        }

        // ================= ACTION =================
        private void PaintAll()
        {
            if (visitedCells.Count == 0) return;

            Undo.RecordObject(level, "Paint Brush Path");

            foreach (var cell in visitedCells)
            {
                level.PaintCell(cell.x, cell.y, CurrentType);
            }

            visitedCells.Clear();
            SceneView.RepaintAll();
        }

        private void ClearList()
        {
            visitedCells.Clear();
            lastX = lastY = -1;
            SceneView.RepaintAll();
        }

        // ================= PREVIEW =================
        private void OnDrawGizmos()
        {
            if (level == null) return;

            Gizmos.color = new Color(1f, 0f, 0f, 0.35f);

            foreach (var cell in visitedCells)
            {
                Vector3 pos = level.GridToWorldCenter(cell.x, cell.y);
                Gizmos.DrawCube(pos, Vector3.one * level.cellSize * 0.9f);
            }
        }
    }
}
