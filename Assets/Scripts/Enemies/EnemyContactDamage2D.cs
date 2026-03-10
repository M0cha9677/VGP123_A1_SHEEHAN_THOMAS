using UnityEngine;

[RequireComponent(typeof(BaseEnemy))]
public class EnemyContactDamage2D : MonoBehaviour
{
    private BaseEnemy _enemyBase;

    private void Awake()
    {
        _enemyBase = GetComponent<BaseEnemy>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerRecieveDamage receiver = collision.collider.GetComponent<PlayerRecieveDamage>();
        if (receiver != null)
            receiver.ApplyDamage(_enemyBase.ContactDamage);
    }
}
