using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;

public class ListPropertiesWindow : EditorWindow {
    public readonly Color selectedColor = new Color(0.5f, 0.9f, 0.5f);

    public ShowProperty showPropertyScript;     //a reference to a show property script

    Vector2 scrollPositionProperties = Vector2.zero;
    Vector2 scrollPositionFields = Vector2.zero;
	public void OnGUI()
    {
        GUILayout.Label("*Inherited properties are displayed in a gray tone.");
        if(showPropertyScript.Script != null)
        {
            GUILayout.BeginHorizontal();

            //properties
            GUILayout.BeginVertical();
            GUILayout.Label("Properties", EditorStyles.boldLabel);
            scrollPositionProperties = GUILayout.BeginScrollView(scrollPositionProperties);
            var props = showPropertyScript.Script.GetType().GetProperties();
            foreach(PropertyInfo pInfo in props)
            {
                if (pInfo.Name.Equals(showPropertyScript.PropertyName)) //check if the property is currently selected
                {
                    GUI.backgroundColor = selectedColor;
                }
                else if(pInfo.DeclaringType == showPropertyScript.Script.GetType())     //inherited properties are shown in gray
                {
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    GUI.backgroundColor = Color.gray;
                }
                if (GUILayout.Button(pInfo.Name))
                {
                    showPropertyScript.PropertyName = pInfo.Name;
                    if (showPropertyScript.Prefix == null || showPropertyScript.Prefix.Equals(""))
                    {
                        showPropertyScript.Prefix = pInfo.Name + ": ";
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(50);

            //fields
            GUILayout.BeginVertical();
            GUILayout.Label("Fields", EditorStyles.boldLabel);
            scrollPositionFields = GUILayout.BeginScrollView(scrollPositionFields);
            var fields = showPropertyScript.Script.GetType().GetFields();
            foreach (FieldInfo fInfo in fields)
            {
                if (fInfo.Name.Equals(showPropertyScript.PropertyName))
                {
                    GUI.backgroundColor = selectedColor;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }
                if (GUILayout.Button(fInfo.Name))
                {
                    showPropertyScript.PropertyName = fInfo.Name;
                    if(showPropertyScript.Prefix == null || showPropertyScript.Prefix.Equals(""))
                    {
                        showPropertyScript.Prefix = fInfo.Name + ": ";
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

        }
        else
        {
            GUILayout.Label("Please reference a script first!");
        }
    }
}
