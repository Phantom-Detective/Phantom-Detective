using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws a red glowing horror crosshair centered on the screen.
/// Attach to any active GameObject in the scene (e.g. the Player or a UI manager).
/// No external assets needed — everything is drawn procedurally.
/// </summary>
public class HorrorCrosshair : MonoBehaviour
{
    [Header("Crosshair Style")]
    [Tooltip("Dot = simple red glow blob. Star = 4-point laser star like the reference image.")]
    public CrosshairStyle style = CrosshairStyle.Star;

    [Header("Appearance")]
    public Color innerColor = new Color(1f, 1f, 1f, 1f);       // bright white-red center
    public Color outerColor = new Color(0.9f, 0f, 0f, 0f);     // red fading to transparent
    public float dotRadius    = 10f;   // radius of the soft glow circle
    public float starLength   = 28f;   // how long each spike of the star is
    public float starWidth    = 6f;    // thickness of each spike at the base
    public float glowRadius   = 22f;   // size of the soft glow behind the star

    [Header("Pulse (optional)")]
    public bool  pulse        = true;
    public float pulseSpeed   = 2f;
    public float pulseAmount  = 0.12f; // how much scale changes during pulse (0 = no pulse)

    public enum CrosshairStyle { Dot, Star }

    // ── internals ──────────────────────────────────────────────────────────────
    private Canvas        canvas;
    private RawImage      glowImage;
    private RawImage      starImage;
    private Texture2D     glowTex;
    private Texture2D     starTex;

    private const int TexSize = 64; // pixel size of each generated texture

    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        BuildCanvas();

        if (style == CrosshairStyle.Dot)
        {
            BuildDot();
        }
        else
        {
            BuildStar();
        }

        // Hide the system cursor — we're drawing our own
        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!pulse) return;

        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        if (glowImage != null)
            glowImage.rectTransform.localScale = Vector3.one * scale;
        if (starImage != null)
            starImage.rectTransform.localScale = Vector3.one * scale;
    }

    private void OnDestroy()
    {
        if (glowTex != null) Destroy(glowTex);
        if (starTex != null) Destroy(starTex);
    }

    // ── Canvas setup ───────────────────────────────────────────────────────────

    private void BuildCanvas()
    {
        GameObject canvasGO = new GameObject("CrosshairCanvas");
        DontDestroyOnLoad(canvasGO);                  // survives scene loads

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;                    // always on top

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
    }

    // ── Dot style ──────────────────────────────────────────────────────────────

    private void BuildDot()
    {
        glowTex   = GenerateRadialGradient(TexSize, innerColor, outerColor);
        glowImage = CreateImage("Glow", glowTex, dotRadius * 2f, dotRadius * 2f);
    }

    // ── Star style ─────────────────────────────────────────────────────────────

    private void BuildStar()
    {
        // Layer 1: soft glow blob behind the star
        glowTex   = GenerateRadialGradient(TexSize,
                        new Color(innerColor.r, innerColor.g, innerColor.b, 0.55f),
                        outerColor);
        glowImage = CreateImage("Glow", glowTex, glowRadius * 2f, glowRadius * 2f);

        // Layer 2: 4-point star spikes on top
        starTex   = GenerateStarTexture(TexSize, innerColor, outerColor, starLength, starWidth);
        starImage = CreateImage("Star", starTex, (float)TexSize, (float)TexSize);
    }

    // ── Image helper ───────────────────────────────────────────────────────────

    private RawImage CreateImage(string goName, Texture2D tex, float w, float h)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(canvas.transform, false);

        RawImage img = go.AddComponent<RawImage>();
        img.texture  = tex;
        img.color    = Color.white;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;          // exact screen center
        rt.sizeDelta = new Vector2(w, h);

        return img;
    }

    // ── Texture generators ─────────────────────────────────────────────────────

    /// Smooth radial gradient — bright center fading to transparent edge.
    private static Texture2D GenerateRadialGradient(int size, Color center, Color edge)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;

        float half = size * 0.5f;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;   // –1 … +1
                float dy = (y - half) / half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy); // 0 = center, 1 = edge

                // Smooth falloff: use smoothstep so glow looks soft, not harsh
                float t = Mathf.Clamp01(dist);
                t = t * t * (3f - 2f * t);          // smoothstep

                pixels[y * size + x] = Color.Lerp(center, edge, t);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    /// 4-point star: two pairs of overlapping spike gradients.
    private static Texture2D GenerateStarTexture(int size, Color center, Color edge,
                                                  float spikeLen, float spikeW)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;

        float half   = size * 0.5f;
        float lenN   = spikeLen / half;   // normalised spike length  (0…1 from center)
        float widN   = spikeW   / half;   // normalised spike half-width

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;

                // Sample the 4 spikes (horizontal + vertical axes)
                float alpha = 0f;

                // Horizontal spike (left & right)
                alpha = Mathf.Max(alpha, SpikeAlpha(dx, dy, lenN, widN));
                // Vertical spike (up & down)  — rotate coords 90°
                alpha = Mathf.Max(alpha, SpikeAlpha(dy, dx, lenN, widN));

                // Tiny bright dot at very centre to mimic laser core
                float coreDist = Mathf.Sqrt(dx * dx + dy * dy);
                float core = Mathf.Clamp01(1f - coreDist / (widN * 0.6f));
                core = core * core;
                alpha = Mathf.Max(alpha, core);

                Color c = Color.Lerp(edge, center, alpha);
                c.a = alpha;
                pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    /// Returns 0-1 intensity of a single spike along the +/- primary axis.
    /// primaryAxis = the axis the spike runs along (e.g. dx for horizontal).
    /// crossAxis   = the perpendicular axis (e.g. dy for horizontal).
    private static float SpikeAlpha(float primaryAxis, float crossAxis,
                                    float spikeLen, float spikeHalfWidth)
    {
        float absPrimary = Mathf.Abs(primaryAxis);
        if (absPrimary > spikeLen) return 0f;

        // Width of the spike narrows linearly toward the tip
        float allowedWidth = spikeHalfWidth * (1f - absPrimary / spikeLen);
        float absCross     = Mathf.Abs(crossAxis);

        if (absCross > allowedWidth) return 0f;

        // Intensity: bright at center of spike, fades toward edges and tip
        float along = 1f - absPrimary / spikeLen;        // 1 at center, 0 at tip
        float across = 1f - absCross / allowedWidth;     // 1 on axis, 0 at edge

        float intensity = along * across;
        // Smooth it
        intensity = intensity * intensity * (3f - 2f * intensity);

        return Mathf.Clamp01(intensity);
    }
}
