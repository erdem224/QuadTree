﻿using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Editor.Ui
{
  public class UiComponentUtil
  {
    public static GameObject CreateUIElementRoot(string name, MenuCommand menuCommand, Vector2 size)
    {
      GameObject parent = menuCommand.context as GameObject;
      if (parent == null || parent.GetComponentInParent<Canvas>() == null)
      {
        parent = GetOrCreateCanvasGameObject();
      }

      GameObject child = new GameObject(name);

      Undo.RegisterCreatedObjectUndo(child, "Create " + name);
      Undo.SetTransformParent(child.transform, parent.transform, "Parent " + child.name);
      GameObjectUtility.SetParentAndAlign(child, parent);

      RectTransform rectTransform = child.AddComponent<RectTransform>();
      rectTransform.sizeDelta = size;
      if (parent != menuCommand.context) // not a context click, so center in sceneview
      {
        SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), rectTransform);
      }

      Selection.activeGameObject = child;
      return child;
    }

    public static GameObject GetOrCreateCanvasGameObject()
    {
      GameObject selectedGo = Selection.activeGameObject;

      // Try to find a gameobject that is the selected GO or one if its parents.
      Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
      if (canvas != null && canvas.gameObject.activeInHierarchy)
        return canvas.gameObject;

      // No canvas in selection or its parents? Then use just any canvas..
      canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
      if (canvas != null && canvas.gameObject.activeInHierarchy)
        return canvas.gameObject;

      // No canvas in the scene at all? Then create a new one.
      return CreateNewUI();
    }

    private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
    {
      // Find the best scene view
      SceneView sceneView = SceneView.lastActiveSceneView;
      if (sceneView == null && SceneView.sceneViews.Count > 0)
        sceneView = SceneView.sceneViews[0] as SceneView;

      // Couldn't find a SceneView. Don't set position.
      if (sceneView == null || sceneView.camera == null)
        return;

      // Create world space Plane from canvas position.
      Vector2 localPlanePosition;
      Camera camera = sceneView.camera;
      Vector3 position = Vector3.zero;
      if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform,
        new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
      {
        // Adjust for canvas pivot
        localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
        localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

        localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
        localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

        // Adjust for anchoring
        position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
        position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

        Vector3 minLocalPosition;
        minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) +
                             itemTransform.sizeDelta.x * itemTransform.pivot.x;
        minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) +
                             itemTransform.sizeDelta.y * itemTransform.pivot.y;

        Vector3 maxLocalPosition;
        maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) -
                             itemTransform.sizeDelta.x * itemTransform.pivot.x;
        maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) -
                             itemTransform.sizeDelta.y * itemTransform.pivot.y;

        position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
        position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
      }

      itemTransform.anchoredPosition = position;
      itemTransform.localRotation = Quaternion.identity;
      itemTransform.localScale = Vector3.one;
    }

    public static void SetDefaultTextValues(TextMeshProUGUI lbl)
    {
      // Set text values we want across UI elements in default controls.
      // Don't set values which are the same as the default values for the Text component,
      // since there's no point in that, and it's good to keep them as consistent as possible.
      lbl.color = DefaultComponentStyle.s_TextColor;
      lbl.fontSize = DefaultComponentStyle.s_FontSize;
    }

    public static GameObject CreateNewUI()
    {
      // Root for the UI
      var root = new GameObject("Canvas");
      root.layer = LayerMask.NameToLayer(DefaultComponentStyle.kUILayerName);
      Canvas canvas = root.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      root.AddComponent<CanvasScaler>();
      root.AddComponent<GraphicRaycaster>();
      Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

      // if there is no event system add one...
      CreateEventSystem(false, null);
      return root;
    }

    private static void CreateEventSystem(bool select, GameObject parent)
    {
      var esys = Object.FindObjectOfType<EventSystem>();
      if (esys == null)
      {
        var eventSystem = new GameObject("EventSystem");
        GameObjectUtility.SetParentAndAlign(eventSystem, parent);
        esys = eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
      }

      if (select && esys != null)
      {
        Selection.activeGameObject = esys.gameObject;
      }
    }

    public static void SetDefaultColorTransitionValues(Selectable slider)
    {
      ColorBlock colors = slider.colors;
      colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
      colors.pressedColor     = new Color(0.698f, 0.698f, 0.698f);
      colors.disabledColor    = new Color(0.521f, 0.521f, 0.521f);
    }
  }
}