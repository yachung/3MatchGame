using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    private Transform trMoveObject;
    private Vector3[] wayPoints;
    private float speed = 2f;
    private float waitTime = 1f;
        
    private int currentIndex = 0;

    public bool IsEndPoint { get; private set; }

    public void MoveStart()
    {
        StopMove();

        StartCoroutine(Process());
    }

    public void StopMove()
    {
        StopAllCoroutines();
    }

    public void Initialize(Vector3[] wayPoints, float speed, float waitTime)
    {
        this.trMoveObject = this.transform;
        this.wayPoints = wayPoints;
        this.speed = speed;
        this.waitTime = waitTime;
    }

    public void Initialize(Transform moveObject, Vector3[] wayPoints)
    {
        this.trMoveObject = moveObject;
        this.wayPoints = wayPoints;
    }

    IEnumerator Process()
    {
        var wait = new WaitForSeconds(waitTime);

        while (true)
        {
            yield return StartCoroutine(MoveToWayPoint(wayPoints[currentIndex]));

            if (currentIndex < wayPoints.Length - 1)
                currentIndex++;
            else
            {
                IsEndPoint = true;
                break;
            }

            yield return wait;
        }
    }

    IEnumerator MoveToWayPoint(Vector3 targetPosition)
    {
        float percent = 0;

        float moveTime = Vector3.Distance(trMoveObject.position, targetPosition) / speed;

        //Debug.Log($"moveTime : {moveTime}");

        while (true)
        {
            //percent += Time.deltaTime / moveTime;
            float distance = (targetPosition - trMoveObject.position).magnitude;
            if (distance <= 0.1f)
                break;

            trMoveObject.position = Vector3.MoveTowards(trMoveObject.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }

    public Vector3 CurrentWayPointPosition()
    {
        return wayPoints[currentIndex];
    }
}   
