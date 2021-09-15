using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Reflection;

[RequireComponent(typeof(Text))]
public class ShowProperty : MonoBehaviour {
    private Text textComponent;
    RectTransform Rect;


    [Header("Refreshing:")]
    [SerializeField] protected bool doRefreshOnEnable = false;     //should the text be refreshed when the gameobject is enabled?
    [SerializeField, Tooltip("0 = Every Frame, Infinity = Never")]
                     protected float refreshTime = Mathf.Infinity; //the time between two refreshes 

                     private bool refreshInProcess = false;         //(infinity = disable auto-refreshing [you need to call the refresh method manually everytime the value changes],
                                                                    //0 = refresh every frame)

    [SerializeField] protected Component script;     //the component script containing the property
    [SerializeField] protected string propertyName;     //the name of the property

    [Header("Extensions:")] //Order: prefix + value + suffix
    [SerializeField] protected string prefix;   //the text before the value
    [SerializeField] protected string suffix;   //the text after the value

    void Awake()
    {
        this.textComponent = GetComponent<Text>();   //setting up the reference to the text
        Rect = transform.parent.GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        if (doRefreshOnEnable)
        {
            Refresh();
        }
    }

    void Update()
    {
        if(refreshTime == 0) //refresh every frame
        {
            Refresh();
        }
        else if(refreshTime < Mathf.Infinity && !refreshInProcess)    //refresh after a certain amount of time
        {
            refreshInProcess = true;
            StartCoroutine(Cor_TimedUpdate(refreshTime));
        }
    }

    //A couroutine which invokes a refresh after a certain amount of time
    private IEnumerator Cor_TimedUpdate(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        Refresh();
        refreshInProcess = false;
        yield break;
    }

    /// <summary> This method can be used to set the text to the current value of the property or field.
    public void Refresh()
    {
        if(Script.GetType().GetProperty(propertyName) != null)
        {
            this.textComponent.text = prefix + Script.GetType().GetProperty(propertyName).GetValue(Script, null) + suffix;
            LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
            //get the value of the property and set it as the textcomponents text
        }
        else if (Script.GetType().GetField(propertyName) != null)  //if there is no such property then search for a field with that name
        {
            this.textComponent.text = prefix + Script.GetType().GetField(propertyName).GetValue(Script) + suffix;
            LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
        }
        else
        {
            Debug.LogErrorFormat("There is no property or field in your script (type = {0}), which is called {1}! Make sure the property is public!", Script.GetType().ToString(), propertyName);
        }

    }

    /// <summary> This method can be used to do a refresh while in editor
    public void EditorTimeRefresh()
    {
        if (Script.GetType().GetProperty(propertyName) != null)
        {
            this.GetComponent<Text>().text = prefix + Script.GetType().GetProperty(propertyName).GetValue(Script, null) + suffix;  //get the value of the property and set it as the textcomponents text
        }
        else if (Script.GetType().GetField(propertyName) != null)  //if there is no such property then search for a field with that name
        {
            this.GetComponent<Text>().text  = prefix + Script.GetType().GetField(propertyName).GetValue(Script) + suffix;
        }
        else
        {
            Debug.LogErrorFormat("There is no property or field in your script (type = {0}), which is called {1}! Make sure the property is public!", Script.GetType().ToString(), propertyName);
        }
    }

    #region GETTERS & SETTERS : Call the set method when you want to react to the change. Use the propertyfield otherwise.

    public Text TextComponent
    {
        get { return textComponent; }
        set { textComponent = value;}
    }
    public void SetTextComponent(Text textComponent)
    {
        this.textComponent = textComponent;
        Refresh();
    }
    public Component Script
    {
        get { return script; }
        set { script = value; }
    }
    public void SetScript(Component script)
    {
        this.script = script;
        Refresh();
    }
    public string PropertyName {
        get { return propertyName; }
        set { propertyName = value;}
    }
    public void SetPropertyName(string propertyName)
    {
        this.propertyName = propertyName;
        Refresh();
    }
    public string Prefix
    {
        get { return prefix; }
        set { prefix = value; }
    }
    public void SetPrefix(string prefix)
    {
        this.prefix = prefix;
        Refresh();
    }
    public string Suffix
    {
        get { return suffix; }
        set { suffix = value; }
    }
    public void SetSuffix(string suffix)
    {
        this.suffix = suffix;
        Refresh();
    }
    public float RefreshTime
    {
        get { return refreshTime; }
        set { refreshTime = value; }
    }
    public void SetRefreshTime(float refreshTime)
    {
        this.refreshTime = refreshTime;
        StopAllCoroutines();
        refreshInProcess = false;
        Update();
    }
    public bool DoRefreshOnEnable
    {
        get { return doRefreshOnEnable; }
        set { doRefreshOnEnable = value; }
    }

    #endregion
}