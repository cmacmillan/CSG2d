using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelCollisionObject : MonoBehaviour {


    public Texture2D t;
    public int pixelWidth;
    public GameObject ground;

    public GameObject cursor;

    public GameObject ball;

    public int scanX;
    public int scanY;

    ////////
    public RenderTexture renderTexture; // renderTextuer that you will be rendering stuff on
    public Renderer renderer; // renderer in which you will apply changed texture
    public Texture2D texture;

    public Rigidbody2D rigidBall;

    void Start()
    {
        //renderTexture = new RenderTexture(pixelWidth * 2, pixelWidth * 2, 16);
        texture = new Texture2D(renderTexture.width, renderTexture.height);
        pixelWidth = renderTexture.width;
        renderer.material.mainTexture = texture;
        rigidBall = ball.GetComponent<Rigidbody2D>();
        //make texture2D because you want to "edit" it. 
        //however this is not a way to apply any post rendering effects because
        //this way, you are reading it through CPU(slow).
    }
    ///////
    Vector2 toOtherObjectSpace(Transform other)//maybe factor rotation eventually?
    {
        return transform.position - other.position;
    }

    int wp2p()
    {
        return 102;//return (int)(1024 / (ground.GetComponent<MeshFilter>().mesh.bounds.extents.x));
    }

    List<List<Vector2>> getKernel()
    {
        var retr = new List<List<Vector2>>();
        float center = pixelWidth / 2;
        for (int i = 0; i < pixelWidth; i++)
        {
            retr.Add(new List<Vector2>());
            for (int j = 0; j < pixelWidth; j++)
            {
                retr[i].Add(new Vector2(center-i,center-j)/4);
            }
        }
        return retr;
    }

    void applyCollisions()
    {
        var otherCenter = toOtherObjectSpace(ground.transform);
        var pixels = t.GetPixels(scanX, scanY, pixelWidth, pixelWidth);
        DrawBuffer(pixels);
        //var pixels = t.GetPixels(wp2p()*Mathf.RoundToInt(otherCenter.x-pixelWidth/2), wp2p()*Mathf.RoundToInt(otherCenter.y-pixelWidth/2), pixelWidth, pixelWidth);//prob lerp? maybe not.. blending would be weird
        var kernel = getKernel();
        var sum = new Vector2();
        int c = 0;
        foreach (var i in pixels)
        {
            sum += i.a*kernel[c%pixelWidth][(c-(c%pixelWidth))/pixelWidth];
            c++;
        }
        if (sum.x!=0 || sum.y != 0)
        {
            rigidBall.velocity = sum.normalized * rigidBall.velocity.magnitude*.5f;
            rigidBall.AddForce(sum * rigidBall.velocity.magnitude * 5f);
        }
    }

    void DrawBuffer(Color[] buffer)
    {
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        int c = buffer.Length;
        for (int i = 0; i < renderTexture.width; i++)
        {
            for (int j = 0; j < renderTexture.height; j++)
            {
                c--;
                texture.SetPixel(j,i,buffer[c]);
            }
        }
        texture.Apply();
        RenderTexture.active = null;
    }
    // Update is called once per frame
    void Update () {
        scanX = (int)((ball.transform.position.x+5f) * wp2p());
        scanY = (int)((ball.transform.position.y+5f)*wp2p());
        cursor.transform.position = new Vector3(-5f+((float)scanX)/wp2p(),-5f+((float)scanY)/wp2p(),0);
        applyCollisions();	    	
	}
}
