using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TinyGiantStudio.Text
{
    public class XRToolkitEditorSetup : Editor
    {
        public static void CreateSetupButton(Object myObject)
        {
            if (myObject == null)
                EditorGUILayout.LabelField("myObject not found");

            GameObject b = myObject as GameObject;
            if (b != null)
                EditorGUILayout.LabelField(b.name);
            else
                EditorGUILayout.LabelField("b not found");
        }
    }
}