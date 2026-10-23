using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

[CustomEditor(typeof(QuizManager))]
public class QuizManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        QuizManager manager = (QuizManager)target;

        EditorGUILayout.HelpBox(
            "Kahoot Quiz Manager: Controla la pregunta, los niveles (Nivel 1, 2 y 3), los 4 botones de respuesta y los paneles de navegación.",
            MessageType.Info
        );

        if (GUILayout.Button("🔍 Auto-detectar y asignar UI, Niveles y Paneles", GUILayout.Height(32)))
        {
            Undo.RecordObject(manager, "Auto-asignar referencias QuizManager");
            manager.AutoFindReferences();
            EditorUtility.SetDirty(manager);
            Debug.Log("[QuizManagerEditor] ¡Referencias de UI, Niveles y Paneles asignadas con éxito!");
        }

        if (GUILayout.Button("⭐ Crear GameObjects de Nivel 1, 2 y 3 automáticamente", GUILayout.Height(28)))
        {
            CreateLevelObjects(manager);
        }

        EditorGUILayout.Space(10);

        // Dibuja el inspector normal con todas las variables serializadas
        DrawDefaultInspector();

        // Controles útiles mientras el juego está en modo Play
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Controles en Tiempo Real (Play Mode)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Aciertos actuales: {manager.CorrectAnswersCount}", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Nivel 1 (Play)", GUILayout.Height(24)))
            {
                manager.PlayLevel(1);
            }

            if (GUILayout.Button("Nivel 2 (Play)", GUILayout.Height(24)))
            {
                manager.PlayLevel(2);
            }

            if (GUILayout.Button("Nivel 3 (Play)", GUILayout.Height(24)))
            {
                manager.PlayLevel(3);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("⏭ Siguiente Pregunta", GUILayout.Height(26)))
            {
                manager.NextQuestion();
            }

            if (GUILayout.Button("🔄 Reiniciar Quiz", GUILayout.Height(26)))
            {
                manager.RestartQuiz();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    [MenuItem("Tools/Kahoot Quiz/Crear GameObjects de Niveles (1, 2 y 3)")]
    public static void CreateLevelsMenu()
    {
        QuizManager manager = FindFirstObjectByType<QuizManager>();
        if (manager != null)
        {
            CreateLevelObjects(manager);
        }
        else
        {
            Debug.LogWarning("[Kahoot Quiz] No se encontró el componente QuizManager en la escena.");
        }
    }

    public static void CreateLevelObjects(QuizManager manager)
    {
        // 1. Buscar o crear contenedor "Levels"
        GameObject levelsRoot = GameObject.Find("Levels");
        if (levelsRoot == null)
        {
            levelsRoot = new GameObject("Levels");
            Undo.RegisterCreatedObjectUndo(levelsRoot, "Crear Levels");
        }

        // Nivel 1
        QuizLevel lvl1 = FindOrCreateLevel(levelsRoot, "Nivel 1", 1);
        // Nivel 2
        QuizLevel lvl2 = FindOrCreateLevel(levelsRoot, "Nivel 2", 2);
        // Nivel 3
        QuizLevel lvl3 = FindOrCreateLevel(levelsRoot, "Nivel 3", 3);

        manager.AutoFindReferences();
        EditorUtility.SetDirty(manager);

        Debug.Log("<color=green>[Kahoot Quiz] ¡GameObjects 'Nivel 1', 'Nivel 2' y 'Nivel 3' creados y configurados con sus 12 preguntas cada uno!</color>");
    }

    private static QuizLevel FindOrCreateLevel(GameObject parent, string name, int levelIndex)
    {
        Transform child = parent.transform.Find(name);
        GameObject go;
        if (child == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            Undo.RegisterCreatedObjectUndo(go, "Crear " + name);
        }
        else
        {
            go = child.gameObject;
        }

        QuizLevel ql = go.GetComponent<QuizLevel>();
        if (ql == null)
        {
            ql = Undo.AddComponent<QuizLevel>(go);
        }

        if (levelIndex == 1) ql.LoadLevel1Questions();
        else if (levelIndex == 2) ql.LoadLevel2Questions();
        else if (levelIndex == 3) ql.LoadLevel3Questions();

        EditorUtility.SetDirty(go);
        return ql;
    }

    [MenuItem("Tools/Kahoot Quiz/Configurar QuizManager en Escena")]
    public static void SetupQuizManagerInScene()
    {
        // 1. Buscar el objeto "Quiz Manager" o "Quiz"
        GameObject quizGo = GameObject.Find("Quiz Manager");
        if (quizGo == null) quizGo = GameObject.Find("Quiz");

        if (quizGo == null)
        {
            quizGo = new GameObject("Quiz Manager");
            Undo.RegisterCreatedObjectUndo(quizGo, "Crear Quiz Manager");
        }

        Undo.RegisterCompleteObjectUndo(quizGo, "Configurar QuizManager");

        QuizManager manager = quizGo.GetComponent<QuizManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<QuizManager>(quizGo);
        }

        // 2. Configurar MainMenu si existe
        GameObject mainMenuGo = GameObject.Find("MainMenu");
        if (mainMenuGo == null) mainMenuGo = GameObject.Find("Main Menu");

        if (mainMenuGo != null)
        {
            Undo.RegisterCompleteObjectUndo(mainMenuGo, "Configurar MainMenu");
            MainMenu menu = mainMenuGo.GetComponent<MainMenu>();
            if (menu == null)
            {
                menu = Undo.AddComponent<MainMenu>(mainMenuGo);
            }
            menu.AutoFindReferences();
            EditorUtility.SetDirty(mainMenuGo);
        }

        // 3. Configurar LevelSelection si existe
        GameObject levelSelGo = GameObject.Find("LevelSelection");
        if (levelSelGo == null) levelSelGo = GameObject.Find("Level Selection");

        if (levelSelGo != null)
        {
            Undo.RegisterCompleteObjectUndo(levelSelGo, "Configurar LevelSelection");
            LevelSelectionMenu lsm = levelSelGo.GetComponent<LevelSelectionMenu>();
            if (lsm == null)
            {
                lsm = Undo.AddComponent<LevelSelectionMenu>(levelSelGo);
            }
            lsm.AutoFindReferences();
            EditorUtility.SetDirty(levelSelGo);
        }

        // 4. Configurar GameOver si existe
        GameObject gameOverGo = GameObject.Find("GameOver menu");
        if (gameOverGo == null) gameOverGo = GameObject.Find("GameOver");
        if (gameOverGo == null) gameOverGo = GameObject.Find("GameOverMenu");
        if (gameOverGo == null) gameOverGo = GameObject.Find("GameOver Panel");

        if (gameOverGo != null)
        {
            Undo.RegisterCompleteObjectUndo(gameOverGo, "Configurar GameOverMenu");

            MainMenu accidentalMenu = gameOverGo.GetComponent<MainMenu>();
            if (accidentalMenu != null)
            {
                Undo.DestroyObjectImmediate(accidentalMenu);
            }

            GameOverMenu goMenu = gameOverGo.GetComponent<GameOverMenu>();
            if (goMenu == null)
            {
                goMenu = Undo.AddComponent<GameOverMenu>(gameOverGo);
            }
            goMenu.AutoFindReferences();
            EditorUtility.SetDirty(gameOverGo);
        }

        manager.AutoFindReferences();
        EditorUtility.SetDirty(quizGo);
        Selection.activeGameObject = quizGo;

        Debug.Log($"<color=green>[Kahoot Quiz] ¡QuizManager, MainMenu, LevelSelection y GameOver configurados con éxito!</color>");
    }

    [MenuItem("Tools/Kahoot Quiz/Conectar Menús, Niveles y Quiz")]
    public static void ConnectAll()
    {
        SetupQuizManagerInScene();
    }
}
