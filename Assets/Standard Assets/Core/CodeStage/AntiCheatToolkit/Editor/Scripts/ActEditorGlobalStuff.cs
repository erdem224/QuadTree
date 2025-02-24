﻿#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

// allows to use internal methods in editor code from the editor code (Prefs editor window)
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Assembly-CSharp-Editor")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Assembly-CSharp-Editor-firstpass")] // thx Daniele! ;)

namespace Standard_Assets.Core.CodeStage.AntiCheatToolkit.Editor.Scripts
{
	internal class ActEditorGlobalStuff
	{
		internal const string LOG_PREFIX = "[ACTk] ";
		internal const string WINDOWS_MENU_PATH = "Tools/Code Stage/Anti-Cheat Toolkit/";

		internal const string PREFS_INJECTION_ENABLED = "ACTDIDEnabledGlobal";
		internal const string REPORT_EMAIL = "support@codestage.ru";
		
		internal const string INJECTION_SERVICE_FOLDER = "InjectionDetectorData";
		internal const string INJECTION_DEFAULT_WHITELIST_FILE = "DefaultWhitelist.bytes";
		internal const string INJECTION_USER_WHITELIST_FILE = "UserWhitelist.bytes";
		internal const string INJECTION_DATA_FILE = "fndid.bytes";
		internal const string INJECTION_DATA_SEPARATOR = ":";

		internal const string ASSEMBLIES_PATH_RELATIVE = "Library/ScriptAssemblies";

		internal static readonly string assetsPath = Application.dataPath;
		internal static readonly string resourcesPath = assetsPath + "/Resources/";
		internal static readonly string assembliesPath = assetsPath + "/../" + ASSEMBLIES_PATH_RELATIVE;

		internal static readonly string injectionDataPath = resourcesPath + INJECTION_DATA_FILE;

		private static readonly string[] hexTable = Enumerable.Range(0, 256).Select(v => v.ToString("x2")).ToArray();

		// left for future cases
		/*private static readonly string[] obsoletePrefs = { "ACTDIDEnabled" };

		[DidReloadScripts]
		private static void RemoveObsoleteStuff()
		{
			foreach (string prefKey in obsoletePrefs.Where(EditorPrefs.HasKey))
			{
				EditorPrefs.DeleteKey(prefKey);
			}
		}*/

		#region files and directories
		internal static void CleanInjectionDetectorData()
		{
			if (!File.Exists(injectionDataPath))
			{
				return;
			}

			RemoveReadOnlyAttribute(injectionDataPath);
			RemoveReadOnlyAttribute(injectionDataPath + ".meta");

			FileUtil.DeleteFileOrDirectory(injectionDataPath);
			FileUtil.DeleteFileOrDirectory(injectionDataPath + ".meta");

			RemoveDirectoryIfEmpty(resourcesPath);
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
		}

		internal static string ResolveInjectionDefaultWhitelistPath()
		{
			return ResolveInjectionServiceFolder() + "/" + INJECTION_DEFAULT_WHITELIST_FILE;
		}

		internal static string ResolveInjectionUserWhitelistPath()
		{
			return ResolveInjectionServiceFolder() + "/" + INJECTION_USER_WHITELIST_FILE;
		}
		
		internal static string ResolveInjectionServiceFolder()
		{
			var result = "";
			var targetFiles = Directory.GetDirectories(assetsPath, INJECTION_SERVICE_FOLDER, SearchOption.AllDirectories);
			if (targetFiles.Length == 0)
			{
				Debug.LogError(LOG_PREFIX + "Can't find " + INJECTION_SERVICE_FOLDER + " folder! Please report to " + REPORT_EMAIL);
			}
			else
			{
				result = targetFiles[0];
			}

			return result;
		}

		internal static string[] FindLibrariesAt(string dir)
		{
			var result = new string[0];

			if (Directory.Exists(dir))
			{
				result = Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories);
				for (var i = 0; i < result.Length; i++)
				{
					result[i] = result[i].Replace('\\', '/');
				}
			}

