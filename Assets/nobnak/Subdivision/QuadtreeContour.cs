using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadtreeContour {
	private Texture2D _img;
	private float _alphaThreshold;
	
	public QuadtreeContour(Texture2D img) {
		this._img = img;
	}
	
	public Mesh Build(int recursionLevel, float alphaThreshold) {
		_alphaThreshold = Mathf.Clamp01(alphaThreshold);
		var quads = new List<int>();
		quads.AddRange(Divide(0, 0, _img.width, _img.height, recursionLevel <= 0 ? 1 : recursionLevel));
		
		var rWidth = 1f / _img.width;
		var rHeight = 1f / _img.height;
		
		var vertices = new List<Vector3>();
		var uvs = new List<Vector2>();
		var triangles = new List<int>();
		for (int i = 0; i < quads.Count; i+=4) {
			var minx = quads[i];
			var miny = quads[i+1];
			var maxx = quads[i+2];
			var maxy = quads[i+3];
			var vertexIndex = vertices.Count;
			triangles.Add(vertexIndex);
			triangles.Add(vertexIndex + 3);
			triangles.Add(vertexIndex + 1);
			triangles.Add(vertexIndex);
			triangles.Add(vertexIndex + 2);
			triangles.Add(vertexIndex + 3);
			vertices.Add(new Vector3(minx, miny, 0));
			vertices.Add(new Vector3(maxx, miny, 0));
			vertices.Add(new Vector3(minx, maxy, 0));
			vertices.Add(new Vector3(maxx, maxy, 0));
			uvs.Add(new Vector2(Mathf.Clamp01(minx * rWidth), Mathf.Clamp01(miny * rHeight)));
			uvs.Add(new Vector2(Mathf.Clamp01(maxx * rWidth), Mathf.Clamp01(miny * rHeight)));
			uvs.Add(new Vector2(Mathf.Clamp01(minx * rWidth), Mathf.Clamp01(maxy * rHeight)));
			uvs.Add(new Vector2(Mathf.Clamp01(maxx * rWidth), Mathf.Clamp01(maxy * rHeight)));
		}
		
		var mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		return mesh;
	}
	
	public int[] Divide(int minx, int miny, int maxx, int maxy, int recursion) {
		if (recursion == 0) {
			for (var y = miny; y < maxy; y++) {
				for (var x = minx; x < maxx; x++) {
					var c = _img.GetPixel(x, y);
					if (c.a >= _alphaThreshold)
						return new int[]{ minx, miny, maxx, maxy };
				}
			}
			return new int[0];
		}
		
		var midx = (minx + maxx) >> 1;
		var midy = (miny + maxy) >> 1;
		var quad0 = Divide(minx, miny, midx, midy, recursion - 1);
		var quad1 = Divide(midx, miny, maxx, midy, recursion - 1);
		var quad2 = Divide(minx, midy, midx, maxy, recursion - 1);
		var quad3 = Divide(midx, midy, maxx, maxy, recursion - 1);
		var nQuads = quad0.Length + quad1.Length + quad2.Length + quad3.Length;
		
		if (quad0.Length == 4 && quad1.Length == 4 && quad2.Length == 4 && quad3.Length == 4)
			return new int[]{ minx, miny, maxx, maxy };
		
		var list = new List<int>(nQuads);
		list.AddRange(quad0);
		list.AddRange(quad1);
		list.AddRange(quad2);
		list.AddRange(quad3);
		return list.ToArray();
	}
}