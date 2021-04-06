using UnityEngine;
using System.Collections;

namespace DevionGames{
	public interface ICollectionEditor {
        string ToolbarName { get; }
		void OnGUI(Rect position);
        void OnDestroy();
	}
}