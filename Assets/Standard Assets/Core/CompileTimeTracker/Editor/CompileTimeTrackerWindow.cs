using System;
using System.Collections.Generic;
using System.Linq;
using Standard_Assets.Core.CompileTimeTracker.Editor.Extensions;
using Standard_Assets.Core.CompileTimeTracker.Editor.Util;
using UnityEditor;
using UnityEngine;

namespace Standard_Assets.Core.CompileTimeTracker.Editor {
  [InitializeOnLoad]
  public class CompileTimeTrackerWindow : EditorWindow {
    // PRAGMA MARK - Static
    static CompileTimeTrackerWindow() {
      CompileTimeTracker.KeyframeAdded += CompileTimeTrackerWindow.LogCompileTimeKeyframe;
    }

    [MenuItem("Window/Compile Time Tracker Window")]
    public static void Open() {
      EditorWindow.GetWindow<CompileTimeTrackerWindow>(false, "Compile Timer Tracker", true);
    }

    private static void LogCompileTimeKeyframe(CompileTimeKeyframe keyframe) {
      bool dontLogToConsole = !CompileTimeTrackerWindow.LogToConsole;
      if (dontLogToConsole) {
        return;
      }

      string compilationFinishedLog = "Compilation Finished: " + TrackingUtil.FormatMSTime(keyframe.elapsedCompileTimeInMS);
      if (keyframe.hadErrors) {
        compilationFinishedLog += " (error)";
      }
      global::UnityEngine.Debug.Log(compilationFinishedLog);
    }

    private static bool ShowErrors {
      get { return EditorPrefs.GetBool("CompileTimeTrackerWindow.ShowErrors"); }
      set { EditorPrefs.SetBool("CompileTimeTrackerWindow.ShowErrors", value); }
    }

    private static bool OnlyToday {
      get { return EditorPrefs.GetBool("CompileTimeTrackerWindow.OnlyToday"); }
      set { EditorPrefs.SetBool("CompileTimeTrackerWindow.OnlyToday", value); }
    }

    private static bool OnlyYesterday {
      get { return EditorPrefs.GetBool("CompileTimeTrackerWindow.OnlyYesterday"); }
      set { EditorPrefs.SetBool("CompileTimeTrackerWindow.OnlyYesterday", value); }
    }

    private static bool LogToConsole {
      get { return EditorPrefs.GetBool("CompileTimeTrackerWindow.LogToConsole", defaultValue: true); }
      set { EditorPrefs.SetBool("CompileTimeTrackerWindow.LogToConsole", value); }
    }


    // PRAGMA MARK - Internal
    private Vector2 _scrollPosition;

    void OnGUI() {
      Rect screenRect = this.position;
      int totalCompileTimeInMS = 0;

      // show filters
      EditorGUILayout.BeginHorizontal(GUILayout.Height(20.0f));
        EditorGUILayout.Space();
        float toggleRectWidth = screenRect.width / 4.0f;
        Rect toggleRect = new Rect(0.0f, 0.0f, width: toggleRectWidth, height: 20.0f);

        // Psuedo enum logic here
        if (CompileTimeTrackerWindow.OnlyToday && CompileTimeTrackerWindow.OnlyYesterday) {
          CompileTimeTrackerWindow.OnlyYesterday = false;
        }

        if (!CompileTimeTrackerWindow.OnlyToday && !CompileTimeTrackerWindow.OnlyYesterday) {
          CompileTimeTrackerWindow.OnlyToday = true;
        }

        bool newOnlyToday = GUI.Toggle(toggleRect, CompileTimeTrackerWindow.OnlyToday, "Today", (GUIStyle)"Button");
        if (newOnlyToday != CompileTimeTrackerWindow.OnlyToday) {
          CompileTimeTrackerWindow.OnlyToday = newOnlyToday;
          CompileTimeTrackerWindow.OnlyYesterday = !newOnlyToday;
        }

        toggleRect.position = toggleRect.position.AddX(toggleRectWidth);
        bool newOnlyYesterday = GUI.Toggle(toggleRect, CompileTimeTrackerWindow.OnlyYesterday, "Yesterday", (GUIStyle)"Button");
        if (newOnlyYesterday != CompileTimeTrackerWindow.OnlyYesterday) {
          CompileTimeTrackerWindow.OnlyYesterday = newOnlyYesterday;
          CompileTimeTrackerWindow.OnlyToday = !newOnlyYesterday;
        }
        // End psuedo enum logic

        toggleRect.position = toggleRect.position.AddX(2.0f * toggleRectWidth);
        CompileTimeTrackerWindow.ShowErrors = GUI.Toggle(toggleRect, CompileTimeTrackerWindow.ShowErrors, "Errors", (GUIStyle)"Button");
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal(GUILayout.Height(20.0f));
        CompileTimeTrackerWindow.LogToConsole = EditorGUILayout.Toggle("Log Compile Time", CompileTimeTrackerWindow.LogToConsole);
      EditorGUILayout.EndHorizontal();

      this._scrollPosition = EditorGUILayout.BeginScrollView(this._scrollPosition, GUILayout.Height(screenRect.height - 60.0f));
        foreach (CompileTimeKeyframe keyframe in this.GetFilteredKeyframes()) {
          string compileText = string.Format("({0:hh:mm tt}): ", keyframe.Date);
          compileText += TrackingUtil.FormatMSTime(keyframe.elapsedCompileTimeInMS);
          if (keyframe.hadErrors) {
            compileText += " (error)";
          }
          GUILayout.Label(compileText);

          totalCompileTimeInMS += keyframe.elapsedCompileTimeInMS;
        }
      EditorGUILayout.EndScrollView();

      string statusBarText = "Total compile time: " + TrackingUtil.FormatMSTime(totalCompileTimeInMS);
      if (EditorApplication.isCompiling) {
        statusBarText = "Compiling.. || " + statusBarText;
      }
      GUILayout.Label(statusBarText);
    }

    void OnEnable() {
      EditorApplicationCompilationUtil.StartedCompiling += this.HandleEditorStartedCompiling;
      CompileTimeTracker.KeyframeAdded += this.HandleCompileTimeKeyframeAdded;
    }

    void OnDisable() {
      EditorApplicationCompilationUtil.StartedCompiling -= this.HandleEditorStartedCompiling;
      CompileTimeTracker.KeyframeAdded -= this.HandleCompileTimeKeyframeAdded;
    }

    private IEnumerable<CompileTimeKeyframe> GetFilteredKeyframes() {
      IEnumerable<CompileTimeKeyframe> filteredKeyframes = CompileTimeTracker.GetCompileTimeHistory();
      if (!CompileTimeTrackerWindow.ShowErrors) {
        filteredKeyframes = filteredKeyframes.Where(keyframe => !keyframe.hadErrors);
      }

      if (CompileTimeTrackerWindow.OnlyToday) {
        filteredKeyframes = filteredKeyframes.Where(keyframe => DateTimeUtil.SameDay(keyframe.Date, DateTime.Now));
      } else if (CompileTimeTrackerWindow.OnlyYesterday) {
        filteredKeyframes = filteredKeyframes.Where(keyframe => DateTimeUtil.SameDay(keyframe.Date, DateTime.Now.AddDays(-1)));
      }

      return filteredKeyframes;
    }

    private void HandleEditorStartedCompiling() {
      this.Repaint();
    }

    private void HandleCompileTimeKeyframeAdded(CompileTimeKeyframe keyframe) {
      this.Repaint();
    }
  }
}