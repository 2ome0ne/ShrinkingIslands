using System;
using UnityEngine;

public class EnemyHpSystem : MonoBehaviour
{
    [Header("Enemy Hp Settings")]
    [SerializeField] private float MaxHp = 100f;
    [SerializeField] private float CurrentHp = 100f;
    public bool KnockedOut = false;
    //[Header("Refrences")]

    private void Start()
    {
        CurrentHp = MaxHp;
    }

    public void TakeDamage(float damage)
    {
        CurrentHp -= damage;
    }
}
