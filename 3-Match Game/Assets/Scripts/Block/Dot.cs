using Const;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

public class Dot : MonoBehaviour
{
    // 현재 좌표
    [SerializeField] private int _currentX;
    [SerializeField] private int _currentY;

    public TileType tileType;

    public bool isMovable = true;                                  // 이동 가능한 타일인지
    public bool isMatchable = false;        // 매칭가능한 타일인지

    public SwapDirection matchableDirection = SwapDirection.None;

    public HashSet<SwapDirection> availableDirections = new HashSet<SwapDirection>();

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 targetPosition;                         // 타일이 이동할 목표 좌표

    private float swipeAngle;         // 이동 방향 각도

    public int CurrentX
    {
        get => _currentX;
        set
        {
            //if (_currentX != value && isMovable)
            //{
            //    StartCoroutine(HorizontalMoveTiles(value, (SwapDirection direction) => 
            //    {
            //        if (isMatchable && availableDirections.Contains(direction))
            //            _currentX = value;
            //        else
            //            StartCoroutine(HorizontalMoveTiles(CurrentX));
            //    }));
            //}
            _currentX = value;
        }
    }

    public int CurrentY
    {
        get => _currentY;
        set
        {
            //if (_currentY != value && isMovable)
            //{
            //    StartCoroutine(VerticalMoveTiles(value, (SwapDirection direction) =>
            //    {
            //        if (isMatchable && availableDirections.Contains(direction))
            //            _currentY = value;
            //        else
            //            StartCoroutine(VerticalMoveTiles(CurrentY));
            //    }));
            //}
            _currentY = value;
        }
    }

    private void Awake()
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

    /*
    * 마우스 놓으면 
    * 터치거리 제한 체크
    * 이동 각도 계산
    * 타일 스왑 함수 실행
    * 계산된 각도로 이동 방향 확인 및 범위 벗어나지 않는지 체크
    * 
    */

    private void MovedTiles()
    {
        // 터치 거리 제한
        float distance = Vector2.Distance(finalTouchPosition, firstTouchPosition);
        if (distance < BoardManager.Instance.swipeThreshold)
            return;
        
        CalculateAngle();

        BoardManager.Instance.TileSwap(this, swipeAngle);
    }

    // 터치 각도 계산
    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * Mathf.Rad2Deg;
    }

    private float speed = 6f;
    private float displacement = 0.05f;
    private IEnumerator _moveCoroutine = null;

    /*
* 매칭 진행
* 매칭 성공시 타일 삭제 진행
* 
*/

    #region Move
    //IEnumerator HorizontalMoveTiles(int targetX, Action<SwapDirection> onComplete = null)
    //{
    //    SwapDirection direction = (targetX - transform.position.x > 0) ? SwapDirection.Right : SwapDirection.Left;

    //    while (true)
    //    {
    //        // Move To Horizontal
    //        if (Mathf.Abs(targetX - transform.position.x) > displacement)
    //        {
    //            targetPosition = new Vector2(targetX, transform.position.y);
    //            transform.position = Vector2.Lerp(transform.position, targetPosition, speed * Time.deltaTime);

    //            yield return null;
    //        }
    //        else
    //        {
    //            targetPosition = new Vector2(targetX, transform.position.y);
    //            transform.position = targetPosition;
    //            BoardManager.Instance.allDots[targetX, CurrentY] = this;
    //            break;
    //        }
    //    }

    //    yield return null;

    //    onComplete?.Invoke(direction);
    //}

    //IEnumerator VerticalMoveTiles(int targetY, Action<SwapDirection> onComplete = null)
    //{
    //    SwapDirection direction = (targetY - transform.position.y > 0) ? SwapDirection.Up : SwapDirection.Down;

    //    while (true)
    //    {
    //        // Move To Vertical
    //        if (Mathf.Abs(targetY - transform.position.y) > displacement)
    //        {
    //            targetPosition = new Vector2(transform.position.x, targetY);
    //            transform.position = Vector2.Lerp(transform.position, targetPosition, speed * Time.deltaTime);

    //            yield return null;
    //        }
    //        else
    //        {
    //            targetPosition = new Vector2(transform.position.x, targetY);
    //            transform.position = targetPosition;
    //            BoardManager.Instance.allDots[CurrentX, targetY] = this;

    //            break;
    //        }
    //    }

    //    // 다른 타일의 코루틴도는것 대기
    //    yield return null;

    //    onComplete?.Invoke(direction);
    //}
    #endregion

    public void Move(int targetX, int targetY, Action<bool> OnComplete = null)
    {
        if (_moveCoroutine != null)
            return;

        _moveCoroutine = MoveCoroutine(targetX, targetY, OnComplete);
        StartCoroutine(_moveCoroutine);
    }

    public IEnumerator MoveCoroutine(int targetX, int targetY, Action<bool> OnComplete = null)
    {
        Vector2 targetPosition = new Vector2(targetX, targetY);

        while (true)
        {
            if (Vector2.Distance(targetPosition, transform.position) > displacement)
            {
                transform.position = Vector2.Lerp(transform.position, targetPosition, speed * Time.deltaTime);

                yield return null;
            }
            else
            {
                transform.position = targetPosition;
                //BoardManager.Instance.allDots[targetX, CurrentY] = this;
                break;
            }
        }

        yield return null;

        OnComplete?.Invoke(true);

        _moveCoroutine = null;
    }
}
