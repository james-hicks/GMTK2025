using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image boomerangCooldown;

    public static UIManager instance;



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

}
