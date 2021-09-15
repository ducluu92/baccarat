using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(ShowProperty))]
public class ShowPropertyEditor : Editor {

    public override void OnInspectorGUI()
    {

        ShowProperty script = target as ShowProperty;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Reference:", EditorStyles.boldLabel);

        //the component
        string title = "Script ";
        if(script != null && script.Script)
        {
            title += "(" + script.Script.GetType().ToString().Split('.')[script.Script.GetType().ToString().Split('.').Length - 1] + ")";
            Component[] monos = script.Script.gameObject.GetComponents<Component>();
            int index = 0;
            int amountOfThatType = 1;
            for (int i = 0; i < monos.Length; i++)
            {
                if (monos[i].GetType() == script.Script.GetType())
                {
                    if(monos[i] == script.Script)
                    {
                        index = amountOfThatType;
                    }
                    else
                    {
                        amountOfThatType++;
                    }
                }
            }
            if(amountOfThatType > 1)
            {
                title += String.Format("({0}/{1})", index, amountOfThatType);
            }
        }
        title += ":";
        script.Script = EditorGUILayout.ObjectField(title, script.Script, typeof(Component), true) as Component;
        if(script.Script != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Previous"))
            {
                Component[] monos = script.Script.gameObject.GetComponents<Component>();
                int index = Array.IndexOf(monos, script.Script);
                script.Script = monos[Math.Max(0, index - 1)];
            }
            if (GUILayout.Button("Next"))
            {
                Component[] monos = script.Script.gameObject.GetComponents<Component>();
                int index = Array.IndexOf(monos, script.Script);
                script.Script = monos[Math.Min(monos.Length - 1, index + 1)];
            }
            EditorGUILayout.EndHorizontal();
        }

        //the property
        EditorGUILayout.BeginHorizontal();
        script.PropertyName = EditorGUILayout.TextField("Property/Field Name:", script.PropertyName);
        if (script != null && script.Script != null && GUILayout.Button("See all"))
        {
            ListPropertiesWindow window = EditorWindow.GetWindow<ListPropertiesWindow>();
            window.showPropertyScript = script;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Extensions:", EditorStyles.boldLabel);
        script.Prefix = EditorGUILayout.TextField("Prefix:", script.Prefix);
        script.Suffix = EditorGUILayout.TextField("Suffix:", script.Suffix);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Refreshing:", EditorStyles.boldLabel);
        script.DoRefreshOnEnable = EditorGUILayout.Toggle("Do Refresh On Enable:", script.DoRefreshOnEnable);
        script.RefreshTime = Math.Max(0, EditorGUILayout.FloatField("Refresh Time:", script.RefreshTime));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(" ");
        if (GUILayout.Button("Every Frame"))
        {
            script.RefreshTime = 0;
        }
        if (GUILayout.Button("Manually"))
        {
            script.RefreshTime = Mathf.Infinity;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (script != null && script.Script != null && GUILayout.Button("Refresh"))
        {
            script.EditorTimeRefresh();
            EditorUtility.SetDirty(script.GetComponent<UnityEngine.UI.Text>());
        }

        EditorUtility.SetDirty(target);

    }
}
