using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private static AsyncOperation asyncOperationScene;
    private static GameObject loadScreenPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadIntoMemory()
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("Assets/2_Resources/3_UI/5_PreFabs/LoadingScreen.prefab");

        // When the loading is complete, invoke the callback with the loaded prefab
        handle.Completed += (result) =>
        {
            if (result.Status == AsyncOperationStatus.Succeeded)
            {
                loadScreenPrefab = handle.Result;
            }
            else
            {
                Debug.LogError($"Failed to load prefab at address: Assets/2_Resources/3_UI/5_PreFabs/LoadingScreen.prefab");
            }
        };
    }

    public static void LoadScene(string sceneName, MonoBehaviour objectForLoad, float minLoadTime = 0, bool loadingScreen = false, bool enableOnLoad = true)
    {
        float startTime = Time.realtimeSinceStartup;
        asyncOperationScene = SceneManager.LoadSceneAsync(sceneName);
        asyncOperationScene.allowSceneActivation = false;

        if (loadingScreen)
        {
            GameObject loadingScreenObj = Object.Instantiate(loadScreenPrefab);
            LoadScreenManager loadingManager = loadingScreenObj.GetComponentInChildren<LoadScreenManager>();
            loadingManager.SignalLoading();

            float endTime = Time.realtimeSinceStartup;
            float elapsedTime = endTime - startTime;
            objectForLoad.StartCoroutine(ChangeLoadingIcon(loadingManager, minLoadTime - elapsedTime, enableOnLoad));

            loadingManager.onLoad += () =>
            {
                asyncOperationScene.allowSceneActivation = true;
            };
        }
        else
        {
            objectForLoad.StartCoroutine(LoadScene(sceneName, minLoadTime));
        }
    }

    private static IEnumerator LoadScene(string sceneName, float minLoadTime = 0)
    {
        if (minLoadTime > 0)
            yield return new WaitForSecondsRealtime(minLoadTime);

        asyncOperationScene.allowSceneActivation = true;
    }

    private static IEnumerator ChangeLoadingIcon(LoadScreenManager loadScreenManager,float loadTime, bool enableOnLoad)
    {
        if (loadTime > 0)
            yield return new WaitForSecondsRealtime(loadTime);

        if (enableOnLoad)
        {
            asyncOperationScene.allowSceneActivation = true;
        }
        else
        {
            loadScreenManager.SignalLoad();
        }
    }
}
