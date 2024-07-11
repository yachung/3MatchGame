using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform wayPointContainer;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private int maximumSpawn = 10;

    [SerializeField] private PathFindingManager pathFindingManager;
    [SerializeField] private Transform objMoveMarker;
    [SerializeField] private bool isStartSpawnCheck = false;
    [SerializeField] private bool isEnemySpawner = false;

    private string minionName = string.Empty;

    private List<Minion> spawnMinionList = new List<Minion>();
    private Vector3[] wayPoints;
    private Camera mainCamera;

    private Coroutine coMinionSpawn;

    private void Awake()
    {
        mainCamera = Camera.main;

        minionName = minionPrefab.name;

        if (wayPointContainer != null )
        {
            wayPoints = new Vector3[wayPointContainer.childCount];

            for (int i = 0; i < wayPointContainer.childCount; ++i)
            {
                wayPoints[i] = wayPointContainer.GetChild(i).transform.position;
            }
        }
    }

    private void Start()
    {
        ObjectPoolingManager.Instance.CreatePool(minionName, minionPrefab, 100);

        OnGameStarted();
    }

    private void Update()
    {
        if (isEnemySpawner)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 movePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            movePosition.z = 0f;

            Vector2Int startPos = new Vector2Int((int)spawnPoint.position.x, (int)spawnPoint.position.y);
            Vector2Int targetPos = new Vector2Int((int)movePosition.x, (int)movePosition.y);

            List<Vector3> pathList = pathFindingManager.PathFinding(startPos, targetPos);

            if (pathList != null)
            {
                wayPoints = pathList.ToArray();
                objMoveMarker.position = movePosition;
            }
            else
            {
                Debug.Log("pathFinding is Fail");
            }
        }
    }

    public void OnGameStarted()
    {
        coMinionSpawn = StartCoroutine(CoEnemyMinionSpawn());
    }

    public void OnGameEnded()
    {
        StopAllCoroutines();
    }

    IEnumerator CoEnemyMinionSpawn()
    {
        var wait = new WaitForSeconds(1f);

        while (true)
        {
            yield return wait;

            if (wayPoints.Length == 0)
                continue;

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
