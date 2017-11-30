using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSGenerator : MonoBehaviour {

    public GameObject redX;
    public GameObject blueX;

    public List<Vector2> baseShape = new List<Vector2>();
    public List<Vector2> notShape = new List<Vector2>();
    public List<GameObject> objs = new List<GameObject>();

    public List<CSGsegment> segments = new List<CSGsegment>();
	// Update is called once per frame
	void Update () {
        foreach (var i in segments)
        {
            Debug.DrawLine(i.start,i.end);
        }
        if (Input.GetMouseButtonDown(0))
        {
            var item = Instantiate(redX);
            objs.Add(item);
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            item.transform.position = pos;
            baseShape.Add(pos);
        }
        if (Input.GetMouseButtonDown(1))
        {
            var item = Instantiate(blueX);
            objs.Add(item);
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            item.transform.position = pos;
            notShape.Add(pos);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var retr = new CSGshape(baseShape).not(new CSGshape(notShape));
            foreach(var i in objs)
            {
                Destroy(i);
            }
            baseShape.Clear();
            notShape.Clear();
            segments.Clear();
            foreach (var j in retr)
            {
                foreach (var i in j.segments)
                {
                    var item = Instantiate(redX);
                    objs.Add(item);
                    item.transform.position = i.start;
                    baseShape.Add(i.start);
                    segments.Add(i);
                    //Debug.DrawLine(i.start,i.end);
                    /*item = Instantiate(redX);
                    item.transform.position = i.end;
                    baseShape.Add(i.end);*/
                }
            }
        }
	}
}
