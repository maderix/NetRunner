using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Linq;

namespace DevionGames{
	/// <summary>
	/// Helper class to create custom assets.
	/// </summary>
	public static class AssetCreator {
		/// <summary>
		/// Creates a custom asset.
		/// </summary>
		/// <returns>The asset.</returns>
		/// <param name="displayFilePanel">If set to <c>true</c> display file panel.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T CreateAsset<T> (bool displayFilePanel) where T : ScriptableObject
		{
			return (T)CreateAsset (typeof(T),displayFilePanel);
		}
		
		/// <summary>
		/// Creates a custom asset.
		/// </summary>
		/// <returns>The asset.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T CreateAsset<T> () where T : ScriptableObject
		{
			return (T)CreateAsset(typeof(T));
		}

		/// <summary>
		/// Creates a custom asset at path.
		/// </summary>
		/// <returns>The asset.</returns>
		/// <param name="path">Path.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T CreateAsset<T> (string path) where T : ScriptableObject
		{
			return (T)CreateAsset (typeof(T), path);;
		}



		public static ScriptableObject CreateAsset(Type type, bool displayFilePanel){
			
			if (displayFilePanel) {
				string mPath = EditorUtility.SaveFilePanelInProject (
					"Create Asset of type " + type.Name,
					"New " + type.Name + ".asset",
					"asset", "");
				return CreateAsset(type,mPath);
			}
			return CreateAsset(type);
		}

		/// <summary>
		/// Creates a custom asset.
		/// </summary>
		/// <returns>The asset.</returns>
		/// <param name="type">Type.</param>
		public static ScriptableObject CreateAsset(Type type){

			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
			if (path == "") {
				path = "Assets";
			} else if (System.IO.Path.GetExtension (path) != "") {
				path = path.Replace (System.IO.Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
			}
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New " + type.Name + ".asset");
			return CreateAsset(type,assetPathAndName);
		}

		/// <summary>
		/// Creates a custom asset at path.
		/// </summary>
		/// <returns>The asset.</returns>
		/// <param name="type">Type.</param>
		/// <param name="path">Path.</param>
		public static ScriptableObject CreateAsset(Type type, string path)
		{
			if (string.IsNullOrEmpty (path)) {
				return null;
			}
			ScriptableObject data = ScriptableObject.CreateInstance (type);
			AssetDatabase.CreateAsset (data, path);
			AssetDatabase.SaveAssets ();
			return data;
		}
	}
}
