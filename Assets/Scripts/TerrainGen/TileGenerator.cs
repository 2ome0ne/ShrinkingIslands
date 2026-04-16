using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TileGenerator : MonoBehaviour
{
    Mesh mesh;
    public Transform[] TransformVerts;
    public Vector3[] verts;
    [SerializeField] List<int> triangles;

    public bool allowTop;
    public bool allowNorth;
    public bool allowSouth;
    public bool allowWest;
    public bool allowEast;

    public void CalulateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = verts;
        mesh.triangles = triangles.ToArray();
        //mesh.RecalculateNormals();
    }

    private void CreateShape()
    {
        //NOTE NORTH IS WHERE THE RED ARROW IN POSITION IS FACEING
        verts = new Vector3[]
        {
            //TOP
            TransformVerts[0].localPosition,
            TransformVerts[1].localPosition,
            TransformVerts[2].localPosition,
            TransformVerts[3].localPosition,
            
            //North
            TransformVerts[4].localPosition,
            TransformVerts[5].localPosition,
            TransformVerts[6].localPosition,
            TransformVerts[7].localPosition,
            //West
            TransformVerts[8].localPosition,
            TransformVerts[9].localPosition,
            TransformVerts[10].localPosition,
            TransformVerts[11].localPosition,
            //South
            TransformVerts[12].localPosition,
            TransformVerts[13].localPosition,
            TransformVerts[14].localPosition,
            TransformVerts[15].localPosition,
            //East
            TransformVerts[16].localPosition,
            TransformVerts[17].localPosition,
            TransformVerts[18].localPosition,
            TransformVerts[19].localPosition,
            /*
            new Vector3(0f, 0, 0),
            new Vector3(0f, 0, 1),
            new Vector3(1f, 0, 0),
            */
        };


        if (allowTop)
        {
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(1);
            triangles.Add(3);
            triangles.Add(2);
        }

        if (allowNorth)
        {
            triangles.Add(4);
            triangles.Add(5);
            triangles.Add(6);
            triangles.Add(5);
            triangles.Add(7);
            triangles.Add(6);
        }

        if (allowWest)
        {
            triangles.Add(8);
            triangles.Add(9);
            triangles.Add(10);
            triangles.Add(9);
            triangles.Add(11);
            triangles.Add(10);
        }

        if (allowSouth)
        {
            triangles.Add(12);
            triangles.Add(13);
            triangles.Add(14);
            triangles.Add(13);
            triangles.Add(15);
            triangles.Add(14);
        }

        if (allowEast)
        {
            triangles.Add(16);
            triangles.Add(17);
            triangles.Add(18);
            triangles.Add(17);
            triangles.Add(19);
            triangles.Add(18);
        }
    }
}
