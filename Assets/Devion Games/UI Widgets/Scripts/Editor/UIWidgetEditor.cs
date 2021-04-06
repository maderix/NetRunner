﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace DevionGames.UIWidgets
{
	public class UIWidgetEditor : EditorWindow
	{
		[UnityEditor.MenuItem ("Tools/Devion Games/UI Widgets", false, -999)]
		public static void ShowWindow ()
		{
			UIWidgetEditor window = EditorWindow.GetWindow<UIWidgetEditor> ("UI Manager");
			Vector2 size = new Vector2 (250f, 200f);
			window.minSize = size;
			window.wantsMouseMove = true;
		}

		[SerializeField]
		private string[] paths;
		[SerializeField]
		private UIWidget[] targets;
		[SerializeField]
		private CanvasGroup[] canvasGroups;

		[SerializeField]
		private Vector2 scroll;
		[SerializeField]
		private int index = -1;

		private void OnEnable ()
		{
			FindWidgets ();

		}

		private void OnFocus ()
		{
			Repaint ();
		}

		private void OnGUI ()
		{
			GUILayout.BeginHorizontal (EditorStyles.toolbar);
			if (GUILayout.Button ("Refresh", EditorStyles.toolbarButton) || targets.Any (x => x == null)) {
				FindWidgets ();
				Focus ();
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();

			scroll = EditorGUILayout.BeginScrollView (scroll);
			for (int i = 0; i < targets.Length; i++) {
				GUIStyle style = new GUIStyle ("PopupCurveSwatchBackground") {
					padding = new RectOffset ()
				};
				if (i == index) {
					style = new GUIStyle ("MeTransitionSelectHead") {
						stretchHeight = false,

					};
					style.overflow = new RectOffset (-1, -2, -2, 2);
				}
				GUILayout.BeginVertical (style);
				GUILayout.BeginHorizontal ();
				GUILayout.Label (paths [i]);

				Rect elementRect = GUILayoutUtility.GetLastRect ();
				elementRect.width = Screen.width - 110f;
				Event ev = Event.current;
				switch (ev.rawType) {
				case EventType.MouseUp:
					if (elementRect.Contains (Event.current.mousePosition)) {
						if (Event.current.button == 0) {
							index = i;
							Selection.activeGameObject = targets [i].gameObject;
							Event.current.Use ();
						} 
					}
					break;
				}
				GUILayout.FlexibleSpace ();
				CanvasGroup canvasGroup = canvasGroups [i];
				if (GUILayout.Button ("Focus", GUILayout.Width (50f))) {
					Transform current = canvasGroup.transform;
					while (current.parent != null && current.parent.GetComponent<Canvas> () == null) {
						current = current.parent;
					}
					current.SetAsLastSibling ();
				}
				if (GUILayout.Button (canvasGroup.alpha > 0.01f ? "Hide" : "Show", GUILayout.Width (50f))) {
					if (canvasGroup.alpha > 0.01f) {
						//Hide
						canvasGroup.alpha = 0f;
						canvasGroup.interactable = false;
						canvasGroup.blocksRaycasts = false;
					} else {
						//Show
						canvasGroup.alpha = 1f;
						canvasGroup.interactable = true;
						canvasGroup.blocksRaycasts = true;
					}
				}
				GUILayout.EndHorizontal ();
				GUILayout.EndVertical ();

			}
			EditorGUILayout.EndScrollView ();
		}

		private void FindWidgets ()
		{
			targets = FindInScene<UIWidget> ().ToArray ();
			canvasGroups = targets.Select (x => x.gameObject.GetComponent<CanvasGroup> ()).ToArray ();
			List<string> widgetPaths = new List<string> ();
			for (int i = 0; i < targets.Length; i++) {
				widgetPaths.Add (GetHierarchyPath (targets [i].transform));
			}
			paths = widgetPaths.ToArray ();
		}

		public static string GetHierarchyPath (Transform transform)
		{
			string path = transform.name;
			while (transform.parent != null) {
				path = transform.parent.name + "/" + path;
				transform = transform.parent;
			}
			return path;
		}

		/// <summary>
		/// Finds components the in scene.
		/// </summary>
		/// <returns>The in scene.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<T> FindInScene<T> () where T : Component
		{
			T[] comps = Resources.FindObjectsOfTypeAll (typeof(T)) as T[];

			List<T> list = new List<T> ();

			foreach (T comp in comps) {
				if (comp.gameObject.hideFlags == 0) {
					string path = AssetDatabase.GetAssetPath (comp.gameObject);
					if (string.IsNullOrEmpty (path))
						list.Add (comp);
				}
			}
			return list;
		}
	}
}