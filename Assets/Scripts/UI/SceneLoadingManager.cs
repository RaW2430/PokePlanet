using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;  
public class SceneLoadingManager : MonoBehaviour
{

    public TMP_Text progressTMP;   // 显示加载进度
    public float smoothSpeed = 1f;  // 百分比平滑速度
    public float minShowTime = 0.3f;  // 显示100%时间

    private void Start()
    {
        // 等一帧，让场景渲染完成
        StartCoroutine(DelayedLoad());
    }

    private IEnumerator DelayedLoad()
    {
        yield return null; // 等一帧，确保 Camera 渲染
        yield return StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        string target = SceneLoader.NextSceneName;

        if (string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("NextSceneName为空，加载索引0场景");
            SceneManager.LoadScene(0);
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(target);
        op.allowSceneActivation = false;

        float displayedPercent = 0f;

        while (!op.isDone)
        {
            float rawPercent = Mathf.Clamp01(op.progress / 0.9f) * 100f;
            displayedPercent = Mathf.MoveTowards(displayedPercent, rawPercent, smoothSpeed * Time.deltaTime);
            progressTMP.text = Mathf.RoundToInt(displayedPercent) + "%";

            if (op.progress >= 0.9f)
            {
                while (displayedPercent < 100f)
                {
                    displayedPercent = Mathf.MoveTowards(displayedPercent, 100f, smoothSpeed * Time.deltaTime);
                    progressTMP.text = Mathf.RoundToInt(displayedPercent) + "%";
                    yield return null;
                }

                float timer = 0f;
                while (timer < minShowTime)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
