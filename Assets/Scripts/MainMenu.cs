using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles")]
    [Tooltip("Panel del menú principal (MainMenu)")]
    [SerializeField] private GameObject mainMenuPanel;

    [Tooltip("Panel de selección de nivel (LevelSelection)")]
    [SerializeField] private GameObject levelSelectionPanel;

    [Tooltip("Panel del juego (Quiz Panel)")]
    [SerializeField] private GameObject quizPanel;

    [Header("Botón Play")]
    [Tooltip("Botón para iniciar la partida (Play BTN)")]
    [SerializeField] private Button playButton;

    [Header("Quiz Manager")]
    [Tooltip("Referencia al QuizManager")]
    [SerializeField] private QuizManager quizManager;

    private void Awake()
    {
        AutoFindReferences();

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(PlayQuiz);
        }
    }

    private void Reset()
    {
        AutoFindReferences();
    }

    [ContextMenu("Auto-asignar referencias")]
    public void AutoFindReferences()
    {
        if (mainMenuPanel == null)
        {
            mainMenuPanel = this.gameObject;
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

        if (playButton == null)
        {
            Transform btnTr = transform.Find("Play BTN");
            if (btnTr == null) btnTr = transform.Find("PlayBTN");
            if (btnTr == null) btnTr = transform.Find("Play Button");

            if (btnTr != null)
            {
                playButton = btnTr.GetComponent<Button>();
            }
            else
            {
                playButton = GetComponentInChildren<Button>(true);
            }
        }

        if (quizManager == null)
        {
            quizManager = FindFirstObjectByType<QuizManager>(FindObjectsInactive.Include);
        }
    }

    /// <summary>
    /// Al pulsar Play, si existe LevelSelection abre la selección de nivel; si no, va directo al Quiz.
    /// </summary>
    public void PlayQuiz()
    {
        // 1. Si existe panel LevelSelection, abrirlo
        if (levelSelectionPanel != null)
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }
            levelSelectionPanel.SetActive(true);
        }
        else
        {
            // 2. Si no hay LevelSelection, ir directo a Quiz Panel como antes
            if (quizPanel != null)
            {
                quizPanel.SetActive(true);
            }

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }

            if (quizManager == null)
            {
                quizManager = FindFirstObjectByType<QuizManager>(FindObjectsInactive.Include);
            }

            if (quizManager != null)
            {
                quizManager.StartGame();
            }
            else
            {
                Debug.LogWarning("[MainMenu] No se encontró el componente QuizManager en la escena.");
            }
        }
    }
}
