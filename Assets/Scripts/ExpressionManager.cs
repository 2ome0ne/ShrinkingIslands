using System;
using Unity.Netcode;
using UnityEngine;

public class ExpressionManager : NetworkBehaviour
{
    public enum Expressions
    {
        idle,
        Closed,
        mad
    }
    public Expressions currentExpression = Expressions.idle;
    
    [SerializeField] private GameObject[] expressions;
    [SerializeField] private Animator[] startmovingAnimators;
    //1.idle 2.closed 3.mad

    public override void OnNetworkSpawn()
    {
        foreach (var expression in expressions)
        {
            expression.GetComponent<Animator>().Play(0);
        }

        foreach (var animator in startmovingAnimators)
        {
            animator.Play(0);
        }
    }

    public void SetExpression(Expressions expression)
    {
        currentExpression = expression;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (currentExpression == Expressions.Closed)
        {
            DisableAllExpressionsRpc();
            SetExpressionRpc(1);
        }
        else if (currentExpression == Expressions.mad)
        {
            DisableAllExpressionsRpc();
            SetExpressionRpc(2);
        }
        else
        {
            DisableAllExpressionsRpc();
            SetExpressionRpc(0);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SetExpressionRpc(int index)
    {
        expressions[index].SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    private void DisableAllExpressionsRpc()
    {
        foreach (var expression in expressions)
        {
            expression.SetActive(false);
        }
    }
}

