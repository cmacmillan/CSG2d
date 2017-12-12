using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class EraseClick : MonoBehaviour {

    PolygonCollider2D baseCollider;
    //MeshRenderer renderer;
    Mesh m;
    public Material mat;
    //RenderTexture targetTexture;
    // Use this for initialization
    void Start() {
        baseCollider = GetComponent<PolygonCollider2D>();
        m = new Mesh();
	}

    //deleting the last piece of an element crashes
    void Draw()
    {
        List<Vector3> points=new List<Vector3>();
        List<int> triangles = new List<int>();
        int sumSoFar = 0;
        for (int i = 0;i<baseCollider.pathCount;i++)
        {
            var currPath = baseCollider.GetPath(i);
            Triangulator t = new Triangulator(currPath);
            triangles.AddRange(new List<int>(t.Triangulate().Select(a => a + sumSoFar)).ToArray());
            sumSoFar += currPath.Count();
            for (int c = 0; c < currPath.Count(); c++)
            {
                points.Add(currPath[c]);
            }
        }
        m.vertices = points.ToArray();
        //triangles.Reverse();
        m.triangles = triangles.ToArray();
        Graphics.DrawMesh(m,Vector3.zero,Quaternion.identity,mat,0);
    }
	
	// Update is called once per frame
	void Update () {
        Draw();
	    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            try
            {
                List<Vector2[]> newShapes = new List<Vector2[]>();
                for (int path=0;path<baseCollider.pathCount;path++)
                {
                    var b = new CSGshape(new List<Vector2>(baseCollider.GetPath(path)));
                    List<Vector2> square = new List<Vector2>();
                    Vector2 center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    square.Add(center + new Vector2(1, 1));
                    square.Add(center + new Vector2(1, -1));
                    square.Add(center + new Vector2(-1, -1));
                    square.Add(center + new Vector2(-1, 1));
                    {
                        var retr = b.not(new CSGshape(square),Input.GetMouseButtonDown(0));
                        foreach (var returnVal in retr)
                        {
                            var foo = new Vector2[returnVal.segments.Count];
                            int c = 0;
                            foreach (var i in returnVal.segments)
                            {
                                foo[c] = i.start;
                                c++;
                            }
                            newShapes.Add(foo);
                            //coll.points = foo;
                        }
                    }
                }
                baseCollider.pathCount = newShapes.Count();
                int counter = 0;
                foreach (var i in newShapes)
                {
                    baseCollider.SetPath(counter, i);
                    counter++;
                }
            }
            catch (Exception e)
            {
                Debug.Log("EXCEPTION:" + e.ToString());
            }
        }
	}
}