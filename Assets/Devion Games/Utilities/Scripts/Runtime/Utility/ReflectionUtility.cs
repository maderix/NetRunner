using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace DevionGames
{
	public static class ReflectionUtility
	{
		private static readonly Dictionary<Type,FieldInfo[]> fieldInfoLookup;
		private static readonly Dictionary<Type,MethodInfo[]> methodInfoLookup;

		static ReflectionUtility ()
		{
			ReflectionUtility.fieldInfoLookup = new Dictionary<Type, FieldInfo[]> ();
			ReflectionUtility.methodInfoLookup = new Dictionary<Type, MethodInfo[]> ();
		}

	/*	public static string[] GetAllComponentNames ()
		{
			IEnumerable<Type> types = typeof(Component).GetAssembly ().GetTypes ().Where (type => type.IsSubclassOf (typeof(Component)));//AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()) .Where(type => type.IsSubclassOf(typeof(Component)));
			return types.Select (x => x.FullName).ToArray ();
		}

		public static string[] GetFieldNames (this Type type)
		{
			FieldInfo[] fields = type.GetAllFields (BindingFlags.Public | BindingFlags.Instance).ToArray ();
			return fields.Select (x => x.Name).ToArray ();
		}

		public static string[] GetPropertyNames (this Type type, bool requiresWrite)
		{
			PropertyInfo[] properties = type.GetProperties (BindingFlags.Public | BindingFlags.Instance).ToArray ();
			if (requiresWrite) {
				properties = properties.Where (x => x.CanWrite).ToArray ();			
			}
			return properties.Select (x => x.Name).ToArray ();
		}

		public static string[] GetPropertyAndFieldNames (this Type type, bool requiresWrite)
		{
			List<string> names = new List<string> (type.GetPropertyNames (requiresWrite));
			names.AddRange (type.GetFieldNames ());
			return names.ToArray ();
		}

		public static string[] GetMethodNames (this Type type)
		{
			MethodInfo[] methods = type
				.GetMethods (BindingFlags.Public | BindingFlags.Instance)
				.ToArray ();
			return methods.Where (y => y.GetParameters ().Length == 0 && y.ReturnType == typeof(void)).Select (x => x.Name).ToArray ();
		}

		public static FieldInfo[] GetAllFields (this Type type, BindingFlags flags)
		{
			if (type == null) {
				return new FieldInfo[0];
			}
			return type.GetFields (flags).Concat (GetAllFields (type.GetBaseType (), flags)).ToArray ();
		}

		public static FieldInfo[] GetPublicFields (this object obj)
		{
			return GetPublicFields (obj.GetType ());
		}

		public static FieldInfo[] GetPublicFields (this Type type)
		{
			if (!ReflectionUtility.fieldInfoLookup.ContainsKey (type)) {
				fieldInfoLookup [type] = type.GetFields (BindingFlags.Instance | BindingFlags.Public);
			}

			return fieldInfoLookup [type];
		}

		public static PropertyInfo[] GetPublicProperties (this object obj)
		{
			return GetPublicProperties (obj.GetType ());
		}

		public static PropertyInfo[] GetPublicProperties (this Type type)
		{
			if (!ReflectionUtility.propertyInfoLookup.ContainsKey (type)) {
				propertyInfoLookup [type] = type.GetProperties (BindingFlags.Instance | BindingFlags.Public);
			}

			return propertyInfoLookup [type];
		}*/




		public static MethodInfo[] GetAllMethods (this Type type, BindingFlags flags)
		{
			MethodInfo[] methods = new MethodInfo[0];
			if (type != null && !ReflectionUtility.methodInfoLookup.TryGetValue (type, out methods)) {
				methods = type.GetMethods (flags).Concat (GetAllMethods (type.GetBaseType (), flags)).ToArray ();
				ReflectionUtility.methodInfoLookup.Add (type, methods);
			}

			return methods;
		}

        public static FieldInfo[] GetAllFields(this Type type, BindingFlags flags)
        {
            if (type == null)
            {
                return new FieldInfo[0];
            }
            return type.GetFields(flags).Concat(GetAllFields(type.GetBaseType(), flags)).ToArray();
        }

        public static FieldInfo[] GetAllSerializedFields(this Type type)
        {
            if (type == null)
            {
                return new FieldInfo[0];
            }
            FieldInfo[] fields = GetSerializedFields(type).Concat(GetAllSerializedFields(type.BaseType)).ToArray();
            fields = fields.OrderBy(x => x.DeclaringType.BaseTypesAndSelf().Count()).ToArray();
            return fields;
        }

        public static FieldInfo[] GetSerializedFields(this Type type)
        {
            FieldInfo[] fields;
            if (!fieldInfoLookup.TryGetValue(type, out fields))
            {
                fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.IsPublic && !x.HasAttribute(typeof(NonSerializedAttribute)) || x.HasAttribute(typeof(SerializeField))).ToArray();
                fieldInfoLookup.Add(type, fields);
            }
            return fields;
        }

        public static IEnumerable<Type> BaseTypesAndSelf(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        public static IEnumerable<Type> BaseTypes(this Type type)
        {
            while (type != null)
            {
                type = type.BaseType;
                yield return type;
            
            }
        }
    }
}