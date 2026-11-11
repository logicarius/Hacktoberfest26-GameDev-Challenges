using UnityEngine;

// Title screen drawn with plain OnGUI (no Canvas or UI package needed).
// Put this on an empty GameObject. The game is paused until Play is pressed.
// Same night-sky look as the game; everything fades in when the screen appears and fades out when you press Play.
public class TitleScreen : MonoBehaviour {
    public string title = "CHICKEN CLIMB";
    public string subtitle = "Climb the stairs. Don't get roasted.";
    public Sprite chickenSprite;                                   // drag the chicken sprite here

    static readonly Color Top = new Color(0.06f, 0.04f, 0.20f, 1f);
    static readonly Color Bottom = new Color(0.24f, 0.12f, 0.50f, 1f);
    static readonly Color Gold = new Color(1f, 0.82f, 0.22f, 1f);
    static readonly Color Pink = new Color(1f, 0.42f, 0.70f, 1f);
    static readonly Color Ink = new Color(0.04f, 0.03f, 0.10f, 1f);

    bool showing, starting;
    float shownAt, startAt;
    GUIStyle style;

    void Awake() { Show(); }

    void Show() { showing = true; starting = false; shownAt = Time.unscaledTime; Time.timeScale = 0f; }

    void Begin() {
        if (starting) return;
        starting = true;
        startAt = Time.unscaledTime;
    }

    void Update() {
        if (!showing) {
            if (Input.GetKeyDown(KeyCode.Escape)) Show();
            return;
        }
        if (!starting && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))) Begin();
        if (starting && Time.unscaledTime - startAt >= 0.4f) { showing = false; starting = false; Time.timeScale = 1f; }
    }

    // text with a dark shadow
    void Text(Rect r, string s, int size, Color c, TextAnchor anchor, bool bold, float a) {
        if (style == null) style = new GUIStyle(GUI.skin.label);
        style.fontSize = size;
        style.alignment = anchor;
        style.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        style.wordWrap = false;
        style.normal.textColor = new Color(c.r, c.g, c.b, c.a * a);
        GUI.Label(r, s, style);
    }

    void Fill(Rect r, Color c) { GUI.color = c; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white; }

    void OnGUI() {
        if (!showing) return;
        GUI.depth = -1000;                       // draw on top of everything else
        float sw = Screen.width, sh = Screen.height;
        float t = Time.unscaledTime;
        float fin = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - shownAt) / 0.8f));
        float fout = starting ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - startAt) / 0.4f)) : 0f;
        float a = fin * (1f - fout);

        // night sky gradient (opaque, fades away when you press Play so the game shows through)
        const int strips = 48;
        for (int i = 0; i < strips; i++) {
            Color c = Color.Lerp(Top, Bottom, i / (float)(strips - 1));
            c.a = 1f - fout;
            Fill(new Rect(0, i * sh / strips, sw, sh / strips + 1f), c);
        }

        // twinkling stars
        for (int i = 0; i < 70; i++) {
            float fx = Mathf.Repeat(Mathf.Sin(i * 12.9898f) * 43758.5453f, 1f);
            float fy = Mathf.Repeat(Mathf.Sin(i * 78.233f) * 12345.6789f, 1f);
            float s = Mathf.Max(2f, sh * (0.004f + 0.004f * (i % 3)));
            float tw = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(t * (0.8f + (i % 5) * 0.3f) + i));
            Fill(new Rect(fx * sw, fy * sh * 0.9f, s, s), new Color(1f, 1f, 1f, tw * (1f - fout)));
        }

        // chicken
        if (chickenSprite != null) {
            Texture2D tex = chickenSprite.texture;
            tex.filterMode = FilterMode.Point;       // sharp pixels
            Rect tr = chickenSprite.textureRect;
            Rect uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);
            float h = sh * 0.27f, w = h * tr.width / tr.height;
            float bob = Mathf.Sin(t * 3f) * sh * 0.012f;
            GUI.color = new Color(1f, 1f, 1f, a);
            GUI.DrawTextureWithTexCoords(new Rect((sw - w) / 2f, sh * 0.08f + bob, w, h), tex, uv);
            GUI.color = Color.white;
        }

        Text(new Rect(0, sh * 0.37f, sw, sh * 0.14f), title, Mathf.RoundToInt(sh * 0.10f), Gold, TextAnchor.MiddleCenter, true, a);
        Text(new Rect(0, sh * 0.52f, sw, sh * 0.06f), subtitle, Mathf.RoundToInt(sh * 0.035f), new Color(0.85f, 0.88f, 1f, 1f), TextAnchor.MiddleCenter, false, a);

        // PLAY button: pink with a dark outline, lights up on hover
        float bw = sh * 0.42f, bh = sh * 0.10f;
        Rect br = new Rect((sw - bw) / 2f, sh * 0.63f, bw, bh);
        bool hover = br.Contains(Event.current.mousePosition);
        float o = Mathf.Max(3f, sh * 0.006f);
        Fill(new Rect(br.x - o, br.y - o, br.width + 2f * o, br.height + 2f * o), new Color(Ink.r, Ink.g, Ink.b, a));
        Fill(br, Color.Lerp(Pink, Color.white, hover ? 0.25f : 0f) * new Color(1f, 1f, 1f, a));
        Fill(new Rect(br.x, br.y, br.width, br.height * 0.18f), new Color(1f, 1f, 1f, 0.35f * a));
        Text(br, "PLAY", Mathf.RoundToInt(sh * 0.055f), Ink, TextAnchor.MiddleCenter, true, a * 0.0f + a);
        if (GUI.Button(br, GUIContent.none, GUIStyle.none)) Begin();

        Text(new Rect(0, sh * 0.755f, sw, sh * 0.04f), "or press Enter", Mathf.RoundToInt(sh * 0.026f), new Color(0.8f, 0.82f, 1f, 1f), TextAnchor.UpperCenter, false, a);

        int small = Mathf.RoundToInt(sh * 0.03f);
        Text(new Rect(0, sh * 0.82f, sw, sh * 0.05f), "WASD or Arrow keys: move, jump, drop down", small, Color.white, TextAnchor.UpperCenter, false, a);
        Text(new Rect(0, sh * 0.87f, sw, sh * 0.05f), "Space: super jump        Esc: back to this screen", small, Color.white, TextAnchor.UpperCenter, false, a);

        Color[] bands = { new Color(1f,0.8f,0.2f), new Color(1f,0.42f,0.7f), new Color(0.62f,0.92f,0.28f), new Color(0.3f,0.92f,0.88f), new Color(0.7f,0.52f,1f), new Color(0.95f,0.35f,0.9f), new Color(0.92f,0.95f,1f) };
        for (int b = 0; b < bands.Length; b++) {
            Color c = bands[b]; c.a = 1f - fout;
            Fill(new Rect(b * sw / bands.Length, sh * 0.975f, sw / bands.Length + 1f, sh * 0.025f), c);
        }
    }
}