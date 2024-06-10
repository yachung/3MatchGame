using Const;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    // 현재 좌표
    [SerializeField] private int _logicalX;
    [SerializeField] private int _logicalY;

    [SerializeField] private SpriteRenderer tileImage;

    [SerializeField] private GameObject tileClearAnimation;

    private int score = 10;

    public TileType tileType;

    public bool isMovable = true;                                  // 이동 가능한 타일인지
    public bool isMatchable = false;                               // 매칭가능한 타일인지

    public Dictionary<SwapDirection, HashSet<(int, int)>> vaildMatchSet = new Dictionary<SwapDirection, HashSet<(int, int)>>(); 

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    private float swipeAngle;         // 이동 방향 각도

    #region Property
    public int LogicalX
    {
        get => _logicalX;
        set
        {
            _logicalX = value;
        }
    }

    public int LogicalY
    {
        get => _logicalY;
        set
        {
            _logicalY = value;
        }
    }

    public bool IsMoving { get; private set; }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

    public void SetMoving(bool isMoving)
    {
        IsMoving = isMoving;
    }
    #endregion

    private void Awake()
    {
        // 좌표 최초 할당 -> set property 호출하지 않음.
        _logicalX = (int)transform.position.x;
        _logicalY = (int)transform.position.y;
        // property로 초기화를 하게 되면 CurrentY가 설정되기 전에 CurrentX의 Set 함수가 호출되면서 초기화가 꼬이게 됨.
        // 따라서 초기화는 필드로 초기화하거나 구조체로 묶어서 한번에 초기화 시키는 등의 방법을 사용해야한다.

        if (tileImage == null)
            tileImage = GetComponent<SpriteRenderer>();
    }

    private ObjectPool pool;

    //void Start()
    //{
    //    pool = FindObjectOfType<ObjectPool>();
    //}

    public void Initialize(Vector2 position)
    {
        SetPosition(position);

        _logicalX = (int)transform.position.x;
        _logicalY = (int)transform.position.y;
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

    public void SetColor(string colorHex)
    {
        tileImage.color = Utils.GetColorByHex(colorHex);
    }

    // 터치 각도 계산
    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * Mathf.Rad2Deg;
    }

    private void MovedTiles()
    {
        // 터치 거리 제한
        float distance = Vector2.Distance(finalTouchPosition, firstTouchPosition);
        if (distance < BoardManager.Instance.swipeThreshold)
            return;
        
        CalculateAngle();

        BoardManager.Instance.TileSwap(this, swipeAngle);
    }

    public void MatchTile()
    {
        GameManager.Instance.AddScore(score);

        ObjectPoolingManager.Instance.ReturnObject("Tile", this.gameObject);
    }

    public void TestTile()
    {
        Color color = GetComponent<SpriteRenderer>().color;

        color.a = 0.5f;

        GetComponent<SpriteRenderer>().color = color;
    }

    public void RemoveTile()
    {
        //if (tileClearAnimation != null)
        //{
        //    GameObject clearAnimation = Instantiate(tileClearAnimation, transform.position, Quaternion.identity);
        //    clearAnimation.transform.SetParent(this.transform);
        //}

        ObjectPoolingManager.Instance.ReturnObject("Tile", this.gameObject);
    }
}
