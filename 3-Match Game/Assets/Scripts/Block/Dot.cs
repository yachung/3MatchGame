using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    // 현재 좌표
    private int currentX;
    private int currentY;

    // 목표 좌표
    private int targetX;
    private int targetY;

    private GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    [SerializeField] private float swipeAngle = 0f;         // 이동 방향 각도

    private BoardManager board;


    void Start()
    {
        board = BoardManager.Instance;

        targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;

        currentX = targetX;
        currentY = targetY;
    }

    void Update()
    {
        targetX = currentX;
        targetY = currentY;

        #region MoveTowards the Target
        // Move To Horizontal
        if (Mathf.Abs(targetX - transform.position.x) > .1f)
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
        }
        else
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            board.allDots[currentX, currentY] = this.gameObject;
        }
        // Move To Vertical
        if (Mathf.Abs(targetY - transform.position.y) > .1f)
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
        }
        else
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
            board.allDots[currentX, currentY] = this.gameObject;
        }
        #endregion
    }

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(firstTouchPosition);
    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    void CalculateAngle()
    {
        float distance = Vector2.Distance(finalTouchPosition, firstTouchPosition);

        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * Mathf.Rad2Deg;

        if (distance < board.swipeThreshold)
            return;

        Debug.Log(swipeAngle);

        MovePieces();
    }

    private void MovePieces()
    {
        // Right Swipe
        if (swipeAngle > -45f && swipeAngle <= 45f && currentX < board.width - 1)
        {
            otherDot = BoardManager.Instance.allDots[currentX + 1, currentY];
            otherDot.GetComponent<Dot>().currentX -= 1;
            currentX += 1;
        }
        // Up Swipe
        else if (swipeAngle > 45f && swipeAngle <= 135f && currentY < board.height - 1)
        {
            otherDot = BoardManager.Instance.allDots[currentX, currentY + 1];
            otherDot.GetComponent<Dot>().currentY -= 1;
            currentY += 1;
        }
        // Left Swipe
        else if (swipeAngle > 135f || swipeAngle <= -135f && currentX > 0)
        {
            otherDot = BoardManager.Instance.allDots[currentX - 1, currentY];
            otherDot.GetComponent<Dot>().currentX += 1;
            currentX -= 1;
        }
        // Down Swipe
        else if (swipeAngle > -135f && swipeAngle <= -45f && currentY > 0)
        {
            otherDot = BoardManager.Instance.allDots[currentX, currentY - 1];
            otherDot.GetComponent<Dot>().currentY += 1;
            currentY -= 1;
        }
    }
}
