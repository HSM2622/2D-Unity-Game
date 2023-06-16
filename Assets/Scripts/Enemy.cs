using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IObject
{
    public int maxHealth = 30; // 最大HP
    public int currentHealth; // 現在のHP
    public int damage;
    Rigidbody2D rigid;
    Animator Animator;
    SpriteRenderer SpriteRenderer;
    BoxCollider2D BoxCollider2D;
    public GameManager GameManager;

    public int nextMove;
    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        BoxCollider2D = GetComponent<BoxCollider2D>();
        Ai();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        //Platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0,1,0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));
        if (rayHit.collider == null) {
            nextMove *= -1;
            bool boolean = nextMove == 1;
            Turn(boolean);
        }
    }

    void Ai() {
        //Set Next Active
        nextMove = Random.Range(-1, 2);

        Animator.SetInteger("WalkSpeed", nextMove);

        //Flip Sprite
        if (nextMove != 0)
            SpriteRenderer.flipX = nextMove == 1;

        float nextThink = Random.Range(2f, 5f);
        Invoke("Ai", nextThink);
    }

    public void Turn(bool turn) {
        SpriteRenderer.flipX = turn;
        CancelInvoke();
        Ai();
    }

    public void OnAttack(Transform kind, Vector2 size){}

    //ぶつかったらダメージを与える。
    public void CollisionRes(Vector2 targetPos, GameObject gameObject){
        gameObject.GetComponent<IObject>().OnDamaged(transform.position, damage);
    }

    public void Stop(){}


    public void OnDamaged (Vector2 targetPos, int damageAmount) {
        //Layer : EnemyOnDamaged
        gameObject.layer = 9;
        //reaction
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 4, ForceMode2D.Impulse);
        // Animator.SetTrigger("onDamaged");
        SpriteRenderer.color = new Color(1, 1, 1, 0.4f);
        currentHealth -= damageAmount;

        if (currentHealth <= 0){
            Die();
        }
        Invoke("OffDamaged", 0.5f);
    }

    public void OffDamaged(){
        gameObject.layer = 3;
        SpriteRenderer.color = new Color(1,1,1,1);
    }

    public void Die(){
        StartCoroutine(GameManager.FadeOutAndDestroy(gameObject, Animator, SpriteRenderer));
    }

}