			return result;
		}

		internal static void RemoveReadOnlyAttribute(string path)
		{
			if (File.Exists(path))
			{
				var attributes = File.GetAttributes(path);
				if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					attributes = attributes & ~FileAttributes.ReadOnly;
					File.SetAttributes(path, attributes);
				}
			}
		}

		private static void RemoveDirectoryIfEmpty(string directoryName)
		{
			if (Directory.Exists(directoryName) && IsDirectoryEmpty(directoryName))
			{
				FileUtil.DeleteFileOrDirectory(directoryName);
				if (File.Exists(Path.GetDirectoryName(directoryName) + ".meta"))
				{
					FileUtil.DeleteFileOrDirectory(Path.GetDirectoryName(directoryName) + ".meta");
				}
			}
		}

		private static bool IsDirectoryEmpty(string path)
		{
			var dirs = Directory.GetDirectories(path);
			var files = Directory.GetFiles(path);
			return dirs.Length == 0 && files.Length == 0;
		}
		#endregion

		#region assemblies
		internal static int GetAssemblyHash(AssemblyName ass)
		{
			var hashInfo = ass.Name;

			var bytes = ass.GetPublicKeyToken();
			if (bytes != null && bytes.Length == 8)
			{
				hashInfo += PublicKeyTokenToString(bytes);
			}

			// Jenkins hash function (http://en.wikipedia.org/wiki/Jenkins_hash_function)
			var result = 0;
			var len = hashInfo.Length;

			for (var i = 0; i < len; ++i)
			{
				result += hashInfo[i];
				result += (result << 10);
				result ^= (result >> 6);
			}
			result += (result << 3);
			result ^= (result >> 11);
			result += (result << 15);

			return result;
		}

		private static string PublicKeyTokenToString(byte[] bytes)
		{
			var result = "";

			// AssemblyName.GetPublicKeyToken() returns 8 bytes
			for (var i = 0; i < 8; i++)
			{
				result += hexTable[bytes[i]];
			}

			return result;
		}
		#endregion

		internal static bool CheckUnityEventHasActivePersistentListener(SerializedProperty unityEvent)
		{
			if (unityEvent == null) return false;

			var calls = unityEvent.FindPropertyRelative("m_PersistentCalls.m_Calls");
			if (calls == null)
			{
				Debug.LogError(LOG_PREFIX + " Can't find Unity Event calls! Please report to " + REPORT_EMAIL);
				return false;
			}
			if (!calls.isArray)
			{
				Debug.LogError(LOG_PREFIX + " Looks like Unity Event calls are not array anymore! Please report to " + REPORT_EMAIL);
				return false;
			}

			var result = false;

			var callsCount = calls.arraySize;
			for (var i = 0; i < callsCount; i++)
			{
				var call = calls.GetArrayElementAtIndex(i);

				var targetProperty = call.FindPropertyRelative("m_Target");
				var methodNameProperty = call.FindPropertyRelative("m_MethodName");
				var callStateProperty = call.FindPropertyRelative("m_CallState");

				if (targetProperty != null && methodNameProperty != null && callStateProperty != null &&
                    targetProperty.propertyType == SerializedPropertyType.ObjectReference &&
					methodNameProperty.propertyType == SerializedPropertyType.String &&
					callStateProperty.propertyType == SerializedPropertyType.Enum)
				{
					var target = targetProperty.objectReferenceValue;
					var methodName = methodNameProperty.stringValue;
					var callState = (UnityEventCallState)callStateProperty.enumValueIndex;

					if (target != null && !string.IsNullOrEmpty(methodName) && callState != UnityEventCallState.Off)
					{
						result = true;
						break;
					}
				}
				else
				{
					Debug.LogError(LOG_PREFIX + " Can't parse Unity Event call! Please report to " + REPORT_EMAIL);
				}
			}
			return result;
		}
	}
}
#endif