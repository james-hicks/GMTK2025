using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public List<Upgrade> upgrades = new List<Upgrade>();
    public List<Upgrade> chosenUpgrades = new List<Upgrade>();

    public UpgradeOption[] options;
    private System.Random rng = new System.Random();

    public void ShowUpgrades()
    {
        Time.timeScale = 0f;
        gameObject.SetActive(true);
        ResetUpgrades();
        SelectUpgrades();
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
        gameObject.SetActive(false);
    }
}
