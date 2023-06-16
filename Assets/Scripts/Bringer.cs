using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Bringer : MonoBehaviour, IObject
{
    float bsaCurTime;
    float healthPercentage;
    public int maxHealth; // 最大HP
    public int currentHealth; // 現在のHP
    public float bsaCoolTime;
    public float maxSpeed;
    public int damage;
    public int direction;
    public int hasPlayerTag;
    public float spellCastingCool;
    public float currentAnimTime;
    public float currentSpellAnimTime;
    public float spellCastingCurCool;
    public Image bossHpBar;
    public Image bossCurrentHpBar;
    public TextMeshProUGUI bossName;
    public Vector2 detectSize; 
    public Vector2 attackSize; 
    public Transform detect;
    public Transform attack;
    public GameManager GameManager;
    public GameObject spell;
    public GameObject finish;
    public Vector2 spellBoxSize; 
    Rigidbody2D rigid;
    Animator Animator;
    SpriteRenderer SpriteRenderer;
    CapsuleCollider2D CapsuleCollider2D;
    AnimatorStateInfo animStateInfo;
    Animator spellAnimator;
    AnimatorStateInfo spellAnimStateInfo;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        spellAnimator = spell.GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        CapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        bossHpBar.gameObject.SetActive(true);
        bossName.text = gameObject.name;
        
    }

    // Update is called once per frame
    void FixedUpdate(){
        
        //アニメーション確認
        animStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        currentAnimTime = animStateInfo.normalizedTime * animStateInfo.length;
        if (spell.activeSelf){
        spellAnimStateInfo = spellAnimator.GetCurrentAnimatorStateInfo(0);
        currentSpellAnimTime = spellAnimStateInfo.normalizedTime * spellAnimStateInfo.length;
        }


        //感知範囲
        Collider2D[] detectColliders = Physics2D.OverlapBoxAll(detect.position, detectSize, 0);
        Collider2D[] attackColliders = Physics2D.OverlapBoxAll(attack.position, attackSize, 0);
        hasPlayerTag = 0;


        //Spellアニメーション
        if (spell.activeSelf && spellAnimStateInfo.IsName("Spell")){
            if (currentSpellAnimTime >= 0.8f && currentSpellAnimTime < spellAnimStateInfo.length)
                OnAttack(spell.transform, spellBoxSize);
        } else if (spellAnimStateInfo.IsName("None") && spell.activeSelf){
            spell.SetActive(false);
        }
    

        foreach (Collider2D collider in detectColliders){
            if (collider.CompareTag("Player")){
                hasPlayerTag = 1;
                foreach (Collider2D collider2 in attackColliders){
                    if (collider2.CompareTag("Player")){
                        hasPlayerTag = 11;
                        if (bsaCurTime <= 0){
                            Stop();
                            Animator.SetTrigger("onAttack");
                            bsaCurTime = bsaCoolTime;
                        }
                        }
                    }
                bsaCurTime -= Time.deltaTime;
                if (spellCastingCurCool <= 0){
                    Stop();
                    Animator.SetTrigger("onCast");
                    spellCastingCurCool = spellCastingCool;
                } else 
                    spellCastingCurCool -= Time.deltaTime;
                if (animStateInfo.IsName("Cast") && currentAnimTime <= animStateInfo.length && currentAnimTime >= 0.8f){
                    spell.transform.position = new Vector2(collider.transform.position.x + 0.03f, collider.transform.position.y + 0.9f);
                    spell.SetActive(true);
                }
                Vector3 dir = collider.transform.position - transform.position;
                dir.Normalize();
                if (dir.x < 0 && dir.x >= -1 && SpriteRenderer.flipX)
                    Turn(false);
                else if (dir.x > 0 && dir.x <= 1 && !SpriteRenderer.flipX)
                    Turn(true);
                break;
            }
        }

         if (!animStateInfo.IsName("Vanish") && hasPlayerTag == 0){
            Animator.SetTrigger("onVanish");
        }

        //攻撃アニメーション
        if (animStateInfo.IsName("Attack")){
            if (currentAnimTime >= 0.4f && currentAnimTime <= 0.7f)
                OnAttack(attack, attackSize);
        }

        //Vanishアニメーション (消えるスキル)
        if (animStateInfo.IsName("Vanish")){
            if (currentAnimTime >= 0.8f && currentAnimTime <= 0.85f){
                Vector3 newPos;
                if (GameManager.player.SpriteRenderer.flipX)
                newPos = new Vector3(GameManager.player.transform.position.x - 3f, GameManager.player.transform.position.y - 1.98f);
                else
                newPos = new Vector3(GameManager.player.transform.position.x + 3f, GameManager.player.transform.position.y - 1.98f);
                gameObject.transform.localPosition = newPos;
            }
        }

        //Move
        if ((animStateInfo.IsName("Walk") || animStateInfo.IsName("Idle")) && hasPlayerTag == 1 && spellCastingCurCool > 0){
            Animator.SetInteger("WalkSpeed", 1);
            Vector2 newVelocity = new Vector2(direction * maxSpeed, rigid.velocity.y);
            if (newVelocity.magnitude > maxSpeed){
                newVelocity = newVelocity.normalized * maxSpeed;
            }
            rigid.velocity = newVelocity;
        } else{
            Animator.SetInteger("WalkSpeed", 0);
        }

        
        //Platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + direction, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0,1,0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 10, LayerMask.GetMask("Platform"));
        if (rayHit.collider == null) {
            rigid.velocity = Vector2.zero;
        }
    }

    public void CollisionRes(Vector2 targetPos, GameObject gameObject){}
    
    public void Turn(bool turn) {
        direction = turn ? 1 : -1;
        SpriteRenderer.flipX = turn;
        Vector2 colliderOffset = CapsuleCollider2D.offset;
        Vector3 pos1 = attack.localPosition;
        colliderOffset.x *= -1;
        pos1.x *= -1;
        CapsuleCollider2D.offset = colliderOffset;
        attack.localPosition = pos1;
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
        Gizmos.DrawWireCube(attack.position, attackSize);    
        Gizmos.DrawWireCube(detect.position, detectSize);    
        Gizmos.DrawWireCube(spell.transform.position, spellBoxSize);    
    }

    public void Stop(){
        rigid.velocity = Vector2.zero;
        Animator.SetInteger("WalkSpeed", 0);
    }


    public void OnDamaged (Vector2 targetPos, int damageAmount) {
        if (gameObject.layer == 3)
        currentHealth -= damageAmount;
        gameObject.layer = 9;
        healthPercentage = (float) currentHealth / maxHealth;
        bossCurrentHpBar.fillAmount = healthPercentage;
        if (currentHealth <= 0){
            Die();
            return;
        }
        //Layer : EnemyOnDamaged
        //reaction
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 2, ForceMode2D.Impulse);
        if (!animStateInfo.IsName("Attack"))
        Animator.SetTrigger("onDamaged");
        SpriteRenderer.color = new Color(1, 1, 1, 0.4f);

        Invoke("OffDamaged", 0.3f);
    }

    public void OffDamaged(){
        gameObject.layer = 3;
        SpriteRenderer.color = new Color(1,1,1,1);
    }
    
    public void Die(){
        StartCoroutine(GameManager.FadeOutAndDestroy(gameObject, Animator, SpriteRenderer));
        finish.SetActive(true);
        bossHpBar.gameObject.SetActive(false);
    }
}
