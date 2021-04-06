using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace DevionGames{
	public class BaseSystemEditor<T> : EditorWindow where T: ScriptableObject {
		public static void ShowWindow(){
			Type childEditorType = GetChildEditorType ();
			BaseSystemEditor<T>[] objArray = (BaseSystemEditor<T>[])Resources.FindObjectsOfTypeAll(childEditorType);
			BaseSystemEditor<T> editor =(objArray.Length <= 0 ? (BaseSystemEditor<T>)ScriptableObject.CreateInstance(childEditorType) : objArray[0]);
	
			      
			editor.hideFlags = HideFlags.HideAndDontSave;
			#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1
			editor.title=childEditorType.Name;
			#else
			editor.titleContent=new GUIContent(childEditorType.Name);
			#endif
			editor.SelectDatabase ();
		}
		
		public static BaseSystemEditor<T> instance;
		public T database;
		private List<ICollectionEditor> childEditors;
		
		[SerializeField]
		protected int toolbarIndex;
		protected string[] toolbarNames
		{
			get{
				string[] items=new string[childEditors.Count];
				for(int i=0;i<childEditors.Count;i++){
					Type type = childEditors[i].GetType();
					if(type.IsGenericType){
						items[i]=type.GetGenericArguments()[0].Name;
					}else{
						items[i]=type.Name.Replace("Editor","");
					}
				}
				
				return items;
			}
			
		}
		
		protected virtual void OnEnable(){
			instance = this;
			ResetChildEditors ();
		}
		
		protected virtual void OnGUI(){
			if(childEditors != null){
				EditorGUILayout.Space ();
				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				toolbarIndex = GUILayout.Toolbar (toolbarIndex, toolbarNames, GUILayout.MinWidth (200));
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
				childEditors [toolbarIndex].OnGUI (new Rect(0f,30f,position.width,position.height-30f));
			}
		}
		
		protected virtual void SelectDatabase(){
			string searchString="Search...";
			T[] databases=UnityEditorUtility.FindAssets<T>();
			UtilityInstanceWindow.ShowWindow("Select Settings",delegate() {
				searchString=UnityEditorUtility.SearchField(searchString);
				
				for (int i=0; i<databases.Length; i++) {
					if(!string.IsNullOrEmpty(searchString) && !searchString.Equals("Search...") && !databases[i].name.Contains(searchString)){
						continue;
					}
					GUIStyle style=new GUIStyle("button");
					style.wordWrap=true;
					if(GUILayout.Button(AssetDatabase.GetAssetPath(databases[i]),style)){
						database=databases[i];
						ResetChildEditors();
						Show();
						UtilityInstanceWindow.CloseWindow();
					}
				}
				GUILayout.FlexibleSpace();
				Color color=GUI.backgroundColor;
				GUI.backgroundColor = Color.green;
				if(GUILayout.Button("Create")){
					T db= AssetCreator.CreateAsset<T>(true);
					if(db != null){
						ArrayUtility.Add<T>(ref databases,db);
					}
				}
				GUI.backgroundColor=color;
			});
		}

		private static Type GetChildEditorType(){
			return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()) .Where(type => type.IsSubclassOf(typeof(BaseSystemEditor<T>)) && type.IsClass && !type.IsAbstract).FirstOrDefault();		
		}

		private void ResetChildEditors(){
			childEditors = new List<ICollectionEditor> ();
			AddChildEditor (childEditors);
		}

		protected virtual void AddChildEditor(List<ICollectionEditor> editors){}
	}
}