using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace DevionGames
{
	public static class AttributeUtility
	{
		private readonly static Dictionary<Type, object[]> typeAttributeCache;
		private readonly static Dictionary<FieldInfo, object[]> fieldAttributeCache;

		static AttributeUtility ()
		{
			AttributeUtility.typeAttributeCache = new Dictionary<Type, object[]> ();
			AttributeUtility.fieldAttributeCache = new Dictionary<FieldInfo, object[]> ();
		}

		public static object[] GetCustomAttributes (Type type)
		{
			object[] customAttributes;
			if (!AttributeUtility.typeAttributeCache.TryGetValue (type, out customAttributes)) {
				customAttributes = type.GetCustomAttributes (true);
				AttributeUtility.typeAttributeCache.Add (type, customAttributes);
			}
			return customAttributes;
		}

		public static object[] GetCustomAttributes (FieldInfo field)
		{
			object[] customAttributes;
			if (!AttributeUtility.fieldAttributeCache.TryGetValue (field, out customAttributes)) {
				customAttributes = field.GetCustomAttributes (true);
				AttributeUtility.fieldAttributeCache.Add (field, customAttributes);
			}
			return customAttributes;
		}

		public static bool IsSerialized (this FieldInfo field)
		{
			object[] objArray = AttributeUtility.GetCustomAttributes (field);
			for (int i = 0; i < (int)objArray.Length; i++) {
				if (objArray [i] is SerializeField) {
					return true;
				}
			}
			return field.IsPublic && !field.IsNotSerialized;
		}

		public static T GetAttribute<T> (this FieldInfo field)
		{
			object[] objArray = AttributeUtility.GetCustomAttributes (field);
			for (int i = 0; i < (int)objArray.Length; i++) {
				if (objArray [i].GetType () == typeof(T) || objArray [i].GetType ().IsSubclassOf (typeof(T))) {
					return (T)objArray [i];
				}
			}
			return default(T);		
		}

		public static bool HasAttribute (this FieldInfo field, Type attributeType)
		{
			object[] objArray = AttributeUtility.GetCustomAttributes (field);
			for (int i = 0; i < (int)objArray.Length; i++) {
				if (objArray [i].GetType () == attributeType || objArray [i].GetType ().IsSubclassOf (attributeType)) {
					return true;
				}
			}
			return false;
		}

		public static bool HasAttribute (this MemberInfo field, Type attributeType)
		{
			object[] objArray = field.GetCustomAttributes (true);
			for (int i = 0; i < (int)objArray.Length; i++) {
				if (objArray [i].GetType () == attributeType || objArray [i].GetType ().IsSubclassOf (attributeType)) {
					return true;
				}
			}
			return false;
		}
    }
}