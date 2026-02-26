using UnityEngine;

public class DamageOnCollision : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var receiver = collision.collider.GetComponent<PlayerRecieveDamage>();
        if (receiver != null)
            receiver.ApplyDamage(damage);
    }
}
