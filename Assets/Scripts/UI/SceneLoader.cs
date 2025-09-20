using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string NextSceneName;

    // 调用此方法进入 LoadingScene，并传递目标场景
    public static void LoadWithLoadingScene(string sceneName)
    {
        NextSceneName = sceneName;
        SceneManager.LoadScene("LoadingScene"); // 确保 LoadingScene 已加入 Build Settings
    }
}
