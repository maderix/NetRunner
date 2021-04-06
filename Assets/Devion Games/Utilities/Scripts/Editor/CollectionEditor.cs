using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

namespace DevionGames{
	/// <summary>
	/// Base class for a collection of items.
	/// </summary>
	public abstract class CollectionEditor<T>: ICollectionEditor {
		private const  float LIST_MIN_WIDTH = 200f;
		private const  float LIST_MAX_WIDTH = 400f;
		private const float LIST_RESIZE_WIDTH = 10f;


		protected Rect sidebarRect = new Rect(0,30,200,1000);
		protected Vector2 scrollPosition;
		protected string searchString="Search...";
		protected Vector2 sidebarScrollPosition;

		protected T selectedItem;
		protected abstract List<T> Items {get;}
        protected virtual bool AddButton {
            get { return true; }
        }
        protected virtual bool RemoveButton
        {
            get { return true; }
        }


        public virtual string ToolbarName
        {
            get{
                Type type = GetType();
                if (type.IsGenericType)
                {
                    return ObjectNames.NicifyVariableName(type.GetGenericArguments()[0].Name);
                }
                else
                {
                    return ObjectNames.NicifyVariableName(type.Name.Replace("Editor", ""));
                }
            }
        }

        public void OnGUI(Rect position){
            int index = EditorPrefs.GetInt("CollectionEditorItemIndex",-1);
            if (index != -1 && index < Items.Count) {
                selectedItem = Items[index];
            }
			sidebarRect = new Rect (position.x, position.y, sidebarRect.width, position.height);
			GUILayout.BeginArea(sidebarRect,"",Styles.background);

			DoSearchGUI ();
            Color color = GUI.color;
            if (AddButton)
            {
                EditorGUILayout.Space();
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Create..."))
                {
                    GUI.FocusControl("");
                    Create();
                    if (Items.Count > 0)
                    {
                        selectedItem = Items[Items.Count - 1];
                    }
                }
                GUI.backgroundColor = color;
            }
			EditorGUILayout.Space ();

			sidebarScrollPosition = GUILayout.BeginScrollView (sidebarScrollPosition);
			for (int i = 0; i < Items.Count; i++) {
				T currentItem = Items[i];
				if(!MatchesSearch(currentItem,searchString)){
					continue;
				}
				GUILayout.BeginHorizontal();
				color = GUI.backgroundColor;
				GUI.backgroundColor = (selectedItem != null && selectedItem.Equals(currentItem) ? new Color(0, 1.0f, 0, 0.3f) : new Color(0, 0, 0, 0.0f));
				if (GUILayout.Button (ButtonLabel(i, currentItem), Styles.selectButton,GUILayout.Height(25))) {
					GUI.FocusControl("");
					selectedItem = currentItem;
                    EditorPrefs.SetInt("CollectionEditorItemIndex",i);
					Select(selectedItem);
				}

				GUI.backgroundColor=color;
                if (RemoveButton)
                {
                    if (GUILayout.Button("", Styles.minusButton, GUILayout.Width(18)))
                    {
                        GUI.FocusControl("");
                        Remove(currentItem);
                    }
                }
				GUILayout.EndHorizontal();
				
			}
			GUILayout.EndScrollView ();
			GUILayout.EndArea ();


			Rect rect = new Rect (sidebarRect.width, sidebarRect.y, position.width - sidebarRect.width, position.height);

			GUILayout.BeginArea (rect, "",Styles.background);
			scrollPosition = GUILayout.BeginScrollView (scrollPosition,EditorStyles.inspectorDefaultMargins);
			if (selectedItem != null && Items.Contains(selectedItem)) {
				DrawItem(selectedItem);
			}
			GUILayout.EndScrollView ();
			GUILayout.EndArea ();
			ResizeSidebar();
		}

        public virtual void OnDestroy() { }

		/// <summary>
		/// Select an item.
		/// </summary>
		/// <param name="item">Item.</param>
		protected virtual void Select(T item){}

		/// <summary>
		/// Create an item.
		/// </summary>
		protected virtual void Create(){}

		/// <summary>
		/// Remove the specified item from collection.
		/// </summary>
		/// <param name="item">Item.</param>
		protected virtual void Remove(T item){}

		/// <summary>
		/// Draws the item properties.
		/// </summary>
		/// <param name="item">Item.</param>
		protected virtual void DrawItem(T item){}

		/// <summary>
		/// Gets the sidebar label displayed in sidebar.
		/// </summary>
		/// <returns>The sidebar label.</returns>
		/// <param name="item">Item.</param>
		protected abstract string GetSidebarLabel(T item);

        protected virtual string ButtonLabel(int index, T item) {
            return "# " + index + "  " + GetSidebarLabel(item);
        }

		/// <summary>
		/// Checks for search.
		/// </summary>
		/// <returns><c>true</c>, if search was matchesed, <c>false</c> otherwise.</returns>
		/// <param name="item">Item.</param>
		/// <param name="search">Search.</param>
		protected abstract bool MatchesSearch (T item, string search);

		protected virtual void DoSearchGUI(){
			searchString = UnityEditorUtility.SearchField (searchString,GUILayout.Width(sidebarRect.width-20));
		}

		private void ResizeSidebar(){
			Rect rect = new Rect (sidebarRect.width - LIST_RESIZE_WIDTH*0.5f, sidebarRect.y, LIST_RESIZE_WIDTH, sidebarRect.height);
			EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			Event ev = Event.current;
			switch (ev.rawType) {
			case EventType.MouseDown:
				if(rect.Contains(ev.mousePosition)){
					GUIUtility.hotControl = controlID;
					ev.Use();
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;
					ev.Use();
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID)
				{
					sidebarRect.width=ev.mousePosition.x;
					sidebarRect.width=Mathf.Clamp(sidebarRect.width,LIST_MIN_WIDTH,LIST_MAX_WIDTH);
                    EditorPrefs.SetFloat("CollectionEditorSidebarWidth"+ToolbarName,sidebarRect.width);
					ev.Use();
				}
				break;
			}
		}

        public static class Styles{
			public static GUIStyle minusButton;
			public static GUIStyle selectButton;
			public static GUIStyle background;

			static Styles(){
				minusButton = new GUIStyle ("OL Minus"){
					margin=new RectOffset(0,0,4,0)
				};
				selectButton = new GUIStyle ("MeTransitionSelectHead"){
					alignment= TextAnchor.MiddleLeft
				};
				background = new GUIStyle("ProgressBarBack");
			}
		}
	}
}