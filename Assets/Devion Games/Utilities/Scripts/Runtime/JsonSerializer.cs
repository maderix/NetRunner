using UnityEngine;
using System.Collections;
using System.Text;
using System;
using System.Collections.Generic;

namespace DevionGames{
	public static class JsonSerializer {
		public static string Serialize(IJsonSerializable[] objs){
			List<object> list = new List<object> ();
			for (int i=0; i<objs.Length; i++) {
				if(objs[i] != null){
					Dictionary<string,object> data = new Dictionary<string, object> ();
					objs[i].GetObjectData (data);
					list.Add(data);
				}
			}
			return MiniJSON.Serialize (list);
		}
		
		public static void Deserialize(string json, IJsonSerializable[] objs){
			if(string.IsNullOrEmpty(json)){
				return;
			}
			List<object> list = MiniJSON.Deserialize (json) as List<object>;
			for (int i = 0; i < list.Count; i++) {
				Dictionary<string,object> data = list[i] as Dictionary<string,object>;
				objs[i].SetObjectData (data);
			}
		}

		public static List<T> Deserialize<T>(string json) where T:IJsonSerializable{
			List<T> result = new List<T> ();
			if(string.IsNullOrEmpty(json)){
				return result;
			}

			List<object> list = MiniJSON.Deserialize (json) as List<object>;
			if (list != null) {
				for (int i = 0; i < list.Count; i++) {
					Dictionary<string,object> data = list [i] as Dictionary<string,object>;
					T obj = default(T);
					if(typeof(ScriptableObject).IsAssignableFrom(typeof(T))){
						obj = (T)(object)ScriptableObject.CreateInstance(typeof(T));
					}else{
						obj = (T)Activator.CreateInstance (typeof(T));
					}
					obj.SetObjectData (data);
					result.Add (obj);
				}
			}
			return result;
		}


		public static string Serialize(IJsonSerializable obj){
			Dictionary<string,object> data = new Dictionary<string, object> ();
			obj.GetObjectData (data);
			return MiniJSON.Serialize (data);
		}

		public static void Deserialize(string json, IJsonSerializable obj){
			if(string.IsNullOrEmpty(json)){
				return;
			}
			Dictionary<string,object> data = MiniJSON.Deserialize (json) as Dictionary<string,object>;
			obj.SetObjectData (data);
		}
	}
}