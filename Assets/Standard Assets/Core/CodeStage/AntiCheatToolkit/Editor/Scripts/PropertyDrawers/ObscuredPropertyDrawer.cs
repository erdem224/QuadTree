﻿#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;

namespace Standard_Assets.Core.CodeStage.AntiCheatToolkit.Editor.Scripts.PropertyDrawers
{
	internal class ObscuredPropertyDrawer : PropertyDrawer
	{
		protected MethodInfo boldFontMethodInfo;

		protected void SetBoldIfValueOverridePrefab(SerializedProperty parentProperty, SerializedProperty valueProperty)
		{
			if (parentProperty.isInstantiatedPrefab)
			{
				SetBoldDefaultFont(valueProperty.prefabOverride);
			}
		}

		protected void ResetBoldFont()
		{
			SetBoldDefaultFont(false);
		}

		protected void SetBoldDefaultFont(bool value)
		{
			if (boldFontMethodInfo == null)
			{
				boldFontMethodInfo = typeof (EditorGUIUtility).GetMethod("SetBoldDefaultFont", BindingFlags.Static | BindingFlags.NonPublic);
			}
			boldFontMethodInfo.Invoke(null, new[] {value as object});
		}
	}
}
#endif