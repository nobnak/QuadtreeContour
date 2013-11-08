using UnityEngine;
using nobnak.Subdivision;

[ExecuteInEditMode]
public class QuadtreeContourGenerator : MonoBehaviour {
	public int subdivisionLevel = 1;
	public float alphaThreshold = 1e-7f;
	
	private Mesh _mesh;

	// Use this for initialization
	public void Generate() {
#if !UNITY_EDITOR
	Destroy(_mesh):
#endif
		
		var image = (Texture2D)GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
		var quad = new QuadtreeContour(image);
		_mesh = quad.Build(subdivisionLevel, alphaThreshold);
		GetComponent<MeshFilter>().mesh = _mesh;
	}

#if !UNITY_EDITOR
	void OnDestroy() {
		Destroy(_mesh):
	}
#endif

}