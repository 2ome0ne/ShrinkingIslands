using System;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void Start()
    {
        if (target == null)
        {
            if (Camera.main)
            {
                target = Camera.main.transform;
            }
        }
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            if (Camera.main)
            {
                target = Camera.main.transform;
            }
        }
        transform.LookAt(target);
    }
}
