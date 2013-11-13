using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace nobnak.Subdivision {
	
	public class QuadtreeContour : System.IDisposable {
		private Texture2D _img;
		private float _alphaThreshold;
		
		public QuadtreeContour(Texture2D img) {
			this._img = img;
		}
		
		public Mesh Build(int recursionLevel, float alphaThreshold) {
			recursionLevel = recursionLevel <= 0 ? 1 : recursionLevel;
			_alphaThreshold = Mathf.Clamp01(alphaThreshold);
			
			var quads = BuildQuads (recursionLevel);
			quads = Optimize (quads);
			return GenerateMesh (quads);
		}

		List<int> BuildQuads (int recursionLevel) {
			var quads = new List<int>();
			var smallerSize = Mathf.Min(_img.width, _img.height);
			var nx = _img.width / smallerSize;
			var ny = _img.height / smallerSize;
			for (var y = 0; y < ny; y++) {
				for (var x = 0; x < nx; x++) {
					var minx = x * smallerSize;
					var miny = y * smallerSize;
					var maxx = minx + smallerSize;
					var maxy = miny + smallerSize;
					quads.AddRange(Divide(minx, miny, maxx, maxy, recursionLevel));
				}
			}
			return quads;
		}
		
		List<int> Optimize(List<int> quads) {
			return quads;
		}

		Mesh GenerateMesh (List<int> quads) {
			var rWidth = 1f / _img.width;
			var rHeight = 1f / _img.height;
			
			var intVertices = new List<IntVertex>();
			var triangles = new List<int>();
			for (int i = 0; i < quads.Count; i+=4) {
				var minx = quads[i];
				var miny = quads[i+1];
				var maxx = quads[i+2];
				var maxy = quads[i+3];
				var vertexIndex = intVertices.Count;
				triangles.Add(vertexIndex);
				triangles.Add(vertexIndex + 3);
				triangles.Add(vertexIndex + 1);
				triangles.Add(vertexIndex);
				triangles.Add(vertexIndex + 2);
				triangles.Add(vertexIndex + 3);
				intVertices.Add(new IntVertex(minx, miny));
				intVertices.Add(new IntVertex(maxx, miny));
				intVertices.Add(new IntVertex(minx, maxy));
				intVertices.Add(new IntVertex(maxx, maxy));
			}
			
			var vertices = new List<Vector3>();
			var uvs = new List<Vector2>();
			var index2vertex = new Dictionary<IntVertex, int>();
			foreach (var intVert in intVertices) {
				if (index2vertex.ContainsKey(intVert))
					continue;
				index2vertex.Add(intVert, vertices.Count);
				var pos = (Vector3)intVert;
				vertices.Add(pos);
				uvs.Add(new Vector2(Mathf.Clamp01(pos.x * rWidth), Mathf.Clamp01(pos.y * rHeight)));
			}
			var compactTriangles = new List<int>();
			foreach (var vertexIndex in triangles)
				compactTriangles.Add(index2vertex[intVertices[vertexIndex]]);
			
			var mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = compactTriangles.ToArray();
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
		
		public struct IntVertex {
			public int x;
			public int y;
			
			public IntVertex(int x, int y) {
				this.x = x;
				this.y = y;
			}
					
			public static explicit operator Vector3(IntVertex intVert) {
				return new Vector3(intVert.x, intVert.y, 0f);
			}
			
			public override int GetHashCode () {
				return 276503 * (x + 710089 * y);
			}
			public override bool Equals (object obj) {
				if (obj == null || obj.GetType() != typeof(IntVertex))
					return false;
				var b = (IntVertex)obj;
				return x == b.x && y == b.y;
			}
		}
		
		public struct IntEdge {
			public int x0, y0;
			public int x1, y1;
			
			public IntEdge(int x0, int y0, int x1, int y1) {
				this.x0 = x0; this.y0 = y0; this.x1 = x1; this.y1 = y1;
				if (x1 < x0 || (x0 == x1 && y1 < y0))
					Swap ();
			}
			
			public void Swap() {
				var tmpX = x0; x0 = x1; x1 = tmpX;
				var tmpY = y0; y0 = y1; y1 = tmpY;
			}
			
			public override int GetHashCode () {
				return 229 * (x0 + 151 * (y0 + 277 * (x1 + 347 * y1)));
			}
			public override bool Equals (object obj) {
				if (obj.GetType() != typeof(IntEdge))
					return false;
				var cmp = (IntEdge)obj;
				return cmp.x0 == x0 && cmp.y0 == y0 && cmp.x1 == x1 && cmp.y1 == y1;
			}
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			throw new System.NotImplementedException ();
		}
		#endregion
	}
}