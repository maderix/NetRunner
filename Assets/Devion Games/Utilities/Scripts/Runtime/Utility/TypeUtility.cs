using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace DevionGames{
	public static class TypeUtility {
		private static Assembly[] assembliesLookup;
		private static Dictionary<string, Type> typeLookup;
		static TypeUtility(){
			assembliesLookup = TypeUtility.GetLoadedAssemblies ();
			// Remove Editor assemblies
			var runtimeAsms = new List<Assembly>();
			foreach (Assembly asm in assembliesLookup) {
				if (!asm.GetName().Name.Contains("Editor"))
					runtimeAsms.Add(asm);
			}
			assembliesLookup = runtimeAsms.ToArray();
			typeLookup = new Dictionary<string, Type> ();
			
		}
		
		public static Assembly[] GetLoadedAssemblies()
		{
			#if NETFX_CORE
			var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			
			List<Assembly> loadedAssemblies = new List<Assembly>();
			
			var folderFilesAsync = folder.GetFilesAsync();
			folderFilesAsync.AsTask().Wait();
			
			foreach (var file in folderFilesAsync.GetResults())
			{
				if (file.FileType == ".dll" || file.FileType == ".exe")
				{
					try
					{
						var filename = file.Name.Substring(0, file.Name.Length - file.FileType.Length);
						AssemblyName name = new AssemblyName { Name = filename };
						Assembly asm = Assembly.Load(name);
						loadedAssemblies.Add(asm);
					}
					catch (BadImageFormatException)
					{
						// Thrown reflecting on C++ executable files for which the C++ compiler stripped the relocation addresses (such as Unity dlls): http://msdn.microsoft.com/en-us/library/x4cw969y(v=vs.110).aspx
					}
				}
			}
			
			return loadedAssemblies.ToArray();
			#else
			return AppDomain.CurrentDomain.GetAssemblies();
			#endif
		}
		
		public static string[] GetSubTypeNames(Type baseType){
			return GetSubTypes (baseType).Select (x => x.Name).ToArray();
		}
		
		public static Type[] GetSubTypes(Type baseType){
			IEnumerable<Type> types= assembliesLookup.SelectMany(assembly => assembly.GetTypes()) .Where(type => type.IsSubclassOf(baseType));
			return types.ToArray();
		}
		
		public static Type GetMemberType(Type type, string name){
			FieldInfo fieldInfo = type.GetField (name);
			if (fieldInfo != null) {
				return fieldInfo.FieldType;			
			}
			PropertyInfo propertyInfo=type.GetProperty(name);
			if(propertyInfo != null){
				return propertyInfo.PropertyType;
			}
			return null;
		}
		
		public static Type GetType (string name) {
            if (string.IsNullOrEmpty(name)) {
                return null;
            }
			Type type = null;
			if (typeLookup.TryGetValue (name, out type)) {
				return type;
			}
			
			foreach (Assembly a in assembliesLookup)
			{
				Type[] assemblyTypes = a.GetTypes();
				for (int j = 0; j < assemblyTypes.Length; j++)
				{
					if (assemblyTypes[j].Name == name)
					{
						typeLookup.Add(name, assemblyTypes[j]);
						return assemblyTypes[j];
					}
				}
			}
			
			return null;
			
		}
		
	}
}