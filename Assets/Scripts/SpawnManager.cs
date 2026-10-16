using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private GameObject[] _powerUpPrefabs;
    // Delay between enemies at the start; it shrinks over time down to the minimum.
    [SerializeField]
    private float _enemySpawnDelay = 3f;
    [SerializeField]
    private float _minEnemySpawnDelay = 0.8f;

    private bool _stopSpawning = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnPowerUpRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        yield return new WaitForSeconds(2.0f);

        while (_stopSpawning == false)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7, 0);
            GameObject newEnemy = Instantiate(_enemyPrefab, posToSpawn, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;

            float delay = Mathf.Max(_minEnemySpawnDelay, _enemySpawnDelay - Time.timeSinceLevelLoad * 0.02f);
            yield return new WaitForSeconds(delay);
        }

    }

    IEnumerator SpawnPowerUpRoutine()
    {
        yield return new WaitForSeconds(5.0f);

        while (_stopSpawning == false && _powerUpPrefabs != null && _powerUpPrefabs.Length > 0)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7, 0);
            Instantiate(_powerUpPrefabs[Random.Range(0, _powerUpPrefabs.Length)], posToSpawn, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(6f, 12f));
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }
}
