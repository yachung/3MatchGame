using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private int posX;
    private int posY;

    private int score = 1;

    public Block(int posX, int posY)
    {
        this.posX = posX;
        this.posY = posY;
    }

    public void SetPosition(int targetX, int targetY)
    {
        posX = targetX;
        posY = targetY;
    }
}

