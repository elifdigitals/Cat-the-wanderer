using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    public int damage = 3;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyBox"))
        {
            var health = other.GetComponentInParent<EnemyHealth>();
            if (health != null) health.ApplyDamage(damage);
        }
    }
}
