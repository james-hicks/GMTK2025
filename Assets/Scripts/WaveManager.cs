using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private UpgradeManager upgradeManager;

    [Header("Wave Settings")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public int enemiesPerWave = 3;
    public float spawnDelay = 0.5f;
    public float waveDelay = 3f;

    private int currentWave = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        while (true)
        {
            currentWave++;
            Debug.Log($"Wave {currentWave} starting!");
            UIManager.instance.UpdateRoundNumber(currentWave);

            yield return StartCoroutine(SpawnWave(currentWave));

            yield return new WaitUntil(() => activeEnemies.Count == 0);

            Debug.Log($"Finished Wave {currentWave}");


            yield return new WaitForSeconds(waveDelay);
            OnFinishWave();
        }
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

        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.OnDeath += () => activeEnemies.Remove(enemy);
        }
    }



    public void OnFinishWave()
    {
        upgradeManager.ShowUpgrades();
    }
}
