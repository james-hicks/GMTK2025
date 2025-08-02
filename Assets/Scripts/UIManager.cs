using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image boomerangCooldown;
    [SerializeField] private Image dashCooldown;
    [SerializeField] private Animator deathPanelAnimator;

    public static UIManager instance;

    public GameObject[] hearts;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void TriggerDeathAnimation()
    {
        deathPanelAnimator.SetTrigger("FadeIn");

    }

    public void UpdateBoomerangCooldown(float current, float max)
    {
        boomerangCooldown.fillAmount = Mathf.Clamp01(current / max);
    }

    public void UpdateDashCooldown(float current, float max)
    {
        dashCooldown.fillAmount = Mathf.Clamp01(current / max);
    }

    public void UpdateHealth(int health)
    {
        health = health - 1;

        for (int i = 0; i < hearts.Length; i++)
        {
            if(i <= health)
            {
                hearts[i].SetActive(true);
            }
            else
            {
                hearts[i].SetActive(false);
            }
        }
    }

}
