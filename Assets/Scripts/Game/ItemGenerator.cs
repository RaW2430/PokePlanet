using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    private GridManager gridManager; // 引用GridManager

    [Header("起始生成设置")]
    public GameObject startPrefab;      // 想生成的拖拽物品
    private Vector2Int startGridPos = new Vector2Int(2, 2); // 指定格子

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        // 场景加载后立刻在指定格子生成一个可拖拽物品
        if (startPrefab != null && gridManager != null)
            GenerateItemAtGridPosition(startGridPos.x, startGridPos.y, startPrefab);
    }

    // 在指定网格位置生成prefab
    public void GenerateItemAtGridPosition(int x, int y, GameObject itemPrefab)
    {
        // 检查GridManager是否存在
        if (gridManager == null)
        {
            Debug.LogError("GridManager未找到, 请确保场景中有GridManager组件");
            return;
        }

        // 检查prefab是否存在
        if (itemPrefab == null)
        {
            Debug.LogError("要生成的Prefab为空");
            return;
        }

        // 检查位置是否有效
        if (x < 0 || x >= gridManager.gridWidth || y < 0 || y >= gridManager.gridHeight)
        {
            Debug.LogError($"位置 ({x}, {y}) 超出网格范围 (0-{gridManager.gridWidth-1}, 0-{gridManager.gridHeight-1})");
            return;
        }

        // 检查该位置是否已经有物品
        if (gridManager.GetItem(x, y) != null)
        {
            Debug.LogWarning($"位置 ({x}, {y}) 已有物品，无法生成新物品");
            return;
        }

        // 获取世界坐标
        Vector3 worldPosition = gridManager.GridToWorldPosition(x, y);

        // 实例化prefab
        GameObject newItem = Instantiate(itemPrefab, worldPosition, Quaternion.identity);

        // 获取新生成的ItemManager组件
        ItemManager newItemManager = newItem.GetComponent<ItemManager>();
        if (newItemManager != null)
        {
            // 设置ItemManager的gridPosition为生成位置
            newItemManager.gridPosition = new Vector2Int(x, y);
            // 在GridManager中注册新item的位置
            gridManager.SetItem(x, y, newItemManager);      
        }
        else
        {
            Debug.LogWarning($"生成的Prefab {itemPrefab.name} 缺少ItemManager组件");
            // 如果没有ItemManager组件，则添加一个
            newItemManager = newItem.AddComponent<ItemManager>();
            newItemManager.gridPosition = new Vector2Int(x, y);
            gridManager.SetItem(x, y, newItemManager);   
        }
        // gridManager.LogAllItems();
        Debug.Log($"在网格位置 ({x}, {y}) 成功生成item: {itemPrefab.name}");
    }

    // 在指定世界坐标位置生成prefab（自动转换为最近的网格位置）
    public void GenerateItemAtWorldPosition(Vector3 worldPosition, GameObject itemPrefab)
    {
        // 检查GridManager是否存在
        if (gridManager == null)
        {
            Debug.LogError("GridManager未找到，请确保场景中有GridManager组件");
            return;
        }

        // 获取世界坐标对应的网格坐标
        Vector2Int gridPos = gridManager.WorldToGridPosition(worldPosition);

        // 调用网格位置生成方法
        GenerateItemAtGridPosition(gridPos.x, gridPos.y, itemPrefab);
    }

    // 批量生成items
    public void GenerateItemsAtPositions(Vector2Int[] positions, GameObject[] prefabs)
    {
        // 检查数组长度是否匹配
        if (positions.Length != prefabs.Length)
        {
            Debug.LogError("位置数组和Prefab数组长度不匹配");
            return;
        }

        // 依次生成每个位置的item
        for (int i = 0; i < positions.Length; i++)
        {
            GenerateItemAtGridPosition(positions[i].x, positions[i].y, prefabs[i]);
        }
    }

    // 随机在空位置生成指定数量的items
    public void GenerateRandomItems(GameObject itemPrefab, int count)
    {
        // 检查GridManager是否存在
        if (gridManager == null)
        {
            Debug.LogError("GridManager未找到，请确保场景中有GridManager组件");
            return;
        }

        // 收集所有空位置
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                if (gridManager.GetItem(x, y) == null)
                {
                    emptyPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        // 检查是否有足够的空位置
        if (emptyPositions.Count < count)
        {
            Debug.LogWarning($"网格中只有 {emptyPositions.Count} 个空位置，少于请求的 {count} 个");
            count = emptyPositions.Count;
        }

        // 随机打乱位置列表
        ShuffleList(emptyPositions);

        // 生成指定数量的items
        for (int i = 0; i < count; i++)
        {
            Vector2Int pos = emptyPositions[i];
            GenerateItemAtGridPosition(pos.x, pos.y, itemPrefab);
        }
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