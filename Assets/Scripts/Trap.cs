using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour, IObject
{
    public int damage;
    
    public void OnDamaged (Vector2 targetPos, int damageAmount){}
    public void OffDamaged(){}
    public void Die(){}
    public void OnAttack(Transform kind, Vector2 size){}
    public void Stop(){}
    public void Turn(bool turn){}
    public void CollisionRes(Vector2 targetPos, GameObject gameObject){
        gameObject.GetComponent<IObject>().OnDamaged(transform.position, damage);
    }
}