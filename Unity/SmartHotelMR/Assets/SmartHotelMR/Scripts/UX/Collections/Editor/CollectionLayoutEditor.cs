using UnityEngine;
using UnityEditor;

namespace SmartHotelMR
{
    [CustomEditor(typeof(CollectionLayout))]
    public class CollectionLayoutEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default
            base.OnInspectorGUI();

            // Place the button at the bottom
            CollectionLayout myScript = (CollectionLayout)target;
            if (GUILayout.Button("Update Collection"))
            {
                myScript.UpdateCollection();
            }
        }
    }
}