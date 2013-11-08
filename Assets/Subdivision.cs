using UnityEngine;
using System.Collections;

public class Subdivision : MonoBehaviour {
	public int subdivisionLevel = 1;
	public float alphaThreshold = 0.5f;
	
	private Mesh _mesh;

	// Use this for initialization
	void Start () {
		var image = (Texture2D)GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
		var quad = new QuadtreeContour(image);
		_mesh = quad.Build(subdivisionLevel, alphaThreshold);
		GetComponent<MeshFilter>().mesh = _mesh;
	}
	
	void OnDestroy() {
		Destroy(_mesh);
	}
}
