using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Efecto procedural de Fuego Dracónico (Rojo & Dorado) para botones de UI.
/// Genera un halo ígneo con respiración/flicker orgánico, ascuas flotantes (embers)
/// y un degradado incandescente sobre el texto de TextMeshPro al hacer hover.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class DragonFireHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("🔥 Colores del Fuego Dracónico")]
    [Tooltip("Color del manto exterior de llamas (Rojo Carmesí)")]
    [SerializeField] private Color outerFlameColor = new Color(0.92f, 0.16f, 0.05f, 0.85f);

    [Tooltip("Color del cuerpo intermedio del fuego (Naranja Ígneo)")]
    [SerializeField] private Color innerFlameColor = new Color(1.0f, 0.58f, 0.05f, 0.90f);

    [Tooltip("Color del núcleo incandescente (Oro Ardiente)")]
    [SerializeField] private Color coreGoldColor = new Color(1.0f, 0.92f, 0.35f, 0.95f);

    [Header("✨ Texto (TextMeshPro)")]
    [Tooltip("Referencia al texto del botón. Si se deja vacío, se busca automáticamente")]
    [SerializeField] private TextMeshProUGUI buttonText;

    [Tooltip("Color superior del texto durante el hover (Oro incandescente)")]
    [SerializeField] private Color textGoldTop = new Color(1.0f, 0.96f, 0.60f, 1f);

    [Tooltip("Color inferior del texto durante el hover (Rojo carmesí de dragón)")]
    [SerializeField] private Color textCrimsonBottom = new Color(0.85f, 0.12f, 0.05f, 1f);

    [Header("🌪️ Dinámica de Llamas y Respiración")]
    [Tooltip("Velocidad de pulsación y parpadeo de las llamas")]
    [Range(2f, 15f)]
    [SerializeField] private float flickerSpeed = 7.5f;

    [Tooltip("Amplitud de la respiración / escala de las llamas")]
    [Range(0.02f, 0.25f)]
    [SerializeField] private float flamePulseAmount = 0.10f;

    [Tooltip("Velocidad de transición al entrar o salir del hover")]
    [SerializeField] private float transitionSpeed = 5.5f;

    [Tooltip("Aumento de escala del botón al hacer hover (1.0 = sin cambio, 1.04 = ligero pulso)")]
    [Range(1f, 1.15f)]
    [SerializeField] private float hoverScaleMultiplier = 1.04f;

    [Header("✨ Ascuas y Chispas Flotantes (Embers)")]
    [Tooltip("Activa o desactiva las ascuas que se desprenden del botón")]
    [SerializeField] private bool enableEmbers = true;

    [Tooltip("Cantidad máxima de ascuas simultáneas en pantalla")]
    [Range(6, 30)]
    [SerializeField] private int maxEmbers = 16;

    [Tooltip("Velocidad ascendente de las ascuas")]
    [SerializeField] private float emberRiseSpeed = 110f;

    [Tooltip("Tiempo de vida promedio de cada ascua en segundos")]
    [SerializeField] private float emberLifetime = 1.2f;

    [Header("🔊 Sonido (Opcional)")]
    [Tooltip("Sonido de llamarada al pasar el ratón")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioSource audioSource;

    // Elementos visuales creados proceduralmente
    private RectTransform buttonRect;
    private Image outerAuraImage;
    private Image innerAuraImage;
    private RectTransform auraContainer;
    private RectTransform emberContainer;

    // Pool de ascuas
    private class Ember
    {
        public RectTransform rect;
        public Image image;
        public Vector2 origin;
        public float currentLife;
        public float maxLife;
        public float speedY;
        public float wobbleFreq;
        public float wobbleAmp;
        public float wobbleOffset;
        public float startSize;
        public bool active;
    }
    private List<Ember> emberPool = new List<Ember>();
    private float emberSpawnTimer = 0f;

    // Estado del hover y transición
    private bool isHovered = false;
    private bool isPressed = false;
    private float currentFade = 0f;
    private Vector3 initialScale;

    // Estado original del texto
    private Color originalTextColor;
    private bool originalGradientEnabled;
    private VertexGradient originalVertexGradient;

    // Textura y sprite procedural compartido
    private static Sprite glowSprite;

    private void Awake()
    {
        buttonRect = GetComponent<RectTransform>();
        initialScale = buttonRect.localScale;

        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (buttonText != null)
        {
            originalTextColor = buttonText.color;
            originalGradientEnabled = buttonText.enableVertexGradient;
            originalVertexGradient = buttonText.colorGradient;
        }

        if (audioSource == null && hoverSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        EnsureGlowSprite();
        SetupVisualHierarchy();
        SetupEmberPool();
    }

    private void OnEnable()
    {
        ResetVisuals();
    }

    private void OnDisable()
    {
        ResetVisuals();
    }

    /// <summary>
    /// Genera una textura radial suave en memoria (sin necesidad de imágenes PNG externas).
    /// </summary>
    private static void EnsureGlowSprite()
    {
        if (glowSprite != null) return;

        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "DragonFire_ProceduralGlowTexture";
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float maxDist = size * 0.5f;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                if (dist >= 1f)
                {
                    pixels[y * size + x] = Color.clear;
                }
                else
                {
                    // Caída suave cuádruple para un halo difuso y núcleo denso
                    float t = 1f - dist;
                    float alpha = t * t * (3f - 2f * t); // smoothstep
                    alpha = alpha * alpha;
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        glowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// Crea los contenedores e imágenes de las llamas alrededor del botón.
    /// </summary>
    private void SetupVisualHierarchy()
    {
        // 1. Contenedor del aura de fuego (detrás del texto)
        if (auraContainer == null)
        {
            Transform existing = transform.Find("[DragonFire_Aura]");
            if (existing != null)
            {
                auraContainer = existing.GetComponent<RectTransform>();
            }
            else
            {
                GameObject auraGo = new GameObject("[DragonFire_Aura]", typeof(RectTransform));
                auraGo.transform.SetParent(transform, false);
                auraGo.transform.SetAsFirstSibling(); // Detrás del texto
                auraContainer = auraGo.GetComponent<RectTransform>();
            }
        }

        auraContainer.anchorMin = Vector2.zero;
        auraContainer.anchorMax = Vector2.one;
        auraContainer.offsetMin = Vector2.zero;
        auraContainer.offsetMax = Vector2.zero;

        // Aura exterior (carmesí)
        if (outerAuraImage == null)
        {
            Transform existingOuter = auraContainer.Find("OuterFlame");
            GameObject outerGo = (existingOuter != null) ? existingOuter.gameObject : new GameObject("OuterFlame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            outerGo.transform.SetParent(auraContainer, false);

            outerAuraImage = outerGo.GetComponent<Image>();
            outerAuraImage.sprite = glowSprite;
            outerAuraImage.type = Image.Type.Simple;
            outerAuraImage.raycastTarget = false; // No bloquea clicks
            outerAuraImage.color = Color.clear;

            RectTransform rt = outerGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(buttonRect.rect.width * 1.55f, buttonRect.rect.height * 2.2f);
        }

        // Aura interior (oro & naranja concentrado)
        if (innerAuraImage == null)
        {
            Transform existingInner = auraContainer.Find("InnerFlame");
            GameObject innerGo = (existingInner != null) ? existingInner.gameObject : new GameObject("InnerFlame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            innerGo.transform.SetParent(auraContainer, false);

            innerAuraImage = innerGo.GetComponent<Image>();
            innerAuraImage.sprite = glowSprite;
            innerAuraImage.type = Image.Type.Simple;
            innerAuraImage.raycastTarget = false;
            innerAuraImage.color = Color.clear;

            RectTransform rt = innerGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(buttonRect.rect.width * 1.25f, buttonRect.rect.height * 1.6f);
        }

        // 2. Contenedor de ascuas (por delante del botón)
        if (emberContainer == null)
        {
            Transform existingEmbers = transform.Find("[DragonFire_Embers]");
            if (existingEmbers != null)
            {
                emberContainer = existingEmbers.GetComponent<RectTransform>();
            }
            else
            {
                GameObject embGo = new GameObject("[DragonFire_Embers]", typeof(RectTransform));
                embGo.transform.SetParent(transform, false);
                embGo.transform.SetAsLastSibling(); // Por delante
                emberContainer = embGo.GetComponent<RectTransform>();
            }
        }

        emberContainer.anchorMin = Vector2.zero;
        emberContainer.anchorMax = Vector2.one;
        emberContainer.offsetMin = Vector2.zero;
        emberContainer.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Prepara el pool de ascuas flotantes para cero reservas de memoria (GC friendly).
    /// </summary>
    private void SetupEmberPool()
    {
        emberPool.Clear();

        for (int i = 0; i < maxEmbers; i++)
        {
            string eName = $"Ember_{i:00}";
            Transform existing = emberContainer.Find(eName);
            GameObject go = (existing != null) ? existing.gameObject : new GameObject(eName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(emberContainer, false);

            Image img = go.GetComponent<Image>();
            img.sprite = glowSprite;
            img.raycastTarget = false;
            img.color = Color.clear;
            go.SetActive(false);

            Ember ember = new Ember
            {
                rect = go.GetComponent<RectTransform>(),
                image = img,
                active = false
            };
            emberPool.Add(ember);
        }
    }

    private void Update()
    {
        // 1. Transición suave de entrada / salida
        float targetFade = isHovered ? 1f : 0f;
        currentFade = Mathf.MoveTowards(currentFade, targetFade, Time.unscaledDeltaTime * transitionSpeed);

        // 2. Respiración y parpadeo orgánico del fuego de dragón (combinación de ondas armónicas)
        float t = Time.unscaledTime * flickerSpeed;
        float flameFlicker = Mathf.Sin(t) * 0.55f + Mathf.Sin(t * 1.9f) * 0.35f + Mathf.Cos(t * 3.3f) * 0.10f;
        float pulseScale = 1f + (flameFlicker * flamePulseAmount * currentFade);

        // 3. Actualizar auras de fuego
        if (outerAuraImage != null)
        {
            Color c = outerFlameColor;
            c.a *= currentFade * (0.85f + flameFlicker * 0.15f);
            outerAuraImage.color = c;
            outerAuraImage.rectTransform.localScale = Vector3.one * (pulseScale * 1.05f);
        }

        if (innerAuraImage != null)
        {
            Color c = Color.Lerp(innerFlameColor, coreGoldColor, 0.4f + flameFlicker * 0.3f);
            c.a *= currentFade * (0.90f + flameFlicker * 0.10f);
            innerAuraImage.color = c;
            innerAuraImage.rectTransform.localScale = Vector3.one * pulseScale;
        }

        // 4. Actualizar escala del botón
        float targetScaleMultiplier = isPressed ? 0.96f : (isHovered ? hoverScaleMultiplier : 1f);
        Vector3 desiredScale = initialScale * Mathf.Lerp(1f, targetScaleMultiplier, currentFade);
        buttonRect.localScale = Vector3.Lerp(buttonRect.localScale, desiredScale, Time.unscaledDeltaTime * 12f);

        // 5. Actualizar texto TextMeshPro
        UpdateBurningText(currentFade, flameFlicker);

        // 6. Gestionar ascuas flotantes
        if (enableEmbers)
        {
            UpdateEmbers();
        }
    }

    /// <summary>
    /// Modula el color y gradiente del texto hacia un resplandor ardiente.
    /// </summary>
    private void UpdateBurningText(float fade, float flicker)
    {
        if (buttonText == null) return;

        if (fade > 0.01f)
        {
            buttonText.enableVertexGradient = true;

            // Oro en la parte superior, rojo carmesí en la base
            Color goldTop = Color.Lerp(textGoldTop, Color.white, flicker * 0.25f);
            Color crimsonBot = textCrimsonBottom;

            VertexGradient fireGradient = new VertexGradient(
                Color.Lerp(originalTextColor, goldTop, fade),
                Color.Lerp(originalTextColor, goldTop, fade),
                Color.Lerp(originalTextColor, crimsonBot, fade),
                Color.Lerp(originalTextColor, crimsonBot, fade)
            );

            buttonText.colorGradient = fireGradient;
        }
        else
        {
            buttonText.enableVertexGradient = originalGradientEnabled;
            if (originalGradientEnabled)
            {
                buttonText.colorGradient = originalVertexGradient;
            }
            buttonText.color = originalTextColor;
        }
    }

    /// <summary>
    /// Actualiza y genera las ascuas ascendentes que se desprenden del botón.
    /// </summary>
    private void UpdateEmbers()
    {
        float dt = Time.unscaledDeltaTime;

        // Spawn de nuevas ascuas si el cursor está sobre el botón
        if (isHovered && currentFade > 0.2f)
        {
            emberSpawnTimer += dt;
            float spawnInterval = emberLifetime / (maxEmbers * 0.75f);
            if (emberSpawnTimer >= spawnInterval)
            {
                emberSpawnTimer = 0f;
                SpawnEmber();
            }
        }

        // Simulación de las ascuas activas
        float width = buttonRect.rect.width;
        for (int i = 0; i < emberPool.Count; i++)
        {
            Ember e = emberPool[i];
            if (!e.active) continue;

            e.currentLife += dt;
            float progress = e.currentLife / e.maxLife;

            if (progress >= 1f)
            {
                e.active = false;
                e.rect.gameObject.SetActive(false);
                continue;
            }

            // Movimiento ascendente con turbulencia lateral sinusoidal
            float currentY = e.origin.y + (progress * e.speedY * e.maxLife);
            float wobbleX = Mathf.Sin(e.currentLife * e.wobbleFreq + e.wobbleOffset) * e.wobbleAmp;
            e.rect.anchoredPosition = new Vector2(e.origin.x + wobbleX, currentY);

            // Escala (se reduce conforme se consume la ascua)
            float size = Mathf.Lerp(e.startSize, e.startSize * 0.25f, progress);
            e.rect.sizeDelta = new Vector2(size, size * 1.3f); // ligeramente alargada hacia arriba

            // Color del ascua a lo largo de su vida: Blanco/Oro -> Naranja Fuego -> Carmesí -> Ceniza
            Color emberColor;
            if (progress < 0.25f)
            {
                emberColor = Color.Lerp(Color.white, coreGoldColor, progress / 0.25f);
            }
            else if (progress < 0.65f)
            {
                emberColor = Color.Lerp(coreGoldColor, innerFlameColor, (progress - 0.25f) / 0.40f);
            }
            else
            {
                emberColor = Color.Lerp(innerFlameColor, outerFlameColor, (progress - 0.65f) / 0.35f);
            }

            // Desvanecimiento suave en la fase final
            float alpha = (progress > 0.7f) ? Mathf.Lerp(1f, 0f, (progress - 0.7f) / 0.3f) : 1f;
            emberColor.a = alpha * 0.95f;
            e.image.color = emberColor;
        }
    }

    private void SpawnEmber()
    {
        // Buscar un ascua inactiva en el pool
        Ember freeEmber = null;
        for (int i = 0; i < emberPool.Count; i++)
        {
            if (!emberPool[i].active)
            {
                freeEmber = emberPool[i];
                break;
            }
        }
        if (freeEmber == null) return;

        float width = buttonRect.rect.width * 0.85f;
        float height = buttonRect.rect.height;

        // Nace en la base o borde inferior del botón
        float startX = Random.Range(-width * 0.5f, width * 0.5f);
        float startY = Random.Range(-height * 0.45f, height * 0.05f);

        freeEmber.origin = new Vector2(startX, startY);
        freeEmber.currentLife = 0f;
        freeEmber.maxLife = emberLifetime * Random.Range(0.75f, 1.35f);
        freeEmber.speedY = emberRiseSpeed * Random.Range(0.8f, 1.4f);
        freeEmber.wobbleFreq = Random.Range(4f, 8f);
        freeEmber.wobbleAmp = Random.Range(8f, 22f);
        freeEmber.wobbleOffset = Random.Range(0f, Mathf.PI * 2f);
        freeEmber.startSize = Random.Range(8f, 18f);

        freeEmber.active = true;
        freeEmber.rect.anchoredPosition = freeEmber.origin;
        freeEmber.rect.gameObject.SetActive(true);
    }

    private void ResetVisuals()
    {
        isHovered = false;
        isPressed = false;
        currentFade = 0f;

        if (buttonRect != null && initialScale != Vector3.zero)
        {
            buttonRect.localScale = initialScale;
        }

        if (outerAuraImage != null) outerAuraImage.color = Color.clear;
        if (innerAuraImage != null) innerAuraImage.color = Color.clear;

        if (buttonText != null)
        {
            buttonText.enableVertexGradient = originalGradientEnabled;
            if (originalGradientEnabled) buttonText.colorGradient = originalVertexGradient;
            buttonText.color = originalTextColor;
        }

        for (int i = 0; i < emberPool.Count; i++)
        {
            if (emberPool[i] != null && emberPool[i].rect != null)
            {
                emberPool[i].active = false;
                emberPool[i].rect.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound, 0.7f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
}
