using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private UpgradeManager upgradeManager;

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            OnFinishWave();
        }
#endif
    }

    public void OnFinishWave()
    {
        upgradeManager.ShowUpgrades();
    }
}
