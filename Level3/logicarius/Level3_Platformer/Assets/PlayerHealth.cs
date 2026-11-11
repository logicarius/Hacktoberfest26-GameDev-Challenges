using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// Put this on the Player. Hearts, rocks scatter ahead when you burn, win at the golden egg,
// game over after the last heart. Both endings go back to the title screen.
public class PlayerHealth : MonoBehaviour {
    [Header("Hearts")]
    public int maxHearts = 3;
    public Sprite heartSprite;             // the heart sprite

    [Header("Rocks lost when you die")]
    public GameObject rockPrefab;          // the rock prefab (Assets/Prefabs/rock_0)
    public float minAhead = 3f;            // new rocks appear this far ahead of your respawn point...
    public float maxAhead = 14f;           // ...up to this far
    public float rockLift = 0.4f;          // how high above the ground a rock floats

    [Header("Ending")]
    public float endDelay = 8f;            // seconds the WIN / GAME OVER screen stays (Enter skips it)
    public int winBonus = 100;

    public int Hearts { get; private set; }

    PlayerLife life;
    PlayerStats stats;
    bool gameOver, won, restarting;
    float showAt, exitAt, winTime;
    GUIStyle bigStyle, smallStyle, hudStyle;
    Sprite rockIcon;

    void Awake() {
        life = GetComponent<PlayerLife>();
        stats = GetComponent<PlayerStats>();
        Hearts = maxHearts;
    }

    void Start() { life.Died += OnDied; }
    void OnDestroy() { if (life != null) life.Died -= OnDied; }

    public bool TryHeal() {
        if (Hearts >= maxHearts) return false;
        Hearts++;
        return true;
    }

    void OnDied() {
        Hearts = Mathf.Max(0, Hearts - 1);
        int lost = (stats != null) ? stats.LoseAllRocks() : 0;

        if (Hearts <= 0) {
            gameOver = true;
            life.BlockRespawn = true;                       // no coming back after the last heart
            showAt = Time.unscaledTime + life.deathPause;   // show the screen after the drumstick moment
            exitAt = showAt + endDelay;
            return;
        }
        ScatterRocks(lost);
    }

    // called by the golden egg
    public void Win() {
        if (won || gameOver) return;
        won = true;
        life.Invulnerable = true;
        life.BlockRespawn = true;
        PlayerMovement mv = GetComponent<PlayerMovement>();
        if (mv != null) mv.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (stats != null) stats.AddBonus(winBonus);
        winTime = Time.timeSinceLevelLoad;
        showAt = Time.unscaledTime + 0.6f;
        exitAt = showAt + endDelay;
    }

    void Update() {
        if ((!gameOver && !won) || restarting) return;
        bool skip = Time.unscaledTime >= showAt && Input.GetKeyDown(KeyCode.Return);
        if (skip || Time.unscaledTime >= exitAt) Restart();
    }

