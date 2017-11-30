using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraseClick : MonoBehaviour {

    PolygonCollider2D baseCollider;
	// Use this for initialization
	void Start () {
        baseCollider = GetComponent<PolygonCollider2D>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetMouseButtonDown(0))
        {
            var b=new CSGshape(new List<Vector2>(baseCollider.points));
            List<Vector2> square = new List<Vector2>();
            Vector2 center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            square.Add(center+new Vector2(1,1));
            square.Add(center+new Vector2(1,-1));
            square.Add(center+new Vector2(-1,-1));
            square.Add(center+new Vector2(-1,1));
            var retr = b.not(new CSGshape(square));
            var foo = new Vector2[retr[0].segments.Count];
            int c = 0;
            foreach (var i in retr[0].segments)
            {
                foo[c] = i.start;
                c++;
            }
            baseCollider.points = foo;
        }
	}
}
