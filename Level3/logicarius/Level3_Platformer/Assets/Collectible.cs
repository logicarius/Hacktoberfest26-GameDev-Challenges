using UnityEngine;

public enum CollectKind { Rock, Worm }

// Put this on every rock and worm. It needs a Collider2D with Is Trigger ticked.
public class Collectible : MonoBehaviour {
    public CollectKind kind = CollectKind.Rock;
    public int points = 10;            // only used by worms
    bool taken;

    void OnTriggerEnter2D(Collider2D other) { TryCollect(other); }
    void OnTriggerStay2D(Collider2D other) { TryCollect(other); }

    void TryCollect(Collider2D other) {
        if (taken) return;
        PlayerLife life = other.GetComponentInParent<PlayerLife>();
        PlayerStats stats = other.GetComponentInParent<PlayerStats>();
        if (life == null || stats == null || !life.IsAlive) return;

        bool collected = (kind == CollectKind.Rock) ? stats.TryAddRock() : stats.TryEatWorm(points);
        if (collected) {
            taken = true;
            Destroy(gameObject);       // stays gone, even after you die and respawn
        }
    }
}