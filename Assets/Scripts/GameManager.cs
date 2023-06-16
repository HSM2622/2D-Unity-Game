using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{  

    public int stageIndex;
    public int health;
    public int maxHealth;
    public Player player;
    public Image hpBar;
    public GameObject restartBtn;
    public TextMeshProUGUI restartBtnText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI stageName;
    public GameObject[] Stages;
    float healthPercentage;
    AnimatorStateInfo animStateInfo;
    
    void Awake(){
        hpText.text = "HP: " + health.ToString() + " / " + maxHealth.ToString();
        stageName.text = Stages[stageIndex].name;
    }

    public void NextStage(){
        if (stageIndex < Stages.Length-1){
        Stages[stageIndex].SetActive(false);
        stageIndex++;
        Stages[stageIndex].SetActive(true);
        PlayerReposition();
        Heal(maxHealth);
        stageName.text = Stages[stageIndex].name;
        } else { //game clear
            restartBtn.GetComponent<Image>().color = Color.white;
            restartBtn.SetActive(true);
            restartBtnText.text = "Clear";
            Time.timeScale = 0;
        }
    }

    void PlayerReposition(){
        player.transform.position = new Vector3(0, 0.62f, 0);
        player.Stop();
    }

    public void HealthDown(int amount){
        if((health - amount) > 0){
            health -= amount;
            healthPercentage = (float) health / maxHealth;
            hpBar.fillAmount = healthPercentage;
            hpText.text = "HP: " + health.ToString() + " / " + maxHealth.ToString();
        }
        else {
            player.Die();
        }
    }

    public void Heal(int amount){
        if((health + amount) <= maxHealth)
            health += amount;
        else
            health = maxHealth;
            healthPercentage = (float) health / maxHealth;
            hpBar.fillAmount = healthPercentage;
            hpText.text = "HP: " + health.ToString() + " / " + maxHealth.ToString();
    }


    public void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.tag == "Player")
            HealthDown(999);
    }

    public void Restart(){
        if (restartBtnText.text == "You Died");
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }






    //グローバル関数
    //オブジェクトの死亡
    public IEnumerator FadeOutAndDestroy(GameObject gameObject, Animator animator, SpriteRenderer spriteRenderer){
        // SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;
        // Animator animator = gameObject.GetComponent<Animator>();
        float fadeDuration = 1f;

        if (animator != null){
            animator.SetTrigger("onDied");
            animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            fadeDuration = animStateInfo.length;
        } else
            fadeDuration = 1f;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration){
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (gameObject.tag == "Player"){
            gameObject.GetComponent<Collider2D>().enabled = false;
            restartBtn.SetActive(true);
            Time.timeScale = 0;
        }
        else 
            gameObject.SetActive(false);
    }

}
