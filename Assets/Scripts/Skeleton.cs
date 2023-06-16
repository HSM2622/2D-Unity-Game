using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour, IObject
{
    public int maxHealth; // 最大HP
    public int currentHealth; // 現在のHP
    public float bsaCurTime;
    public float bsaCoolTime;
    public int maxSpeed;
    public int damage;
    public int direction;
    public int shield;
    public int hasPlayerTag;
    public float currentAnimTime;
    public Vector2 detectSize; 
    public Vector2 attack1Size; 
    public Vector2 attack2Size; 
    public Transform detect;
    public Transform attack1;
    public Transform attack2;
    public GameManager GameManager;
    Rigidbody2D rigid;
    Animator Animator;
    SpriteRenderer SpriteRenderer;
    CapsuleCollider2D CapsuleCollider2D;
    AnimatorStateInfo animStateInfo;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        CapsuleCollider2D = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void FixedUpdate(){
        
        //アニメーション確認
        animStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        currentAnimTime = animStateInfo.normalizedTime * animStateInfo.length;


        //感知範囲
        Collider2D[] detectColliders = Physics2D.OverlapBoxAll(detect.position, detectSize, 0);
        Collider2D[] attack2Colliders = Physics2D.OverlapBoxAll(attack2.position, attack2Size, 0);
        hasPlayerTag = 0;

        foreach (Collider2D collider in detectColliders){
            if (collider.CompareTag("Player")){
                hasPlayerTag = 1;
                foreach (Collider2D collider2 in attack2Colliders){
                    if (collider2.CompareTag("Player")){
                        hasPlayerTag = 11;
                        if (bsaCurTime <= 0){
                            Stop();
                            int randomValue = Random.Range(0, 2);
                            if (randomValue == 0)
                            Animator.SetTrigger("onAttack2");
                            else 
                            Animator.SetTrigger("onAttack1");
                            bsaCurTime = bsaCoolTime;
                        } else {
                            if (bsaCurTime > 0.5f)
                                Animator.SetBool("onShield", true);
                            bsaCurTime -= Time.deltaTime;
                        }
                    }
                }
                Vector3 dir = collider.transform.position - transform.position;
                dir.Normalize();
                if (dir.x < 0 && dir.x >= -1 && (!SpriteRenderer.flipX || direction == 0))
                    Turn(true);
                else if (dir.x > 0 && dir.x <= 1 && (SpriteRenderer.flipX || direction == 0))
                    Turn(false);
                break;
            }
        }

        //攻撃アニメーション
        if (animStateInfo.IsName("Attack1") || animStateInfo.IsName("Attack2")){
            if (currentAnimTime >= 0.6 && currentAnimTime <= animStateInfo.length)
                if (animStateInfo.IsName("Attack1"))
                    OnAttack(attack1, attack1Size);
                else if (animStateInfo.IsName("Attack2"))
                    OnAttack(attack2, attack2Size);
        }
        //防御アニメーション
        if (animStateInfo.IsName("Shield")){
            if (hasPlayerTag != 11 || bsaCurTime <= 0.5f)
                Animator.SetBool("onShield", false);
            else
                shield = 15;
        } else shield = 0;

        //Move
        if ((animStateInfo.IsName("Walk") || animStateInfo.IsName("Idle")) && hasPlayerTag == 1){
            Animator.SetInteger("WalkSpeed", 1);
            Vector2 newVelocity = new Vector2(direction * maxSpeed, rigid.velocity.y);
            if (newVelocity.magnitude > maxSpeed){
                newVelocity = newVelocity.normalized * maxSpeed;
            }
            rigid.velocity = newVelocity;
        } else
            Animator.SetInteger("WalkSpeed", 0);

        
        //Platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + direction, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0,1,0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));
        if (rayHit.collider == null) {
            rigid.velocity = Vector2.zero;
        }
    }

    public void CollisionRes(Vector2 targetPos, GameObject gameObject){}
    
    public void Turn(bool turn) {
        direction = turn ? -1 : 1;
        SpriteRenderer.flipX = turn;
        Vector2 colliderOffset = CapsuleCollider2D.offset;
        Vector3 pos1 = attack1.localPosition;
        Vector3 pos2 = attack2.localPosition;
        colliderOffset.x *= -1f;
        CapsuleCollider2D.offset = colliderOffset;
        pos1.x *= -1;
        pos2.x *= -1;
        attack1.localPosition = pos1;
        attack2.localPosition = pos2;
    }


    //攻撃
    public void OnAttack(Transform kind, Vector2 size){
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(kind.position, size, 0); 
        foreach (Collider2D collider in collider2Ds){
            if (collider.gameObject.layer == 7){
                collider.GetComponent<Player>().OnDamaged(transform.position, damage);
            }
        }
    }

    //範囲Debug
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(attack1.position, attack1Size);    
        Gizmos.DrawWireCube(attack2.position, attack2Size);    
        Gizmos.DrawWireCube(detect.position, detectSize);    
    }

    public void Stop(){
        rigid.velocity = Vector2.zero;
        Animator.SetInteger("WalkSpeed", 0);
    }


    public void OnDamaged (Vector2 targetPos, int damageAmount) {
        if (gameObject.layer == 3)
        gameObject.layer = 9;
        currentHealth -= (damageAmount - shield);
        if (currentHealth <= 0){
            Die();
            return;
        }
        //Layer : EnemyOnDamaged
        //reaction
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 4, ForceMode2D.Impulse);
        Animator.SetTrigger("onDamaged");
        SpriteRenderer.color = new Color(1, 1, 1, 0.4f);


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
