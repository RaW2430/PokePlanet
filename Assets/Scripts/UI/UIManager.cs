using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 添加场景管理引用

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        // 初始界面点击任意键进入下个场景
        if (SceneManager.GetActiveScene().buildIndex == 0 && Input.anyKeyDown)
        {
            LoadNextScene();
        }
    }

    // 点击任意键进入下个场景
    public void LoadNextScene()
    {
        // 获取当前场景索引
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // 加载下一个场景（循环到第一个场景）
        SceneManager.LoadScene((currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings);
    }

    // 点击按钮加载指定场景
    public void LoadScene(string sceneName)
    {
        // 检查场景名称是否为空
        if (!string.IsNullOrEmpty(sceneName))
        {
            // 加载指定名称的场景
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("场景名称不能为空");
        }
    }

    // 点击返回初始界面
    public void ReturnToMainMenu()
    {
        // 加载第一个场景（初始界面）
        SceneManager.LoadScene(0);
    }

    // 点击退出游戏
    public void QuitGame()
    {
        // 在编辑器中停止播放
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        // 在构建版本中退出游戏
        Application.Quit();
    }
}