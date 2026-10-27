using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Question
{
    [TextArea(2, 4)]
    [Tooltip("El texto o enunciado de la pregunta")]
    public string questionText = "¿Cuál es el planeta más grande del sistema solar?";

    [Tooltip("Las 4 respuestas para los botones (0 = Respuesta 1, 1 = Respuesta 2, 2 = Respuesta 3, 3 = Respuesta 4)")]
    public string[] answers = new string[4] { "Tierra", "Júpiter", "Marte", "Saturno" };

    [Range(0, 3)]
    [Tooltip("Índice de la respuesta correcta (0 a 3)")]
    public int correctAnswerIndex = 1;

    [TextArea(2, 4)]
    [Tooltip("Explicación breve de la respuesta")]
    public string explanation = "";

    public Question() { }

    public Question(string question, string[] answers, int correctIndex)
    {
        this.questionText = question;
        this.answers = answers;
        this.correctAnswerIndex = correctIndex;
        this.explanation = "";
    }

    public Question(string question, string[] answers, int correctIndex, string explanation)
    {
        this.questionText = question;
        this.answers = answers;
        this.correctAnswerIndex = correctIndex;
        this.explanation = explanation;
    }
}

public class QuizManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("Componente TextMeshProUGUI donde se muestra el texto de la pregunta")]
    [SerializeField] private TextMeshProUGUI questionText;

    [Tooltip("Texto opcional donde se muestra la explicación tras responder (Explanation Text)")]
    [SerializeField] private TextMeshProUGUI explanationText;

    [Tooltip("Los 4 botones de respuesta (Respuesta 1 a 4)")]
    [SerializeField] private Button[] answerButtons = new Button[4];

    [Header("Marcador de Aciertos")]
    [Tooltip("Componente TextMeshProUGUI que muestra las respuestas correctas (objeto 'Correct Answers')")]
    [SerializeField] private TextMeshProUGUI correctAnswersText;

    [Tooltip("Texto prefijo para el marcador")]
    [SerializeField] private string correctAnswersPrefix = "Respuestas Correctas: ";

    [Header("Paneles y Navegación")]
    [Tooltip("Panel del menú principal (MainMenu)")]
    [SerializeField] private GameObject mainMenuPanel;

    [Tooltip("Panel de selección de nivel (LevelSelection)")]
    [SerializeField] private GameObject levelSelectionPanel;

    [Tooltip("Panel del juego (Quiz Panel)")]
    [SerializeField] private GameObject quizPanel;

    [Tooltip("Botón para iniciar el juego desde el menú (Play BTN)")]
    [SerializeField] private Button playButton;

    [Tooltip("Botón dentro de Quiz Panel para volver a la selección de niveles (Return BTN)")]
    [SerializeField] private Button returnToLevelSelectionButton;

    [Header("GameOver Panel")]
    [Tooltip("Panel de fin de partida (GameOver menu)")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Botón para volver al menú principal desde GameOver (return to main menu)")]
    [SerializeField] private Button returnToMainMenuButton;

    [Tooltip("Texto donde se muestra la puntuación final (last score text)")]
    [SerializeField] private TextMeshProUGUI lastScoreText;

    [Tooltip("Prefijo para la puntuación en GameOver")]
    [SerializeField] private string lastScorePrefix = "Puntuación: ";

    [Header("Niveles de Preguntas (GameObjects Vacíos con QuizLevel)")]
    [Tooltip("Componente QuizLevel asignado al Nivel 1 (Bajo)")]
    [SerializeField] private QuizLevel level1;

    [Tooltip("Componente QuizLevel asignado al Nivel 2 (Medio)")]
    [SerializeField] private QuizLevel level2;

    [Tooltip("Componente QuizLevel asignado al Nivel 3 (Alto)")]
    [SerializeField] private QuizLevel level3;

    [Header("Efectos de Sonido (Audio Feedback)")]
    [Tooltip("Sonido de acierto o checkpoint al responder correctamente")]
    [SerializeField] private AudioClip correctAnswerSound;

    [Tooltip("Sonido de error o fallo al responder incorrectamente")]
    [SerializeField] private AudioClip wrongAnswerSound;

    [Tooltip("AudioSource para reproducir los sonidos de acierto y fallo")]
    [SerializeField] private AudioSource audioSource;

    [Header("Colores de Iluminación (Feedback)")]
    [Tooltip("Color que iluminará el botón cuando la respuesta es CORRECTA")]
    [SerializeField] private Color correctColor = new Color(0.18f, 0.80f, 0.44f, 1f); // Verde

    [Tooltip("Color que iluminará el botón cuando la respuesta es INCORRECTA")]
    [SerializeField] private Color wrongColor = new Color(0.92f, 0.26f, 0.21f, 1f); // Rojo

    [Tooltip("Color normal/inicial del botón antes de ser pulsado")]
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0f); // Transparente como en la escena

    [Header("Ajuste de Texto al Iluminar (Opcional)")]
    [Tooltip("Si está activo, cambia el color del texto a blanco al iluminar para máxima legibilidad")]
    [SerializeField] private bool changeTextColorOnFeedback = true;
    [SerializeField] private Color feedbackTextColor = Color.white;

    [Header("Configuración y Flujo del Quiz")]
    [Tooltip("Lista de preguntas que se jugarán en la ronda actual")]
    [SerializeField] private List<Question> questions = new List<Question>();

    [Tooltip("Si está activo, pasa automáticamente a la siguiente pregunta tras unos segundos")]
    [SerializeField] private bool autoAdvance = true;

    [Tooltip("Segundos de espera antes de mostrar la siguiente pregunta")]
    [SerializeField] private float delayBeforeNextQuestion = 2.5f;

    [Tooltip("Si es true, resalta también en verde la respuesta correcta cuando el usuario falla")]
    [SerializeField] private bool showCorrectAnswerOnMistake = true;

    // Estado interno
    private int currentQuestionIndex = 0;
    private int correctAnswersCount = 0;
    private bool hasAnswered = false;
    private Coroutine nextQuestionCoroutine;

    public int CorrectAnswersCount => correctAnswersCount;

    private struct ButtonVisualState
    {
        public ColorBlock originalColors;
        public Color originalImageColor;
        public Color originalTextColor;
        public TextMeshProUGUI textComponent;
    }
    private ButtonVisualState[] originalButtonStates;

    private void Awake()
    {
        AutoFindReferences();
        InitializeDefaultQuestions();
        SetupButtons();

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartGame);
        }

        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.onClick.RemoveAllListeners();
            returnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (returnToLevelSelectionButton != null)
        {
            returnToLevelSelectionButton.onClick.RemoveListener(ReturnToLevelSelection);
            returnToLevelSelectionButton.onClick.AddListener(ReturnToLevelSelection);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    private void Start()
    {
        if (returnToLevelSelectionButton != null)
        {
            returnToLevelSelectionButton.onClick.RemoveListener(ReturnToLevelSelection);
            returnToLevelSelectionButton.onClick.AddListener(ReturnToLevelSelection);
        }

        correctAnswersCount = 0;
        UpdateScoreText();

        // Asegurarse de que GameOver y LevelSelection empiezan desactivados
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(false);
        }

        // Si hay un MainMenu activo, nos aseguramos de que el Quiz Panel esté oculto al empezar
        if (mainMenuPanel != null && mainMenuPanel.activeInHierarchy)
        {
            if (quizPanel != null)
            {
                quizPanel.SetActive(false);
            }
            return;
        }

        if (questions.Count > 0)
        {
            DisplayQuestion(0);
        }
    }

    private void Reset()
    {
        AutoFindReferences();
        InitializeDefaultQuestions();
    }

    [ContextMenu("Auto-asignar referencias")]
    public void AutoFindReferences()
    {
        // 1. Localizar Question Text (TextMeshProUGUI)
        if (questionText == null)
        {
            Transform qChild = transform.Find("Question Text");
            if (qChild != null)
            {
                questionText = qChild.GetComponent<TextMeshProUGUI>();
            }

            if (questionText == null)
            {
                GameObject qGo = GameObject.Find("Question Text");
                if (qGo != null)
                {
                    questionText = qGo.GetComponent<TextMeshProUGUI>();
                }
            }

            if (questionText == null)
            {
                TextMeshProUGUI[] tmps = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var tmp in tmps)
                {
                    if (tmp.GetComponentInParent<Button>() == null && tmp.gameObject.name.Contains("Question"))
                    {
                        questionText = tmp;
                        break;
                    }
                }
            }
        }

        // Localizar Explanation Text (opcional)
        if (explanationText == null)
        {
            Transform expChild = transform.Find("Explanation Text");
            if (expChild != null)
            {
                explanationText = expChild.GetComponent<TextMeshProUGUI>();
            }

            if (explanationText == null)
            {
                GameObject expGo = GameObject.Find("Explanation Text");
                if (expGo == null) expGo = GameObject.Find("Explanation");
                if (expGo == null) expGo = GameObject.Find("Explicacion Text");
                if (expGo == null) expGo = GameObject.Find("Explicacion");
                if (expGo != null)
                {
                    explanationText = expGo.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        // 2. Localizar los 4 botones de respuesta
        if (answerButtons == null || answerButtons.Length == 0 || answerButtons[0] == null)
        {
            Transform buttonsParent = transform.Find("Buttons");
            if (buttonsParent == null)
            {
                GameObject bGo = GameObject.Find("Buttons");
                if (bGo != null)
                {
                    buttonsParent = bGo.transform;
                }
            }

            if (buttonsParent != null)
            {
                List<Button> orderedButtons = new List<Button>();
                for (int i = 1; i <= 4; i++)
                {
                    Transform r = buttonsParent.Find($"Respuesta {i}");
                    if (r != null && r.TryGetComponent<Button>(out Button b))
                    {
                        orderedButtons.Add(b);
                    }
                }

                if (orderedButtons.Count == 4)
                {
                    answerButtons = orderedButtons.ToArray();
                }
                else
                {
                    List<Button> candidateButtons = new List<Button>();
                    foreach (var b in buttonsParent.GetComponentsInChildren<Button>(true))
                    {
                        string bName = b.gameObject.name.ToLower();
                        if (!bName.Contains("return") && !bName.Contains("back") && !bName.Contains("menu"))
                        {
                            candidateButtons.Add(b);
                        }
                    }
                    answerButtons = candidateButtons.ToArray();
                }
            }
            else
            {
                answerButtons = GetComponentsInChildren<Button>(true);
            }
        }

        // 3. Localizar el texto del contador de aciertos (Correct Answers)
        if (correctAnswersText == null)
        {
            Transform caChild = transform.Find("Correct Answers");
            if (caChild != null)
            {
                correctAnswersText = caChild.GetComponent<TextMeshProUGUI>();
            }

            if (correctAnswersText == null)
            {
                GameObject caGo = GameObject.Find("Correct Answers");
                if (caGo != null)
                {
                    correctAnswersText = caGo.GetComponent<TextMeshProUGUI>();
                }
            }

            if (correctAnswersText == null)
            {
                GameObject caGo2 = GameObject.Find("Respuestas Correctas");
                if (caGo2 != null)
                {
                    correctAnswersText = caGo2.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        // 4. Localizar paneles (MainMenu, LevelSelection, Quiz Panel) y botón Play BTN
        if (mainMenuPanel == null)
        {
            mainMenuPanel = GameObject.Find("MainMenu");
            if (mainMenuPanel == null) mainMenuPanel = GameObject.Find("Main Menu");
        }

        if (levelSelectionPanel == null)
        {
            levelSelectionPanel = GameObject.Find("LevelSelection");
            if (levelSelectionPanel == null) levelSelectionPanel = GameObject.Find("Level Selection");
        }

        if (quizPanel == null)
        {
            quizPanel = GameObject.Find("Quiz Panel");
            if (quizPanel == null) quizPanel = GameObject.Find("QuizPanel");
            if (quizPanel == null && transform.parent != null && transform.parent.name.Contains("Panel"))
            {
                quizPanel = transform.parent.gameObject;
            }
        }

        if (playButton == null)
        {
            GameObject playBtnGo = GameObject.Find("Play BTN");
            if (playBtnGo == null) playBtnGo = GameObject.Find("PlayBTN");
            if (playBtnGo == null) playBtnGo = GameObject.Find("Play Button");
            if (playBtnGo != null)
            {
                playButton = playBtnGo.GetComponent<Button>();
            }
            else if (mainMenuPanel != null)
            {
                playButton = mainMenuPanel.GetComponentInChildren<Button>(true);
            }
        }

        // Localizar Return BTN en Quiz Panel (para volver a LevelSelection)
        if (returnToLevelSelectionButton == null)
        {
            if (quizPanel != null)
            {
                Button[] qBtns = quizPanel.GetComponentsInChildren<Button>(true);
                foreach (var b in qBtns)
                {
                    string bName = b.gameObject.name.ToLower();
                    if (bName.Contains("return") || bName.Contains("back") || bName.Contains("volver"))
                    {
                        returnToLevelSelectionButton = b;
                        break;
                    }
                }
            }

            if (returnToLevelSelectionButton == null)
            {
                GameObject retBtnGo = GameObject.Find("Return BTN");
                if (retBtnGo == null) retBtnGo = GameObject.Find("ReturnBTN");
                if (retBtnGo == null) retBtnGo = GameObject.Find("Return Button");
                if (retBtnGo != null)
                {
                    returnToLevelSelectionButton = retBtnGo.GetComponent<Button>();
                }
            }
        }

        // 5. Localizar GameOver menu, return to main menu button y last score text
        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.Find("GameOver menu");
            if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOver");
            if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverMenu");
            if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOver Panel");
        }

        if (gameOverPanel != null)
        {
            if (returnToMainMenuButton == null)
            {
                Button[] btns = gameOverPanel.GetComponentsInChildren<Button>(true);
                foreach (var b in btns)
                {
                    string bName = b.gameObject.name.ToLower();
                    if (bName.Contains("return") || bName.Contains("main") || bName.Contains("menu"))
                    {
                        returnToMainMenuButton = b;
                        break;
                    }
                }
                if (returnToMainMenuButton == null && btns.Length > 0)
                {
                    returnToMainMenuButton = btns[0];
                }
            }

            if (lastScoreText == null)
            {
                TextMeshProUGUI[] tmps = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    if (t.GetComponentInParent<Button>() != null) continue;
                    string tName = t.gameObject.name.ToLower();
                    if (tName.Contains("score") || tName.Contains("puntuacion") || tName.Contains("last"))
                    {
                        lastScoreText = t;
                        break;
                    }
                }
                if (lastScoreText == null)
                {
                    foreach (var t in tmps)
                    {
                        if (t.GetComponentInParent<Button>() == null)
                        {
                            lastScoreText = t;
                            break;
                        }
                    }
                }
            }
        }

        // 6. Localizar los GameObjects de Niveles (con componente QuizLevel)
        QuizLevel[] allLevels = FindObjectsByType<QuizLevel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var lvl in allLevels)
        {
            string lName = lvl.gameObject.name.ToLower();
            if (lName.Contains("1") && level1 == null) level1 = lvl;
            else if (lName.Contains("2") && level2 == null) level2 = lvl;
            else if (lName.Contains("3") && level3 == null) level3 = lvl;
        }

        // 7. Configurar AudioSource y sonidos por defecto
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

#if UNITY_EDITOR
        if (correctAnswerSound == null)
        {
            correctAnswerSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/quiz_correct_checkpoint.wav");
        }
        if (wrongAnswerSound == null)
        {
            wrongAnswerSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/quiz_wrong_error.wav");
        }
#endif
    }

    private void InitializeDefaultQuestions()
    {
        if (questions == null || questions.Count == 0)
        {
            LoadDefaultLevelFallback(1);
        }
    }

    private void LoadDefaultLevelFallback(int levelIndex)
    {
        if (levelIndex == 1)
        {
            questions = new List<Question>
            {
                new Question("1. ¿Qué es Unity?", new string[4] { "A) Un motor de videojuegos", "B) Un lenguaje de modelado 3D", "C) Una tarjeta gráfica", "D) Un programa para editar vídeos" }, 0),
                new Question("2. ¿Para qué sirve Blender?", new string[4] { "A) Para programar redes", "B) Para crear modelos 3D y animaciones", "C) Para publicar videojuegos automáticamente", "D) Para gestionar servidores" }, 1),
                new Question("3. ¿Qué es un GameObject en Unity?", new string[4] { "A) Un objeto de la escena", "B) Un tipo de ordenador", "C) Una tienda de videojuegos", "D) Un archivo de audio exclusivamente" }, 0),
                new Question("4. ¿Qué es una escena en Unity?", new string[4] { "A) Una lista de contraseñas", "B) Un nivel o espacio donde organizamos objetos", "C) Un modelo 3D obligatorio", "D) Un tipo de textura" }, 1),
                new Question("5. ¿Qué es un material en Unity?", new string[4] { "A) Un objeto que guarda las partidas", "B) Un elemento que define cómo se ve una superficie", "C) Un nivel completo", "D) Un archivo de texto" }, 1),
                new Question("6. ¿Para qué sirve una textura?", new string[4] { "A) Para añadir detalles visuales a una superficie", "B) Para aumentar automáticamente los FPS", "C) Para crear código C#", "D) Para guardar una escena" }, 0),
                new Question("7. ¿Qué lenguaje se utiliza habitualmente para programar en Unity?", new string[4] { "A) HTML", "B) C#", "C) SQL", "D) CSS" }, 1),
                new Question("8. ¿Qué hace la cámara de un videojuego?", new string[4] { "A) Dibuja los modelos 3D", "B) Muestra la vista del juego", "C) Compila el código", "D) Crea las texturas" }, 1),
                new Question("9. ¿Qué significa FPS en videojuegos?", new string[4] { "A) Frames Per Second (fotogramas por segundo)", "B) Fast Player System", "C) Final Project Scene", "D) First Program Started" }, 0),
                new Question("10. ¿Qué conviene hacer mientras desarrollas un juego?", new string[4] { "A) No probarlo hasta terminarlo", "B) Probarlo frecuentemente", "C) Borrar todas las versiones anteriores", "D) Crear todos los gráficos antes de tener una idea" }, 1),
                new Question("11. ¿Qué es un bug?", new string[4] { "A) Un error o fallo del programa", "B) Un tipo de personaje", "C) Un motor gráfico", "D) Un efecto de sonido" }, 0),
                new Question("12. ¿Cuál es la mejor forma de empezar a crear tu primer videojuego?", new string[4] { "A) Crear un MMO enorme", "B) Empezar por un proyecto pequeño", "C) Comprar el ordenador más caro", "D) Programar sin probar nada" }, 1)
            };
        }
        else if (levelIndex == 2)
        {
            questions = new List<Question>
            {
                new Question("1. ¿Qué es un Prefab en Unity?", new string[4] { "A) Una copia de seguridad del proyecto", "B) Una plantilla reutilizable de un GameObject", "C) Una textura para modelos 3D", "D) Un tipo de escena" }, 1),
                new Question("2. ¿Para qué sirve el componente Rigidbody?", new string[4] { "A) Para aplicar física a un objeto", "B) Para cambiar el color de un objeto", "C) Para controlar la cámara", "D) Para importar modelos 3D" }, 0),
                new Question("3. ¿Qué diferencia hay entre Update() y FixedUpdate()?", new string[4] { "A) No existe ninguna diferencia", "B) Update se usa para físicas y FixedUpdate para interfaz", "C) Update se ejecuta por fotograma y FixedUpdate sigue el paso fijo de física", "D) FixedUpdate solo funciona en 2D" }, 2),
                new Question("4. ¿Qué es un Collider en Unity?", new string[4] { "A) Un sistema de iluminación", "B) Un componente que define una forma para detectar colisiones", "C) Un formato de audio", "D) Una herramienta para crear animaciones" }, 1),
                new Question("5. ¿Para qué sirve el UV Mapping en Blender?", new string[4] { "A) Para reducir automáticamente los polígonos", "B) Para asignar una textura 2D a la superficie de un modelo 3D", "C) Para crear luces", "D) Para exportar scripts" }, 1),
                new Question("6. ¿Qué ocurre al aumentar demasiado el número de polígonos de un modelo?", new string[4] { "A) Siempre mejora el rendimiento", "B) El modelo deja de necesitar texturas", "C) Puede aumentar el coste de renderizado y afectar al rendimiento", "D) Unity los elimina siempre al importar" }, 2),
                new Question("7. ¿Qué es un shader?", new string[4] { "A) Un programa que determina cómo se representa visualmente una superficie", "B) Un archivo de guardado", "C) Una herramienta de audio", "D) Una copia de una escena" }, 0),
                new Question("8. ¿Para qué se utiliza el Bake de iluminación?", new string[4] { "A) Para guardar el proyecto en la nube", "B) Para precalcular parte de la iluminación y reutilizar sus resultados", "C) Para comprimir scripts C#", "D) Para animar personajes automáticamente" }, 1),
                new Question("9. ¿Qué es un Array en programación?", new string[4] { "A) Una colección de elementos del mismo tipo, con tamaño definido al crearse", "B) Una función que destruye objetos", "C) Un tipo de material", "D) Una cámara secundaria" }, 0),
                new Question("10. ¿Qué función tiene Git durante el desarrollo de un videojuego?", new string[4] { "A) Aumentar los FPS", "B) Crear modelos 3D", "C) Controlar versiones y registrar cambios en el proyecto", "D) Generar música automáticamente" }, 2),
                new Question("11. ¿Qué significa que una variable sea privada (private) en C#?", new string[4] { "A) Que solo puede utilizarse desde cualquier clase", "B) Que no puede cambiar nunca", "C) Que su acceso directo está limitado principalmente a la clase que la declara", "D) Que Unity la elimina al iniciar" }, 2),
                new Question("12. ¿Qué es un sistema LOD en videojuegos 3D?", new string[4] { "A) Un sistema que cambia el nivel de detalle según la distancia o condiciones de visualización", "B) Un formato de sonido", "C) Una técnica para crear interfaces", "D) Un sistema de guardado de partidas" }, 0)
            };
        }
        else if (levelIndex == 3)
        {
            questions = new List<Question>
            {
                new Question(
                    "1. Si eliminas el componente Mesh Renderer de un cubo 3D, pero mantienes su Mesh Filter, ¿qué ocurre normalmente?",
                    new string[4] {
                        "A) El cubo deja de existir en la escena.",
                        "B) El cubo sigue teniendo su geometría, pero deja de renderizarse normalmente.",
                        "C) Unity elimina automáticamente su Transform.",
                        "D) El cubo se convierte automáticamente en un objeto 2D."
                    },
                    1,
                    "El Mesh Filter contiene la referencia a la malla; el Mesh Renderer se encarga de representarla visualmente."
                ),
                new Question(
                    "2. Un objeto hijo tiene posición local (0, 0, 2) y su padre se desplaza 5 en X. Sin rotación ni escala, ¿cuál es su posición mundial?",
                    new string[4] {
                        "A) El hijo mantiene exactamente la misma posición mundial.",
                        "B) El hijo pasa a tener una posición mundial (5, 0, 2).",
                        "C) El hijo pasa a (0, 0, 7).",
                        "D) Unity elimina su posición local."
                    },
                    1,
                    "La posición local se define respecto al padre. Al desplazarse el padre, también cambia la posición mundial del hijo."
                ),
                new Question(
                    "3. Instancias un Prefab y cambias su color en Inspector. Luego modificas el Prefab original. ¿Qué afirmación es correcta?",
                    new string[4] {
                        "A) Las modificaciones siempre se transfieren automáticamente al Prefab original.",
                        "B) La instancia hereda cambios, pero las modificaciones locales se mantienen como overrides.",
                        "C) Modificar un Prefab nunca afecta a sus instancias.",
                        "D) El Prefab original se destruye al crear una instancia."
                    },
                    1,
                    "Unity gestiona las modificaciones de instancias mediante overrides, conservando los cambios locales específicos."
                ),
                new Question(
                    "4. Un componente está habilitado, pero su GameObject inicia desactivado. Al activarse, ¿cuál es el orden habitual de ejecución?",
                    new string[4] {
                        "A) Start() -> Awake() -> OnEnable()",
                        "B) OnEnable() -> Start() -> Awake()",
                        "C) Awake() -> OnEnable() -> Start()",
                        "D) Start() -> OnEnable() -> Awake()"
                    },
                    2,
                    "En un objeto inactivo al cargar, Awake() se ejecuta primero al activarse, luego OnEnable() y antes del primer Update(), Start()."
                ),
                new Question(
                    "5. Dos cubos tienen Box Collider pero ninguno tiene Rigidbody. ¿Qué describe mejor su comportamiento?",
                    new string[4] {
                        "A) Siempre rebotan al tocarse.",
                        "B) Unity los trata como colliders estáticos; no se simulan dinámicamente con gravedad.",
                        "C) Se convierten automáticamente en triggers.",
                        "D) No pueden participar en ningún tipo de detección de colisiones."
                    },
                    1,
                    "Los colliders sin Rigidbody suelen considerarse geometría estática; para simular movimiento dinámico se requiere Rigidbody."
                ),
                new Question(
                    "6. Un objeto tiene Box Collider con Is Trigger activo. Otro objeto atraviesa su volumen. ¿Qué comportamiento se espera?",
                    new string[4] {
                        "A) El objeto queda bloqueado como ante una pared sólida.",
                        "B) Se produce un evento como OnTriggerEnter, sin actuar como superficie sólida que impida el paso.",
                        "C) Unity destruye ambos objetos automáticamente.",
                        "D) El trigger solo funciona si el objeto es visible por la cámara."
                    },
                    1,
                    "Los triggers detectan entradas y salidas de volúmenes espaciales sin bloquear físicamente el movimiento."
                ),
                new Question(
                    "7. Quieres aplicar fuerzas mediante Rigidbody.AddForce() a un objeto físico. ¿Dónde es más apropiado ejecutar esa lógica?",
                    new string[4] {
                        "A) En FixedUpdate(), teniendo en cuenta el modo de fuerza utilizado.",
                        "B) Exclusivamente en OnGUI().",
                        "C) En Awake(), una sola vez, para controlar todo el movimiento futuro.",
                        "D) En LateUpdate(), porque siempre se ejecuta a intervalos fijos."
                    },
                    0,
                    "FixedUpdate() se ejecuta con intervalos fijos y constantes, siendo el lugar estándar para aplicar fuerzas físicas."
                ),
                new Question(
                    "8. Para mantener un tamaño visual parejo de la interfaz en distintas resoluciones, ¿qué ajuste suele usarse en el Canvas?",
                    new string[4] {
                        "A) Canvas Scaler en Constant Pixel Size sin ningún otro ajuste.",
                        "B) Canvas Scaler en Scale With Screen Size, con resolución de referencia y ajuste apropiado.",
                        "C) Eliminar el componente Rect Transform de todos los elementos.",
                        "D) Convertir el Canvas en un objeto 3D con Rigidbody."
                    },
                    1,
                    "Scale With Screen Size adapta la escala de la UI respecto a una resolución de referencia dada (ej. 1920x1080)."
                ),
                new Question(
                    "9. Un objeto está en la escena, pero no aparece en la vista de juego (Game View). ¿Cuál de estas causas es posible?",
                    new string[4] {
                        "A) Su capa está excluida de Culling Mask de la cámara.",
                        "B) El objeto tiene un nombre que contiene números.",
                        "C) Su Transform tiene una posición local distinta de cero.",
                        "D) El objeto tiene un Box Collider."
                    },
                    0,
                    "La cámara solo renderiza los objetos cuyas capas (Layers) estén marcadas y habilitadas en su Culling Mask."
                ),
                new Question(
                    "10. Varios objetos usan el mismo material. ¿Qué riesgo hay al cambiar el color de un material compartido directamente en el asset?",
                    new string[4] {
                        "A) El color solo cambia en el objeto seleccionado y nunca en los demás.",
                        "B) La modificación afecta a todos los objetos que usan el material y altera la configuración guardada.",
                        "C) Unity convierte automáticamente el material en una textura.",
                        "D) El material deja de funcionar con cualquier luz."
                    },
                    1,
                    "Un material compartido afecta a todos los renderers que lo referencian. Para cambios individuales se usa una instancia o MaterialPropertyBlock."
                ),
                new Question(
                    "11. Un objeto mantiene una referencia directa a un GameObject de otra escena que es descargada. ¿Qué problema ocurre?",
                    new string[4] {
                        "A) La referencia se convierte automáticamente en un Prefab permanente.",
                        "B) El objeto referenciado deja de estar disponible y la referencia queda nula o destruida.",
                        "C) Unity carga siempre de nuevo la escena sin solicitarlo.",
                        "D) La escena descargada sigue consumiendo exactamente los mismos recursos."
                    },
                    1,
                    "Descargar una escena destruye sus objetos en memoria; las referencias directas hacia ellos quedan nulas (MissingReference)."
                ),
                new Question(
                    "12. Un muro opaco tapa objetos que están dentro del campo visual de la cámara. ¿Qué técnica evita renderizarlos?",
                    new string[4] {
                        "A) Occlusion Culling, si la escena y configuración permiten detectar esa oclusión.",
                        "B) Frustum Culling exclusivamente, porque descarta objetos tapados por paredes.",
                        "C) Texture Filtering, porque elimina objetos invisibles.",
                        "D) VSync, porque impide renderizar geometría oculta."
                    },
                    0,
                    "Frustum Culling descarta fuera de cámara; Occlusion Culling descarta objetos dentro del campo de visión que quedan ocultos tras objetos opacos."
                )
            };
        }
    }

    private void SetupButtons()
    {
        if (answerButtons == null || answerButtons.Length == 0) return;

        originalButtonStates = new ButtonVisualState[answerButtons.Length];

        for (int i = 0; i < answerButtons.Length; i++)
        {
            Button btn = answerButtons[i];
            if (btn == null) continue;

            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();

            originalButtonStates[i] = new ButtonVisualState
            {
                originalColors = btn.colors,
                originalImageColor = btn.image != null ? btn.image.color : defaultColor,
                originalTextColor = btnText != null ? btnText.color : Color.white,
                textComponent = btnText
            };

            int answerIndex = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnAnswerSelected(answerIndex));
        }
    }

    /// <summary>
    /// Muestra en pantalla la pregunta con el índice indicado y restablece los 4 botones.
    /// </summary>
    public void DisplayQuestion(int index)
    {
        if (questions == null || questions.Count == 0)
        {
            Debug.LogWarning("[QuizManager] No hay preguntas configuradas.");
            return;
        }

        if (index < 0 || index >= questions.Count)
        {
            EndQuiz();
            return;
        }

        currentQuestionIndex = index;
        hasAnswered = false;

        Question currentQ = questions[currentQuestionIndex];

        // 1. Mostrar texto de la pregunta
        if (questionText != null)
        {
            questionText.text = currentQ.questionText;
        }

        // Ocultar texto de explicacin mientras no se haya respondido
        if (explanationText != null)
        {
            explanationText.text = "";
            explanationText.gameObject.SetActive(false);
        }

        // 2. Configurar y restaurar cada botón
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;

            ResetButton(i);

            TextMeshProUGUI btnText = (originalButtonStates != null && i < originalButtonStates.Length)
                ? originalButtonStates[i].textComponent
                : answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
            {
                if (currentQ.answers != null && i < currentQ.answers.Length)
                {
                    btnText.text = currentQ.answers[i];
                }
                else
                {
                    btnText.text = "";
                }
            }
        }
    }

    /// <summary>
    /// Método invocado al hacer click en cualquiera de los 4 botones de respuesta.
    /// </summary>
    public void OnAnswerSelected(int selectedIndex)
    {
        if (hasAnswered) return;
        hasAnswered = true;

        if (currentQuestionIndex < 0 || currentQuestionIndex >= questions.Count) return;
        Question currentQ = questions[currentQuestionIndex];

        bool isCorrect = (selectedIndex == currentQ.correctAnswerIndex);

        if (isCorrect)
        {
            correctAnswersCount++;
            UpdateScoreText();

            IlluminateButton(selectedIndex, correctColor);
            PlaySound(correctAnswerSound);
            Debug.Log($"<color=green>[QuizManager] ¡Respuesta CORRECTA! Opción {selectedIndex + 1}. Total aciertos: {correctAnswersCount}</color>");
        }
        else
        {
            IlluminateButton(selectedIndex, wrongColor);
            PlaySound(wrongAnswerSound);
            Debug.Log($"<color=red>[QuizManager] Respuesta INCORRECTA. Opción pulsada: {selectedIndex + 1}. La correcta era: {currentQ.correctAnswerIndex + 1}</color>");

            if (showCorrectAnswerOnMistake && currentQ.correctAnswerIndex >= 0 && currentQ.correctAnswerIndex < answerButtons.Length)
            {
                IlluminateButton(currentQ.correctAnswerIndex, correctColor);
            }
        }

        // Mostrar explicación si existe
        if (!string.IsNullOrEmpty(currentQ.explanation))
        {
            if (explanationText != null)
            {
                explanationText.text = currentQ.explanation;
                explanationText.gameObject.SetActive(true);
            }
            Debug.Log($"<color=white>[QuizManager] Explicación: {currentQ.explanation}</color>");
        }

        // Desactivar interacción en todos los botones para evitar pulsaciones múltiples
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].interactable = false;
            }
        }

        if (autoAdvance)
        {
            if (nextQuestionCoroutine != null) StopCoroutine(nextQuestionCoroutine);
            nextQuestionCoroutine = StartCoroutine(NextQuestionRoutine());
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        audioSource.PlayOneShot(clip);
    }

    private void IlluminateButton(int buttonIndex, Color feedbackColor)
    {
        if (buttonIndex < 0 || buttonIndex >= answerButtons.Length) return;
        Button btn = answerButtons[buttonIndex];
        if (btn == null) return;

        if (btn.image != null)
        {
            btn.image.color = feedbackColor;
        }

        ColorBlock cb = btn.colors;
        cb.normalColor = feedbackColor;
        cb.highlightedColor = feedbackColor;
        cb.pressedColor = feedbackColor;
        cb.selectedColor = feedbackColor;
        cb.disabledColor = feedbackColor;
        btn.colors = cb;

        if (changeTextColorOnFeedback && originalButtonStates != null && buttonIndex < originalButtonStates.Length)
        {
            TextMeshProUGUI txt = originalButtonStates[buttonIndex].textComponent;
            if (txt != null)
            {
                txt.color = feedbackTextColor;
            }
        }
    }

    private void ResetButton(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= answerButtons.Length) return;
        Button btn = answerButtons[buttonIndex];
        if (btn == null) return;

        if (originalButtonStates != null && buttonIndex < originalButtonStates.Length)
        {
            if (btn.image != null)
            {
                btn.image.color = defaultColor;
            }

            btn.colors = originalButtonStates[buttonIndex].originalColors;

            if (originalButtonStates[buttonIndex].textComponent != null)
            {
                originalButtonStates[buttonIndex].textComponent.color = originalButtonStates[buttonIndex].originalTextColor;
            }
        }
        else
        {
            if (btn.image != null) btn.image.color = defaultColor;
        }

        btn.gameObject.SetActive(true);
        btn.interactable = true;
    }

    public void UpdateScoreText()
    {
        if (correctAnswersText != null)
        {
            correctAnswersText.text = $"{correctAnswersPrefix}{correctAnswersCount}";
        }
    }

    private IEnumerator NextQuestionRoutine()
    {
        yield return new WaitForSeconds(delayBeforeNextQuestion);

        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Count)
        {
            DisplayQuestion(currentQuestionIndex);
        }
        else
        {
            EndQuiz();
        }
    }

    public void NextQuestion()
    {
        if (nextQuestionCoroutine != null) StopCoroutine(nextQuestionCoroutine);
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            DisplayQuestion(currentQuestionIndex);
        }
        else
        {
            EndQuiz();
        }
    }

    public void RestartQuiz()
    {
        if (nextQuestionCoroutine != null) StopCoroutine(nextQuestionCoroutine);
        correctAnswersCount = 0;
        UpdateScoreText();
        currentQuestionIndex = 0;
        DisplayQuestion(0);
    }

    /// <summary>
    /// Al hacer clic en Play en el MainMenu, abre LevelSelection si existe, o inicia el juego directo.
    /// </summary>
    public void StartGame()
    {
        if (levelSelectionPanel != null)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (quizPanel != null) quizPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            levelSelectionPanel.SetActive(true);
        }
        else
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (quizPanel != null) quizPanel.SetActive(true);
            RestartQuiz();
        }
    }

    /// <summary>
    /// Carga las preguntas del Nivel especificado (1, 2 o 3) e inicia la partida.
    /// </summary>
    public void PlayLevel(int levelNumber)
    {
        if (levelSelectionPanel != null) levelSelectionPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        QuizLevel targetLevel = GetLevel(levelNumber);
        if (targetLevel != null && targetLevel.questions != null && targetLevel.questions.Count > 0)
        {
            this.questions = new List<Question>(targetLevel.questions);
            Debug.Log($"<color=yellow>[QuizManager] Nivel {levelNumber} cargado con {questions.Count} preguntas.</color>");
        }
        else
        {
            Debug.LogWarning($"[QuizManager] No se encontró el componente QuizLevel para el Nivel {levelNumber}. Usando preguntas por defecto.");
            LoadDefaultLevelFallback(levelNumber);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }

        RestartQuiz();
    }

    /// <summary>
    /// Obtiene el componente QuizLevel para el nivel indicado.
    /// </summary>
    public QuizLevel GetLevel(int levelNumber)
    {
        if (levelNumber == 1 && level1 != null) return level1;
        if (levelNumber == 2 && level2 != null) return level2;
        if (levelNumber == 3 && level3 != null) return level3;

        QuizLevel[] allLevels = FindObjectsByType<QuizLevel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var lvl in allLevels)
        {
            string lName = lvl.gameObject.name.ToLower();
            if (lName.Contains(levelNumber.ToString()))
            {
                return lvl;
            }
        }

        return null;
    }

    /// <summary>
    /// Vuelve al menú principal y oculta todos los demás paneles.
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (nextQuestionCoroutine != null) StopCoroutine(nextQuestionCoroutine);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        RestartQuiz();
    }

    /// <summary>
    /// Vuelve al menú de selección de niveles (LevelSelection) desde el Quiz Panel.
    /// Invocado por el botón "Return BTN".
    /// </summary>
    public void ReturnToLevelSelection()
    {
        if (nextQuestionCoroutine != null)
        {
            StopCoroutine(nextQuestionCoroutine);
            nextQuestionCoroutine = null;
        }

        hasAnswered = false;

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(true);
        }

        RestartQuiz();

        Debug.Log("<color=yellow>[QuizManager] Volviendo al menú de selección de niveles (LevelSelection)...</color>");
    }

    private void EndQuiz()
    {
        // 1. Desactivar botones de respuesta
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        // 2. Actualizar el texto de puntuación final en GameOver
        if (lastScoreText != null)
        {
            lastScoreText.text = $"{lastScorePrefix}{correctAnswersCount} / {questions.Count}";
        }

        // 3. Notificar al componente GameOverMenu si existe
        if (gameOverPanel != null)
        {
            GameOverMenu gom = gameOverPanel.GetComponent<GameOverMenu>();
            if (gom != null)
            {
                gom.SetFinalScore(correctAnswersCount, questions.Count);
            }

            gameOverPanel.SetActive(true);
        }

        // 4. Desactivar el panel de Quiz
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        Debug.Log($"<color=cyan>[QuizManager] ¡Quiz finalizado! Aciertos: {correctAnswersCount} de {questions.Count}. Activando GameOver menu.</color>");
    }
}
