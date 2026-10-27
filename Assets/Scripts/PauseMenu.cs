using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseMenu : MonoBehaviour
{
    [Header("Panel de Pausa")]
    [Tooltip("El GameObject del panel de pausa. Si se deja vacío o es este mismo objeto, se usará CanvasGroup para ocultar/mostrar.")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Ocultar el menú automáticamente al iniciar el juego")]
    [SerializeField] private bool hideOnStart = true;

    [Tooltip("Detener el tiempo del juego (Time.timeScale = 0) al pausar")]
    [SerializeField] private bool pauseTimeScale = true;

    [Header("Marcadores de Texto")]
    [Tooltip("Texto TextMeshPro que muestra la puntuación del Equipo A (Score A)")]
    [SerializeField] private TextMeshProUGUI scoreTextA;

    [Tooltip("Texto TextMeshPro que muestra la puntuación del Equipo B (Score B)")]
    [SerializeField] private TextMeshProUGUI scoreTextB;

    [Tooltip("Formato para mostrar la puntuación. Usa {0} para el número.")]
    [SerializeField] private string scoreFormat = "{0}";

    [Header("Puntuaciones")]
    [Tooltip("Puntuación inicial del Equipo A")]
    [SerializeField] private int initialScoreA = 0;

    [Tooltip("Puntuación inicial del Equipo B")]
    [SerializeField] private int initialScoreB = 0;

    [Header("Control de Teclas")]
    [Tooltip("Si es true, solo se sumarán puntos con A y B cuando el menú de pausa esté abierto. Si es false, se puede sumar en cualquier momento.")]
    [SerializeField] private bool onlyScoreWhenMenuIsOpen = false;

    // Estado interno
    private int scoreA;
    private int scoreB;
    private bool isPaused = false;
    private CanvasGroup canvasGroup;

    public int ScoreA => scoreA;
    public int ScoreB => scoreB;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        AutoFindReferences();
        SetupVisibilityComponent();

        scoreA = initialScoreA;
        scoreB = initialScoreB;
    }

    private void Start()
    {
        UpdateScoreDisplay();

        if (hideOnStart)
        {
            SetPauseState(false);
        }
        else
        {
            SetPauseState(isPaused);
        }
    }

    private void Update()
    {
        CheckInputs();
    }

    private void OnDisable()
    {
        // Asegura que no dejamos el juego congelado al desactivar o destruir el objeto
        if (pauseTimeScale && isPaused)
        {
            Time.timeScale = 1f;
        }
    }

    private void OnDestroy()
    {
        if (pauseTimeScale && isPaused)
        {
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Comprueba las pulsaciones de teclado (ESC para pausar, A y B para sumar puntos)
    /// </summary>
    private void CheckInputs()
    {
        bool escPressed = false;
        bool aPressed = false;
        bool bPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            escPressed = Keyboard.current.escapeKey.wasPressedThisFrame;
            aPressed = Keyboard.current.aKey.wasPressedThisFrame;
            bPressed = Keyboard.current.bKey.wasPressedThisFrame;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        escPressed = Input.GetKeyDown(KeyCode.Escape);
        aPressed = Input.GetKeyDown(KeyCode.A);
        bPressed = Input.GetKeyDown(KeyCode.B);
#endif

        // Toggle del menú con ESC
        if (escPressed)
        {
            TogglePause();
        }

        // Sumar puntos con A o B
        if (!onlyScoreWhenMenuIsOpen || isPaused)
        {
            if (aPressed)
            {
                AddScoreA(1);
            }

            if (bPressed)
            {
                AddScoreB(1);
            }
        }
    }

    /// <summary>
    /// Alterna la visibilidad del menú de pausa
    /// </summary>
    public void TogglePause()
    {
        SetPauseState(!isPaused);
    }

    /// <summary>
    /// Abre el menú de pausa
    /// </summary>
    public void Pause()
    {
        SetPauseState(true);
    }

    /// <summary>
    /// Cierra el menú de pausa y reanuda el juego
    /// </summary>
    public void Resume()
    {
        SetPauseState(false);
    }

    /// <summary>
    /// Aplica el estado de pausa (visibilidad y Time.timeScale)
    /// </summary>
    public void SetPauseState(bool pause)
    {
        isPaused = pause;

        if (pauseTimeScale)
        {
            Time.timeScale = isPaused ? 0f : 1f;
        }

        // Si usamos CanvasGroup (mismo GameObject o panel asignado con CanvasGroup)
        if (canvasGroup != null)
        {
            canvasGroup.alpha = isPaused ? 1f : 0f;
            canvasGroup.interactable = isPaused;
            canvasGroup.blocksRaycasts = isPaused;
        }
        else if (pauseMenuPanel != null && pauseMenuPanel != this.gameObject)
        {
            pauseMenuPanel.SetActive(isPaused);
        }
    }

    /// <summary>
    /// Suma puntos al Equipo A
    /// </summary>
    public void AddScoreA(int points = 1)
    {
        scoreA += points;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Suma puntos al Equipo B
    /// </summary>
    public void AddScoreB(int points = 1)
    {
        scoreB += points;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Resta puntos al Equipo A
    /// </summary>
    public void SubtractScoreA(int points = 1)
    {
        scoreA = Mathf.Max(0, scoreA - points);
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Resta puntos al Equipo B
    /// </summary>
    public void SubtractScoreB(int points = 1)
    {
        scoreB = Mathf.Max(0, scoreB - points);
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Reinicia los marcadores a su valor inicial
    /// </summary>
    public void ResetScores()
    {
        scoreA = initialScoreA;
        scoreB = initialScoreB;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Actualiza los textos de Score A y Score B en tiempo real
    /// </summary>
    public void UpdateScoreDisplay()
    {
        if (scoreTextA != null)
        {
            scoreTextA.text = string.Format(scoreFormat, scoreA);
        }

        if (scoreTextB != null)
        {
            scoreTextB.text = string.Format(scoreFormat, scoreB);
        }
    }

    private void SetupVisibilityComponent()
    {
        // Si el panel es este mismo objeto (o no se especificó otro), usamos CanvasGroup
        // para poder seguir ejecutando Update() y escuchar la tecla ESC aunque esté oculto.
        if (pauseMenuPanel == null || pauseMenuPanel == this.gameObject)
        {
            pauseMenuPanel = this.gameObject;
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            canvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
        }
    }

    private void Reset()
    {
        AutoFindReferences();
    }

    /// <summary>
    /// Busca y asigna automáticamente referencias a Score A, Score B y PauseMenu
    /// </summary>
    [ContextMenu("Auto-asignar referencias")]
    public void AutoFindReferences()
    {
        if (pauseMenuPanel == null)
        {
            pauseMenuPanel = this.gameObject;
        }

        if (scoreTextA == null)
        {
            // 1. Buscar entre los hijos (Score A)
            Transform childA = transform.Find("Score A");
            if (childA != null)
            {
                scoreTextA = childA.GetComponent<TextMeshProUGUI>();
            }

            // 2. Si no es hijo directo, buscar por nombre en la escena
            if (scoreTextA == null)
            {
                GameObject objA = GameObject.Find("Score A");
                if (objA != null)
                {
                    scoreTextA = objA.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        if (scoreTextB == null)
        {
            // 1. Buscar entre los hijos (Score B)
            Transform childB = transform.Find("Score B");
            if (childB != null)
            {
                scoreTextB = childB.GetComponent<TextMeshProUGUI>();
            }

            // 2. Si no es hijo directo, buscar por nombre en la escena
            if (scoreTextB == null)
            {
                GameObject objB = GameObject.Find("Score B");
                if (objB != null)
                {
                    scoreTextB = objB.GetComponent<TextMeshProUGUI>();
                }
            }
        }
    }
}
