//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using UnityEngine;

//[System.Serializable]
//public class Node : IComparable<Node>
//{
//    public Node(bool _isWall, int _x, int _y) { isWall = _isWall; x = _x; y = _y; }

//    public bool isWall;
//    public Node parentNode;

//    // G : 시작으로부터 이동했던 거리, H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, F : G + H
//    public int x, y, G, H;
//    public int F { get { return G + H; } }

//    public int CompareTo(Node other)
//    {
//        if (F == other.F)  // F 값을 기준으로 크기를 비교
//            return 0;
//        return F > other.F ? 1 : -1;
//    }
//}

//public class PathFindingManager : MonoBehaviour
//{
//    public Vector2Int bottomLeft, topRight;
//    private Vector2Int startPos, targetPos;

//    public Transform startObj;
//    //public Transform targetObj;

//    public List<Node> finalNodeList;
//    public bool allowDiagonal, dontCrossCorner;

//    private int sizeX, sizeY;

//    private int nodeSize = 1;

//    Node[,] nodeArray;
//    Node startNode, targetNode, curNode;

//    PriorityQueue<Node> openList;
//    List<Node> closeList;

//    private float timeChecker;

//    private void Start()
//    {
//        // NodeArray의 크기 정해주고, isWall, x, y 대입
//        sizeX = topRight.x - bottomLeft.x + 1;
//        sizeY = topRight.y - bottomLeft.y + 1;
//        nodeArray = new Node[sizeX, sizeY];

//        for (int i = 0; i < sizeX; i++)
//        {
//            for (int j = 0; j < sizeY; j++)
//            {
//                bool isWall = false;
//                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), 0.4f))
//                {
//                    if (col.gameObject.tag == "Wall") isWall = true;
//                    //if (col.gameObject.layer == LayerMask.NameToLayer("Wall")) isWall = true;
//                }

//                nodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y);
//            }
//        }
//    }

//    public List<Vector3> PathFinding(Vector2 _startPos, Vector2 _targetPos)
//    {
//        Stopwatch watch = new Stopwatch();

//        watch.Start();

//        //Debug.Log($"PathFinding Start Time : {timeChecker:N1}");

//        List<Vector3> pathList = new List<Vector3>();

//        this.startPos = new Vector2Int((int)_startPos.x, (int)_startPos.y);
//        this.targetPos = new Vector2Int((int)_targetPos.x, (int)_targetPos.y);

//        // 시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
//        startNode = nodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];
//        targetNode = nodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

//        openList = new PriorityQueue<Node>();
//        openList.Enqueue(startNode);

//        closeList = new List<Node>();
//        finalNodeList = new List<Node>();

//        while (openList.Count > 0)
//        {
//            // 열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
//            curNode = openList.Dequeue();

//            closeList.Add(curNode);

//            // 마지막
//            if (curNode == targetNode)
//            {
//                Node TargetCurNode = targetNode;
//                while (TargetCurNode != startNode)
//                {
//                    finalNodeList.Add(TargetCurNode);
//                    TargetCurNode = TargetCurNode.parentNode;
//                }
//                finalNodeList.Add(startNode);
//                finalNodeList.Reverse();

//                foreach (var node in finalNodeList)
//                    pathList.Add(new Vector2(node.x, node.y));

//                //for (int i = 0; i < finalNodeList.Count; i++) print(i + "번째는 " + finalNodeList[i].x + ", " + finalNodeList[i].y);

//                watch.Stop();
//                UnityEngine.Debug.Log($"PathFinding Success Time : {watch.ElapsedMilliseconds + " ms"}");

//                return pathList;
//            }

//            // ↗↖↙↘
//            if (allowDiagonal)
//            {
//                OpenListAdd(curNode.x + nodeSize, curNode.y + nodeSize);
//                OpenListAdd(curNode.x - nodeSize, curNode.y + nodeSize);
//                OpenListAdd(curNode.x - nodeSize, curNode.y - nodeSize);
//                OpenListAdd(curNode.x + nodeSize, curNode.y - nodeSize);
//            }

//            // ↑ → ↓ ←
//            OpenListAdd(curNode.x, curNode.y + nodeSize);
//            OpenListAdd(curNode.x + nodeSize, curNode.y);
//            OpenListAdd(curNode.x, curNode.y - nodeSize);
//            OpenListAdd(curNode.x - nodeSize, curNode.y);
//        }

