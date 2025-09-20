using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 添加UI引用
using TMPro; // 添加TextMeshPro引用 
public class MissonController : MonoBehaviour
{
    public GameObject orgnizationIcon;
    public GameObject neededPanel;
    public GameObject textPanel;
    public GameObject successImage;
    // public GameObject successImage2;
    public int creaturesValue = 10; // 每个creature成功触发后增加的金币数
    public GameObject[] creatures;
    // 需要的数量
    public int neededNum;
    // 添加对GridManager的引用
    private GridManager gridManager;
    
    // 添加对OrganizationManager的引用
    private OrganizationManager organizationManager;
    
    // 添加字典存储按钮与对应的id和level
    public Dictionary<Button, KeyValuePair<int, int>> missionTargets = new Dictionary<Button, KeyValuePair<int, int>>();
    
    // 添加HashSet来跟踪已成功触发的按钮
    private HashSet<Button> completedButtons = new HashSet<Button>();
    public TMP_Text countdownTextMeshPro;
    public int countdownTime = 15; // 倒计时秒数
    // 添加倒计时相关变量
    private bool isCountingDown = false;
    private float countdownTimer = 0f;
    void Start()
    {
        // 初始化需要的数量
        neededNum = creatures.Length;

        // 查找场景中的GridManager实例
        gridManager = FindObjectOfType<GridManager>();

        // 查找场景中的OrganizationManager实例
        organizationManager = FindObjectOfType<OrganizationManager>();

        // 为creatures数组中的每个GameObject添加按钮点击监听器
        foreach (GameObject creature in creatures)
        {
            Button button = creature.GetComponent<Button>();
            if (button != null)
            {
                // 从按钮的Image组件sprite名称中提取id和level
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null && buttonImage.sprite != null)
                {
                    string spriteName = buttonImage.sprite.name;
                    // 假设命名格式为item_id_level，例如item_0_3
                    if (spriteName.StartsWith("item_"))
                    {
                        string[] parts = spriteName.Split('_');
                        if (parts.Length >= 3)
                        {
                            if (int.TryParse(parts[1], out int id) && int.TryParse(parts[2], out int level))
                            {
                                missionTargets[button] = new KeyValuePair<int, int>(id, level);
                            }
                        }
                    }
                }

                // 添加按钮点击监听器
                button.onClick.AddListener(() => OnMissionButtonClick(button));
            }
        }
        // 初始化倒计时显示
        if (countdownTextMeshPro != null)
        {
            countdownTextMeshPro.text = countdownTime.ToString();
        }
        //开始倒计时
        StartCountdown();
    }

    // Update is called once per frame
    void Update()
    {
        // 处理倒计时逻辑
        if (isCountingDown)
        {
            
            countdownTimer -= Time.deltaTime;

            // 更新倒计时显示
            if (countdownTextMeshPro != null)
            {
                countdownTextMeshPro.text = Mathf.CeilToInt(countdownTimer).ToString();
            }

            if (countdownTimer <= 0)
            {
                // 倒计时结束
                OnCountdownFinished();
                isCountingDown = false;
            }
        }
    }
    
    // 倒计时结束的处理方法
    private void OnCountdownFinished()
    {
        // Debug.Log("倒计时结束");
        if (organizationManager.organizationNums == 1 && organizationManager.coins < organizationManager.coinsNeeded && AudioManager.instance != null)
        {
            AudioManager.instance.PlayFailSound();
        }

        // 更新organization数据，但不给金币奖励
        if (organizationManager != null)
        {
            organizationManager.organizationNums = organizationManager.organizationNums == 0 ? 0 : organizationManager.organizationNums - 1 ;
            // 更新文本显示
            organizationManager.UpdateTexts();
            // 激活下一个panel并隐藏当前激活的panel
            if (organizationManager.organizationNums > 0)
            {
                organizationManager.ActivateNextPanel();
            }
            // Debug.Log($"倒计时结束，已更新OrganizationManager的值：organizationNums = {organizationManager.organizationNums}");
        }
    }
    // 添加开始倒计时的方法
    public void StartCountdown()
    {
        if (!isCountingDown)
        {
            isCountingDown = true;
            countdownTimer = countdownTime;
            // Debug.Log("开始倒计时");
        }
    }
    // 按钮点击事件处理函数
    private void OnMissionButtonClick(Button clickedButton)
    {
        Debug.Log("按钮被点击");
        // 检查按钮是否已经成功触发过
        if (completedButtons.Contains(clickedButton))
        {
            Debug.Log("按钮已经成功触发过，无法再次触发");
            return;
        }
        
        // 检查GridManager是否存在
        if (gridManager == null)
        {
            Debug.LogError("GridManager实例未找到");
            return;
        }

        // 检查按钮是否在任务目标字典中
        if (!missionTargets.ContainsKey(clickedButton))
        {
            Debug.LogWarning("按钮未注册为任务目标");
            return;
        }

        // 获取目标id和level
        KeyValuePair<int, int> target = missionTargets[clickedButton];
        int targetId = target.Key;
        int targetLevel = target.Value;

        // 查找GridManager中匹配id和level的item
        List<Vector2Int> matchingItems = new List<Vector2Int>();
        
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                ItemManager item = gridManager.GetItem(x, y);
                if (item != null && item.itemData != null)
                {
                    if (item.itemData.id == targetId && item.itemData.level == targetLevel)
                    {
                        matchingItems.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        // 如果找到了匹配的item
        if (matchingItems.Count > 0)
        {
            // 随机选择一个匹配的item进行删除
            Vector2Int selectedItemPos = matchingItems[Random.Range(0, matchingItems.Count)];
            
            // 获取item引用
            ItemManager selectedItem = gridManager.GetItem(selectedItemPos.x, selectedItemPos.y);
            
            // 销毁游戏对象
            if (selectedItem != null)
            {
                Destroy(selectedItem.gameObject);
            }
            
            // 从GridManager中注销注册
            gridManager.ClearItem(selectedItemPos.x, selectedItemPos.y);
            
            // 将按钮添加到已完成的集合中
            completedButtons.Add(clickedButton);
            
            // 激活名为SuccessImage的子对象
            Transform successImageTransform = clickedButton.transform.Find("SuccessImage");
            if (successImageTransform != null)
            {
                successImageTransform.gameObject.SetActive(true);
                // 可选：在几秒后自动隐藏successImage
                // StartCoroutine(HideSuccessImageAfterDelay(2f));
            }
            
            // 调用OrganizationManager的接口修改coins和organizationNums值
            if (organizationManager != null)
            {
                // 增加coins值（例如增加10）
                organizationManager.coins += creaturesValue;
                neededNum -= 1;
                // 增加organizationNums值
                if (neededNum == 0)
                {
                    organizationManager.organizationNums -= 1;
                    if(organizationManager.organizationNums == 0)
                    {
                        isCountingDown = false;
                    }
                    if (AudioManager.instance != null)
                    {
                        AudioManager.instance.PlayCoinSound();
                    }
                    // 更新文本显示
                    organizationManager.UpdateTexts();
                    // 激活下一个panel并隐藏当前激活的panel
                    if (organizationManager.organizationNums > 0)
                    {
                        organizationManager.ActivateNextPanel();
                    }
                    Debug.Log($"已更新OrganizationManager的值：coins = {organizationManager.coins}, organizationNums = {organizationManager.organizationNums}");
                }
                
            }
            
            Debug.Log($"成功删除位置 ({selectedItemPos.x}, {selectedItemPos.y}) 的item (ID: {targetId}, Level: {targetLevel})");
        }
        else
        {
            Debug.Log($"未找到匹配的item (ID: {targetId}, Level: {targetLevel})");
        }
    }

    // 延迟隐藏successImage的协程
    private IEnumerator HideSuccessImageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (successImage != null)
        {
            successImage.SetActive(false);
        }
    }

    // 1. 激活或隐藏某个UI对象
    public void ToggleUIObject(GameObject uiObject, bool isActive)
    {
        if (uiObject != null)
        {
            uiObject.SetActive(isActive);
        }
    }

    // 2. 激活某个组件指定秒数
    public void ActivateComponentForSeconds(Component component, float seconds)
    {
        if (component != null)
        {
            component.gameObject.SetActive(true);
            StartCoroutine(DeactivateAfterDelay(component.gameObject, seconds));
        }
    }

    private IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

    // 3. 为按钮点击提供点击绑定事件
    public void BindButtonClickListener(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction listener)
    {
        if (button != null && listener != null)
        {
            button.onClick.AddListener(listener);
        }
    }

    // 4. 空白接口，用来更改某个类的数据
    public void UpdateData<T>(T dataContainer, System.Action<T> updateAction) where T : class
    {
        if (dataContainer != null && updateAction != null)
        {
            updateAction(dataContainer);
        }
    }
}