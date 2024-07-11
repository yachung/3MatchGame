using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public Vector2Int bottomLeft, topRight;
    private Vector2 startPos, targetPos;

    public Transform startObj;
    //public Transform targetObj;

    public List<Node> finalNodeList;
    public bool allowDiagonal, dontCrossCorner;

    private int sizeX, sizeY;

    private float nodeSize = 0.5f;

    Node[,] nodeArray;
    Node startNode, targetNode, curNode;

    PriorityQueue<Node> openList;
    List<Node> closeList;

    private void Start()
    {
        // NodeArray의 크기 정해주고, isWall, x, y 대입
        sizeX = (topRight.x - bottomLeft.x + 1) * 2;
        sizeY = (topRight.y - bottomLeft.y + 1) * 2;

        nodeArray = new Node[sizeX, sizeY];

        for (int i = 0; i < sizeX; ++i)
        {
            for (int j = 0; j < sizeY; ++j)
            {
                bool isWall = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i / 2f + bottomLeft.x, j / 2f + bottomLeft.y), 0.2f))
                {
                    if (col.gameObject.tag == "Wall") isWall = true;
                    //if (col.gameObject.layer == LayerMask.NameToLayer("Wall")) isWall = true;
                }

                nodeArray[i, j] = new Node(isWall, i / 2f + bottomLeft.x, j / 2f + bottomLeft.y);
            }
        }
    }

    public List<Vector3> PathFinding(Vector2 startPos, Vector2 targetPos)
    {
        List<Vector3> pathList = new List<Vector3>();

        this.startPos = startPos;
        this.targetPos = targetPos;

        // 시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
        startNode = nodeArray[(int)(startPos.x * 2) - bottomLeft.x, (int)(startPos.y * 2) - bottomLeft.y];
        targetNode = nodeArray[(int)(targetPos.x * 2) - bottomLeft.x, (int)(targetPos.y * 2) - bottomLeft.y];

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

        return null;
    }

    void OpenListAdd(float checkX, float checkY)
    {
        int xPos = (int)(checkX * 2);
        int yPos = (int)(checkY * 2);

        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
        if (checkX >= bottomLeft.x &&
            checkX < topRight.x + 1 &&
            checkY >= bottomLeft.y &&
            checkY < topRight.y + 1 &&
            !nodeArray[xPos - bottomLeft.x, yPos - bottomLeft.y].isWall &&
            !closeList.Contains(nodeArray[xPos - bottomLeft.x, yPos - bottomLeft.y]))
        {
            // 대각선 허용시, 벽 사이로 통과 안됨
            if (allowDiagonal) if (nodeArray[(int)(curNode.x * 2) - bottomLeft.x, yPos - bottomLeft.y].isWall && nodeArray[xPos - bottomLeft.x, (int)(curNode.y * 2) - bottomLeft.y].isWall) return;

            // 코너를 가로질러 가지 않을시, 이동 중에 수직수평 장애물이 있으면 안됨
            if (dontCrossCorner) if (nodeArray[(int)(curNode.x * 2) - bottomLeft.x, yPos - bottomLeft.y].isWall || nodeArray[xPos - bottomLeft.x, (int)(curNode.y * 2) - bottomLeft.y].isWall) return;

            // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
            Node NeighborNode = nodeArray[xPos - bottomLeft.x, yPos - bottomLeft.y];
            float MoveCost = curNode.G + ((int)(curNode.x * 2) - xPos == 0 || (int)(curNode.y * 2) - yPos == 0 ? 10 : 14);

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
        if (finalNodeList == null)
            return;

        if (finalNodeList.Count != 0) for (int i = 0; i < finalNodeList.Count - 1; i++)
                Gizmos.DrawLine(new Vector2(finalNodeList[i].x, finalNodeList[i].y), new Vector2(finalNodeList[i + 1].x, finalNodeList[i + 1].y));
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
