using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DevionGames
{
    public static class UIUtility 
    {
   
        public static bool IsPointerOverUI() {
            if (EventSystem.current == null || EventSystem.current.currentInputModule == null){
                return false;
            }
            Type type = EventSystem.current.currentInputModule.GetType();
            MethodInfo methodInfo;
            methodInfo = type.GetMethod("GetLastPointerEventData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (methodInfo == null){
                return false;
            }
            PointerEventData eventData = (PointerEventData)methodInfo.Invoke(EventSystem.current.currentInputModule, new object[] { PointerInputModule.kMouseLeftId });
            if (eventData != null && eventData.pointerEnter)
            {
                return eventData.pointerEnter.layer == 5; 
            }
            return false;
        }
    }
}
