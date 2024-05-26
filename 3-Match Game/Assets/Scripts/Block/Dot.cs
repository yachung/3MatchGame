using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    private int column;
    private int row;
    private int targetX;
    private int targetY;

    private GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    private BoardManager board;

    [SerializeField] private float swipeAngle = 0f;

    public int Column { get => column; set => column = value; }
    public int Row { get => row; set => row = value; }
    public int TargetX
    {
        get => targetX;
        set
        {
            if (value != targetX)
                targetX = value;
        }
    }
    public int TargetY { get => targetY; set => targetY = value; }

    void Start()
    {
        board = BoardManager.Instance;

        TargetX = (int)transform.position.x;
        targetY = (int)transform.position.y;

        Column = TargetX;
        Row = TargetY;
    }

    // Update is called once per frame
    void Update()
    {
        TargetX = Column;
        TargetY = Row;

        // MoveTowards the Target
        if (Mathf.Abs(TargetX - transform.position.x) > .5f)
        {
            tempPosition = new Vector2(TargetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
        }
        else
        {
            tempPosition = new Vector2(TargetX, transform.position.y);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
        }

        if (Mathf.Abs(TargetY - transform.position.y) > .1f)
        {
            tempPosition = new Vector2(transform.position.x, TargetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
        }
        else
        {
            tempPosition = new Vector2(transform.position.x, TargetY);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
        }
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
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * Mathf.Rad2Deg;

        Debug.Log(swipeAngle);
        MovePieces();
    }

    private void MovePieces()
    {
        // Right Swap
        if (swipeAngle > -45f && swipeAngle <= 45f && Column < board.width)
        {
            otherDot = BoardManager.Instance.allDots[Column + 1, Row];
            otherDot.GetComponent<Dot>().Column -= 1;
            Column += 1;
        }
        // Up Swipe
        else if (swipeAngle > 45f && swipeAngle <= 135f && Row < board.height)
        {
            // 
            otherDot = BoardManager.Instance.allDots[Column, Row + 1];
            otherDot.GetComponent<Dot>().Row -= 1;
            Row += 1;
        }
        // Left Swipe
        else if (swipeAngle > 135f || swipeAngle <= -135f && Column > 0)
        {
            otherDot = BoardManager.Instance.allDots[Column - 1, Row];
            otherDot.GetComponent<Dot>().Column += 1;
            Column -= 1;
        }
        // Down Swipe
        else if (swipeAngle < -45f || swipeAngle >= -135f && Row > 0)
        {
            otherDot = BoardManager.Instance.allDots[Column, Row - 1];
            otherDot.GetComponent<Dot>().Row += 1;
            Row -= 1;
        }
    }
}
