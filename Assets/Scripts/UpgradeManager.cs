using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public bool IsUpgradeFinished { get; private set; } = false;
    public List<Upgrade> upgrades = new List<Upgrade>();
    public List<Upgrade> chosenUpgrades = new List<Upgrade>();

    public UpgradeOption[] options;
    private System.Random rng = new System.Random();

    [SerializeField] private Animator upgradeAnimator;

    public void ShowUpgrades()
    {
        IsUpgradeFinished = false;
        ResetUpgrades();
        SelectUpgrades();

        gameObject.SetActive(true);
        upgradeAnimator.SetTrigger("FadeIn");

        StartCoroutine(FreezeTimeAfterFade());
    }

    public void SelectUpgrades()
    {
        // Clear chosen upgrades for this selection phase
        chosenUpgrades.Clear();

        // Shuffle and pick 3 upgrades or as many as possible if less remain
        List<Upgrade> availableUpgrades = new List<Upgrade>(upgrades);

        for (int i = 0; i < options.Length; i++)
        {
            int index = rng.Next(availableUpgrades.Count); // Random index
            Upgrade selected = availableUpgrades[index];

            // Assign to UI option
            options[i].InitUpgrade(selected);

            // Move upgrade to chosen list so it can't repeat
            chosenUpgrades.Add(selected);
            availableUpgrades.RemoveAt(index);
        }
    }

    public void ResetUpgrades()
    {
        // Return chosen upgrades to the pool for next phase
        foreach (var upgrade in chosenUpgrades)
        {
            if (!upgrades.Contains(upgrade))
                upgrades.Add(upgrade);
        }
        chosenUpgrades.Clear();
    }

    public void SelectedUpgrade(Upgrade up)
    {
        Time.timeScale = 1f;
        FindFirstObjectByType<PlayerController>().ApplyUpgrades(up.UpgradeIndex);

        //Trigger fade-out animation
        upgradeAnimator.SetTrigger("FadeOut");

        //Delay hiding the UI until after fade
        StartCoroutine(DelayedHide(2.0f));

        IsUpgradeFinished = true;
    }

    private IEnumerator DelayedHide(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        gameObject.SetActive(false);
    }

    private IEnumerator FreezeTimeAfterFade()
    {
        yield return new WaitForSeconds(2f);
        Time.timeScale = 0f;
    }
}
