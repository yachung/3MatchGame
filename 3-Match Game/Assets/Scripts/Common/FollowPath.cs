using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    [SerializeField] private Transform trMoveObject;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float unitPerSecond;
        
    private int currentIndex = 0;

    private void Start()
    {
        MoveStart();
    }

    public void MoveStart()
    {
        StartCoroutine(Process());
    }

    public void Initialize(Transform[] wayPoints, float speed, float waitTime)
    {
        this.trMoveObject = this.transform;
        this.wayPoints = wayPoints;
        this.speed = speed;
        this.waitTime = waitTime;
    }

    public void Initialize(Transform moveObject, Transform[] wayPoints)
    {
        this.trMoveObject = moveObject;
        this.wayPoints = wayPoints;
    }

    IEnumerator Process()
    {
        var wait = new WaitForSeconds(waitTime);

        while (true)
        {
            yield return StartCoroutine(MoveToWayPoint(wayPoints[currentIndex].position));

            if (currentIndex < wayPoints.Length - 1)
                currentIndex++;
            else
                break;

            yield return wait;
        }
    }

    IEnumerator MoveToWayPoint(Vector3 targetPosition)
    {
        float percent = 0;

        float moveTime = Vector3.Distance(trMoveObject.position, targetPosition) / speed;

        Debug.Log($"moveTime : {moveTime}");

        while (percent < 1)
        {
            percent += Time.deltaTime / moveTime;
            trMoveObject.position = Vector3.MoveTowards(trMoveObject.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }
    
}
