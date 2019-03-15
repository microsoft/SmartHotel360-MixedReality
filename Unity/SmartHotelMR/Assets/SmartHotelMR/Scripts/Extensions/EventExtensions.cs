using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SmartHotelMR
{
    public enum EventExecutionMethod
    {
        Singlecast,
        BroadcastDown,
        BroadcastUp,
        BroadcastAll
    }

    public static class EventExtensions
    {

        public static void SendEvent<T>(this Component root, BaseEventData eventData, ExecuteEvents.EventFunction<T> callbackFunction, EventExecutionMethod method = EventExecutionMethod.Singlecast) where T : IEventSystemHandler
        {
            if (root == null)
            {
                return;
            }

            Transform[] transforms = new Transform[0];

            switch (method)
            {
                case EventExecutionMethod.Singlecast:
                    transforms = new Transform[] { root.transform };
                    break;
                case EventExecutionMethod.BroadcastDown:
                    transforms = root.GetComponentsInChildren<Transform>();
                    break;
                case EventExecutionMethod.BroadcastUp:
                    transforms = root.GetComponentsInParent<Transform>();
                    break;
                case EventExecutionMethod.BroadcastAll:
                    transforms = GameObject.FindObjectsOfType(typeof(Transform)) as Transform[];
                    break;
                default:
                    UnityEngine.Debug.Log("Unknown EventExecutionMethod!");
                    break;
            }

            for (int i = 0; i < transforms.Length; i++)
            {
                var go = transforms[i].gameObject;
                ExecuteEvents.Execute<T>(go, eventData, callbackFunction);
            }
        }
    }
}