using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Equipment Settings")]
    public ItemClass itemData; // Item的属性数据
    public GameObject[] itemToGenerate; // 点击时要生成的Item Prefab数组
    public int maxGenerateCount = 1; // 最大可生成数量（一次点击只生成一个）
    public int maxGeneratePerTime = 6;
    private int currentGenerateCount = 0;
    private bool isOnCooldown = false;
    private bool isDragging = false;
    private Vector2 offset;
    private Camera mainCamera;
    private GridManager gridManager; // 引用GridManager
    private Vector2Int gridPosition; // 记录当前所在的网格位置
    
    // 添加高亮效果相关变量
    private bool isHighlighted = false;
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    // 用于判断是点击还是拖拽的阈值
    private float dragThreshold = 5f; // 增加阈值以适应屏幕坐标
    private Vector3 mouseDownPosition;

    void Start()
    {
        mainCamera = Camera.main;
        // 查找场景中的GridManager实例
        gridManager = FindObjectOfType<GridManager>();

        // 获取SpriteRenderer组件用于高亮效果
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 记录原始缩放
        originalScale = transform.localScale;

        // 确保Equipment也有ItemManager组件
        ItemManager itemManager = GetComponent<ItemManager>();
        if (itemManager == null)
        {
            // 如果没有ItemManager组件，则添加一个
            itemManager = gameObject.AddComponent<ItemManager>();
        }
        // 设置ItemManager的属性
        itemManager.itemData = itemData;
        gridPosition = itemManager.gridPosition; // 初始化gridPosition
        
        // 设置贴图为ItemClass中指定的贴图，并调整大小以适应当前item
        if (itemData != null && itemData.sprite != null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = itemData.sprite;

                // 调整贴图大小以适应当前item，而不改变transform的scale
                if (spriteRenderer.sprite != null)
                {
                    // 获取当前SpriteRenderer的边界大小
                    Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

                    // 获取当前对象的预期大小（这里假设是1x1单位，您可以根据实际情况调整）
                    Vector2 targetSize = new Vector2(1, 1); // 默认大小

                    // 如果您的对象有Collider，可以使用Collider的大小
                    Collider2D col = GetComponent<Collider2D>();
                    if (col != null)
                    {
                        targetSize = col.bounds.size;
                    }

                    // 设置SpriteRenderer为Sliced模式以允许自定义大小
                    spriteRenderer.drawMode = SpriteDrawMode.Sliced;

                    // 设置贴图大小以匹配目标大小
                    spriteRenderer.size = targetSize;
                }
            }
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition - offset;
        }
    }

    void OnMouseDown()
    {
        Debug.Log("Equipment被点击 - 开始处理");
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        mouseDownPosition = Input.mousePosition;
        isDragging = true; // 开始拖拽
    }

    void OnMouseUp()
    {
        isDragging = false; // 结束拖拽
        
        Vector2 mouseUpPosition = Input.mousePosition;
        float distance = Vector2.Distance(mouseDownPosition, mouseUpPosition);

        // 如果鼠标移动距离小于阈值，则认为是点击，否则是拖拽
        if (distance < dragThreshold)
        {
            Debug.Log("Equipment被点击 - 触发生成事件");
            HighlightEquipment(true);
            Invoke("ResetHighlight", 0.2f); // 0.2秒后取消高亮
            GenerateItemsAroundEquipment();
            // 增加生成计数
            currentGenerateCount++;
            
            // 检查是否达到最大生成数量
            if (currentGenerateCount >= maxGeneratePerTime)
            {
                // 开始冷却
                StartCoroutine(StartCooldown());
            }
        }
        else
        {
            Debug.Log("Equipment被拖拽");

            // 如果存在GridManager，则将物体吸附到最近的网格
            if (gridManager != null)
            {
                // 获取当前世界坐标对应的网格坐标
                Vector2Int newGridPos = gridManager.WorldToGridPosition(transform.position);

                // 检查位置是否有效
                if (newGridPos.x >= 0 && newGridPos.x < gridManager.gridWidth &&
                    newGridPos.y >= 0 && newGridPos.y < gridManager.gridHeight)
                {
                    // 检查目标位置是否已被占用
                    ItemManager existingItem = gridManager.GetItem(newGridPos.x, newGridPos.y);
                    if (existingItem != null && existingItem != GetComponent<ItemManager>())
                    {
                        // 如果目标位置已被其他item占用，寻找最近的合法位置
                        newGridPos = FindNearestValidPosition(newGridPos);
                    }

                    // 如果新位置与原位置不同，需要清除原位置的注册信息
                    if (newGridPos.x != gridPosition.x || newGridPos.y != gridPosition.y)
                    {
                        // 清除原来位置的注册信息
                        gridManager.ClearItem(gridPosition.x, gridPosition.y);
                    }

                    // 更新网格位置
                    gridPosition = newGridPos;

                    // 将物体位置设置到网格中心
                    Vector3 snapPosition = gridManager.GridToWorldPosition(gridPosition.x, gridPosition.y);
                    transform.position = snapPosition;

                    // 将Equipment作为Item注册到GridManager中
                    ItemManager itemManager = GetComponent<ItemManager>();
                    if (itemManager != null)
                    {
                        gridManager.SetItem(gridPosition.x, gridPosition.y, itemManager);
                        Debug.Log($"Equipment吸附到网格位置: ({gridPosition.x}, {gridPosition.y}) 并注册为Item");
                    }
                }
            }
        }
    }

    // 高亮Equipment
    void HighlightEquipment(bool highlight)
    {
        if (spriteRenderer != null)
        {
            if (highlight)
            {
                // 放大1.1倍并改变颜色
                transform.localScale = originalScale * 1.1f;
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.7f);
            }
            else
            {
                // 恢复原始状态
                transform.localScale = originalScale;
                spriteRenderer.color = originalColor;
            }
        }
        isHighlighted = highlight;
    }

    // 重置高亮状态
    void ResetHighlight()
    {
        HighlightEquipment(false);
    }

    // 在Equipment周围生成item
    public void GenerateItemsAroundEquipment()
    {
        if (itemToGenerate == null || gridManager == null)
        {
            Debug.LogError("要生成的Item Prefab或GridManager为空");
            return;
        }
        // 检查是否在冷却中
        if (isOnCooldown)
        {
            Debug.Log("正在冷却中，无法生成新的item");
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayFailMergeSound();
            }
            return;
        }
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayIncubateSound();
        }
        // 获取当前装备的位置
        Vector2Int currentPos = gridPosition;
        
        Debug.Log($"开始在位置 ({currentPos.x}, {currentPos.y}) 周围生成item");
        
        // 定义八个方向：上下左右 + 四个对角线方向
        List<Vector2Int> directions = new List<Vector2Int>()
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            new Vector2Int(-1, 1),  // 左上
            new Vector2Int(1, 1),   // 右上
            new Vector2Int(-1, -1), // 左下
            new Vector2Int(1, -1)   // 右下
        };
        
        // 随机打乱方向列表
        ShuffleList(directions);
        
        // 首先尝试在八个方向上生成
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPos = currentPos + direction;
            
            // 检查相邻位置是否有效且不等于Equipment自身位置
            if (neighborPos.x >= 0 && neighborPos.x < gridManager.gridWidth &&
                neighborPos.y >= 0 && neighborPos.y < gridManager.gridHeight &&
                !(neighborPos.x == currentPos.x && neighborPos.y == currentPos.y))
            {
                // 检查相邻位置是否为空（没有item且没有equipment）
                if (IsPositionEmpty(neighborPos))
                {
                    Debug.Log($"在位置 ({neighborPos.x}, {neighborPos.y}) 生成item");
                    // 随机选择一个Prefab生成
                    GameObject randomPrefab = itemToGenerate[Random.Range(0, itemToGenerate.Length)];
                    GenerateItemAtPosition(neighborPos, randomPrefab);
                    
                    return; // 一次点击只生成一个
                }
            }
        }
        
        // 如果八个方向都没有空位，使用广度优先搜索继续生成
        GenerateItemWithBFS(currentPos);
    }
    // 添加冷却协程
    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        Debug.Log("开始3秒冷却");
        yield return new WaitForSeconds(3f);
        isOnCooldown = false;
        currentGenerateCount = 0;
        Debug.Log("冷却结束，可以继续生成");
    }

    // 使用广度优先搜索生成item
    void GenerateItemWithBFS(Vector2Int startPosition)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        queue.Enqueue(startPosition);
        visited.Add(startPosition);
        
        int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
        int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };
        
        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();
            
            // 检查当前位置是否为空且不等于Equipment位置
            if (!(currentPos.x == gridPosition.x && currentPos.y == gridPosition.y) &&
                currentPos.x >= 0 && currentPos.x < gridManager.gridWidth &&
                currentPos.y >= 0 && currentPos.y < gridManager.gridHeight &&
                IsPositionEmpty(currentPos))
            {
                Debug.Log($"通过BFS在位置 ({currentPos.x}, {currentPos.y}) 生成item");
                // 随机选择一个Prefab生成
                GameObject randomPrefab = itemToGenerate[Random.Range(0, itemToGenerate.Length)];
                GenerateItemAtPosition(currentPos, randomPrefab);
                return; // 一次点击只生成一个
            }
            
            // 将相邻位置加入队列
            for (int i = 0; i < 8; i++)
            {
                Vector2Int nextPos = new Vector2Int(currentPos.x + dx[i], currentPos.y + dy[i]);
                
                if (nextPos.x >= 0 && nextPos.x < gridManager.gridWidth &&
                    nextPos.y >= 0 && nextPos.y < gridManager.gridHeight &&
                    !visited.Contains(nextPos))
                {
                    queue.Enqueue(nextPos);
                    visited.Add(nextPos);
                }
            }
        }
        
        Debug.Log("没有找到空位置生成item");
    }

    // 检查指定位置是否为空（没有item）
    bool IsPositionEmpty(Vector2Int position)
    {
        // 检查是否有item（包括equipment，因为equipment也是item）
        bool hasItem = gridManager.GetItem(position.x, position.y) != null;
        
        return !hasItem;
    }
    
    // 寻找最近的合法位置
    Vector2Int FindNearestValidPosition(Vector2Int originalPosition)
    {
        // 使用广度优先搜索寻找最近的空位置
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        queue.Enqueue(originalPosition);
        visited.Add(originalPosition);
        
        int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
        int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };
        
        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();
            
            // 检查当前位置是否有效且为空
            if (currentPos.x >= 0 && currentPos.x < gridManager.gridWidth &&
                currentPos.y >= 0 && currentPos.y < gridManager.gridHeight &&
                IsPositionEmpty(currentPos))
            {
                Debug.Log($"找到最近的合法位置: ({currentPos.x}, {currentPos.y})");
                return currentPos;
            }
            
            // 将相邻位置加入队列
            for (int i = 0; i < 8; i++)
            {
                Vector2Int nextPos = new Vector2Int(currentPos.x + dx[i], currentPos.y + dy[i]);
                
                if (nextPos.x >= 0 && nextPos.x < gridManager.gridWidth &&
                    nextPos.y >= 0 && nextPos.y < gridManager.gridHeight &&
                    !visited.Contains(nextPos))
                {
                    queue.Enqueue(nextPos);
                    visited.Add(nextPos);
                }
            }
        }
        
        // 如果没有找到空位置，返回原始位置
        Debug.Log("未找到合法位置，返回原始位置");
        return originalPosition;
    }

    // 在指定位置生成item
    void GenerateItemAtPosition(Vector2Int position, GameObject itemPrefab)
    {
        // 获取世界坐标
        Vector3 worldPosition = gridManager.GridToWorldPosition(position.x, position.y);

        // 实例化item prefab
        GameObject newItem = Instantiate(itemPrefab, worldPosition, Quaternion.identity);

        // 获取新生成的ItemManager组件
        ItemManager newItemManager = newItem.GetComponent<ItemManager>();
        if (newItemManager != null)
        {
            // 在GridManager中注册新item的位置
            gridManager.SetItem(position.x, position.y, newItemManager);
            newItem.GetComponent<ItemManager>().gridPosition = position;
        }

        Debug.Log($"在位置 ({position.x}, {position.y}) 生成item: {itemPrefab.name}");
        // Debug.Log($"gridManager 生成item: {itemPrefab.name}");
    }
    
    // 随机打乱列表
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}