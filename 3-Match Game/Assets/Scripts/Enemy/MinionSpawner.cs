using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private int maximumSpawn = 10;

    private string minionName = string.Empty;

    List<Minion> spawnMinionList = new List<Minion>();

    Coroutine coMinionSpawn;

    private void Awake()
    {
        minionName = minionPrefab.name;
    }

    private void Start()
    {
        ObjectPoolingManager.Instance.CreatePool(minionName, minionPrefab, 20);

        OnGameStarted();
    }

    public void OnGameStarted()
    {
        coMinionSpawn = StartCoroutine(CoEnemyMinionSpawn());
    }

    public void OnGameEnded()
    {
        StopAllCoroutines();
        //StopCoroutine(coMinionSpawn);
    }

    IEnumerator CoEnemyMinionSpawn()
    {
        var wait = new WaitForSeconds(1f);

        while (true)
        {
            yield return wait;

            if (spawnMinionList.Count >= maximumSpawn)
                continue;

            GameObject enemyMinion = ObjectPoolingManager.Instance.GetObject(minionName);
            enemyMinion.transform.position = spawnPoint.position;
            Minion minion = enemyMinion.GetComponent<Minion>();
            minion.Initialized(wayPoints, speed, waitTime);
            spawnMinionList.Add(minion);
        }
    }

    public void EnemyMinionSpawn()
    {
        GameObject enemyMinion = ObjectPoolingManager.Instance.GetObject(minionName);
        enemyMinion.transform.position = spawnPoint.position;
        enemyMinion.GetComponent<FollowPath>().Initialize(wayPoints, speed, waitTime);
    }
}
