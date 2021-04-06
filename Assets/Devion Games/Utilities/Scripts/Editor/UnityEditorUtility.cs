using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Reflection;
using System;
using System.Linq;

namespace DevionGames{
	/// <summary>
	/// Editor helper class.
	/// </summary>
	public static class UnityEditorUtility {
		private readonly static Dictionary<Type, object[]> typeAttributeCache;
		private readonly static Dictionary<FieldInfo, object[]> fieldAttributeCache;
        private static GUISkin m_CustomEditorSkin;

        static UnityEditorUtility(){
			typeAttributeCache = new Dictionary<Type, object[]>();
			fieldAttributeCache = new Dictionary<FieldInfo, object[]>();
            m_CustomEditorSkin = Resources.Load<GUISkin>("EditorSkin");
        }

		/// <summary>
		/// Gets custom attributes from type.
		/// </summary>
		/// <returns>The custom attributes.</returns>
		/// <param name="type">Type.</param>
		public static object[] GetCustomAttributes(Type type)
		{
			object[] customAttributes;
			if (!UnityEditorUtility.typeAttributeCache.TryGetValue(type, out customAttributes))
			{
				customAttributes = type.GetCustomAttributes(true);
				UnityEditorUtility.typeAttributeCache.Add(type, customAttributes);
			}
			return customAttributes;
		}

		/// <summary>
		/// Gets the custom attributes.
		/// </summary>
		/// <returns>The custom attributes.</returns>
		/// <param name="field">Field.</param>
		public static object[] GetCustomAttributes(FieldInfo field)
		{
			object[] customAttributes;
			if (!UnityEditorUtility.fieldAttributeCache.TryGetValue(field, out customAttributes))
			{
				customAttributes = field.GetCustomAttributes(true);
				UnityEditorUtility.fieldAttributeCache.Add(field, customAttributes);
			}
			return customAttributes;
		}

		/// <summary>
		/// Gets the attribute.
		/// </summary>
		/// <returns>The attribute.</returns>
		/// <param name="field">Field.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetAttribute<T>(this FieldInfo field){
			object[] objArray=UnityEditorUtility.GetCustomAttributes(field);
			for (int i = 0; i < (int)objArray.Length; i++)
			{
				if (objArray[i].GetType() == typeof(T) || objArray[i].GetType().IsSubclassOf(typeof(T)))
				{
					return (T)objArray[i];
				}
			}
			return default(T);		
		}

