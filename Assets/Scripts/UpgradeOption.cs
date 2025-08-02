using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeOption : MonoBehaviour
{
    public UpgradeManager um;
    public Upgrade upgrade;

    [SerializeField] private TextMeshProUGUI upgradeTitle;
    [SerializeField] private Image upgradeIcon;

    [SerializeField] private TextMeshProUGUI upgradeDesc;


    public void InitUpgrade(Upgrade _upgrade)
    {
        upgrade = _upgrade;

        upgradeTitle.text = upgrade.UpgradeName;
        upgradeIcon.sprite = upgrade.UpgradeIcon;
        upgradeDesc.text = upgrade.UpgradeDescription;
    }

    public void SelectUpgrade()
    {
        um.SelectedUpgrade(upgrade);
    }
}
