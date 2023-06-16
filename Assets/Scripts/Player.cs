using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IObject{
    public float maxSpeed;
    public float jumpPower;
    public int damage;
    public float bsaCurTime;
    public float bsaCoolTime = 1.6f;
    public float slideCurTime;
    public float slideCoolTime = 1f;
    public GameManager GameManager;
    public Vector2 boxSize; 
    public Transform playerBasicAtk;
    public Sprite squareSprite;
    Rigidbody2D rigid;
    public SpriteRenderer SpriteRenderer;
    Animator Animator;
    CapsuleCollider2D CapsuleCollider2D;
    Sprite Square;
    AnimatorStateInfo animStateInfo;
    

    void Awake(){
        rigid = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
        CapsuleCollider2D = GetComponent<CapsuleCollider2D>();
    }

    private void Update() {

        animStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        if (animStateInfo.IsName("Death"))
            return;

        // Jump
        if (Input.GetButtonDown("Jump") && !Animator.GetBool("isJumping")) {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            Animator.SetBool("isJumping", true);
        }

        //攻撃アニメーション実行
        if (bsaCurTime <= 0){
            if (Input.GetButtonDown("Fire1")){
                Animator.SetTrigger("onAttack");
                bsaCurTime = bsaCoolTime;
            }
        } else {
                bsaCurTime -= Time.deltaTime;
        }

        //スライディング
        if (slideCurTime <= 0 && gameObject.layer != 8){
            if (Input.GetKeyDown(KeyCode.L)){
                animStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
                Vector2 slidePos = new Vector2 ((SpriteRenderer.flipX ? -1 : 1) * 10, 0); 
                rigid.AddForce(slidePos, ForceMode2D.Impulse);
                Animator.SetTrigger("onSlide");
                gameObject.layer = 8;
                SpriteRenderer.color = new Color(1, 1, 1, 0.4f);
                Invoke("OffDamaged", animStateInfo.length);
                slideCurTime = slideCoolTime;
            }
        } else {
                slideCurTime -= Time.deltaTime;
        }

        
        if (Input.GetButtonUp("Horizontal") && !Animator.GetBool("isJumping")){
            Stop();
        }

        //Direction Sprite
        if (Input.GetButton("Horizontal")){
                bool flipX = Input.GetAxisRaw("Horizontal") == -1;
                if (flipX != SpriteRenderer.flipX){
                    Turn(flipX);
                }
            }

        if (rigid.velocity.normalized.x == 0)
            Animator.SetBool("isRunning", false);
        else
            Animator.SetBool("isRunning", true);
    }

    void FixedUpdate(){

        //アニメーション確認
        animStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        if (animStateInfo.IsName("Death"))
            return;

        //공격 애니메이션
        if (animStateInfo.IsName("Attack")){
            float currentTime = animStateInfo.normalizedTime * animStateInfo.length;
            if (currentTime >= 0.5 && currentTime <= 0.8)
                OnAttack(playerBasicAtk, boxSize);
            if (currentTime >= 0.9)
                OnAttack(playerBasicAtk, boxSize);
        }

        //ダッシュ攻撃アニメーション
        if (animStateInfo.IsName("Dash-Attack")){
            float currentTime = animStateInfo.normalizedTime * animStateInfo.length;
            if (currentTime >= 0.3 && currentTime <= 0.5)
                OnAttack(playerBasicAtk, boxSize);
        }

        float h = Input.GetAxisRaw("Horizontal");

        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if (rigid.velocity.x > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        if (rigid.velocity.x < maxSpeed*(-1))
            rigid.velocity = new Vector2(maxSpeed*(-1), rigid.velocity.y);


        //landing platform
        if (rigid.velocity.y < 0){
        Debug.DrawRay(rigid.position, Vector3.down, new Color(0,1,0));
        RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
        if (rayHit.collider != null && Animator.GetBool("isJumping")) {
            if (rayHit.distance < 1.5f) //half of player size
                Animator.SetBool("isJumping", false);
        }
        }
    }

    public void Turn(bool turn){
        SpriteRenderer.flipX = turn;
        Vector2 colliderOffset = CapsuleCollider2D.offset;
        colliderOffset.x *= -1f;
        CapsuleCollider2D.offset = colliderOffset;
    }

    public void Stop(){
        rigid.velocity = Vector2.zero;
    }
    
    //殴られた時
    public void OnDamaged (Vector2 targetPos, int damage) {
        if (gameObject.layer == 7)
        GameManager.HealthDown(damage);
        //Layer : PlayerOnDamaged
        gameObject.layer = 8;
        //reaction
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 4, ForceMode2D.Impulse);
        Animator.SetTrigger("onDamaged");
        SpriteRenderer.color = new Color(1, 1, 1, 0.4f);
        Invoke("OffDamaged", 2f);
    }

    //無敵時間終了
    public void OffDamaged(){
        gameObject.layer = 7;
        SpriteRenderer.color = new Color(1,1,1,1);
    }

    public void Die(){
        StartCoroutine(GameManager.FadeOutAndDestroy(gameObject, Animator, SpriteRenderer));
    }

    public void CollisionRes(Vector2 targetPos, GameObject gameObject){}


    //攻撃
    public void OnAttack(Transform kind, Vector2 size){
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(kind.position, size, 0);
        Vector3 pos = kind.localPosition;
        pos.x = SpriteRenderer.flipX ? -Mathf.Abs(pos.x) : Mathf.Abs(pos.x);
        kind.localPosition = pos;
        foreach (Collider2D collider in collider2Ds){
            if (collider.gameObject.layer == 3){
                collider.GetComponent<IObject>().OnDamaged(transform.position, damage);
            }
        }
    }

    //攻撃範囲Debug
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(playerBasicAtk.position, boxSize);    
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "EnemyHitBox")
            collision.gameObject.GetComponent<IObject>().CollisionRes(transform.position, gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "EnemyHitBox")
            collision.gameObject.GetComponent<IObject>().CollisionRes(transform.position, gameObject);
        if (collision.gameObject.tag == "Item"){
            if (collision.gameObject.name.Contains("Bronze"))
                GameManager.Heal(25);
            if (collision.gameObject.name.Contains("Silver"))
                GameManager.Heal(50);
            if (collision.gameObject.name.Contains("Gold"))
                GameManager.Heal(GameManager.maxHealth);
            collision.gameObject.SetActive(false);
        }
        //次のステージ
        if (collision.gameObject.tag == "Finish"){
            GameManager.NextStage();
        }
    }


}
