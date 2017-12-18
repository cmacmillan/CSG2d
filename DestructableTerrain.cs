using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableTerrain : MonoBehaviour
{
	List<CollisionHull> colliders;
	public Shader GroundShader;
	PolygonCollider2D startingCollider;
	Material baseMeshMaterial;
	[SerializeField]
	private Texture2D basetex;
	public Texture2D BaseTexture
	{
		get { return basetex; }
		set { if (baseMeshMaterial != null) { baseMeshMaterial.SetTexture("_MainTex", basetex); } }
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

	private void ShapeOp(CSGshape other, bool direction)
	{
		List<CollisionHull> hulls = new List<CollisionHull>();
		foreach (var coll in colliders)
		{
			bool none;
			var result=CSGshape.not(coll.externalHull, other, direction, out none,coll.holes);
			hulls.AddRange(result);
		}
	}

	void Start()
	{
		startingCollider = GetComponent<PolygonCollider2D>();
		if (startingCollider == null)
		{
			message("Warning: No PolygonCollider2D found on root node. Using default collider.");
		}
		if (BaseTexture == null)
		{
			message("Warning: Missing Base Texture");
		}
		if (GroundShader != null)
		{
			baseMeshMaterial = new Material(GroundShader);
		} else
		{
			message("Error! Missing Ground Shader");
		}
	}

	void Update()
	{

	}
}
