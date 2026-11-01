using UnityEngine;

// Put this on the Main Camera. It follows the chicken smoothly.
public class CameraFollow : MonoBehaviour {
    public Transform target;                         // leave empty: it finds the chicken by itself
    public Vector2 offset = new Vector2(3f, 2f);     // look a little ahead and above the chicken
    public float smoothness = 5f;

    void Start() {
        if (target == null) {
            PlayerMovement p = FindFirstObjectByType<PlayerMovement>();
            if (p != null) target = p.transform;
        }
        if (target != null) {
            transform.position = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        }
    }

    void LateUpdate() {
        if (target == null) return;
        Vector3 goal = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, goal, Mathf.Min(1f, smoothness * Time.deltaTime));
    }
}