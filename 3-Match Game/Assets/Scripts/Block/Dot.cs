using Const;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Dot : MonoBehaviour
{
    // 현재 좌표
    [SerializeField] private int _currentX;
    [SerializeField] private int _currentY;

    public TileType tileType;

    public bool isMovable = true;                                  // 이동 가능한 타일인지

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 targetPosition;                         // 타일이 이동할 목표 좌표

    private float swipeAngle;         // 이동 방향 각도

    public int CurrentX
    {
        get => _currentX;
        set
        {
            if (_currentX != value && isMovable)
            {
                _currentX = value;
                StartCoroutine(HorizontalMoveTiles());
            }
        }
    }

    public int CurrentY
    {
        get => _currentY;
        set
        {
            if (_currentY != value && isMovable)
            {
                _currentY = value;
                StartCoroutine(VerticalMoveTiles());
            }
        }
    }

    void Start()
    {
        // 좌표 최초 할당 -> set property 호출하지 않음.
        _currentX = (int)transform.position.x;
        _currentY = (int)transform.position.y;
        // property로 초기화를 하게 되면 CurrentY가 설정되기 전에 CurrentX의 Set 함수가 호출되면서 초기화가 꼬이게 됨.
        // 따라서 초기화는 필드로 초기화하거나 구조체로 묶어서 한번에 초기화 시키는 등의 방법을 사용해야한다.
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

    IEnumerator HorizontalMoveTiles(Action onComplete = null)
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

        yield return null;

        onComplete?.Invoke();
    }

    IEnumerator VerticalMoveTiles(Action onComplete = null)
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

        // 다른 타일의 코루틴도는것 대기
        yield return null;

        onComplete?.Invoke();
    }
}
