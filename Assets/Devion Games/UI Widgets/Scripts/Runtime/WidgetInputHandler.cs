using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevionGames.UIWidgets
{
    public class WidgetInputHandler : MonoBehaviour
    {
        private static Dictionary<KeyCode, List<UIWidget>> m_WidgetKeyBindings;
      
        // Update is called once per frame
        void Update()
        {
            if (m_WidgetKeyBindings == null) {
                return;
            }

            foreach (KeyValuePair<KeyCode, List<UIWidget>> kvp in m_WidgetKeyBindings)
            {
                if (Input.GetKeyDown(kvp.Key)){
                    for (int i = 0; i < kvp.Value.Count; i++) {
                        kvp.Value[i].Toggle();
                    }
                }
            }
        }

        public static void RegisterInput(KeyCode key, UIWidget widget) {
            if (m_WidgetKeyBindings == null) {
                if (GameObject.FindObjectOfType<WidgetInputHandler>() == null)
                {
                    GameObject handlerObject = new GameObject("WidgetInputHandler");
                    handlerObject.AddComponent<WidgetInputHandler>();
                }
                m_WidgetKeyBindings = new Dictionary<KeyCode, List<UIWidget>>();
            }
            if (key == KeyCode.None) {
                return;
            }

            List<UIWidget> widgets;
            if (!m_WidgetKeyBindings.TryGetValue(key, out widgets))
            {
                m_WidgetKeyBindings.Add(key, new List<UIWidget>() { widget });
            }else {
                widgets.Add(widget);
                m_WidgetKeyBindings[key] = widgets;
            }
        }
    }
}