//        watch.Stop();
//        UnityEngine.Debug.Log($"PathFinding Fail Time : {watch.ElapsedMilliseconds + " ms"}");

//        return null;
//    }

//    void OpenListAdd(int checkX, int checkY)
//    {
//        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
//        if (checkX >= bottomLeft.x &&
//            checkX < topRight.x + 1 &&
//            checkY >= bottomLeft.y &&
//            checkY < topRight.y + 1 &&
//            !nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y].isWall &&
//            !closeList.Contains(nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y]))
//        {
//            // 대각선 허용시, 벽 사이로 통과 안됨
//            if (allowDiagonal) if (nodeArray[curNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall && nodeArray[checkX - bottomLeft.x, curNode.y - bottomLeft.y].isWall) return;

//            // 코너를 가로질러 가지 않을시, 이동 중에 수직수평 장애물이 있으면 안됨
//            if (dontCrossCorner) if (nodeArray[curNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall || nodeArray[checkX - bottomLeft.x, curNode.y - bottomLeft.y].isWall) return;

//            // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
//            Node NeighborNode = nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];
//            int MoveCost = curNode.G + (curNode.x - checkX == 0 || curNode.y - checkY == 0 ? 10 : 14);

//            // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면 G, H, ParentNode를 설정 후 열린리스트에 추가
//            if (MoveCost < NeighborNode.G || !openList.Contains(NeighborNode))
//            {
//                NeighborNode.G = MoveCost;
//                NeighborNode.H = (Mathf.Abs(NeighborNode.x - targetNode.x) + Mathf.Abs(NeighborNode.y - targetNode.y)) * 10;
//                NeighborNode.parentNode = curNode;

//                openList.Enqueue(NeighborNode);
//            }
//        }
//    }

//    void OnDrawGizmos()
//    {
//        if (finalNodeList == null)
//            return;

//        if (finalNodeList.Count != 0) for (int i = 0; i < finalNodeList.Count - 1; i++)
//                Gizmos.DrawLine(new Vector2(finalNodeList[i].x, finalNodeList[i].y), new Vector2(finalNodeList[i + 1].x, finalNodeList[i + 1].y));
//    }
//}


//class PriorityQueue<T> where T : IComparable<T>
//{
//    private List<T> heap = new List<T>();

//    public int Count { get { return heap.Count; } }

//    public void Enqueue(T item)
//    {
//        heap.Add(item);
//        int i = Count - 1;
//        while (i > 0)
//        {
//            int parent = (i - 1) / 2;
//            if (heap[parent].CompareTo(heap[i]) <= 0)
//                break;
//            Swap(parent, i);
//            i = parent;
//        }
//    }

//    public T Dequeue()
//    {
//        T result = heap[0];
//        heap[0] = heap[Count - 1];
//        heap.RemoveAt(Count - 1);

//        int i = 0;
//        while (true)
//        {
//            int leftChild = 2 * i + 1;
//            int rightChild = 2 * i + 2;
//            int smallest = i;

//            if (leftChild < Count && heap[leftChild].CompareTo(heap[smallest]) < 0)
//                smallest = leftChild;
//            if (rightChild < Count && heap[rightChild].CompareTo(heap[smallest]) < 0)
//                smallest = rightChild;

//            if (smallest == i)
//                break;

//            Swap(i, smallest);
//            i = smallest;
//        }

//        return result;
//    }

//    public bool Contains(T item)
//    {
//        int index = 0;
//        int count = Count;

//        while (count-- > 0)
//        {
//            if (item == null)
//            {
//                if (heap[index] == null)
//                    return true;
//            }
//            else if (heap[index] != null && heap[index].Equals(item))
//            {
//                return true;
//            }
//            index = (index + 1) % heap.Count;
//        }

//        return false;
//    }

//    private void Swap(int i, int j)
//    {
//        (heap[j], heap[i]) = (heap[i], heap[j]);
//    }
//}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Node : IComparable<Node>
{
    public Node(bool _isWall, float _x, float _y) { isWall = _isWall; x = _x; y = _y; }

    public bool isWall;
    public Node parentNode;

    // G : 시작으로부터 이동했던 거리, H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, F : G + H
    public float x, y, G, H;
    public float F { get { return G + H; } }

    public int CompareTo(Node other)
    {
        if (F == other.F)  // F 값을 기준으로 크기를 비교
            return 0;
        return F > other.F ? 1 : -1;
    }
}

