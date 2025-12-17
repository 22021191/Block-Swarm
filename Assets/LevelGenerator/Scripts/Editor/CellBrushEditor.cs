using UnityEditor;
using UnityEngine;
using Connect.Common;

namespace Connect.Generator
{
    [InitializeOnLoad]
    public static class CellBrushEditor
    {
        public static BlockType CurrentType = BlockType.None;

        static CellBrushEditor()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static CellData lastPaintedCell;

        static void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

            if (!hit)
            {
                lastPaintedCell = null;
                return;
            }

            CellData cell = hit.collider.GetComponent<CellData>();
            if (cell == null)
            {
                lastPaintedCell = null;
                return;
            }

            // ================= HIGHLIGHT =================
            DrawHighlight(cell);

            // ================= PAINT =================
            bool isLeftMouse = e.button == 0;
            bool isErase = e.control || e.command;

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                Debug.Log("oks");
                if (cell != lastPaintedCell)
                {
                    if (isErase)
                        Erase(cell);
                    else
                        Paint(cell);

                    lastPaintedCell = cell;
                }

                e.Use();
            }

            if (e.type == EventType.MouseUp)
            {
                lastPaintedCell = null;
            }
        }

        // ================= ACTIONS =================
        static void Paint(CellData cell)
        {
            if (cell.type == CurrentType)
                return;

            Undo.RecordObject(cell, "Paint Cell");
            cell.type = CurrentType;
            EditorUtility.SetDirty(cell);
        }

        static void Erase(CellData cell)
        {
            if (cell.type == BlockType.None)
                return;

            Undo.RecordObject(cell, "Erase Cell");
            cell.type = BlockType.None;
            EditorUtility.SetDirty(cell);
        }

        // ================= VISUAL =================
        static void DrawHighlight(CellData cell)
        {
            Handles.color = Color.yellow;
            Handles.DrawWireCube(
                cell.transform.position,
                Vector3.one * 0.98f
            );

            SceneView.RepaintAll();
        }
    }
}
