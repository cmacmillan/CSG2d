using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallRain : MonoBehaviour {

	// Use this for initialization
	void Start () {
        counter = 0;
	}

    public GameObject ball;
    public float timeToLive;
    public float spawnRate;
    public Transform spawnCenter;
    public float width;
    public List<Tuple<GameObject, float>> balls = new List<Tuple<GameObject,float>>();
    public float counter;
	
	// Update is called once per frame
	void Update () {
        counter += Time.deltaTime;
        if (counter > spawnRate)
        {
            counter = 0;
            spawnBall();
        }
        updateBalls();
	}

    void spawnBall()
    {
        var newBall = new Tuple<GameObject, float>();
        newBall.first = Instantiate(ball);
        newBall.first.transform.position = spawnCenter.position + new Vector3(Random.Range(-width,width),0,0);
        newBall.second = timeToLive;
        balls.Add(newBall);
    }

    void updateBalls()
    {
        List<Tuple<GameObject,float>> g = new List<Tuple<GameObject,float>>();
        foreach (var i in balls)
        {
            i.first.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1,Mathf.Pow((i.second/timeToLive),.5f));
            i.second -= Time.deltaTime;
            if (i.second < 0)
            {
                Destroy(i.first);
                g.Add(i);
            }
        }
        foreach (var i in g)
        {
            balls.Remove(i);
        }
    }
}