    void Restart() {
        restarting = true;
        Time.timeScale = 1f;
#if UNITY_EDITOR
        EditorSceneManager.LoadSceneInPlayMode(SceneManager.GetActiveScene().path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
    }

    // ---------- rocks ----------
    void ScatterRocks(int count) {
        if (count <= 0) return;
        if (rockPrefab == null) { Debug.LogWarning("PlayerHealth: drag the rock prefab into Rock Prefab"); return; }
        if (ArenaBuilder.Current != null) { ScatterInArena(count); return; }

        Vector3 from = life.RespawnPoint;
        int placed = 0;
        for (int tries = 0; tries < 80 && placed < count; tries++) {
            float x = from.x + Random.Range(minAhead, maxAhead);
            Vector2 spot;
            if (FindSpot(x, from.y, out spot)) {
                Instantiate(rockPrefab, spot, Quaternion.identity);
                placed++;
            }
        }
        if (placed < count) Debug.LogWarning("PlayerHealth: could only place " + placed + " of " + count + " rocks");
    }

    // in the generated arena: pick free spots ahead of you on the same floor
    void ScatterInArena(int count) {
        Vector3 from = life.RespawnPoint;
        int placed = 0;
        for (int i = 0; i < count; i++) {
            Vector2 spot;
            if (ArenaBuilder.Current.TryPickSpotAhead(from.x, from.y, minAhead, maxAhead, rockLift, out spot)) {
                Instantiate(rockPrefab, spot, Quaternion.identity);
                placed++;
            }
        }
        if (placed < count) Debug.LogWarning("PlayerHealth: could only place " + placed + " of " + count + " rocks");
    }

    // look straight down at this x for ground; reject spots on fire or on top of another pickup
    bool FindSpot(float x, float fromY, out Vector2 spot) {
        spot = Vector2.zero;
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, fromY + 20f), Vector2.down, 60f);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider == null || hit.collider.isTrigger) continue;
            if (hit.collider.GetComponentInParent<PlayerLife>() != null) continue;
            if (hit.collider.GetComponent<Egg>() != null) continue;

            Vector2 p = new Vector2(x, hit.point.y + rockLift);
            foreach (Collider2D c in Physics2D.OverlapCircleAll(p, 0.7f)) {
                if (c.GetComponent<Hazard>() != null || c.GetComponent<Collectible>() != null) return false;
            }
            spot = p;
            return true;
        }
        return false;
    }

    // ---------- drawing ----------
    void DrawSprite(Sprite s, Rect r) {
        Texture2D tex = s.texture;
        tex.filterMode = FilterMode.Point;           // sharp pixels
        Rect tr = s.textureRect;
        Rect uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);
        GUI.DrawTextureWithTexCoords(r, tex, uv);
    }

    static string Clock(float seconds) {
        int s = Mathf.FloorToInt(seconds);
        return (s / 60) + ":" + (s % 60).ToString("00");
    }

    // ---------- HUD: one dark panel top-left with hearts, rocks, score and time ----------
    Sprite RockIcon() {
        if (rockIcon == null && rockPrefab != null) {
            SpriteRenderer sr = rockPrefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) rockIcon = sr.sprite;
        }
        return rockIcon;
    }

    void Label(Rect r, string text, int size, TextAnchor anchor, Color c) {
        if (hudStyle == null) hudStyle = new GUIStyle(GUI.skin.label);
        hudStyle.fontSize = size;
        hudStyle.alignment = anchor;
        hudStyle.fontStyle = FontStyle.Bold;
        hudStyle.normal.textColor = new Color(0f, 0f, 0f, 0.8f);          // shadow
        GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), text, hudStyle);
        hudStyle.normal.textColor = c;
        GUI.Label(r, text, hudStyle);
    }

    void DrawHud(float sw, float sh) {
        float u = Mathf.Max(24f, sh * 0.05f);                 // one HUD unit
        float pad = u * 0.3f, w = u * 8.2f, h = u * 3.4f;
        Rect panel = new Rect(u * 0.4f, u * 0.4f, w, h);

        GUI.color = new Color(0.03f, 0.02f, 0.10f, 0.82f);    // border + dark fill
        GUI.DrawTexture(new Rect(panel.x - 3f, panel.y - 3f, panel.width + 6f, panel.height + 6f), Texture2D.whiteTexture);
        GUI.color = new Color(0.12f, 0.09f, 0.28f, 0.95f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = Color.white;

        float x = panel.x + pad, y = panel.y + pad;
        int fs = Mathf.RoundToInt(u * 0.7f);

        // hearts row
        if (heartSprite != null) {
            for (int i = 0; i < maxHearts; i++) {
                GUI.color = (i < Hearts) ? Color.white : new Color(0f, 0f, 0f, 0.55f);
                DrawSprite(heartSprite, new Rect(x + i * (u * 1.05f), y, u, u));
            }
            GUI.color = Color.white;
        } else {
            Label(new Rect(x, y, u * 6f, u), "Hearts " + Hearts + "/" + maxHearts, fs, TextAnchor.MiddleLeft, Color.white);
        }

        // time, right side of the first row
        float shown = won ? winTime : Time.timeSinceLevelLoad;
        Label(new Rect(panel.xMax - pad - u * 3.2f, y, u * 3.2f, u), Clock(shown), fs, TextAnchor.MiddleRight, new Color(0.7f, 0.95f, 1f, 1f));

        // rocks row: three slots, filled as you pick them up
        int max = (stats != null) ? stats.maxRocks : 3, have = (stats != null) ? stats.Rocks : 0;
        float y2 = y + u * 1.15f;
        Sprite rs = RockIcon();
        for (int i = 0; i < max; i++) {
            Rect slot = new Rect(x + i * (u * 1.05f), y2, u, u);
            GUI.color = new Color(1f, 1f, 1f, 0.12f);
            GUI.DrawTexture(slot, Texture2D.whiteTexture);
            if (i < have && rs != null) { GUI.color = Color.white; DrawSprite(rs, slot); }
            else if (i < have) { GUI.color = new Color(0.8f, 0.8f, 0.85f, 1f); GUI.DrawTexture(new Rect(slot.x + u * 0.2f, slot.y + u * 0.2f, u * 0.6f, u * 0.6f), Texture2D.whiteTexture); }
        }
        GUI.color = Color.white;
        bool full = have >= max;
        Label(new Rect(x + max * (u * 1.05f) + pad, y2, u * 4f, u), full ? "FULL: eat worms!" : "Rocks " + have + "/" + max, Mathf.RoundToInt(u * 0.55f), TextAnchor.MiddleLeft, full ? new Color(1f, 0.9f, 0.3f, 1f) : Color.white);

        Label(new Rect(panel.x, panel.yMax + 4f, w, u * 0.7f), "S / Down: drop through steps", Mathf.RoundToInt(u * 0.45f), TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.75f));

        // score
        int score = (stats != null) ? stats.Score : 0;
        Label(new Rect(x, y2 + u * 1.15f, w - pad * 2f, u * 0.9f), "SCORE  " + score, Mathf.RoundToInt(u * 0.8f), TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.25f, 1f));
    }
    void OnGUI() {
        GUI.depth = -500;                                  // the title screen (-1000) stays on top
        float sw = Screen.width, sh = Screen.height;

        DrawHud(sw, sh);


        bool ending = (gameOver || won) && Time.unscaledTime >= showAt;
        if (!ending) return;

        if (bigStyle == null) {
            bigStyle = new GUIStyle(GUI.skin.label);
            bigStyle.alignment = TextAnchor.MiddleCenter;
            bigStyle.fontStyle = FontStyle.Bold;
            smallStyle = new GUIStyle(GUI.skin.label);
            smallStyle.alignment = TextAnchor.MiddleCenter;
        }
        bigStyle.fontSize = Mathf.RoundToInt(sh * 0.12f);
        bigStyle.normal.textColor = won ? new Color(1f, 0.85f, 0.25f, 1f) : new Color(1f, 0.3f, 0.2f, 1f);
        smallStyle.fontSize = Mathf.RoundToInt(sh * 0.035f);
        smallStyle.normal.textColor = Color.white;

        GUI.color = won ? new Color(0.04f, 0.08f, 0.04f, 0.88f) : new Color(0.08f, 0.02f, 0.04f, 0.88f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        int score = (stats != null) ? stats.Score : 0;
        GUI.Label(new Rect(0, sh * 0.26f, sw, sh * 0.16f), won ? "YOU WIN!" : "GAME OVER", bigStyle);
        if (won) {
            GUI.Label(new Rect(0, sh * 0.45f, sw, sh * 0.06f), "You reached the golden egg!", smallStyle);
            GUI.Label(new Rect(0, sh * 0.52f, sw, sh * 0.06f), "Score: " + score + "      Time: " + Clock(winTime) + "      Hearts left: " + Hearts, smallStyle);
        } else {
            GUI.Label(new Rect(0, sh * 0.48f, sw, sh * 0.06f), "Score: " + score, smallStyle);
        }
        GUI.Label(new Rect(0, sh * 0.64f, sw, sh * 0.06f), "Back to the start screen...  (Enter to skip)", smallStyle);
    }
}