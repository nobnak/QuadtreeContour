using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(QuadtreeContourGenerator))]
public class QuadtreeContourGeneratorEditor : Editor {
	private QuadtreeContourGenerator _gen;
	
	void OnEnable() {
		_gen = (QuadtreeContourGenerator)target;
	}

	public override void OnInspectorGUI() {
		_gen.subdivisionLevel = EditorGUILayout.IntSlider("Subdivision", _gen.subdivisionLevel, 0, 10);
		_gen.alphaThreshold = Mathf.Clamp01(EditorGUILayout.FloatField("Alpha Theshold", _gen.alphaThreshold));
		
		var invalidated = GUILayout.Button("Update");
		
		if (GUI.changed)
			EditorUtility.SetDirty(_gen);
		
		if (invalidated)
			Generate();
	}
	
	void Generate() {
		var image = (Texture2D)_gen.GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
		ChangeTextureSettings(image, true, TextureImporterFormat.RGBA32);

		var quad = new nobnak.Subdivision.QuadtreeContour(image);
		var mesh = quad.Build(_gen.subdivisionLevel, _gen.alphaThreshold);
		_gen.GetComponent<MeshFilter>().mesh = mesh;

		ChangeTextureSettings(image, false, TextureImporterFormat.AutomaticCompressed);
	}
	
	void ChangeTextureSettings(Texture2D image, bool readable, TextureImporterFormat textureFormat) {
		var imagePath = AssetDatabase.GetAssetPath(image);
		var imageImporter = (TextureImporter)TextureImporter.GetAtPath(imagePath);
		imageImporter.isReadable = readable;
		imageImporter.textureFormat = textureFormat;
		AssetDatabase.WriteImportSettingsIfDirty(imagePath);
		AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.ForceSynchronousImport);
		AssetDatabase.Refresh();
	}
}
