using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
public class UIFunctions : MonoBehaviour
{
    public GameObject pauseButton;
    public GameObject resumeButton;
    // public UIManager _UIManager;
    public GameObject UICanvasObj;
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
    private static bool IsDontDestroyOnLoadObject(GameObject obj)
    {
        // DontDestroyOnLoad对象的场景名称特殊
        return obj.scene.buildIndex == -1 && obj.scene.name == "DontDestroyOnLoad";
    }
    public void ReturnToEncyclopedia()
    {
        
    }
    public void ReturnToMainMenu()
    {
        string targetName = "UICanvasStartMenu";
        UICanvasObj = GameObject.Find(targetName);
        if (UICanvasObj == null)
        {
            // 遍历所有根对象，查找名为UICanvas的对象
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var obj in allObjects)
            {
                // 检查是否是DontDestroyOnLoad对象
                if (IsDontDestroyOnLoadObject(obj) &&
                    // 检查是否未激活
                    !obj.activeSelf &&
                    // 检查名称是否匹配（忽略大小写）
                    obj.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
                {
                    UICanvasObj = obj;
                    break;
                }
            }
        }
        if (UICanvasObj != null)
        {
            Debug.Log("Found UICanvasStartMenu");
            UICanvasObj.SetActive(true);
            // _UIManager.ShowMenu();
            // 加载第一个场景（初始界面）
            SceneManager.LoadScene(0);
        }
        else
        {
            Debug.LogError("UICanvasStartMenu not found");
        }
    }
}
