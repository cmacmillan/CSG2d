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
        m.RecalculateNormals();
        Graphics.DrawMesh(m,Vector3.zero,Quaternion.identity,mat,0);
    }

	Vector2[] collisionHullToVector2(CSGshape h)
	{
		var foo = new Vector2[h.segments.Count];
		int c = 0;
		foreach (var i in h.segments)
		{
			foo[c] = i.start;
			c++;
		}
		return foo;
	}

	List<Vector2> getSquare(Vector2 center)
	{
		List<Vector2> square = new List<Vector2>();
		square.Add(center + new Vector2(1, 1));
		square.Add(center + new Vector2(1, -1));
		square.Add(center + new Vector2(-1, -1));
		square.Add(center + new Vector2(-1, 1));
		return square;
	}

	// Update is called once per frame
	void Update () {
        Draw();
	    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            try
            {
				Vector2 center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                List<Vector2[]> newShapes = new List<Vector2[]>();
				//List<CollisionHull> shapesSoFar = new List<CollisionHull>();//#garbagelife
				//Debug.Log("shapesofarleng:" + shapesSoFar.Count);
				bool isShapesUsedUp = false;//prevents applying positive and to multiple 
                for (int path=0;path<baseCollider.pathCount;path++)
                {
					if (!isShapesUsedUp || Input.GetMouseButtonDown(0))
					{
						var b = new CSGshape(new List<Vector2>(baseCollider.GetPath(path)));
						List<Vector2> square = getSquare(center);
						{
							var retr = CSGshape.not(b, new CSGshape(square), Input.GetMouseButtonDown(0), out isShapesUsedUp, null);
							//retr = CSGshape.prune(shapesSoFar,retr);
							//shapesSoFar.AddRange(retr);
							foreach (var returnVal in retr)
							{
								newShapes.Add(collisionHullToVector2(returnVal.externalHull));
							}
						}
					} else
					{
						newShapes.Add(baseCollider.GetPath(path));
					}
                }
				if (!isShapesUsedUp && Input.GetMouseButtonDown(1))
				{
					newShapes.Add(getSquare(center).ToArray());
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