public class PathFindingManager : MonoBehaviour
{
    [SerializeField] Tilemap map;

    public Vector2Int bottomLeft, topRight;
    private Vector2 startPos, targetPos;

    public List<Node> finalNodeList;
    public bool allowDiagonal, dontCrossCorner;

    private int sizeX, sizeY;

    [SerializeField] private int nodeMultiplyRate = 2;
    private float nodeSize;

    Node[,] nodeArray;
    Node startNode, targetNode, curNode;

    PriorityQueue<Node> openList;
    List<Node> closeList;

    private void Start()
    {
        nodeSize = 1 / (float)nodeMultiplyRate;

        Vector3 minWorldPoint = map.CellToWorld(new Vector3Int(map.cellBounds.xMin, map.cellBounds.yMin, map.cellBounds.zMin));
        Vector3 maxWorldPoint = map.CellToWorld(new Vector3Int(map.cellBounds.xMax - 1, map.cellBounds.yMax - 1, map.cellBounds.zMax - 1));

        bottomLeft = new Vector2Int((int)minWorldPoint.x, (int)minWorldPoint.y);
        topRight = new Vector2Int((int)maxWorldPoint.x, (int)maxWorldPoint.y);

        // NodeArray의 크기 정해주고, isWall, x, y 대입
        sizeX = (topRight.x - bottomLeft.x + 1) * nodeMultiplyRate;
        sizeY = (topRight.y - bottomLeft.y + 1) * nodeMultiplyRate;

        nodeArray = new Node[sizeX, sizeY];

        for (int i = 0; i < sizeX; ++i)
        {
            for (int j = 0; j < sizeY; ++j)
            {
                bool isWall = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i / (float)nodeMultiplyRate + bottomLeft.x, j / (float)nodeMultiplyRate + bottomLeft.y), nodeSize / 2f))
                {
                    if (col.gameObject.tag == "Wall") isWall = true;
                    //if (col.gameObject.layer == LayerMask.NameToLayer("Wall")) isWall = true;
                }

