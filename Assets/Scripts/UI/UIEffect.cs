using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 添加TextMeshPro引用

public class UIEffect : MonoBehaviour
{
    // 定义可选择的效果类型枚举
    public enum EffectType
    {
        None,
        Blink,
        Fade,
        Pulse
    }
    
    [Header("Effect Settings")]
    [Tooltip("选择要应用的UI效果类型")]
    public EffectType selectedEffect = EffectType.None;
    
    [Header("UI Component")]
    [Tooltip("拖入要应用效果的TextMeshPro组件")]
    public TMP_Text targetText; // 开发者需要拖入的TextMeshPro组件
    
    // Blink效果参数
    [Header("Blink Settings")]
    public float blinkSpeed = 1.0f; // 闪烁速度
    public float minAlpha = 0.0f; // 最小透明度
    public float maxAlpha = 1.0f; // 最大透明度
    
    private Color originalColor; // 原始颜色
    private bool isBlinking = false; // 是否正在闪烁

    // Start is called before the first frame update
    void Start()
    {
        // 获取原始颜色
        if (targetText != null)
        {
            originalColor = targetText.color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 根据选择的效果类型执行相应效果
        switch (selectedEffect)
        {
            case EffectType.Blink:
                if (!isBlinking && targetText != null)
                {
                    StartCoroutine(BlinkEffect());
                    isBlinking = true;
                }
                break;
            case EffectType.None:
            default:
                // 恢复原始状态
                if (targetText != null)
                {
                    targetText.color = originalColor;
                }
                StopAllCoroutines();
                isBlinking = false;
                break;
        }
    }
    
    // 闪烁效果协程
    IEnumerator BlinkEffect()
    {
        while (true)
        {
            // 逐渐降低透明度
            for (float t = 0; t < 1; t += Time.deltaTime * blinkSpeed)
            {
                if (targetText != null)
                {
                    Color newColor = targetText.color;
                    newColor.a = Mathf.Lerp(maxAlpha, minAlpha, t);
                    targetText.color = newColor;
                }
                yield return null;
            }
            
            // 逐渐增加透明度
            for (float t = 0; t < 1; t += Time.deltaTime * blinkSpeed)
            {
                if (targetText != null)
                {
                    Color newColor = targetText.color;
                    newColor.a = Mathf.Lerp(minAlpha, maxAlpha, t);
                    targetText.color = newColor;
                }
                yield return null;
            }
        }
    }
    
    // 当组件被禁用时停止所有效果
    private void OnDisable()
    {
        if (targetText != null && selectedEffect == EffectType.Blink)
        {
            targetText.color = originalColor;
        }
        StopAllCoroutines();
        isBlinking = false;
    }
}