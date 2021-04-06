using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevionGames
{
    public static class CoroutineUtility
    {
        private static CoroutineHandler m_Handler;
        private static CoroutineHandler Handler {
            get {
                if (m_Handler == null) {
                    GameObject handlerObject = new GameObject("Coroutine Handler");
                    m_Handler = handlerObject.AddComponent<CoroutineHandler>();
                }
                return m_Handler;
            }
        }

        public static Coroutine StartCoroutine(IEnumerator routine) {
            return Handler.StartCoroutine(routine);
        }

        public static Coroutine StartCoroutine(string methodName, object value)
        {
            return Handler.StartCoroutine(methodName,value);
        }

        public static Coroutine StartCoroutine(string methodName)
        {
            return Handler.StartCoroutine(methodName);
        }

        public static void StopCoroutine(IEnumerator routine)
        {
            Handler.StopCoroutine(routine);
        }

        public static void StopCoroutine(string methodName)
        {
            Handler.StopCoroutine(methodName);
        }

        public static void StopCoroutine(Coroutine routine)
        {
            Handler.StopCoroutine(routine);
        }

        public static void StopAllCoroutines()
        {
            Handler.StopAllCoroutines();
        }
    }
}