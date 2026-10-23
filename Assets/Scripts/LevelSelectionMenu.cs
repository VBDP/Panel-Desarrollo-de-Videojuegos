using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionMenu : MonoBehaviour
{
    [Header("Paneles")]
    [Tooltip("Panel de selección de nivel (este GameObject)")]
    [SerializeField] private GameObject levelSelectionPanel;

    [Tooltip("Panel del menú principal (MainMenu)")]
    [SerializeField] private GameObject mainMenuPanel;

    [Tooltip("Panel del Quiz (Quiz Panel)")]
    [SerializeField] private GameObject quizPanel;

    [Header("Botones de Nivel")]
    [Tooltip("Botón para seleccionar el Nivel 1 (Level1 BTN)")]
    [SerializeField] private Button level1Button;

    [Tooltip("Botón para seleccionar el Nivel 2 (Level2 BTN)")]
    [SerializeField] private Button level2Button;

    [Tooltip("Botón para seleccionar el Nivel 3 (Level3 BTN)")]
    [SerializeField] private Button level3Button;

    [Tooltip("Botón opcional para regresar al MainMenu")]
    [SerializeField] private Button backButton;

    [Header("Quiz Manager")]
    [Tooltip("Referencia al QuizManager")]
    [SerializeField] private QuizManager quizManager;

    private void Awake()
    {
        AutoFindReferences();

        if (level1Button != null)
        {
            level1Button.onClick.RemoveAllListeners();
            level1Button.onClick.AddListener(() => SelectLevel(1));
        }

        if (level2Button != null)
        {
            level2Button.onClick.RemoveAllListeners();
            level2Button.onClick.AddListener(() => SelectLevel(2));
        }

        if (level3Button != null)
        {
            level3Button.onClick.RemoveAllListeners();
            level3Button.onClick.AddListener(() => SelectLevel(3));
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackToMainMenu);
        }
    }

    private void Reset()
    {
        AutoFindReferences();
    }

    [ContextMenu("Auto-asignar referencias")]
    public void AutoFindReferences()
    {
        if (levelSelectionPanel == null)
        {
            levelSelectionPanel = this.gameObject;
        }

        if (mainMenuPanel == null)
        {
            mainMenuPanel = GameObject.Find("MainMenu");
            if (mainMenuPanel == null) mainMenuPanel = GameObject.Find("Main Menu");
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

        // Buscar botones Level1 BTN, Level2 BTN, Level3 BTN
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            string n = b.gameObject.name.ToLower().Replace(" ", "").Replace("_", "");
            if (n.Contains("level1") || n.Contains("nivel1"))
            {
                if (level1Button == null) level1Button = b;
            }
            else if (n.Contains("level2") || n.Contains("nivel2"))
            {
                if (level2Button == null) level2Button = b;
            }
            else if (n.Contains("level3") || n.Contains("nivel3"))
            {
                if (level3Button == null) level3Button = b;
            }
            else if (n.Contains("back") || n.Contains("volver") || n.Contains("return"))
            {
                if (backButton == null) backButton = b;
            }
        }
    }

    /// <summary>
    /// Selecciona el nivel indicado (1, 2 o 3) y arranca el Quiz.
    /// </summary>
    public void SelectLevel(int levelNumber)
    {
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(false);
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
            quizManager.PlayLevel(levelNumber);
        }
        else
        {
            Debug.LogWarning("[LevelSelectionMenu] No se encontró el componente QuizManager.");
            if (quizPanel != null)
            {
                quizPanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Vuelve al menú principal.
    /// </summary>
    public void BackToMainMenu()
    {
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
}
