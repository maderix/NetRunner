using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;

namespace DevionGames{
	/// <summary>
	/// Reorderable list with dynamic height.
	/// </summary>
	public class ReorderableObjectList {
		public delegate void HeaderCallbackDelegate(Rect rect);
		public ReorderableObjectList.HeaderCallbackDelegate drawHeaderCallback;
		public delegate void AddCallbackDelegate(ReorderableObjectList list);
		public ReorderableObjectList.AddCallbackDelegate onAddCallback;
		public delegate void RemoveCallbackDelegate(ReorderableObjectList list);
		public ReorderableObjectList.RemoveCallbackDelegate onRemoveCallback;
		
		public float headerHeight = 18f;
		public float footerHeight = 13f;
		private int selectedIndex;
		public int index{
			get{return this.selectedIndex;}
			set{this.selectedIndex = value;}
		}
		public int count
		{
			get{
				return this.elements.arraySize;
			}
		}
		
		private SerializedObject serializedObject;
		private SerializedProperty elements;
		private bool draggable;
		private bool displayHeader;
		private static ReorderableObjectList.Defaults defaults;
		private bool isDragging;
		private Rect draggingLineRect;
		private int dragIndex=-1;
		private int swapIndex=-1;
		
		public ReorderableObjectList(SerializedObject serializedObject,SerializedProperty elements){
			this.serializedObject = serializedObject;
			this.elements = elements;
			this.draggable = true;
			this.displayHeader = true;
		}
		
		public ReorderableObjectList(SerializedObject serializedObject,SerializedProperty elements,bool draggable,bool displayHeader){
			this.serializedObject = serializedObject;
			this.elements = elements;
			this.draggable = draggable;
			this.displayHeader = displayHeader;
		}
		
		public void DoLayoutList(){
			if (defaults == null) {
				defaults= new ReorderableObjectList.Defaults();
			}
			serializedObject.Update ();
			DoListHeader ();
			DoListElements ();
			DoListFooter ();
			serializedObject.ApplyModifiedProperties ();
		}
		
		private void DoListHeader()
		{
			if (displayHeader) {
				Rect headerRect = GUILayoutUtility.GetRect (0f, 18f, new GUILayoutOption[] { GUILayout.ExpandWidth (true) });
				defaults.DrawHeaderBackground (headerRect);
				headerRect.xMin = headerRect.xMin + 6f;
				headerRect.xMax = headerRect.xMax - 6f;
				headerRect.height = headerRect.height - 2f;
				headerRect.y = headerRect.y + 1f;
				if (this.drawHeaderCallback != null)
				{
					this.drawHeaderCallback(headerRect);
				}
			}
		}
		
