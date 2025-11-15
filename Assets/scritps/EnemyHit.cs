using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerBox"))
        {
            var health = other.GetComponentInParent<PlayerHealth>();
            if (health != null) health.ApplyDamage(damage);
        }
    }
}
