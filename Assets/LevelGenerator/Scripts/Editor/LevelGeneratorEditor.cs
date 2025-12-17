using UnityEditor;
using UnityEngine;
using Connect.Generator;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : EditorWindow
{
    private LevelGenerator generator;

    [MenuItem("Tools/Level Editor")]
    public static void Open()
    {
        GetWindow<LevelGeneratorEditor>("Level Editor");
    }

   private void OnGUI()
    {
        GUILayout.Label("LEVEL EDITOR", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // ⭐ FIELD KÉO THẢ
        generator = (LevelGenerator)EditorGUILayout.ObjectField(
            "Level Generator",
            generator,
            typeof(LevelGenerator),
            true
        );

        GUILayout.Space(10);

        if (generator == null)
        {
            EditorGUILayout.HelpBox(
                "Drag a LevelGenerator from Hierarchy here",
                MessageType.Info
            );
            return;
        }

        DrawLevelButtons();
        GUILayout.Space(10);
        DrawBrushButtons();
    }
    private void DrawLevelButtons()
    {
        if (GUILayout.Button("CREATE LEVEL", GUILayout.Height(30)))
        {
            generator.SpawnBoard();
        }

        if (GUILayout.Button("CLEAR LEVEL", GUILayout.Height(30)))
        {
            generator.ClearBoard();
        }

        if (GUILayout.Button("SAVE LEVEL", GUILayout.Height(30)))
        {
            generator.SaveLevel();
        }
    }
    private void DrawBrushButtons()
    {
        GUILayout.Label("Brush Settings", EditorStyles.boldLabel);

        CellBrushEditor.CurrentType = (BlockType)EditorGUILayout.EnumPopup(
            "Current Brush",
           CellBrushEditor.CurrentType
        );

    }
}
