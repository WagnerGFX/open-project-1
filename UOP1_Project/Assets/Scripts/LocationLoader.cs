﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class LocationLoader : MonoBehaviour
{
    public GameScene[] mainMenuScenes;
    [Header("Loading Screen")]
    public GameObject loadingInterface;
    public Image loadingProgressBar;

    [Header("Load Event")]
    //Load Event we are listening to
    [SerializeField] private LoadEvent _loadEvent = default;

    //List of the scenes to load and track progress
    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();
    //List of scenes to unload
    private List<Scene> _ScenesToUnload = new List<Scene>();

    private void OnEnable()
    {
        _loadEvent.loadEvent += LoadScenes;
    }

    private void OnDisable()
    {
        _loadEvent.loadEvent -= LoadScenes;
    }

    private void Start()
    {
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        LoadScenes(mainMenuScenes, false);
    }

    /*
     * Scenes methods
     */
    //Load the scenes passed as parameter
    public void LoadScenes(GameScene[] locationsToLoad, bool showLoadingScreen)
    {
        //Add all current open scenes to unload list
        AddScenesToUnload();

        for (int i = 0; i < locationsToLoad.Length; ++i)
        {
            String currentSceneName = locationsToLoad[i].sceneName;
            if (!CheckLoadState(currentSceneName))
            {
                //Add the scene to the list of scenes to load asynchronously in the background
                _scenesToLoad.Add(SceneManager.LoadSceneAsync(currentSceneName, LoadSceneMode.Additive));
            }
        }
        //Show the progress bar and track progress if loadScreen is true
        if (showLoadingScreen)
        {
            StartCoroutine(LoadingScreen());
        }
        else
        {
            //Clear the scenes to load
            _scenesToLoad.Clear();
        }
        //Unload the scenes
        UnloadScenes();
    }

    public void AddScenesToUnload()
    {
        if (SceneManager.sceneCount > 0)
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name != "ScenesLoader")
                {
                    Debug.Log("Added scene to unload = " + scene.name);
                    //Add the scene to the list of the scenes to unload
                    _ScenesToUnload.Add(scene);
                }
            }
        }
    }
    public void UnloadScenes()
    {
        if(_ScenesToUnload != null)
        {
            for (int i = 0; i < _ScenesToUnload.Count; ++i)
            {
                //Unload the scene asynchronously in the background
                SceneManager.UnloadSceneAsync(_ScenesToUnload[i]);
            }
        }
        _ScenesToUnload.Clear();
    }

    //Check if a scene is already loaded
    public bool CheckLoadState(String sceneName)
    {
        if (SceneManager.sceneCount > 0)
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //Update loading progress
    IEnumerator LoadingScreen()
    {
        float totalProgress = 0;
        //When the scene reaches 0.9f, it means that it is loaded
        //The remaining 0.1f are for the integration
        while (totalProgress <= 0.9f)
        {
            //Reset the progress for the new values
            totalProgress = 0;
            //Iterate through all the scenes to load
            for (int i = 0; i < _scenesToLoad.Count; ++i)
            {
                Debug.Log("Scene" + i + " :" + _scenesToLoad[i].isDone + "progress = " + _scenesToLoad[i].progress);
                //Adding the scene progress to the total progress
                totalProgress += _scenesToLoad[i].progress;
            }
            //the fillAmount for all scenes, so we devide the progress by the number of scenes to load
            loadingProgressBar.fillAmount = totalProgress / _scenesToLoad.Count;
            Debug.Log("progress bar" + loadingProgressBar.fillAmount + "and value =" + totalProgress / _scenesToLoad.Count);
            yield return null;
        }
        //Clear the scenes to load
        _scenesToLoad.Clear();
        //Hide progress bar when loading is done
        loadingInterface.SetActive(false);

    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit!");
    }

}
