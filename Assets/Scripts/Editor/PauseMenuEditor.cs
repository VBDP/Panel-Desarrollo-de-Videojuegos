using UnityEngine;
using UnityEditor;
using TMPro;

[CustomEditor(typeof(PauseMenu))]
public class PauseMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PauseMenu pauseMenu = (PauseMenu)target;

        EditorGUILayout.HelpBox(
            "PauseMenu: Pulsa la tecla ESC para abrir y cerrar el menú.\n" +
            "Pulsa A para sumar 1 punto al Equipo A.\n" +
            "Pulsa B para sumar 1 punto al Equipo B.",
            MessageType.Info
        );

        EditorGUILayout.Space(5);

        if (GUILayout.Button("🔍 Auto-detectar referencias (Score A y Score B)", GUILayout.Height(30)))
        {
            Undo.RecordObject(pauseMenu, "Auto-asignar referencias PauseMenu");
            pauseMenu.AutoFindReferences();
            EditorUtility.SetDirty(pauseMenu);
            Debug.Log("<color=green>[PauseMenuEditor]</color> ¡Referencias de Score A y Score B detectadas y asignadas!");
        }

        EditorGUILayout.Space(10);
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Controles en Tiempo Real (Play Mode)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Estado: {(pauseMenu.IsPaused ? "⏸️ PAUSADO" : "▶️ JUGANDO")}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Equipo A: {pauseMenu.ScoreA} pts | Equipo B: {pauseMenu.ScoreB} pts", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            if (GUILayout.Button(pauseMenu.IsPaused ? "Reanudar Menú (ESC)" : "Abrir Menú de Pausa (ESC)", GUILayout.Height(26)))
            {
                pauseMenu.TogglePause();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("➕ Equipo A (+1)", GUILayout.Height(26)))
            {
                pauseMenu.AddScoreA(1);
            }
            if (GUILayout.Button("➖ Equipo A (-1)", GUILayout.Height(26)))
            {
                pauseMenu.SubtractScoreA(1);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("➕ Equipo B (+1)", GUILayout.Height(26)))
            {
                pauseMenu.AddScoreB(1);
            }
            if (GUILayout.Button("➖ Equipo B (-1)", GUILayout.Height(26)))
            {
                pauseMenu.SubtractScoreB(1);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("🔄 Reiniciar Marcadores", GUILayout.Height(24)))
            {
                pauseMenu.ResetScores();
            }

            EditorGUILayout.EndVertical();
        }
    }

    [MenuItem("Tools/Configurar PauseMenu en la Escena")]
    public static void SetupPauseMenuInScene()
    {
        GameObject pauseMenuObj = GameObject.Find("PauseMenu");
        if (pauseMenuObj == null)
        {
            Debug.LogError("[PauseMenu] No se encontró ningún objeto llamado 'PauseMenu' en la escena activa.");
            return;
        }

        PauseMenu pauseMenu = pauseMenuObj.GetComponent<PauseMenu>();
        if (pauseMenu == null)
        {
            pauseMenu = Undo.AddComponent<PauseMenu>(pauseMenuObj);
        }

        CanvasGroup cg = pauseMenuObj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = Undo.AddComponent<CanvasGroup>(pauseMenuObj);
        }

        pauseMenu.AutoFindReferences();
        EditorUtility.SetDirty(pauseMenuObj);
        EditorUtility.SetDirty(pauseMenu);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(pauseMenuObj.scene);

        Debug.Log("<color=green>[PauseMenu]</color> ¡PauseMenu configurado con éxito! Se han enlazado Score A, Score B y el CanvasGroup.");
    }
}
