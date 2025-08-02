using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Upgrades/New Upgrade")]
public class Upgrade : ScriptableObject
{
    public string UpgradeName;
    public Sprite UpgradeIcon;
    public string UpgradeDescription;

    public int UpgradeIndex;
}
