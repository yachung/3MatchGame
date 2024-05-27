using Const;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Dot : MonoBehaviour
{
    // 현재 좌표
    private int currentX;
    private int currentY;

    public TileType tileType;

    public bool isMovable = true;                                  // 이동 가능한 타일인지

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 targetPosition;                         // 타일이 이동할 목표 좌표

    private float swipeAngle;         // 이동 방향 각도

    public int CurrentX
    {
        get => currentX;
        set
        {
            if (currentX != value && isMovable)
            {
                currentX = value;
                StartCoroutine(HorizontalMoveTiles(() => BoardManager.Instance.BFS(this)));
            }
        }
    }

    public int CurrentY
    {
        get => currentY;
        set
        {
            if (currentY != value && isMovable)
            {
                currentY = value;
                StartCoroutine(VerticalMoveTiles(() => BoardManager.Instance.BFS(this)));
            }
        }
    }

    void Start()
    {
        CurrentX = (int)transform.position.x;
        CurrentY = (int)transform.position.y;
    }

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(firstTouchPosition);
    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        MovedTiles();
    }

    private void MovedTiles()
    {
        CalculateAngle();

        // 터치 거리 제한
        float distance = Vector2.Distance(finalTouchPosition, firstTouchPosition);
        if (distance < BoardManager.Instance.swipeThreshold)
            return;

        BoardManager.Instance.TileSwap(this, swipeAngle);
    }

    // 터치 각도 계산
    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * Mathf.Rad2Deg;
    }

    IEnumerator HorizontalMoveTiles(Action onComplete)
    {
        while (true)
        {
            // Move To Horizontal
            if (Mathf.Abs(CurrentX - transform.position.x) > .1f)
            {
                targetPosition = new Vector2(CurrentX, transform.position.y);
                transform.position = Vector2.Lerp(transform.position, targetPosition, .4f);

                yield return null;
            }
            else
            {
                targetPosition = new Vector2(CurrentX, transform.position.y);
                transform.position = targetPosition;
                BoardManager.Instance.allDots[CurrentX, CurrentY] = this;
                break;
            }
        }

        onComplete?.Invoke();
    }

    IEnumerator VerticalMoveTiles(Action onComplete)
    {
        while (true)
        {
            // Move To Vertical
            if (Mathf.Abs(CurrentY - transform.position.y) > .1f)
            {
                targetPosition = new Vector2(transform.position.x, CurrentY);
                transform.position = Vector2.Lerp(transform.position, targetPosition, .4f);

                yield return null;
            }
            else
            {
                targetPosition = new Vector2(transform.position.x, CurrentY);
                transform.position = targetPosition;
                BoardManager.Instance.allDots[CurrentX, CurrentY] = this;

                break;
            }
        }

        onComplete?.Invoke();
    }
}
