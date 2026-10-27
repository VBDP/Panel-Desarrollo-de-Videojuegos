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

        if (GUILayout.Button("🔥 Añadir / Configurar Fuego Dracónico en Play BTN", GUILayout.Height(30)))
        {
            SetupDragonFireEffect();
        }

        if (GUILayout.Button("🔥 Configurar Fuego en Botones de Niveles (1, 2 y 3)", GUILayout.Height(30)))
        {
            SetupDragonFireOnLevelButtons();
        }

        if (GUILayout.Button("🔊 Configurar Sonidos de Acierto y Fallo en Quiz", GUILayout.Height(30)))
        {
            SetupQuizAudioFeedback();
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
        if (levelSelGo == null) levelSelGo = FindButtonInSceneOrPanels("LevelSelection");

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

    [MenuItem("Tools/Kahoot Quiz/🔊 Configurar Sonidos de Acierto y Fallo en Quiz")]
    public static void SetupQuizAudioFeedback()
    {
        QuizManager manager = FindFirstObjectByType<QuizManager>(FindObjectsInactive.Include);
        if (manager == null)
        {
            Debug.LogWarning("[Kahoot Quiz] No se encontró el componente QuizManager en la escena.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(manager.gameObject, "Configurar Sonidos de Quiz");

        AudioSource src = manager.GetComponent<AudioSource>();
        if (src == null)
        {
            src = Undo.AddComponent<AudioSource>(manager.gameObject);
            src.playOnAwake = false;
        }

        manager.AutoFindReferences();
        EditorUtility.SetDirty(manager);
        Debug.Log("<color=green>[Kahoot Quiz] ¡Sonidos de acierto (checkpoint) y fallo (error) configurados con éxito en QuizManager!</color>");
    }

    [MenuItem("Tools/Kahoot Quiz/🔥 Añadir Fuego Dracónico a Play BTN")]
    public static void SetupDragonFireEffect()
    {
        GameObject playBtn = FindButtonInSceneOrPanels("Play BTN");
        if (playBtn == null) playBtn = FindButtonInSceneOrPanels("PlayBTN");
        if (playBtn == null) playBtn = FindButtonInSceneOrPanels("Play Button");

        if (playBtn == null)
        {
            Debug.LogWarning("[Kahoot Quiz] No se encontró el objeto 'Play BTN' en la escena.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(playBtn, "Añadir Fuego Dracónico Play BTN");
        DragonFireHoverEffect effect = playBtn.GetComponent<DragonFireHoverEffect>();
        if (effect == null)
        {
            effect = Undo.AddComponent<DragonFireHoverEffect>(playBtn);
        }

        // Asignar el sonido de fuego suave si existe en el proyecto
        SerializedObject so = new SerializedObject(effect);
        SerializedProperty soundProp = so.FindProperty("hoverSound");
        if (soundProp != null && soundProp.objectReferenceValue == null)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/dragon_fire_hover.wav");
            if (clip != null)
            {
                soundProp.objectReferenceValue = clip;
                so.ApplyModifiedProperties();
            }
        }

        EditorUtility.SetDirty(playBtn);
        Selection.activeGameObject = playBtn;
        Debug.Log("<color=orange>[Kahoot Quiz] ¡Efecto de Fuego Dracónico (Rojo & Dorado) configurado con éxito en Play BTN!</color>");
    }

    [MenuItem("Tools/Kahoot Quiz/🔥 Configurar Fuego en Botones de Niveles (1, 2 y 3)")]
    public static void SetupDragonFireOnLevelButtons()
    {
        ConfigureLevelButton("Level1 BTN", "Assets/Art/Audio/fire_level1_soft.wav", 12, 6.5f);
        ConfigureLevelButton("Level2 BTN", "Assets/Art/Audio/fire_level2_medium.wav", 16, 8.0f);
        ConfigureLevelButton("Level3 BTN", "Assets/Art/Audio/fire_level3_heavy.wav", 22, 9.5f);

        Debug.Log("<color=orange>[Kahoot Quiz] ¡Efecto de Fuego Dracónico configurado en los 3 botones de niveles con sus 3 sonidos únicos!</color>");
    }

    [MenuItem("Tools/Kahoot Quiz/🔥 Configurar Fuego en TODOS los Botones (Play + Niveles 1, 2 y 3)")]
    public static void SetupDragonFireOnAllButtons()
    {
        SetupDragonFireEffect();
        SetupDragonFireOnLevelButtons();
    }

    private static void ConfigureLevelButton(string buttonName, string audioPath, int maxEmbers, float flickerSpeed)
    {
        GameObject btnGo = FindButtonInSceneOrPanels(buttonName);
        if (btnGo == null)
        {
            Debug.LogWarning($"[Kahoot Quiz] No se encontró '{buttonName}' en la escena.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(btnGo, "Configurar Fuego Dracónico " + buttonName);

        DragonFireHoverEffect effect = btnGo.GetComponent<DragonFireHoverEffect>();
        if (effect == null)
        {
            effect = Undo.AddComponent<DragonFireHoverEffect>(btnGo);
        }

        SerializedObject so = new SerializedObject(effect);

        SerializedProperty soundProp = so.FindProperty("hoverSound");
        if (soundProp != null)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
            if (clip != null)
            {
                soundProp.objectReferenceValue = clip;
            }
        }

        SerializedProperty embersProp = so.FindProperty("maxEmbers");
        if (embersProp != null)
        {
            embersProp.intValue = maxEmbers;
        }

        SerializedProperty flickerProp = so.FindProperty("flickerSpeed");
        if (flickerProp != null)
        {
            flickerProp.floatValue = flickerSpeed;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(btnGo);
        Debug.Log($"<color=yellow>[Kahoot Quiz] {buttonName} configurado con sonido: {audioPath}</color>");
    }

    private static GameObject FindButtonInSceneOrPanels(string buttonName)
    {
        GameObject go = GameObject.Find(buttonName);
        if (go != null) return go;

        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in allTransforms)
        {
            if (t.hideFlags != HideFlags.None) continue;
            if (EditorUtility.IsPersistent(t.gameObject)) continue;
            if (t.name.Equals(buttonName, System.StringComparison.OrdinalIgnoreCase))
            {
                return t.gameObject;
            }
        }
        return null;
    }
}
