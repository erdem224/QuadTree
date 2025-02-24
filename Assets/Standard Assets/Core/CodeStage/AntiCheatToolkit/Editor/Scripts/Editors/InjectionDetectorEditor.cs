﻿#if UNITY_EDITOR
using CodeStage.AntiCheat.Detectors;
using Standard_Assets.Core.CodeStage.AntiCheatToolkit.Editor.Scripts.Windows;
using UnityEditor;
using UnityEngine;

namespace Standard_Assets.Core.CodeStage.AntiCheatToolkit.Editor.Scripts.Editors
{
	[CustomEditor(typeof (InjectionDetector))]
	internal class InjectionDetectorEditor : ActDetectorEditor
	{
		protected override void DrawUniqueDetectorProperties()
		{
			if (!EditorPrefs.GetBool(ActEditorGlobalStuff.PREFS_INJECTION_ENABLED))
			{
				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("Injection Detector is not enabled!", EditorStyles.boldLabel);
				if (GUILayout.Button("Enable in Settings..."))
				{
					ActSettings.ShowWindow();
				}
			}
			else if (!ActPostprocessor.IsInjectionDetectorTargetCompatible())
			{
				EditorGUILayout.LabelField("Injection Detector disabled for this platform.", EditorStyles.boldLabel);
			}
		}
	}
}
#endif