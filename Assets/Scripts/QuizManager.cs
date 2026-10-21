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

    public Question() { }

    public Question(string question, string[] answers, int correctIndex)
    {
        this.questionText = question;
        this.answers = answers;
        this.correctAnswerIndex = correctIndex;
    }
}

public class QuizManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("Componente TextMeshProUGUI donde se muestra el texto de la pregunta")]
    [SerializeField] private TextMeshProUGUI questionText;

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

    [Tooltip("Panel del juego (Quiz Panel)")]
    [SerializeField] private GameObject quizPanel;

    [Tooltip("Botón para iniciar el juego desde el menú (Play BTN)")]
    [SerializeField] private Button playButton;

    [Header("GameOver Panel")]
    [Tooltip("Panel de fin de partida (GameOver menu)")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Botón para volver al menú principal desde GameOver (return to main menu)")]
    [SerializeField] private Button returnToMainMenuButton;

    [Tooltip("Texto donde se muestra la puntuación final (last score text)")]
    [SerializeField] private TextMeshProUGUI lastScoreText;

    [Tooltip("Prefijo para la puntuación en GameOver")]
    [SerializeField] private string lastScorePrefix = "Puntuación: ";

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
    [Tooltip("Lista de preguntas que se jugarán")]
    [SerializeField] private List<Question> questions = new List<Question>();

    [Tooltip("Si está activo, pasa automáticamente a la siguiente pregunta tras unos segundos")]
    [SerializeField] private bool autoAdvance = true;

    [Tooltip("Segundos de espera antes de mostrar la siguiente pregunta")]
    [SerializeField] private float delayBeforeNextQuestion = 2.5f;

    [Tooltip("Si es true, resalta también en verde la respuesta correcta cuando el usuario falla")]
    [SerializeField] private bool showCorrectAnswerOnMistake = true;

    [Tooltip("Si es true, vuelve a empezar el quiz al terminar todas las preguntas")]
    [SerializeField] private bool loopQuiz = true;

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
    }

    private void Start()
    {
        correctAnswersCount = 0;
        UpdateScoreText();

        // Asegurarse de que el GameOver menu siempre empieza desactivado
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
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
            // Intentar encontrar como hijo llamado "Question Text"
            Transform qChild = transform.Find("Question Text");
            if (qChild != null)
            {
                questionText = qChild.GetComponent<TextMeshProUGUI>();
            }

            // Si no, buscar en la escena completa por nombre
            if (questionText == null)
            {
                GameObject qGo = GameObject.Find("Question Text");
                if (qGo != null)
                {
                    questionText = qGo.GetComponent<TextMeshProUGUI>();
                }
            }

            // Como último recurso, buscar cualquier TextMeshProUGUI que no sea hijo de un botón
            if (questionText == null)
            {
                TextMeshProUGUI[] tmps = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
                foreach (var tmp in tmps)
                {
                    if (tmp.GetComponentInParent<Button>() == null)
                    {
                        questionText = tmp;
                        break;
                    }
                }
            }
        }

        // 2. Localizar los 4 botones
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
                // Buscar ordenadamente "Respuesta 1", "Respuesta 2", "Respuesta 3", "Respuesta 4"
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
                    answerButtons = buttonsParent.GetComponentsInChildren<Button>();
                }
            }
            else
            {
                answerButtons = GetComponentsInChildren<Button>();
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

        // 4. Localizar paneles (MainMenu, Quiz Panel) y botón Play BTN
        if (mainMenuPanel == null)
        {
            mainMenuPanel = GameObject.Find("MainMenu");
            if (mainMenuPanel == null) mainMenuPanel = GameObject.Find("Main Menu");
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
    }

    [ContextMenu("Cargar las 12 preguntas oficiales")]
    public void LoadGameQuestions()
    {
        questions = new List<Question>
        {
            // Ronda 1
            new Question(
                "1. ¿Qué es Unity?",
                new string[4] {
                    "A) Un motor de videojuegos",
                    "B) Un lenguaje de modelado 3D",
                    "C) Una tarjeta gráfica",
                    "D) Un programa para editar vídeos"
                },
                0 // Correcta: A
            ),
            new Question(
                "2. ¿Para qué sirve Blender?",
                new string[4] {
                    "A) Para programar redes",
                    "B) Para crear modelos 3D y animaciones",
                    "C) Para publicar videojuegos automáticamente",
                    "D) Para gestionar servidores"
                },
                1 // Correcta: B
            ),
            new Question(
                "3. ¿Qué es un GameObject en Unity?",
                new string[4] {
                    "A) Un objeto de la escena",
                    "B) Un tipo de ordenador",
                    "C) Una tienda de videojuegos",
                    "D) Un archivo de audio exclusivamente"
                },
                0 // Correcta: A
            ),
            new Question(
                "4. ¿Qué es una escena en Unity?",
                new string[4] {
                    "A) Una lista de contraseñas",
                    "B) Un nivel o espacio donde organizamos objetos",
                    "C) Un modelo 3D obligatorio",
                    "D) Un tipo de textura"
                },
                1 // Correcta: B
            ),

            // Ronda 2: crear un videojuego
            new Question(
                "5. ¿Qué es un material en Unity?",
                new string[4] {
                    "A) Un objeto que guarda las partidas",
                    "B) Un elemento que define cómo se ve una superficie",
                    "C) Un nivel completo",
                    "D) Un archivo de texto"
                },
                1 // Correcta: B
            ),
            new Question(
                "6. ¿Para qué sirve una textura?",
                new string[4] {
                    "A) Para añadir detalles visuales a una superficie",
                    "B) Para aumentar automáticamente los FPS",
                    "C) Para crear código C#",
                    "D) Para guardar una escena"
                },
                0 // Correcta: A
            ),
            new Question(
                "7. ¿Qué lenguaje se utiliza habitualmente para programar en Unity?",
                new string[4] {
                    "A) HTML",
                    "B) C#",
                    "C) SQL",
                    "D) CSS"
                },
                1 // Correcta: B
            ),
            new Question(
                "8. ¿Qué hace la cámara de un videojuego?",
                new string[4] {
                    "A) Dibuja los modelos 3D",
                    "B) Muestra la vista del juego",
                    "C) Compila el código",
                    "D) Crea las texturas"
                },
                1 // Correcta: B
            ),

            // Ronda 3: curiosidades y preguntas divertidas
            new Question(
                "9. ¿Qué significa FPS en videojuegos?",
                new string[4] {
                    "A) Frames Per Second (fotogramas por segundo)",
                    "B) Fast Player System",
                    "C) Final Project Scene",
                    "D) First Program Started"
                },
                0 // Correcta: A
            ),
            new Question(
                "10. ¿Qué conviene hacer mientras desarrollas un juego?",
                new string[4] {
                    "A) No probarlo hasta terminarlo",
                    "B) Probarlo frecuentemente",
                    "C) Borrar todas las versiones anteriores",
                    "D) Crear todos los gráficos antes de tener una idea"
                },
                1 // Correcta: B
            ),
            new Question(
                "11. ¿Qué es un bug?",
                new string[4] {
                    "A) Un error o fallo del programa",
                    "B) Un tipo de personaje",
                    "C) Un motor gráfico",
                    "D) Un efecto de sonido"
                },
                0 // Correcta: A
            ),
            new Question(
                "12. ¿Cuál es la mejor forma de empezar a crear tu primer videojuego?",
                new string[4] {
                    "A) Crear un MMO enorme",
                    "B) Empezar por un proyecto pequeño",
                    "C) Comprar el ordenador más caro",
                    "D) Programar sin probar nada"
                },
                1 // Correcta: B
            )
        };
    }

    private void InitializeDefaultQuestions()
    {
        if (questions == null || questions.Count == 0)
        {
            LoadGameQuestions();
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

            int answerIndex = i; // Copia local para la clausura lambda
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

        // 2. Configurar y restaurar cada botón
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;

            ResetButton(i);

            // Asignar el texto de la opción correspondiente
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
    /// <param name="selectedIndex">Índice del botón presionado (0 a 3)</param>
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

            // Iluminar en VERDE el botón correcto pulsado
            IlluminateButton(selectedIndex, correctColor);
            Debug.Log($"<color=green>[QuizManager] ¡Respuesta CORRECTA! Opción {selectedIndex + 1}. Total aciertos: {correctAnswersCount}</color>");
        }
        else
        {
            // Iluminar en ROJO el botón incorrecto pulsado
            IlluminateButton(selectedIndex, wrongColor);
            Debug.Log($"<color=red>[QuizManager] Respuesta INCORRECTA. Opción pulsada: {selectedIndex + 1}. La correcta era: {currentQ.correctAnswerIndex + 1}</color>");

            // Mostrar también en verde la respuesta que era correcta
            if (showCorrectAnswerOnMistake && currentQ.correctAnswerIndex >= 0 && currentQ.correctAnswerIndex < answerButtons.Length)
            {
                IlluminateButton(currentQ.correctAnswerIndex, correctColor);
            }
        }

        // Desactivar la interacción en todos los botones para evitar pulsaciones adicionales
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].interactable = false;
            }
        }

        // Si autoAdvance está activo, pasar a la siguiente pregunta tras la espera
        if (autoAdvance)
        {
            if (nextQuestionCoroutine != null) StopCoroutine(nextQuestionCoroutine);
            nextQuestionCoroutine = StartCoroutine(NextQuestionRoutine());
        }
    }

    /// <summary>
    /// Ilumina el botón con el color especificado (verde o rojo) y ajusta los estados para que persista.
    /// </summary>
    private void IlluminateButton(int buttonIndex, Color feedbackColor)
    {
        if (buttonIndex < 0 || buttonIndex >= answerButtons.Length) return;
        Button btn = answerButtons[buttonIndex];
        if (btn == null) return;

        // 1. Iluminar la imagen del botón (fondo)
        if (btn.image != null)
        {
            btn.image.color = feedbackColor;
        }

        // 2. Modificar ColorBlock para que conserve el color feedback incluso al estar disabled o seleccionado
        ColorBlock cb = btn.colors;
        cb.normalColor = feedbackColor;
        cb.highlightedColor = feedbackColor;
        cb.pressedColor = feedbackColor;
        cb.selectedColor = feedbackColor;
        cb.disabledColor = feedbackColor;
        btn.colors = cb;

        // 3. Opcional: cambiar el color del texto a blanco para que contraste perfectamente
        if (changeTextColorOnFeedback && originalButtonStates != null && buttonIndex < originalButtonStates.Length)
        {
            TextMeshProUGUI txt = originalButtonStates[buttonIndex].textComponent;
            if (txt != null)
            {
                txt.color = feedbackTextColor;
            }
        }
    }

    /// <summary>
    /// Restablece la apariencia normal (transparente) e interactividad del botón.
    /// </summary>
    private void ResetButton(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= answerButtons.Length) return;
        Button btn = answerButtons[buttonIndex];
        if (btn == null) return;

        if (originalButtonStates != null && buttonIndex < originalButtonStates.Length)
        {
            // Restaurar color de la imagen al default (transparente)
            if (btn.image != null)
            {
                btn.image.color = defaultColor;
            }

            // Restaurar ColorBlock original del botón
            btn.colors = originalButtonStates[buttonIndex].originalColors;

            // Restaurar color del texto original
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

    /// <summary>
    /// Actualiza el texto en pantalla de las respuestas correctas.
    /// </summary>
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

    /// <summary>
    /// Pasa a la siguiente pregunta manualmente.
    /// </summary>
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

    /// <summary>
    /// Reinicia el quiz desde la primera pregunta y resetea el contador.
    /// </summary>
    public void RestartQuiz()
    {
        if (nextQuestionCoroutine != null) StopCoroutine(nextQuestionCoroutine);
        correctAnswersCount = 0;
        UpdateScoreText();
        currentQuestionIndex = 0;
        DisplayQuestion(0);
    }

    /// <summary>
    /// Desactiva el menú principal (MainMenu), activa el Quiz Panel e inicia las preguntas desde el principio.
    /// </summary>
    public void StartGame()
    {
        // 1. Asegurar que GameOver esté desactivado
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 2. Activar el panel del Quiz
        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }

        // 3. Desactivar el panel del Menú
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        // 4. Reiniciar y comenzar
        RestartQuiz();
    }

    /// <summary>
    /// Vuelve al menú principal y oculta el panel de Quiz y el de GameOver.
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (nextQuestionCoroutine != null) StopCoroutine(nextQuestionCoroutine);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
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

            // Activar automáticamente el panel de GameOver
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