		private void DoListElements(){
			Rect elementsRect = GUILayoutUtility.GetRect(10f, GetHeight(), new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
			
			if (Event.current.type == EventType.Repaint)
			{
				ReorderableObjectList.defaults.boxBackground.Draw(elementsRect,false,false,false,false);
			}
			
			elementsRect.y += 2f;
			elementsRect.width -= 21f;
			elementsRect.x += 17f;
			List<Rect> rects = new List<Rect> ();
			if (count == 0) {
				elementsRect.x-=10f;
				EditorGUI.LabelField(elementsRect,"List is Empty");
			}
			for (int i=0; i<count; i++) {
				Rect header = new Rect (elementsRect.x+3f-17f,elementsRect.y,elementsRect.width-3f+17f,GetElementHeight(i));
				if (Event.current.type == EventType.Repaint)
				{
					Rect handle=new Rect(elementsRect.x-12f,elementsRect.y+5f,10f,elementsRect.height);;
					if(selectedIndex == i && !isDragging){
						ReorderableObjectList.defaults.selectButton.Draw(header,false,false,false,false);
					}
					ReorderableObjectList.defaults.draggingHandle.Draw(handle,false,false,false,false);
					if(isDragging){
						ReorderableObjectList.defaults.selectButton.Draw(draggingLineRect,false,false,false,false);
					}
				}
				
				SerializedProperty property = elements.GetArrayElementAtIndex(i);
				bool enabled=GUI.enabled;
				if(isDragging){
					GUI.enabled=!(i == index); 
				}
				SerializedObject obj = new SerializedObject (property.objectReferenceValue);
				obj.Update();
				property = obj.GetIterator ();
				property.NextVisible(true);
				elementsRect.height=EditorGUI.GetPropertyHeight(property);
				EditorGUI.PropertyField(elementsRect,property);
				elementsRect.y+=21f;
				
				while (property.NextVisible(false)) {
					float height=EditorGUI.GetPropertyHeight(property);
					elementsRect.height=height;
					EditorGUI.PropertyField(elementsRect,property);
					elementsRect.y+=height+2f;
				}
				GUI.enabled=enabled;
				obj.ApplyModifiedProperties();
				rects.Add(new Rect(header.x,header.y,header.width,GetElementHeight(i)));
				
			}
			
			HandleEvents (rects);
		}
		
		private void HandleEvents(List<Rect> rects){
			for (int i=0; i< rects.Count; i++) {
				Rect elementRect=rects[i];
				
				switch (Event.current.type) {
				case EventType.MouseDown:
					if (elementRect.Contains (Event.current.mousePosition) && Event.current.button == 0) {
						selectedIndex=i;
						dragIndex = i;
						isDragging = draggable?true:false;
						draggingLineRect = new Rect (elementRect.x, elementRect.y, elementRect.width, 2);
						GUI.FocusControl ("");
						Event.current.Use();
					}
					break;
				case EventType.MouseUp:
					if(isDragging){
						isDragging = false;
						Event.current.Use();
					}
					break;
				case EventType.MouseDrag:
					if (elementRect.Contains (Event.current.mousePosition) && Event.current.button == 0 && count > 1 && draggable) {
						if (Event.current.mousePosition.y < elementRect.y + elementRect.height * 0.5f) {
							draggingLineRect = new Rect (elementRect.x, elementRect.y, elementRect.width, 2);
							swapIndex = (dragIndex > i ? i : i - 1);
						}
						if (Event.current.mousePosition.y > elementRect.y + elementRect.height * 0.5f) {
							draggingLineRect = new Rect (elementRect.x, elementRect.y + elementRect.height, elementRect.width, 2);
							swapIndex = (dragIndex > i ? i + 1 : i);
						}
						Event.current.Use();
					}
					break;
				}
			}
			
			if (swapIndex != -1 && !isDragging && dragIndex != -1 && draggable) {
				
				if (count > dragIndex && dragIndex>-1) {
					elements.MoveArrayElement(dragIndex,swapIndex);
					selectedIndex=swapIndex;
				}
				swapIndex=-1;
			}
			
			if (!isDragging) {
				dragIndex=-1;	
			}
		}
		
		private float GetElementHeight(int index){
			float height = 0f;
			SerializedProperty property=elements.GetArrayElementAtIndex(index);
			SerializedObject obj = new SerializedObject (property.objectReferenceValue);
			property = obj.GetIterator ();
			
			property.NextVisible(true);
			while (property.NextVisible(false)) {
				height+=EditorGUI.GetPropertyHeight(property)+2f;
			}
			return height + 21f;
		}
		
		public float GetHeight(){
			float height = 0f;
			for (int i=0; i<elements.arraySize; i++) {
				height+=GetElementHeight(i);
			}
			return Mathf.Max(height,21f)+7f;
		}
		
		private void DoListFooter()
		{
			Rect footerRect = GUILayoutUtility.GetRect(4f, this.footerHeight, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
			ReorderableObjectList.defaults.DrawFooter(footerRect, this);
		}
		
		public class Defaults{
			public GUIContent iconToolbarPlus;
			public GUIContent iconToolbarPlusMore;
			public GUIContent iconToolbarMinus;
			
			public readonly GUIStyle draggingHandle;
			public readonly GUIStyle headerBackground;
			public readonly GUIStyle footerBackground;
			public readonly GUIStyle boxBackground;
			public readonly GUIStyle preButton;
			public readonly GUIStyle selectButton;


			public Defaults(){
				iconToolbarPlus=new GUIContent(EditorGUIUtility.FindTexture("Toolbar Plus"));
				iconToolbarPlusMore=new GUIContent(EditorGUIUtility.FindTexture("Toolbar Plus More"));
				iconToolbarMinus=new GUIContent(EditorGUIUtility.FindTexture("Toolbar Minus"));
				
				draggingHandle=new GUIStyle("RL DragHandle");
				headerBackground=new GUIStyle("RL Header");
				footerBackground=new GUIStyle("RL Footer");
				boxBackground= new GUIStyle("RL Background");
				preButton  =new GUIStyle("RL FooterButton");

				selectButton = new GUIStyle ("MeTransitionSelectHead"){
					alignment= TextAnchor.MiddleLeft
				};
			}
			
			public void DrawHeaderBackground(Rect headerRect)
			{
				if (Event.current.type == EventType.Repaint)
				{
					this.headerBackground.Draw(headerRect, false, false, false, false);
				}
			}
			
			public void DrawFooter(Rect rect, ReorderableObjectList list)
			{
				float single = rect.xMax;
				float single1 = single - 8f;
				if (list.onAddCallback != null)
				{
					single1 = single1 - 25f;
				}
				if (list.onRemoveCallback != null)
				{
					single1 = single1 - 25f;
				}
				rect = new Rect(single1, rect.y, single - single1, rect.height);
				Rect rect1 = new Rect(single1 + 4f, rect.y - 3f, 25f, 13f);
				Rect rect2 = new Rect(single - 29f, rect.y - 3f, 25f, 13f);
				if (Event.current.type == EventType.Repaint)
				{
					this.footerBackground.Draw(rect, false, false, false, false);
				}
				if (list.onAddCallback != null)
				{
					if (GUI.Button(rect1, this.iconToolbarPlus, this.preButton))
					{
						list.onAddCallback(list);
					}
				}
				if (list.onRemoveCallback != null)
				{
					EditorGUI.BeginDisabledGroup(list.index < 0 || list.index >= list.count);
					if (GUI.Button(rect2, this.iconToolbarMinus, this.preButton))
					{
						list.onRemoveCallback(list);
					}
					EditorGUI.EndDisabledGroup();
				}
			}
		}
		
	}
}