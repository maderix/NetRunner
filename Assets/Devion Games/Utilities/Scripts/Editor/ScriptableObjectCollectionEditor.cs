using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DevionGames
{
	/// <summary>
	/// A collection class for ScriptableObjects.
	/// </summary>
	public class ScriptableObjectCollectionEditor<T> : CollectionEditor<T> where T: ScriptableObject, INameable
	{
		[SerializeField]
		protected List<T> items;
		[SerializeField]
		protected UnityEngine.Object target;

		protected Editor editor;

		public ScriptableObjectCollectionEditor (UnityEngine.Object target, List<T> items)
		{
			this.target = target;
			this.items = items;
            sidebarRect.width = EditorPrefs.GetFloat("CollectionEditorSidebarWidth" + ToolbarName, sidebarRect.width);
        }

		protected override List<T> Items {
			get {
				return items;
			}
		}

		protected override bool MatchesSearch (T item, string search)
		{
			return item.Name.ToLower ().Contains (search.ToLower ()) || search == "Search...";
		}

        protected override void Create ()
		{
			Type[] types = AppDomain.CurrentDomain.GetAssemblies ().SelectMany (assembly => assembly.GetTypes ()).Where (type => typeof(T).IsAssignableFrom (type) && type.IsClass && !type.IsAbstract).ToArray ();		
			if (types.Length > 1) {
				GenericMenu menu = new GenericMenu ();
				foreach (Type type in types) {
					Type mType = type;
					menu.AddItem (new GUIContent (mType.Name), false, delegate() {
						CreateItem (mType);
					});		
				}
				menu.ShowAsContext ();
			} else {
				CreateItem (types [0]);
			}
		}

		protected virtual void CreateItem (Type type)
		{

			T item = (T)ScriptableObject.CreateInstance (type);
			item.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset (item, target);
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
			Items.Add (item);
			selectedItem = item;
			EditorUtility.SetDirty (target);
		}

		protected override void Remove (T item)
		{
			if (EditorUtility.DisplayDialog ("Delete Item", "Are you sure you want to delete " + item.Name + "?", "Yes", "No")) {
				GameObject.DestroyImmediate (item, true);
				AssetDatabase.SaveAssets ();
				AssetDatabase.Refresh ();
				Items.Remove (item);
                OnDestroy();
				EditorUtility.SetDirty (target);
			}
		}

		protected override void DrawItem (T item)
		{
			if (item != null) {
				if (editor == null || editor.target != item) {
					editor = Editor.CreateEditor (item);
				}
				editor.OnInspectorGUI ();
			}
		}

        public override void OnDestroy()
        {
            ScriptableObject.DestroyImmediate(editor);
        }

        protected override string GetSidebarLabel (T item)
		{
			return item.Name;
		}
	}
}