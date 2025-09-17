using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("Merge Effects")]
    public AudioClip mergeSound; // 合并成功音效
    public AudioClip failMergeSound; // 合并失败音效
    public float animationDuration = 1.0f; // 动画持续时间
    private AudioSource audioSource; // 音频源组件

    [Header("Item Settings")]
    public ItemClass itemData; // Item的属性数据
    [Header("Gizmos Settings")]
    public Color gizmosColor = Color.black; // Gizmos线条颜色，默认为黑色
    private bool isDragging = false;
    private Vector2 offset;
    private Camera mainCamera;
    private GridManager gridManager; // 引用GridManager
    public Vector2Int gridPosition; // 记录当前所在的网格位置
    private Vector2Int previousGridPosition; // 记录之前的网格位置

    // 添加点击判断：鼠标移动距离阈值
    private float clickDistanceThreshold = 5f; // 像素单位
    private Vector3 mouseDownScreenPos;

    void Start()
    {
        mainCamera = Camera.main;
        // 查找场景中的GridManager实例
        gridManager = FindObjectOfType<GridManager>();
        // 获取或添加AudioSource组件
        // audioSource = GetComponent<AudioSource>();
        // if (audioSource == null)
        // {
        //     audioSource = gameObject.AddComponent<AudioSource>();
        // }

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
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        previousGridPosition = gridPosition; // 记录拖拽前的位置

        // 记录鼠标按下时的屏幕位置
        mouseDownScreenPos = Input.mousePosition;
    }

    void OnMouseUp()
    {
        isDragging = false;

        // 判断是否为点击（时间小于阈值且几乎没有移动）
        float distance = Vector3.Distance(Input.mousePosition, mouseDownScreenPos);
        if (distance < clickDistanceThreshold)
        {
            HandleMouseClick();
            return; // 点击事件处理后直接返回
        }

        // 拖拽
        // 如果存在GridManager，则将物体吸附到最近的网格
        if (gridManager != null)
        {
            // 获取当前世界坐标对应的网格坐标
            Vector2Int newGridPos = gridManager.WorldToGridPosition(transform.position);

            // 检查位置是否在网格范围内
            if (newGridPos.x < 0 || newGridPos.x >= gridManager.gridWidth ||
                newGridPos.y < 0 || newGridPos.y >= gridManager.gridHeight)
            {
                // 如果超出网格范围，返回原始位置
                transform.position = gridManager.GridToWorldPosition(gridPosition.x, gridPosition.y);
                PlayFailMergeSound();
                return;
            }

            // 检查位置是否有效
            if (newGridPos.x >= 0 && newGridPos.x < gridManager.gridWidth &&
                newGridPos.y >= 0 && newGridPos.y < gridManager.gridHeight)
            {
                // 检查目标位置是否已被占用
                ItemManager existingItem = gridManager.GetItem(newGridPos.x, newGridPos.y);
                if (existingItem != null && existingItem != this)
                {
                    // 如果目标位置已有其他item，检查是否可以合并
                    if (CanMergeWith(existingItem))
                    {
                        // 检查是否已经达到最大等级
                        if (this.itemData.level < this.itemData.maxLevel)
                        {
                            // 执行合并逻辑
                            MergeWith(existingItem);
                            PlayMergeSound();
                        }
                        else
                        {
                            // 如果已达到最大等级，不能合并，寻找最近的合法位置
                            newGridPos = FindNearestValidPosition(newGridPos);
                            // 播放合并失败音效
                            PlayFailMergeSound();
                        }
                        // 无论是否合并，都需要更新位置
                    }
                    else
                    {
                        // 如果不能合并，寻找最近的合法位置
                        newGridPos = FindNearestValidPosition(newGridPos);
                        PlayFailMergeSound();
                    }
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

                // 在GridManager中注册当前位置的item
                gridManager.SetItem(gridPosition.x, gridPosition.y, this);

                // 输出GridManager中所有已存在的item信息
                gridManager.LogAllItems();

                // 输出四个方向网格中item的class属性
                LogSurroundingItems(gridPosition);

                // 检查相邻格子是否有相同item可以合并
                //CheckAndMergeItems(gridPosition);
            }
        }
    }

    void HandleMouseClick()
    {
        if (itemData != null && itemData.level != itemData.maxLevel)
        {
            return;
        }

    }
    // 检查是否可以与目标item合并
    bool CanMergeWith(ItemManager targetItem)
    {
        // 如果目标item为空，则可以放置
        if (targetItem == null) return true;

        // 如果目标item与当前item具有相同的ID和等级，则可以合并
        if (itemData != null && targetItem.itemData != null &&
            itemData.id == targetItem.itemData.id &&
            itemData.level == targetItem.itemData.level)
        {
            return true;
        }

        return false;
    }

    // 执行与目标item的合并
    void MergeWith(ItemManager targetItem)
    {
        // 检查是否已经达到最大等级
        if (this.itemData.level < this.itemData.maxLevel)
        {
            // 获取合并后的prefab
            GameObject mergedPrefab = GetMergedItemPrefab(this.itemData);

            if (mergedPrefab == null)
            {
                // 播放合并失败音效
                PlayFailMergeSound();
                // 原Debug语句已删除
                return;
            }

            // 获取目标item的位置（合并位置）
            Vector2Int targetPos = gridManager.WorldToGridPosition(targetItem.transform.position);

            // 从GridManager中清除被拖拽item的位置信息（当前item）
            gridManager.ClearItem(gridPosition.x, gridPosition.y);

            // 不需要清除目标item的位置信息，因为新物品将生成在这里

            // 销毁两个item
            Destroy(targetItem.gameObject);
            Destroy(this.gameObject);

            // 生成下一个等级的item prefab在合并位置
            SpawnMergedItem(targetPos, mergedPrefab);
        }
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
                // 原Debug语句已删除
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
        // 原Debug语句已删除
        return originalPosition;
    }

    // 检查指定位置是否为空
    bool IsPositionEmpty(Vector2Int position)
    {
        // 检查是否有item（包括equipment，因为equipment也是item）
        bool hasItem = gridManager.GetItem(position.x, position.y) != null;

        return !hasItem;
    }

    // 输出四个方向网格中item的class属性
    void LogSurroundingItems(Vector2Int gridPos)
    {
        // 定义四个方向：上、下、左、右
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        string[] directionNames = { "上", "下", "左", "右" };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int neighborPos = gridPos + directions[i];

            // 检查相邻位置是否有效
            if (neighborPos.x >= 0 && neighborPos.x < gridManager.gridWidth &&
                neighborPos.y >= 0 && neighborPos.y < gridManager.gridHeight)
            {
                // 从GridManager获取相邻格子中的ItemManager组件
                ItemManager neighborItem = gridManager.GetItem(neighborPos.x, neighborPos.y);
                if (neighborItem != null)
                {
                    ItemClass itemClass = neighborItem.itemData;
                    if (itemClass != null)
                    {
                        // 原Debug语句已删除
                    }
                    else
                    {
                        // 原Debug语句已删除
                    }
                }
                else
                {
                    // 原Debug语句已删除
                }
            }
            else
            {
                // 原Debug语句已删除
            }
        }
    }

    // 检查并合并相邻的相同item
    void CheckAndMergeItems(Vector2Int gridPos)
    {
        // 定义四个方向：上、下、左、右
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPos = gridPos + direction;

            // 检查相邻位置是否有效
            if (neighborPos.x >= 0 && neighborPos.x < gridManager.gridWidth &&
                neighborPos.y >= 0 && neighborPos.y < gridManager.gridHeight)
            {
                // 从GridManager获取相邻格子中的ItemManager组件
                ItemManager neighborItem = gridManager.GetItem(neighborPos.x, neighborPos.y);

                if (neighborItem != null && neighborItem != this)
                {
                    // 检查是否为相同类型的item（ID和Level都相同）
                    if (neighborItem.itemData.id == this.itemData.id &&
                        neighborItem.itemData.level == this.itemData.level)
                    {
                        // 检查是否已经达到最大等级
                        if (this.itemData.level < this.itemData.maxLevel)
                        {
                            // 获取合并后的prefab
                            GameObject mergedPrefab = GetMergedItemPrefab(this.itemData);

                            if (mergedPrefab == null)
                            {
                                // 原Debug语句已删除
                                return;
                            }

                            // 从GridManager中清除两个item的位置信息
                            gridManager.ClearItem(neighborPos.x, neighborPos.y);
                            gridManager.ClearItem(gridPos.x, gridPos.y);

                            // 销毁两个item
                            Destroy(neighborItem.gameObject);
                            Destroy(this.gameObject);

                            // 生成下一个等级的item prefab
                            SpawnMergedItem(neighborPos, mergedPrefab);
                        }
                        // 如果已达到最大等级，则不执行任何操作，允许两个最大等级的item相邻放置
                        return; // 一次只合并一次
                    }
                }
            }
        }
    }

    // 获取合并后的item prefab
    GameObject GetMergedItemPrefab(ItemClass originalItem)
    {
        // 这里需要根据你的游戏逻辑来获取下一个等级的prefab
        // 示例实现：假设prefab名称为 "Item_[ID]_[Level+1]"
        string prefabName = $"Item_{originalItem.id}_{originalItem.level + 1}";
        return Resources.Load<GameObject>($"Prefabs/{prefabName}");
    }

    // 生成合并后的item
    void SpawnMergedItem(Vector2Int position, GameObject prefab)
    {
        // 获取世界坐标
        Vector3 worldPosition = gridManager.GridToWorldPosition(position.x, position.y);

        // 实例化prefab
        GameObject mergedItem = Instantiate(prefab, worldPosition, Quaternion.identity);

        // 获取新生成的ItemManager组件
        ItemManager newItemManager = mergedItem.GetComponent<ItemManager>();
        if (newItemManager != null)
        {
            Debug.Log($"生成了新的item (ID: {newItemManager.itemData.id}, Level: {newItemManager.itemData.level}) at Grid Position ({position.x}, {position.y})");
            // 在GridManager中注册新item的位置
            newItemManager.gridPosition = position;
            gridManager.SetItem(position.x, position.y, newItemManager);

            // 播放合并成功音效
            newItemManager.PlayMergeSound();

            // 启动放大缩小动画
            StartCoroutine(newItemManager.PlayScaleAnimation(mergedItem));
        }
        // 启动放大缩小动画
        // StartCoroutine(PlayScaleAnimation(mergedItem));
    }
    // 新增的放大缩小动画协程
    IEnumerator PlayScaleAnimation(GameObject target)
    {
        if (target == null) yield break;
        
        float duration = 1.0f; // 动画持续时间1秒
        float elapsed = 0f;
        Vector3 originalScale = target.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f; // 放大到1.2倍
        
        // 放大阶段
        while (elapsed < duration / 2)
        {
            target.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / (duration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 缩小阶段
        elapsed = 0f;
        while (elapsed < duration / 2)
        {
            target.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / (duration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终回到原始大小
        target.transform.localScale = originalScale;
    }
    void OnDrawGizmos()
    {
        // 设置Gizmos颜色
        Gizmos.color = gizmosColor;

        // 获取Collider组件来确定边界
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // 绘制矩形轮廓（无论是什么类型的Collider）
            DrawBoxColliderGizmos(col as BoxCollider2D);
        }
        else
        {
            // 如果没有Collider，绘制默认大小的轮廓
            Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 0));
        }
    }

    // 绘制BoxCollider2D的轮廓
    void DrawBoxColliderGizmos(BoxCollider2D boxCollider)
    {
        // 如果传入的不是BoxCollider2D，使用默认边界
        if (boxCollider == null)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 0));
            }
            return;
        }

        Vector3 center = transform.TransformPoint(boxCollider.offset);
        Vector3 size = new Vector3(boxCollider.size.x * transform.lossyScale.x,
                                  boxCollider.size.y * transform.lossyScale.y,
                                  0);

        Gizmos.DrawWireCube(center, size);
    }
    // 播放合并成功音效
    public void PlayMergeSound()
    {
        Debug.Log("播放合并成功音效");
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMergeSound();
        }
    }

    // 播放合并失败音效
    public void PlayFailMergeSound()
    {
        Debug.Log("播放合并失败音效");
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayFailMergeSound();
        }
    }
}