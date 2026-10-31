using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float speed = 5f;
    public Rigidbody2D body;

    [Header("Dash (Left Shift or Space)")]
    public float dashSpeed = 15f;
    public float dashTime = 0.2f;
    public float dashCooldown = 0.8f;

    [Header("Giant mode (press E to toggle)")]
    public float giantScale = 2f;

    float dashTimer;
    float cooldownTimer;
    Vector2 dashDirection;
    Vector2 lastDirection = Vector2.right;
    Vector3 normalScale;
    bool isGiant;

    void Start() {
        if (body == null) body = GetComponent<Rigidbody2D>();
        normalScale = transform.localScale;
    }

    void Update() {
        float xInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");
        Vector2 direction = new Vector2(xInput, yInput).normalized;

        // remember the last way we were facing so a dash works while standing still
        if (direction != Vector2.zero) lastDirection = direction;

        // Dash
        cooldownTimer -= Time.deltaTime;
        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space);
        if (dashPressed && cooldownTimer <= 0f && dashTimer <= 0f) {
            dashTimer = dashTime;
            cooldownTimer = dashCooldown;
            dashDirection = lastDirection;
        }

        if (dashTimer > 0f) {
            dashTimer -= Time.deltaTime;
            body.linearVelocity = dashDirection * dashSpeed;
        } else {
            body.linearVelocity = direction * speed;
        }

        // Giant mode
        if (Input.GetKeyDown(KeyCode.E)) {
            isGiant = !isGiant;
            transform.localScale = isGiant ? normalScale * giantScale : normalScale;
        }
    }
}