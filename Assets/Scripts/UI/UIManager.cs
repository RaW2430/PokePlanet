using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager instance; // 添加单例引用
    public GameObject UICanvasObj;
    public GameObject[] allPanels;
    public GameObject menuPanel;
    public GameObject levelTipsPanel;
    public GameObject encyclopediaPanel;
    public GameObject zhuHuanEncPanel;
    public GameObject baiXianEncPanel;
    public GameObject pauseButton;
    public GameObject resumeButton;

    public bool isBaiXianEncUnlocked = false;
    public GameObject isBaiXianEncLockIcon;
    
    public bool isZhuHuanEncUnlocked = false;
    public GameObject isZhuHuanEncLockIcon;

    void Awake()
    {
        // 实现单例模式
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (menuPanel != null)
        {
            hideAllPanel();
            menuPanel.SetActive(true);
        }
        Time.timeScale = 1f; // 确保游戏开始时未暂停
    }

    void Update()
    {
        // 初始界面点击任意键进入下个场景
        // if (SceneManager.GetActiveScene().buildIndex == 0 && Input.anyKeyDown)
        // {
        //     LoadNextScene();
        // }
    }

    public void hideAllPanel()
    {
        foreach (GameObject panel in allPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
    public void ShowZhuHuanEnc()
    {
        if (zhuHuanEncPanel != null && isZhuHuanEncUnlocked)
        {
            hideAllPanel();
            zhuHuanEncPanel.SetActive(true);
        }
    }
    public void ShowBaiXianEnc()
    {
        if (baiXianEncPanel != null && isBaiXianEncUnlocked)
        {
            hideAllPanel();
            baiXianEncPanel.SetActive(true);
        }
    }
    // public void UnlockZhuHuanEnc()
    // {
    //     isZhuHuanEncUnlocked = true;
    //     if (isZhuHuanEncLockIcon != null)
    //     {
    //         isZhuHuanEncLockIcon.SetActive(false);
    //     }
    // }
    // public void UnlockBaiXianEnc()
    // {
    //     isBaiXianEncUnlocked = true;
    //     if (isBaiXianEncLockIcon != null)
    //     {
    //         isBaiXianEncLockIcon.SetActive(false);
    //     }
    // }
    public void PauseGame()
    {
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
            Time.timeScale = 0f; // 暂停游戏
            resumeButton.SetActive(true);
        }
    }
    public void ResumeGame()
    {
        if (resumeButton != null)
        {
            resumeButton.SetActive(false);
            Time.timeScale = 1f; // 恢复游戏
            pauseButton.SetActive(true);
        }
    }
    public void ShowMenu()
    {
        hideAllPanel();
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
    }   
    public void ShowEncyclopedia()
    {
        hideAllPanel();
        if (encyclopediaPanel != null)
        {
            encyclopediaPanel.SetActive(true);
        }
    }
    public void ShowLevelTips()
    {
        if (levelTipsPanel != null)
        {
            hideAllPanel();
            levelTipsPanel.SetActive(true);
        }
    }
    public void HideLevelTips()
    {
        if (levelTipsPanel != null)
        {
            hideAllPanel();
            menuPanel.SetActive(true);
        }
    }
    // 点击任意键进入下个场景
    public void LoadNextScene()
    {
        // 获取当前场景索引
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        // 加载下一个场景
        SceneManager.LoadScene((currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings);
        
        UICanvasObj.SetActive(false);
    }

    // 点击按钮加载指定场景
    public void LoadScene(string sceneName)
    {
        // 检查场景名称是否为空
        if (!string.IsNullOrEmpty(sceneName))
        {
            // 使用 SceneLoader 跳转到 LoadingScene，并传递目标场景名
            SceneLoader.LoadWithLoadingScene(sceneName);

            UICanvasObj.SetActive(false);
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