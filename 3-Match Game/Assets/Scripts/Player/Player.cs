using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float hp;
    [SerializeField] private float mp;

    [SerializeField] private float attackPoint;

    private bool isDeath = false;

    private void Awake()
    {
        hp = 100f;
        mp = 100f;
    }

    public void PlayerAttack()
    {

    }

    public void PlayerDamaged(float point)
    {
        hp = (hp - point) > 0 ? (hp - point) : 0;

        PlayerDeath();
    }

    public void PlayerDeath()
    {
        if (hp <= 0 && !isDeath)
            isDeath = true;
    }
}
