using Const;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    // ���� ��ǥ
    private int currentX;
    private int currentY;

    public TileType tileType { get; set; }

    public bool isMovable;                                  // �̵� ������ Ÿ������

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 targetPosition;                         // Ÿ���� �̵��� ��ǥ ��ǥ

    [SerializeField] private float swipeAngle = 0f;         // �̵� ���� ����

    public int CurrentX
    {
        get => currentX;
        set
        {
            if (currentX != value)
            {
                currentX = value;
                StartCoroutine(HorizontalMoveTiles(CurrentX));
            }
        }
    }

    public int CurrentY
    {
        get => currentY;
        set
        {
            if (currentY != value)
            {
                currentY = value;
                StartCoroutine(VerticalMoveTiles(CurrentY));
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

        // ��ġ �Ÿ� ����
        float distance = Vector2.Distance(finalTouchPosition, firstTouchPosition);
        if (distance < BoardManager.Instance.swipeThreshold)
            return;

        BoardManager.Instance.TileSwap(this, swipeAngle);
    }

    // ��ġ ���� ���
    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * Mathf.Rad2Deg;
    }

    // Ÿ�� �̵� ��ǥ ����
    //private void SetTargetPosition()
    //{
    //    // Right Swipe
    //    if (swipeAngle > -45f && swipeAngle <= 45f && CurrentX < board.width - 1)
    //    {
    //        otherDot = BoardManager.Instance.allDots[CurrentX + 1, CurrentY];
    //        otherDot.GetComponent<Dot>().CurrentX -= 1;
    //        CurrentX += 1;
    //    }
    //    // Up Swipe
    //    else if (swipeAngle > 45f && swipeAngle <= 135f && CurrentY < board.height - 1)
    //    {
    //        otherDot = BoardManager.Instance.allDots[CurrentX, CurrentY + 1];
    //        otherDot.GetComponent<Dot>().CurrentY -= 1;
    //        CurrentY += 1;
    //    }
    //    // Left Swipe
    //    else if (swipeAngle > 135f || swipeAngle <= -135f && CurrentX > 0)
    //    {
    //        otherDot = BoardManager.Instance.allDots[CurrentX - 1, CurrentY];
    //        otherDot.GetComponent<Dot>().CurrentX += 1;
    //        CurrentX -= 1;
    //    }
    //    // Down Swipe
    //    else if (swipeAngle > -135f && swipeAngle <= -45f && CurrentY > 0)
    //    {
    //        otherDot = BoardManager.Instance.allDots[CurrentX, CurrentY - 1];
    //        otherDot.GetComponent<Dot>().CurrentY += 1;
    //        CurrentY -= 1;
    //    }
    //}

    IEnumerator HorizontalMoveTiles(int targetX)
    {
        while (true)
        {
            // Move To Horizontal
            if (Mathf.Abs(targetX - transform.position.x) > .1f)
            {
                targetPosition = new Vector2(targetX, transform.position.y);
                transform.position = Vector2.Lerp(transform.position, targetPosition, .4f);

                yield return null;
            }
            else
            {
                targetPosition = new Vector2(targetX, transform.position.y);
                transform.position = targetPosition;
                BoardManager.Instance.allDots[CurrentX, CurrentY] = this;
                break;
            }
        }

        yield break;
    }

    IEnumerator VerticalMoveTiles(int targetY)
    {
        while (true)
        {
            // Move To Vertical
            if (Mathf.Abs(targetY - transform.position.y) > .1f)
            {
                targetPosition = new Vector2(transform.position.x, targetY);
                transform.position = Vector2.Lerp(transform.position, targetPosition, .4f);

                yield return null;
            }
            else
            {
                targetPosition = new Vector2(transform.position.x, targetY);
                transform.position = targetPosition;
                BoardManager.Instance.allDots[CurrentX, CurrentY] = this;

                break;
            }
        }

        yield break;
    }
}
