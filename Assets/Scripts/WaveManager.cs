using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private GameObject countDownPanel;
    [SerializeField] private GameObject wavePanelObject;
    [SerializeField] private Animator waveClearedAnimator;
    [SerializeField] private Animator waveCountAnimator;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject spawnFXPrefab;

    [Header("Wave Settings")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public int enemiesPerWave = 3;
    public float spawnDelay = 1f;
    public float waveDelay = 5f;

    private int currentWave = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        countdownText.text = $"Next wave in: 3";
        currentWave++;
        countDownPanel.SetActive(true);
        waveCountAnimator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1f);
        float countdown = Mathf.Ceil(waveDelay);

        while (countdown > 0)
        {
            countdownText.text = $"Next wave in: {countdown}";
            yield return new WaitForSecondsRealtime(1f);
            countdown--;
        }
        waveCountAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(1f);
        countDownPanel.SetActive(false);
        wavePanelObject.SetActive(false);

        Debug.Log($"Wave {currentWave} starting!");
        UIManager.instance.UpdateRoundNumber(currentWave);

        yield return StartCoroutine(SpawnWave(currentWave));
        yield return new WaitUntil(() => activeEnemies.Count == 0);

        Debug.Log($"Finished Wave {currentWave}");
        OnFinishWave();
    }

    private IEnumerator SpawnWave(int waveNumber)
    {
        int enemiesToSpawn = enemiesPerWave + (waveNumber - 1) * 2;

        int unlockedEnemies = Mathf.Clamp(1 + (waveNumber / 3), 1, enemyPrefabs.Length);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy(unlockedEnemies);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnEnemy(int unlockedEnemies)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, unlockedEnemies)];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        activeEnemies.Add(enemy);

        if (spawnFXPrefab != null)
        {
            Instantiate(spawnFXPrefab, spawnPoint.position, Quaternion.identity);
        }

        EnemyManager.Instance.RegisterEnemy(enemy.transform);

        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.OnDeath += () => {
                activeEnemies.Remove(enemy);
                EnemyManager.Instance.UnregisterEnemy(enemy.transform);
            };
        }
    }



    public void OnFinishWave()
    {
        StartCoroutine(HandlePostWaveSequence());
    }

    private IEnumerator HandlePostWaveSequence()
    {
        if (wavePanelObject != null)
        {
            wavePanelObject.SetActive(true);
        }
        waveClearedAnimator.SetTrigger("FadeIn");

        yield return new WaitForSeconds(2.5f);

        upgradeManager.ShowUpgrades();

        yield return new WaitUntil(() => upgradeManager.IsUpgradeFinished);
        yield return new WaitForSeconds(1f);

        StartCoroutine(RunWaves());
    }
}
