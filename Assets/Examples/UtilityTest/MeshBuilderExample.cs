using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK;

public class MeshBuilderExample : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		var mf = GetComponent<MeshFilter>();

		MeshBuilder mb = new MeshBuilder();
		mb.Start(MeshBuilderChannels.NORMALS);
		mb.AddGeometry(new TriangulatedLineGeometry()
		{
			p1 = new Vector3(-5, 0, 0),
			p2 = new Vector3(5, 0, 0),
			normal = -Camera.main.transform.forward,
			width = 1.25f
		});

		Mesh m = new Mesh();
		mb.WriteMeshData(m);
		mb.Stop();

		mf.sharedMesh = m;
	}

	private void OnDestroy()
	{
		var mf = GetComponent<MeshFilter>();
		Destroy(mf.sharedMesh);
	}
}
