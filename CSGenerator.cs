using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSGenerator : MonoBehaviour {

    public GameObject redX;
    public GameObject blueX;

    public List<Vector2> baseShape = new List<Vector2>();
    public List<Vector2> notShape = new List<Vector2>();
    public List<GameObject> objs = new List<GameObject>();
	
	// Update is called once per frame
	void Update () {
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
            foreach(var i in retr[0].segments)
            {
                var item = Instantiate(redX);
                item.transform.position = i.start;
                baseShape.Add(i.start);
                /*item = Instantiate(redX);
                item.transform.position = i.end;
                baseShape.Add(i.end);*/
            }
        }
	}
}
