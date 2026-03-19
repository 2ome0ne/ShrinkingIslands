using System;
using UnityEngine;

public class TestRay : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);
            Debug.Log(hit.collider.gameObject.name);
        }
    }
}
