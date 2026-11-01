using System.Collections;
using UnityEngine;

// Put this on the Player. Handles dying (turns into a roasted drumstick) and coming back at the last checkpoint.
public class PlayerLife : MonoBehaviour {
    [Header("Death look")]
    public Sprite deadSprite;          // drag drumstick_cute here
    public float deadSize = 1.2f;      // how big the drumstick is, in world units
    public float deathPause = 1.2f;    // seconds the drumstick stays before the chicken comes back

    [Header("Falling")]
    public float fallLimit = -30f;     // if the chicken falls below this height it counts as dead

    public bool IsAlive { get; private set; } = true;
    public Vector3 RespawnPoint { get; private set; }
    public event System.Action Respawned;    // other scripts (eggs, HUD) can listen to this

    SpriteRenderer sr;
    Rigidbody2D body;
    PlayerMovement movement;
    Sprite aliveSprite;
    Vector3 aliveScale;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        aliveSprite = sr.sprite;
        aliveScale = transform.localScale;
    }

    void Start() {
        // the start point is simply wherever you placed the chicken in the scene
        RespawnPoint = transform.position;
    }

    void Update() {
        if (IsAlive && transform.position.y < fallLimit) Die();
    }

    public void SetCheckpoint(Vector3 position) {
        RespawnPoint = position;
    }

    public void Die() {
        if (!IsAlive) return;
        IsAlive = false;

        if (movement != null) movement.enabled = false;
        body.linearVelocity = Vector2.zero;
        body.simulated = false;            // freeze in place: no gravity, no collisions

        if (deadSprite != null) {
            sr.sprite = deadSprite;
            float s = deadSize / deadSprite.bounds.size.x;
            transform.localScale = new Vector3(s, s, 1f);
        }
        StartCoroutine(ComeBack());
    }

    IEnumerator ComeBack() {
        yield return new WaitForSeconds(deathPause);

        transform.position = RespawnPoint;
        sr.sprite = aliveSprite;
        transform.localScale = aliveScale;
        body.simulated = true;
        body.linearVelocity = Vector2.zero;
        if (movement != null) movement.enabled = true;
        IsAlive = true;

        if (Respawned != null) Respawned();
    }
}