		/// <summary>
		/// Gets the attribute.
		/// </summary>
		/// <returns>The attribute.</returns>
		/// <param name="type">Type.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetAttribute<T>(this Type type){
			object[] objArray=UnityEditorUtility.GetCustomAttributes(type);
			for (int i = 0; i < (int)objArray.Length; i++)
			{
				if (objArray[i].GetType() == typeof(T) || objArray[i].GetType().IsSubclassOf(typeof(T)))
				{
					return (T)objArray[i];
				}
			}
			return default(T);		
		}

		/// <summary>
		/// Search field gui.
		/// </summary>
		/// <returns>The field.</returns>
		/// <param name="search">Search.</param>
		/// <param name="options">Options.</param>
		public static string[] SearchField(string search,string filter,List<string> filters,params GUILayoutOption[] options){
			GUILayout.BeginHorizontal ();
			string[] result = new string[]{filter,search};
			string before = search;

			Rect rect = GUILayoutUtility.GetRect (GUIContent.none,(GUIStyle)"ToolbarSeachTextFieldPopup",options);
			Rect rect1 = GUILayoutUtility.GetLastRect ();
			rect1.width = 20;

			int filterIndex = filters.IndexOf (filter);
			filterIndex = EditorGUI.Popup (rect1, filterIndex, filters.ToArray (),"label");
			if (filterIndex != -1) {
				result [0] = filters [filterIndex];
				if(filters.Contains(search)){
					before=result[0];
				}
			}
			string after = EditorGUI.TextField (rect,"", before, (GUIStyle)"ToolbarSeachTextFieldPopup");

			if (GUILayout.Button ("", (GUIStyle)"ToolbarSeachCancelButton", GUILayout.Width (18f))) {
				after = result[0];
				GUIUtility.keyboardControl = 0;
			}
			GUILayout.EndHorizontal();
			result [1] = after;
			return result;
		}

		public static string SearchField(string search,params GUILayoutOption[] options){
			GUILayout.BeginHorizontal ();
			string before = search;
			string after = EditorGUILayout.TextField ("", before, "SearchTextField",options);
			
			if (GUILayout.Button ("", "SearchCancelButton", GUILayout.Width (18f))) {
				after = "Search...";
				GUIUtility.keyboardControl = 0;
			}
			GUILayout.EndHorizontal();
			return after;
		}

		/// <summary>
		/// Finds components the in scene.
		/// </summary>
		/// <returns>The in scene.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<T> FindInScene<T> () where T : Component
		{
			T[] comps = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
			
			List<T> list = new List<T>();
			
			foreach (T comp in comps)
			{
				if (comp.gameObject.hideFlags == 0)
				{
					string path = AssetDatabase.GetAssetPath(comp.gameObject);
					if (string.IsNullOrEmpty(path)) list.Add(comp);
				}
			}
			return list;
		}

		public static void DrawProperties(SerializedObject obj,params string[] properties){
			for (int i=0; i<properties.Length; i++) {
				SerializedProperty property=obj.FindProperty(properties[i]);
				if(property != null){
					EditorGUILayout.PropertyField(obj.FindProperty(properties[i]));
				}
			}
		}

        public static T[] FindAssets<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets.ToArray();
        }

        public static bool RightArrowButton(string text, params GUILayoutOption[] options)
        {
            return RightArrowButton(new GUIContent(text), options);
        }

     

        public static bool RightArrowButton(GUIContent content, params GUILayoutOption[] options)
        {
            GUI.skin = m_CustomEditorSkin;
            bool result = false;
            if (GUILayout.Button(content, "leftbutton", options))
            {
                result = true;
            }
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.y += 5f;
            rect.x += rect.width - 14f;
            GUI.Label(rect, "", "rightarrow");
            GUI.skin = null;
            return result;
        }

        public static void Seperator() {
            GUIStyle style=new GUIStyle("IN Title");
            style.fixedHeight = 1f;
            GUILayout.Label(GUIContent.none,style);
        }

        private static GUIStyle m_LineStyle;
        private static GUIStyle LineStyle
        {
            get
            {
                if (m_LineStyle == null)
                {

                    m_LineStyle = new GUIStyle("IN Title");
                    m_LineStyle.fixedHeight = 1f;
                }
                return m_LineStyle;
            }
        }

        public static bool Titlebar(UnityEngine.Object target, GenericMenu menu)
        {
            int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
            GUIContent content = new GUIContent(ObjectNames.NicifyVariableName(target.GetType().Name));
            GUIStyle inspectorTitle = new GUIStyle("IN Foldout")
            {
                overflow = new RectOffset(0, 0, -3, 0),
                fixedWidth = 0,
                fixedHeight = 20

            };
            GUIStyle inspectorTitleText = "IN TitleText";

            Rect position = GUILayoutUtility.GetRect(GUIContent.none, inspectorTitle);
            Rect rect = new Rect(position.x + (float)inspectorTitle.padding.left, position.y + (float)inspectorTitle.padding.top, 16f, 16f);
            Rect rect1 = new Rect(position.xMax - (float)inspectorTitle.padding.right - 2f - 16f, rect.y, 16f, 16f);
            Rect rect4 = rect1;
            rect4.x = rect4.x - 18f;

            Rect rect2 = new Rect(position.x + 2f + 2f + 16f * 3, rect.y, 100f, rect.height)
            {
                xMax = rect4.xMin - 2f
            };

            Rect rect3 = new Rect(position.x + 16f, rect.y, 20f, 20f);
            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            IconAttribute iconAttribute = target.GetType().GetAttribute<IconAttribute>();
            if (iconAttribute != null){
                if (iconAttribute.type != null)
                {
                    icon = AssetPreview.GetMiniTypeThumbnail(iconAttribute.type);
                }
                else {
                    icon = Resources.Load<Texture2D>(iconAttribute.path);
                }
            }

            GUI.Label(new Rect(position.x + 13f, rect.y, 18f, 18f), icon);// EditorGUIUtility.FindTexture("cs Script Icon"));
            Rect rect5 = rect3;
            rect5.x = rect5.x + 16f;
            if (typeof(MonoBehaviour).IsAssignableFrom(target.GetType()))
            {
                MonoBehaviour behaviour = target as MonoBehaviour;
                behaviour.enabled = GUI.Toggle(rect5, behaviour.enabled, GUIContent.none);
            }
            else {
                FieldInfo enableField = target.GetType().GetField("enabled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (enableField != null) {
                    bool isEnabled = GUI.Toggle(rect5, (bool)enableField.GetValue(target), GUIContent.none);
                    enableField.SetValue(target, isEnabled);
                }
            }



            if (menu != null && GUI.Button(rect1, EditorGUIUtility.FindTexture("_popup"), inspectorTitleText))
            {
                menu.ShowAsContext();
            }

            EventType eventType = Event.current.type;
            if (menu != null && eventType == EventType.MouseDown && Event.current.button == 1 && position.Contains(Event.current.mousePosition))
            {
                menu.ShowAsContext();
            }

            bool isFolded = EditorPrefs.GetBool("Instance ID" + target.GetInstanceID(), true); 
            if (eventType != EventType.MouseDown)
            {
                if (eventType == EventType.Repaint)
                {
                    inspectorTitle.Draw(position, GUIContent.none, controlID, isFolded);

                   LineStyle.Draw(position, GUIContent.none, controlID);
                    inspectorTitleText.Draw(rect2, content, controlID, isFolded);
                }
            }

            bool flag = DoToggleForward(position, controlID, isFolded, GUIContent.none, GUIStyle.none);
            if (flag != isFolded)
            {
                EditorPrefs.SetBool("Instance ID" + target.GetInstanceID(), flag);
            }

            return flag;
        }

        private static bool DoToggleForward(Rect position, int id, bool value, GUIContent content, GUIStyle style)
        {
            Event ev = Event.current;
            if (MainActionKeyForControl(ev, id))
            {
                value = !value;
                ev.Use();
                GUI.changed = true;
            }
            if (EditorGUI.showMixedValue)
            {
                style = "ToggleMixed";
            }
            EventType eventType = ev.type;
            bool flag = (ev.type != EventType.MouseDown ? false : ev.button != 0);
            if (flag)
            {
                ev.type = EventType.Ignore;
            }
            bool flag1 = GUI.Toggle(position, id, (!EditorGUI.showMixedValue ? value : false), content, style);
            if (flag)
            {
                ev.type = eventType;
            }
            else if (ev.type != eventType)
            {
                GUIUtility.keyboardControl = id;
            }
            return flag1;
        }

        private static bool MainActionKeyForControl(Event evt, int controlId)
        {
            if (GUIUtility.keyboardControl != controlId)
            {
                return false;
            }
            bool flag = (evt.alt || evt.shift || evt.command ? true : evt.control);
            if (evt.type == EventType.KeyDown && evt.character == ' ' && !flag)
            {
                evt.Use();
                return false;
            }
            return (evt.type != EventType.KeyDown || evt.keyCode != KeyCode.Space && evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter ? false : !flag);
        }

        public static string CovertToAliasString(Type type) {
            if (type == typeof(System.Boolean)){
                return "bool";
            } else if (type== typeof(System.Byte)) {
                return "byte";
            }
            else if (type == typeof(System.SByte))
            {
                return "sbyte";
            }
            else if (type == typeof(System.Char))
            {
                return "char";
            }
            else if (type == typeof(System.Decimal))
            {
                return "decimal";
            }
            else if (type == typeof(System.Double))
            {
                return "double";
            }
            else if (type == typeof(System.Single))
            {
                return "float";
            }
            else if (type == typeof(System.Int32))
            {
                return "int";
            }
            else if (type == typeof(System.UInt32))
            {
                return "uint";
            }
            else if (type == typeof(System.Int64))
            {
                return "long";
            }
            else if (type == typeof(System.UInt64))
            {
                return "ulong";
            }
            else if (type == typeof(System.Object))
            {
                return "object";
            }
            else if (type == typeof(System.Int16))
            {
                return "short";
            }
            else if (type == typeof(System.UInt16))
            {
                return "ushort";
            }
            else if (type == typeof(System.String))
            {
                return "string";
            }
            else if (type == typeof(void))
            {
                return "void";
            }

            return type.Name;
        }
    }
}