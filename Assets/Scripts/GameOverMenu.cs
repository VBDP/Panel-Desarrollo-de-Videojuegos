using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverMenu : MonoBehaviour
{
    [Header("Paneles")]
    [Tooltip("Panel de GameOver (este panel)")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Panel del Menú Principal (MainMenu)")]
    [SerializeField] private GameObject mainMenuPanel;

    [Tooltip("Panel de Selección de Nivel (LevelSelection)")]
    [SerializeField] private GameObject levelSelectionPanel;

    [Tooltip("Panel del Quiz (Quiz Panel)")]
    [SerializeField] private GameObject quizPanel;

    [Header("Elementos de GameOver")]
    [Tooltip("Botón para volver al menú principal (return to main menu)")]
    [SerializeField] private Button returnToMainMenuButton;

    [Tooltip("Texto donde se muestra la puntuación final (last score text)")]
    [SerializeField] private TextMeshProUGUI lastScoreText;

    [Tooltip("Formato del texto de puntuación")]
    [SerializeField] private string scoreFormat = "Puntuación: {0} / {1}";

    [Header("Quiz Manager")]
    [SerializeField] private QuizManager quizManager;

    private void Awake()
    {
        AutoFindReferences();

        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.onClick.RemoveAllListeners();
            returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenuClicked);
        }
    }

    private void Reset()
    {
        AutoFindReferences();
    }

    [ContextMenu("Auto-asignar referencias")]
    public void AutoFindReferences()
    {
        if (gameOverPanel == null)
        {
            gameOverPanel = this.gameObject;
        }

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
        }

        if (quizManager == null)
        {
            quizManager = FindFirstObjectByType<QuizManager>(FindObjectsInactive.Include);
        }

        // 1. Localizar el botón 'return to main menu'
        if (returnToMainMenuButton == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string bName = b.gameObject.name.ToLower();
                if (bName.Contains("return") || bName.Contains("main") || bName.Contains("menu"))
                {
                    returnToMainMenuButton = b;
                    break;
                }
            }

            if (returnToMainMenuButton == null && buttons.Length > 0)
            {
                returnToMainMenuButton = buttons[0];
            }
        }

        // 2. Localizar el texto 'last score text'
        if (lastScoreText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                // Ignorar el texto que esté dentro del botón
                if (t.GetComponentInParent<Button>() != null) continue;

                string tName = t.gameObject.name.ToLower();
                if (tName.Contains("score") || tName.Contains("puntuacion") || tName.Contains("last"))
                {
                    lastScoreText = t;
                    break;
                }
            }

            // Si no tiene nombre específico, tomar el primer TextMeshProUGUI que no sea hijo de un botón
            if (lastScoreText == null)
            {
                foreach (var t in texts)
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

    /// <summary>
    /// Actualiza el texto con el resultado obtenido en el quiz.
    /// </summary>
    public void SetFinalScore(int score, int totalQuestions)
    {
        if (lastScoreText != null)
        {
            lastScoreText.text = string.Format(scoreFormat, score, totalQuestions);
        }
    }

    /// <summary>
    /// Desactiva GameOver, activa MainMenu y reinicia el estado del Quiz.
    /// </summary>
    public void OnReturnToMainMenuClicked()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (quizManager != null)
        {
            quizManager.RestartQuiz();
        }
    }
}
