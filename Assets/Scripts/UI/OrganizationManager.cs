using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting; // 添加TextMeshPro引用

public class OrganizationManager : MonoBehaviour
{
    // 保存UI panel的列表
    public List<GameObject> panelList = new List<GameObject>();
    // 金币数
    public int coins;
    // 通关金币数
    public int coinsNeeded = 10;
    // 组织数量
    public int organizationNums;
    // 当前激活的panel索引
    public int currentPanelIndex = 0;
    public GameObject orgsText;
    public GameObject coinText;
    
    // TextMeshPro组件引用
    private TMP_Text orgsTextMeshPro;
    private TMP_Text coinTextMeshPro;

    // 结算面板
    public GameObject scorePanel;

    // Start is called before the first frame update
    void Start()
    {
        // 获取TextMeshPro组件
        if (orgsText != null)
            orgsTextMeshPro = orgsText.GetComponent<TMP_Text>();
        
        if (coinText != null)
            coinTextMeshPro = coinText.GetComponent<TMP_Text>();
        
        // 初始化显示
        UpdateTexts();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTexts();
        UpdateScorePanel();
    }
    
    // 更新文本显示的方法
    public void UpdateTexts()
    {
        if (orgsTextMeshPro != null)
            orgsTextMeshPro.text = organizationNums.ToString();
            
        if (coinTextMeshPro != null)
            coinTextMeshPro.text = coins.ToString() + " / " + coinsNeeded.ToString();
    }
    
    // 触发接口：激活指定索引的panel
    public void ActivatePanel(int index)
    {
        if (index >= 0 && index < panelList.Count)
        {
            // 如果之前有激活的panel，先将其关闭
            if (currentPanelIndex >= 0 && currentPanelIndex < panelList.Count)
            {
                panelList[currentPanelIndex].SetActive(false);
            }
            
            // 激活指定索引的panel
            panelList[index].SetActive(true);
            currentPanelIndex = index;
        }
        else
        {
            Debug.LogWarning("Invalid panel index: " + index);
        }
    }

    public void UpdateScorePanel()
    {
        if (organizationNums == 0 && scorePanel != null)
        {
            scorePanel.SetActive(true);
            if (coins >= coinsNeeded)
            {
                // 通关
                scorePanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "You Win! \n New Creature Unlocked!";
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.PlayWinSound();
                }
            }
            else
            {
                // 未通关
                scorePanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "Game Over";
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.PlayFailSound();
                }
            }   
        }
    }   

    // 触发接口：激活下一个panel
    public void ActivateNextPanel()
    {
        // 如果列表为空，直接返回
        if (panelList.Count == 0)
        {
            Debug.LogWarning("Panel list is empty");
            return;
        }

        // 计算下一个panel的索引
        int nextIndex = (currentPanelIndex + 1) % panelList.Count;

        // 激活下一个panel
        ActivatePanel(nextIndex);
    }
    
    // 触发接口：删除指定索引的panel
    public void RemovePanel(int index)
    {
        if (index >= 0 && index < panelList.Count)
        {
            GameObject panelToRemove = panelList[index];
            panelList.RemoveAt(index);
            Destroy(panelToRemove);
            
            // 如果删除的是当前激活的panel或之前的panel，需要更新currentPanelIndex
            if (index == currentPanelIndex)
            {
                // 当前激活的panel被删除，重置索引
                currentPanelIndex = -1;
            }
            else if (index < currentPanelIndex)
            {
                // 删除的panel在当前激活panel之前，索引需要调整
                currentPanelIndex--;
            }
        }
        else
        {
            Debug.LogWarning("Invalid panel index: " + index);
        }
    }
}