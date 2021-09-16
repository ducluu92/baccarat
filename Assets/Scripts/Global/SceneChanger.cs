using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    private static string SceneName = "Login";
    public Slider LoadingBar;
    public Text LoadingText;
    private float progress;
    private static bool changeNow = false;

    private void Start()
    {
        Time.timeScale = 1.0f;

        this.ChangeScene();
    }

    static public void CallSceneLoader(string scenename)
    {
        Debug.Log ( "Changing Scene 3333" );
        SceneName = scenename;
        SceneManager.LoadScene("Loading");
        //SceneManager.LoadScene(scenename);

    }

    public void CallSceneLoader_nonstatic(string scenename)
    {
        Debug.Log ( "Changing Scene 2222" );
        SceneName = scenename;
        SceneManager.LoadScene("Loading");
    }

    public void ChangeScene()
    {
        
        Debug.Log ( "Changing Scene" );
        if(SceneName != null)
        StartCoroutine(ChangeScene_Coroutine());
    }

    IEnumerator ChangeScene_Coroutine()
    {
        yield return null;

        changeNow = true;

        AsyncOperation asyncOper = SceneManager.LoadSceneAsync(SceneName);

        asyncOper.allowSceneActivation = false;

        yield return new WaitForSeconds(1.0f);

        while (!asyncOper.isDone)
        {
            yield return null;

            if(asyncOper.progress < 0.9f)
            {
                yield return new WaitForSeconds(0.1f);
                if(LoadingBar != null && LoadingText != null)
                {
                    LoadingBar.value = asyncOper.progress;
                    LoadingText.text = String.Format("로딩중 입니다.. ({0}%)", asyncOper.progress * 100.0f);
                }

            }
            else
            {
                if (LoadingBar != null && LoadingText != null)
                {
                    yield return new WaitForSeconds(2.5f);
                    LoadingBar.value = 1.0f;
                    LoadingText.text = "입장중 입니다..";
                }

                asyncOper.allowSceneActivation = true;
                changeNow = false;
                SceneName = null;
                yield break;
            }

            Debug.Log("PROGRESS => " + asyncOper.progress);
        }

        

    }

}
