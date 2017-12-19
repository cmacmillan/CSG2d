using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestructableTerrain : MonoBehaviour
{
	List<CollisionHull> colliders;
	public Shader GroundShader;
	public Shader HoleShader;
	public RenderTexture holeTexture;
	Mesh groundMesh;
	Mesh holeMesh;
	public bool isRenderDirty = true;
	Camera holeCamera;
	PolygonCollider2D startingCollider;
	Material groundMaterial;
	Material holeMaterial;
	[SerializeField]
	private Texture2D basetex;
	public Texture2D BaseTexture
	{
		get { return basetex; }
		set { if (groundMaterial != null) { groundMaterial.SetTexture("_MainTex", basetex); } }
	}

	private void message(string s)
	{
		Debug.Log("'"+gameObject.name + "' says '"+s+"'");
	}
	
	public void RemoveShape(CSGshape other)
	{
		ShapeOp(other, true);
	}

	public void AddShape(CSGshape other)
	{
		ShapeOp(other,false);
	}

	private void ShapeOp(CSGshape other, bool isNot)
	{
		List<CollisionHull> hulls = new List<CollisionHull>();
		bool isShapeUsedUp = false;
		foreach (var coll in colliders)
		{
			if (!isShapeUsedUp || isNot == false)
			{
				var result = CSGshape.not(coll.externalHull, other, isNot, out isShapeUsedUp, coll.holes);
				hulls.AddRange(result);
			}
		}
		if (!isShapeUsedUp && !isNot)
		{
			var foo = new CollisionHull();
			foo.externalHull = other;
			hulls.Add(foo);
		}
		colliders = hulls;
	}

	Vector2[] shapeToVector2(CSGshape h)
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

	List<Vector2> getCircle(Vector2 center, float radius, int segments)
	{
		List<Vector2> circle = new List<Vector2>();
		for (var i = 0; i < segments; i++)
		{
			var xComp = Mathf.Cos(Mathf.PI*2*i/segments);
			var yComp = Mathf.Sin(Mathf.PI*2*i/segments);
			circle.Add(center + new Vector2(xComp*radius,yComp*radius));
		}
		return circle;
	}

	void Start()
	{
		colliders = new List<CollisionHull>();
		startingCollider = GetComponent<PolygonCollider2D>();
		if (startingCollider == null)
		{
			message("Warning: No PolygonCollider2D found on root node. Using default empty collider.");
		}
		else
		{
			var foo = new CollisionHull();
			foo.externalHull = new CSGshape(new List<Vector2>(startingCollider.GetPath(0)));
			colliders.Add(foo);
		}
		if (BaseTexture == null)
		{
			message("Warning: Missing Base Texture");
		}
		if (GroundShader != null)
		{
			groundMaterial = new Material(GroundShader);
			groundMaterial.SetTexture("_MainTex", basetex);
			groundMaterial.SetFloat("_SCALE", 10);
		} else
		{
			message("Error! Missing Ground Shader");
		}
		if (HoleShader != null)
		{
			holeMaterial = new Material(HoleShader);
		} else
		{
			message("Error! Missing Hole Shader");
		}
		groundMesh = new Mesh();
		holeMesh = new Mesh();
		//holeCamera = gameObject.AddComponent<Camera>();
		//holeCamera.CopyFrom(Camera.main);
		if (holeTexture == null)
		{
			holeTexture = new RenderTexture(Camera.main.pixelWidth,Camera.main.pixelWidth,0);
		}
		//holeCamera.targetTexture = holeTexture;
		//groundMaterial.SetTexture("_MaskTex",holeTexture);
	}

	void shapesToMesh(Mesh m,List<CSGshape> shapes)
	{
		List<Vector3> points = new List<Vector3>();
		List<int> tris = new List<int>();
		int sumSoFar = 0;
		for (var i = 0; i < shapes.Count; i++)
		{
			var currPath = shapeToVector2(shapes[i]);
			Triangulator t = new Triangulator(currPath);
			tris.AddRange(new List<int>(t.Triangulate().Select(a => a + sumSoFar)).ToArray());
			sumSoFar += currPath.Length;
			for (int c = 0; c < currPath.Length; c++)
			{
				points.Add(currPath[c]);
			}
		}
		m.Clear();
        m.vertices = points.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
	}

	void DrawTerrain()
	{
		if (isRenderDirty)
		{
			var shapes = new List<CSGshape>();
			var hulls = new List<CSGshape>();
			foreach(var i in colliders)
			{
				shapes.AddRange(i.holes);
				hulls.Add(i.externalHull);
			}
			shapesToMesh(holeMesh, shapes);
			shapesToMesh(groundMesh, hulls);
			holeMaterial.SetPass(0);
			Graphics.SetRenderTarget(holeTexture);
			Graphics.DrawMeshNow(holeMesh, Vector3.zero, Quaternion.identity);
		}
		//Graphics.SetRenderTarget(null);
		Graphics.DrawMesh(holeMesh/*groundMesh*/,Vector3.zero,Quaternion.identity,groundMaterial,0);
	}

	void Update()
	{
		DrawTerrain();
		var mouseCenter = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		if (Input.GetMouseButtonDown(0))
		{
			RemoveShape(new CSGshape(getCircle(mouseCenter,1,20)));
		} else if (Input.GetMouseButtonDown(1))
		{
			AddShape(new CSGshape(getCircle(mouseCenter, 1, 20)));
		}
	}
}