                nodeArray[i, j] = new Node(isWall, i / (float)nodeMultiplyRate + bottomLeft.x, j / (float)nodeMultiplyRate + bottomLeft.y);
            }
        }
    }

    public List<Vector3> PathFinding(Vector2 _startPos, Vector2 _targetPos)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();


        List<Vector3> pathList = new List<Vector3>();

        this.startPos = _startPos;
        this.targetPos = _targetPos;

        // 시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
        startNode = nodeArray[(int)(startPos.x * nodeMultiplyRate) - bottomLeft.x * nodeMultiplyRate, (int)(startPos.y * nodeMultiplyRate) - bottomLeft.y * nodeMultiplyRate];
        targetNode = nodeArray[(int)(targetPos.x * nodeMultiplyRate) - bottomLeft.x * nodeMultiplyRate, (int)(targetPos.y * nodeMultiplyRate) - bottomLeft.y * nodeMultiplyRate];

        openList = new PriorityQueue<Node>();
        openList.Enqueue(startNode);

        closeList = new List<Node>();
        finalNodeList = new List<Node>();

        while (openList.Count > 0)
        {
            // 열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
            curNode = openList.Dequeue();

            closeList.Add(curNode);

            // 마지막
            if (curNode == targetNode)
            {
                Node TargetCurNode = targetNode;
                while (TargetCurNode != startNode)
                {
                    finalNodeList.Add(TargetCurNode);
                    TargetCurNode = TargetCurNode.parentNode;
                }
                finalNodeList.Add(startNode);
                finalNodeList.Reverse();

                foreach (var node in finalNodeList)
                    pathList.Add(new Vector2(node.x, node.y));

                //for (int i = 0; i < finalNodeList.Count; i++) print(i + "번째는 " + finalNodeList[i].x + ", " + finalNodeList[i].y);

                watch.Stop();
                UnityEngine.Debug.Log($"{nodeMultiplyRate} * PathFinding Success Time : {watch.ElapsedMilliseconds + " ms"}");

                return pathList;
            }

            // ↗↖↙↘
            if (allowDiagonal)
            {
                OpenListAdd(curNode.x + nodeSize, curNode.y + nodeSize);
                OpenListAdd(curNode.x - nodeSize, curNode.y + nodeSize);
                OpenListAdd(curNode.x - nodeSize, curNode.y - nodeSize);
                OpenListAdd(curNode.x + nodeSize, curNode.y - nodeSize);
            }

            // ↑ → ↓ ←
            OpenListAdd(curNode.x, curNode.y + nodeSize);
            OpenListAdd(curNode.x + nodeSize, curNode.y);
            OpenListAdd(curNode.x, curNode.y - nodeSize);
            OpenListAdd(curNode.x - nodeSize, curNode.y);
        }

        watch.Stop();
        UnityEngine.Debug.Log($"{nodeMultiplyRate} * PathFinding Fail Time : {watch.ElapsedMilliseconds + " ms"}");

        return null;
    }

    void OpenListAdd(float checkX, float checkY)
    {
        int xPos = (int)(checkX * nodeMultiplyRate) - (bottomLeft.x * nodeMultiplyRate);
        int yPos = (int)(checkY * nodeMultiplyRate) - (bottomLeft.y * nodeMultiplyRate);

        int curXPos = (int)(curNode.x * nodeMultiplyRate) - (bottomLeft.x * nodeMultiplyRate);
        int curYPos = (int)(curNode.y * nodeMultiplyRate) - (bottomLeft.y * nodeMultiplyRate);

        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
        if (checkX >= bottomLeft.x &&
            checkX < topRight.x + 1 &&
            checkY >= bottomLeft.y &&
            checkY < topRight.y + 1 &&
            !nodeArray[xPos, yPos].isWall &&
            !closeList.Contains(nodeArray[xPos, yPos]))
        {
            // 대각선 허용시, 벽 사이로 통과 안됨
            if (allowDiagonal) if (nodeArray[curXPos, yPos].isWall && nodeArray[xPos, curYPos].isWall) return;

            // 코너를 가로질러 가지 않을시, 이동 중에 수직수평 장애물이 있으면 안됨
            if (dontCrossCorner) if (nodeArray[curXPos, yPos].isWall || nodeArray[xPos, curYPos].isWall) return;

            // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
            Node NeighborNode = nodeArray[xPos, yPos];
            float MoveCost = curNode.G + (curXPos - xPos == 0 || curYPos - yPos == 0 ? 10 : 14);

            // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면 G, H, ParentNode를 설정 후 열린리스트에 추가
            if (MoveCost < NeighborNode.G || !openList.Contains(NeighborNode))
            {
                NeighborNode.G = MoveCost;
                NeighborNode.H = (Mathf.Abs(NeighborNode.x - targetNode.x) + Mathf.Abs(NeighborNode.y - targetNode.y)) * 10;
                NeighborNode.parentNode = curNode;

                openList.Enqueue(NeighborNode);
            }
        }
    }

    void OnDrawGizmos()
    {
        //if (finalNodeList == null)
        //    return;

        //if (finalNodeList.Count != 0) for (int i = 0; i < finalNodeList.Count - 1; ++i)
        //        Gizmos.DrawLine(new Vector2(finalNodeList[i].x, finalNodeList[i].y), new Vector2(finalNodeList[i + 1].x, finalNodeList[i + 1].y));
    }
}


class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> heap = new List<T>();

    public int Count { get { return heap.Count; } }

    public void Enqueue(T item)
    {
        heap.Add(item);
        int i = Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[parent].CompareTo(heap[i]) <= 0)
                break;
            Swap(parent, i);
            i = parent;
        }
    }

    public T Dequeue()
    {
        T result = heap[0];
        heap[0] = heap[Count - 1];
        heap.RemoveAt(Count - 1);

        int i = 0;
        while (true)
        {
            int leftChild = 2 * i + 1;
            int rightChild = 2 * i + 2;
            int smallest = i;

            if (leftChild < Count && heap[leftChild].CompareTo(heap[smallest]) < 0)
                smallest = leftChild;
            if (rightChild < Count && heap[rightChild].CompareTo(heap[smallest]) < 0)
                smallest = rightChild;

            if (smallest == i)
                break;

            Swap(i, smallest);
            i = smallest;
        }

        return result;
    }

    public bool Contains(T item)
    {
        int index = 0;
        int count = Count;

        while (count-- > 0)
        {
            if (item == null)
            {
                if (heap[index] == null)
                    return true;
            }
            else if (heap[index] != null && heap[index].Equals(item))
            {
                return true;
            }
            index = (index + 1) % heap.Count;
        }

        return false;
    }

    private void Swap(int i, int j)
    {
        (heap[j], heap[i]) = (heap[i], heap[j]);
    }
}
