using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image boomerangCooldown;

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


    public void UpdateCooldownGraphic(float cooldown)
    {
        boomerangCooldown.fillAmount = cooldown / 4f;
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
