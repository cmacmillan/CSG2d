using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraseClick : MonoBehaviour {

    List<PolygonCollider2D> baseColliders;
	// Use this for initialization
	void Start () {
        baseColliders = new List<PolygonCollider2D>(GetComponents<PolygonCollider2D>());
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetMouseButtonDown(0))
        {
            try
            {
                List<Vector2[]> newShapes = new List<Vector2[]>();
                foreach (var coll in baseColliders)
                {
                    var b = new CSGshape(new List<Vector2>(coll.points));
                    List<Vector2> square = new List<Vector2>();
                    Vector2 center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    square.Add(center + new Vector2(1, 1));
                    square.Add(center + new Vector2(1, -1));
                    square.Add(center + new Vector2(-1, -1));
                    square.Add(center + new Vector2(-1, 1));
                    {
                        var retr = b.not(new CSGshape(square));
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
                if (baseColliders.Count < newShapes.Count)
                {
                    while (baseColliders.Count < newShapes.Count)
                    {
                        baseColliders.Add(gameObject.AddComponent<PolygonCollider2D>());
                    }
                } else if (baseColliders.Count > newShapes.Count)
                {
                    while (baseColliders.Count > newShapes.Count)
                    {
                        Destroy(baseColliders[0]);
                        baseColliders.RemoveAt(0);
                    }
                }
                int counter = 0;
                foreach (var i in newShapes)
                {
                    baseColliders[counter].points = i;
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

/*
 	    if (Input.GetMouseButtonDown(0))
        {
            List<Vector2[]> nextColliders = new List<Vector2[]>();
            foreach (var coll in baseColliders)
            {
                var b = new CSGshape(new List<Vector2>(coll.points));
                var retr = b.not(new CSGshape(square));
                foreach (var ret in retr)
                {
                    var foo = new Vector2[ret.segments.Count];
                    nextColliders.Add(foo);
                    int c = 0;
                    foreach (var i in ret.segments)
                    {
                        foo[c] = i.start;
                        c++;
                    }
                    //nextColliders.Add(foo)
                    //coll.points = foo;
                }
            }
            if (baseColliders.Count < nextColliders.Count)
            {
                Debug.Log("SHIIIIT");
                for (int i = 0; i < nextColliders.Count - baseColliders.Count; i++)
                {
                    gameObject.AddComponent<PolygonCollider2D>();
                }
            }
            baseColliders = new List<PolygonCollider2D>(GetComponents<PolygonCollider2D>());
            int count = 0;
            foreach (var i in nextColliders)
            {
                baseColliders[count].points = i;
                count++;
            }
        }*/
