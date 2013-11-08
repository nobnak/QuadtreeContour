using UnityEngine;
using UnityEditor;
using System.Collections;
using nobnak.Subdivision;
	
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
			_gen.Generate();
	}
}