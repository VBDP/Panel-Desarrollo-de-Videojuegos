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
            "Kahoot Quiz Manager: Controla la pregunta, las 4 opciones de respuesta y la iluminación en verde (acierto) o rojo (fallo).",
            MessageType.Info
        );

        if (GUILayout.Button("🔍 Auto-detectar y asignar UI (Pregunta, Botones, Menú y GameOver)", GUILayout.Height(30)))
        {
            Undo.RecordObject(manager, "Auto-asignar referencias QuizManager");
            manager.AutoFindReferences();
            EditorUtility.SetDirty(manager);
            Debug.Log("[QuizManagerEditor] ¡Referencias de UI (incluyendo MainMenu y GameOver) asignadas correctamente!");
        }

        if (GUILayout.Button("📝 Cargar las 12 Preguntas del Concurso", GUILayout.Height(30)))
        {
            Undo.RecordObject(manager, "Cargar preguntas QuizManager");
            manager.LoadGameQuestions();
            EditorUtility.SetDirty(manager);
            Debug.Log("<color=green>[QuizManagerEditor] ¡Las 12 preguntas oficiales han sido cargadas en la lista!</color>");
        }

        EditorGUILayout.Space(10);

        // Dibuja el inspector normal con todas las variables serializadas
        DrawDefaultInspector();

        // Controles útiles mientras el juego está en modo Play
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Controles en Tiempo Real (Play Mode)", EditorStyles.boldLabel);
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

    [MenuItem("Tools/Kahoot Quiz/Configurar QuizManager en Escena")]
    public static void SetupQuizManagerInScene()
    {
        // 1. Buscar el objeto "Quiz"
        GameObject quizGo = GameObject.Find("Quiz");
        if (quizGo == null)
        {
            // Si no existe, buscar en Quiz Canvas o crearlo
            GameObject canvasGo = GameObject.Find("Quiz Canvas");
            if (canvasGo != null)
            {
                quizGo = canvasGo;
            }
            else
            {
                quizGo = new GameObject("QuizManager");
            }
        }

        Undo.RegisterCompleteObjectUndo(quizGo, "Configurar QuizManager");

        QuizManager manager = quizGo.GetComponent<QuizManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<QuizManager>(quizGo);
        }

        manager.AutoFindReferences();
        manager.LoadGameQuestions();

        // 2. Si existe MainMenu en la escena, configurar también el componente MainMenu
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
            Debug.Log("<color=green>[Kahoot Quiz] ¡Componente MainMenu configurado en '" + mainMenuGo.name + "'!</color>");
        }

        // 3. Si existe GameOver en la escena, configurar el componente GameOverMenu
        GameObject gameOverGo = GameObject.Find("GameOver menu");
        if (gameOverGo == null) gameOverGo = GameObject.Find("GameOver");
        if (gameOverGo == null) gameOverGo = GameObject.Find("GameOverMenu");
        if (gameOverGo == null) gameOverGo = GameObject.Find("GameOver Panel");

        if (gameOverGo != null)
        {
            Undo.RegisterCompleteObjectUndo(gameOverGo, "Configurar GameOverMenu");

            // Si tenía el script MainMenu por duplicación accidental, lo removemos limpiamente
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
            Debug.Log("<color=green>[Kahoot Quiz] ¡Componente GameOverMenu configurado en '" + gameOverGo.name + "'!</color>");
        }

        EditorUtility.SetDirty(quizGo);
        Selection.activeGameObject = quizGo;

        Debug.Log($"<color=green>[Kahoot Quiz] ¡QuizManager y Paneles configurados con éxito!</color>");
    }

    [MenuItem("Tools/Kahoot Quiz/Conectar MainMenu y Quiz Panel")]
    public static void ConnectMenuAndQuiz()
    {
        SetupQuizManagerInScene();
    }
}
