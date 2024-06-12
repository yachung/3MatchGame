using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private Transform EnemySpawnPoint;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private Transform EnemyEndPoint;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;

    private void Awake()
    {
    }

    private void Start()
    {
        ObjectPoolingManager.Instance.CreatePool("EnemyMinion", minionPrefab, 20);

        StartCoroutine(CoEnemyMinionSpawn());
    }

    public void OnGameStarted()
    {

    }

    IEnumerator CoEnemyMinionSpawn()
    {
        var wait = new WaitForSeconds(1f);

        while (true)
        {
            yield return wait;
            GameObject enemyMinion = ObjectPoolingManager.Instance.GetObject("EnemyMinion");
            enemyMinion.transform.position = EnemySpawnPoint.position;
            enemyMinion.GetComponent<FollowPath>().Initialize(wayPoints, speed, waitTime);
        }
    }

    public void EnemyMinionSpawn()
    {
        GameObject enemyMinion = ObjectPoolingManager.Instance.GetObject("EnemyMinion");
        enemyMinion.transform.position = EnemySpawnPoint.position;
        enemyMinion.GetComponent<FollowPath>().Initialize(wayPoints, speed, waitTime);
    }
}
