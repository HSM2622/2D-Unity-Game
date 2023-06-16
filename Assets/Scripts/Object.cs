using UnityEngine;

public interface IObject
{
    void OnDamaged (Vector2 targetPos, int damageAmount);
    void OffDamaged();
    void Die();
    void OnAttack(Transform kind, Vector2 size);
    void Stop();
    void Turn(bool turn);
    void CollisionRes(Vector2 targetPos, GameObject gameObject){}
}