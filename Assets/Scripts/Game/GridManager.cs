using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 1f;
    public Vector2 gridOffset = Vector2.zero;

    [Header("Visual Settings")]
    public Color gridLineColor = Color.white;
    public Color cellColor = new Color(1f, 1f, 0.8f, 0.5f); // 淡黄色
    public float lineWidth = 0.05f;

    [Header("Texture Settings")]
    public Texture2D backgroundTexture;
    public Texture2D cellTexture;
    public Texture2D borderTexture; 
    public Texture2D blockTexture;
    public Texture2D[] creaturesTextures;
    public bool useTextures = false;

    // 添加指定棋盘在Game中位置的属性
    [Header("Position Settings")]
    public Vector2 gamePosition = Vector2.zero;

    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer borderRenderer; // 新增：外围渲染器
    private GameObject gridContainer;
    private Dictionary<Vector2Int, GameObject> gridCells;
    
    // 添加二维数组来记录每个位置的item信息
    private ItemManager[,] itemGrid;

    

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        // 创建网格容器
        gridContainer = new GameObject("GridContainer");
        gridContainer.transform.SetParent(transform);
        // 设置棋盘在Game中的位置
        gridContainer.transform.position = gamePosition;

        gridCells = new Dictionary<Vector2Int, GameObject>();
        
        // 初始化itemGrid二维数组
        itemGrid = new ItemManager[gridWidth, gridHeight];

        // 创建背景
        CreateBackground();

        // 创建外围贴图（在背景之后，格子之前）
        CreateBorder();

        // 创建网格单元
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                CreateGridCell(x, y);
            }
        }
        CreateOuterBlocks();
        // 创建网格线
        // DrawGridLines();
    }
    void CreateOuterBlocks()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // 检查是否为最外一圈的cell
                if (x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1)
                {
                    // 基于坐标生成伪随机数，确保相同位置总是得到相同的结果
                    int seed = x * 1000 + y;
                    System.Random random = new System.Random(seed);
                    
                    // 使用伪随机数决定是否生成block（例如50%概率）
                    if (random.NextDouble() > 0.5)
                    {
                        CreateBlock(x, y);
                    }
                }
            }
        }
    }
    void CreateBlock(int x, int y)
    {
        GameObject block = new GameObject($"Block_{x}_{y}");
        block.transform.SetParent(gridContainer.transform);
        // 将位置设置为格子的正中心
        block.transform.localPosition = new Vector3(
            gridOffset.x + x * cellSize + cellSize / 2f,
            gridOffset.y + y * cellSize + cellSize / 2f,
            0.6f // 设置z轴位置略高于普通格子，确保显示在上层
        );

        // 先随机选择一个creaturesTextures贴图
        Texture2D selectedCreatureTexture = null;
        if (useTextures && creaturesTextures != null && creaturesTextures.Length > 0)
        {
            // 基于坐标生成伪随机数，确保相同位置总是得到相同的结果
            int seed = x * 1000 + y;
            System.Random random = new System.Random(seed);
            int randomIndex = random.Next(creaturesTextures.Length);
            selectedCreatureTexture = creaturesTextures[randomIndex];
        }

        // 创建第一个SpriteRenderer用于显示creaturesTextures贴图
        if (useTextures && selectedCreatureTexture != null)
        {
            GameObject creatureObject = new GameObject("CreatureTexture");
            creatureObject.transform.SetParent(block.transform);
            creatureObject.transform.localPosition = Vector3.zero; // 与父对象在同一位置
            
            SpriteRenderer creatureRenderer = creatureObject.AddComponent<SpriteRenderer>();
            creatureRenderer.sortingOrder = 1; // 确保渲染顺序正确
            creatureRenderer.drawMode = SpriteDrawMode.Sliced; // 允许调整大小
            
            creatureRenderer.sprite = CreateSpriteFromTexture(selectedCreatureTexture);
            // 设置贴图大小以完全覆盖cell
            creatureRenderer.size = new Vector2(cellSize, cellSize);
        }

        // 创建第二个SpriteRenderer用于显示blockTexture贴图
        if (useTextures && blockTexture != null)
        {
            GameObject blockObject = new GameObject("BlockTexture");
            blockObject.transform.SetParent(block.transform);
            blockObject.transform.localPosition = Vector3.zero; // 与父对象在同一位置
            
            SpriteRenderer blockRenderer = blockObject.AddComponent<SpriteRenderer>();
            blockRenderer.sortingOrder = 2; // 确保渲染顺序正确，显示在creatureTexture之上
            blockRenderer.drawMode = SpriteDrawMode.Sliced; // 允许调整大小
            
            blockRenderer.sprite = CreateSpriteFromTexture(blockTexture);
            // 设置贴图大小以完全覆盖cell
            blockRenderer.size = new Vector2(cellSize, cellSize);
        }
        else
        {
            // 如果没有指定blockTexture，创建一个默认的纯色贴图
            Texture2D blockTexture2D = new Texture2D(1, 1);
            blockTexture2D.SetPixel(0, 0, new Color(0.7f, 0.7f, 0.9f, 1f)); // 默认淡蓝色
            blockTexture2D.Apply();
            
            GameObject blockObject = new GameObject("DefaultBlockTexture");
            blockObject.transform.SetParent(block.transform);
            blockObject.transform.localPosition = Vector3.zero; // 与父对象在同一位置
            
            SpriteRenderer blockRenderer = blockObject.AddComponent<SpriteRenderer>();
            blockRenderer.sortingOrder = 2; // 确保渲染顺序正确
            blockRenderer.drawMode = SpriteDrawMode.Sliced; // 允许调整大小
            
            blockRenderer.sprite = CreateSpriteFromTexture(blockTexture2D);
            // 设置贴图大小以完全覆盖cell
            blockRenderer.size = new Vector2(cellSize, cellSize);
        }
        // 添加ItemManager组件表示该单元格已被占用
        ItemManager itemManager = block.AddComponent<ItemManager>();
        // 注册到GridManager中
        SetItem(x, y, itemManager);
    }

    void CreateBackground()
    {
        GameObject background = new GameObject("GridBackground");
        background.transform.SetParent(gridContainer.transform);
        // 朝右上角移动半个格子
        background.transform.localPosition = new Vector3(
            gridOffset.x + (gridWidth * cellSize) / 2f - cellSize / 2f + cellSize / 2f,
            gridOffset.y + (gridHeight * cellSize) / 2f - cellSize / 2f + cellSize / 2f,
            0.1f
        );

        backgroundRenderer = background.AddComponent<SpriteRenderer>();
        backgroundRenderer.sortingOrder = -1;

        // 创建背景贴图
        if (useTextures && backgroundTexture != null)
        {
            backgroundRenderer.sprite = CreateSpriteFromTexture(backgroundTexture);
        }
        else
        {
            // 创建纯色背景
            Texture2D bgTexture = new Texture2D(1, 1);
            bgTexture.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.7f, 1f));
            bgTexture.Apply();
            backgroundRenderer.sprite = CreateSpriteFromTexture(bgTexture);
        }

        // 设置背景大小
        backgroundRenderer.transform.localScale = new Vector3(
            gridWidth * cellSize,
            gridHeight * cellSize,
            1f
        );
    }
    void CreateGridCell(int x, int y)
    {
        GameObject cell = new GameObject($"Cell_{x}_{y}");
        cell.transform.SetParent(gridContainer.transform);
        // 将位置设置为格子的正中心，而不是左下角
        cell.transform.localPosition = new Vector3(
            gridOffset.x + x * cellSize + cellSize / 2f,
            gridOffset.y + y * cellSize + cellSize / 2f,
            0.5f
        );

        SpriteRenderer cellRenderer = cell.AddComponent<SpriteRenderer>();
        cellRenderer.sortingOrder = 0;
        cellRenderer.drawMode = SpriteDrawMode.Sliced; // 允许调整大小

        // 创建格子贴图
        if (useTextures && cellTexture != null)
        {
            cellRenderer.sprite = CreateSpriteFromTexture(cellTexture);
        }
        else
        {
            // 创建纯色格子
            Texture2D cellTexture2D = new Texture2D(1, 1);
            cellTexture2D.SetPixel(0, 0, cellColor);
            cellTexture2D.Apply();
            cellRenderer.sprite = CreateSpriteFromTexture(cellTexture2D);
        }

        // 使用size属性而不是scale来调整贴图大小，确保贴图在格子内部
        lineWidth = 0f; // 设置线宽为0，贴图间无缝隙
        cellRenderer.size = new Vector2(cellSize - lineWidth, cellSize - lineWidth);

        gridCells[new Vector2Int(x, y)] = cell;
    }
    // 添加创建外围贴图的方法
    void CreateBorder()
    {
        // 创建外围对象
        GameObject borderObject = new GameObject("GridBorder");
        borderObject.transform.SetParent(gridContainer.transform);
        borderObject.transform.localPosition = new Vector3(
            gridOffset.x + (gridWidth * cellSize) / 2f,
            gridOffset.y + (gridHeight * cellSize) / 2f,
            0.05f // 在背景和格子之间
        );
        
        // 添加SpriteRenderer组件
        borderRenderer = borderObject.AddComponent<SpriteRenderer>();
        borderRenderer.sortingOrder = -1; // 确保在外围，但比格子低一层
        borderRenderer.drawMode = SpriteDrawMode.Sliced; // 允许调整大小

        // 创建外围贴图
        if (useTextures && borderTexture != null)
        {
            borderRenderer.sprite = CreateSpriteFromTexture(borderTexture);
            // 设置外围贴图大小略大于背景，使用size属性而不是scale
            float borderSizeMultiplier = 1.05f; // 可根据需要调整
            borderRenderer.size = new Vector2(
                gridWidth * cellSize * borderSizeMultiplier,
                gridHeight * cellSize * borderSizeMultiplier
            );
        }
        // else
        // {
        //     // 如果没有指定外围贴图，创建一个默认的外围贴图
        //     Texture2D borderTexture2D = new Texture2D(1, 1);
        //     borderTexture2D.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.6f, 1f));
        //     borderTexture2D.Apply();
        //     borderRenderer.sprite = CreateSpriteFromTexture(borderTexture2D);
        // }
        
        
    }
    void DrawGridLines()
    {
        GameObject lineContainer = new GameObject("GridLines");
        lineContainer.transform.SetParent(gridContainer.transform);
        lineContainer.transform.localPosition = Vector3.zero;

        // 为每条线创建独立的LineRenderer
        // 创建水平线
        for (int y = 0; y <= gridHeight; y++)
        {
            GameObject lineObject = new GameObject($"HorizontalLine_{y}");
            lineObject.transform.SetParent(lineContainer.transform);
            lineObject.transform.localPosition = Vector3.zero;

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = gridLineColor;
            lineRenderer.endColor = gridLineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.sortingOrder = 1;
            lineRenderer.useWorldSpace = false;

            float yPos = gridOffset.y + y * cellSize;
            Vector3[] points = new Vector3[]
            {
                new Vector3(gridOffset.x, yPos, 0f),
                new Vector3(gridOffset.x + gridWidth * cellSize, yPos, 0f)
            };

            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(points);
        }

        // 创建垂直线
        for (int x = 0; x <= gridWidth; x++)
        {
            GameObject lineObject = new GameObject($"VerticalLine_{x}");
            lineObject.transform.SetParent(lineContainer.transform);
            lineObject.transform.localPosition = Vector3.zero;

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = gridLineColor;
            lineRenderer.endColor = gridLineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.sortingOrder = 1;
            lineRenderer.useWorldSpace = false;

            float xPos = gridOffset.x + x * cellSize;
            Vector3[] points = new Vector3[]
            {
                new Vector3(xPos, gridOffset.y, 0f),
                new Vector3(xPos, gridOffset.y + gridHeight * cellSize, 0f)
            };

            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(points);
        }
    }

    Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    // 获取指定位置的格子
    public GameObject GetCell(int x, int y)
    {
        Vector2Int key = new Vector2Int(x, y);
        if (gridCells.ContainsKey(key))
        {
            return gridCells[key];
        }
        return null;
    }

    // 获取世界坐标对应的格子坐标
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - gridOffset.x - gamePosition.x) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.y - gridOffset.y - gamePosition.y) / cellSize);
        return new Vector2Int(x, y);
    }

    // 获取格子中心的世界坐标
    public Vector3 GridToWorldPosition(int x, int y)
    {
        return new Vector3(
            gamePosition.x + gridOffset.x + x * cellSize + cellSize / 2f,
            gamePosition.y + gridOffset.y + y * cellSize + cellSize / 2f,
            0f
        );
    }

    // 设置指定位置的item
    public void SetItem(int x, int y, ItemManager item)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            itemGrid[x, y] = item;
            // 每次设置item后输出所有已存在的item属性
            LogAllItems();
        }
    }

    // 获取指定位置的item
    public ItemManager GetItem(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return itemGrid[x, y];
        }
        return null;
    }

    // 清除指定位置的item
    public void ClearItem(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            itemGrid[x, y] = null;
            // 每次清除item后输出所有已存在的item属性
            LogAllItems();
        }
    }

    // 输出所有已存在于grid中的item信息
    public void LogAllItems()
    {
        Debug.Log("=== 网格中所有物品信息 ===");
        bool hasItems = false;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                ItemManager item = itemGrid[x, y];
                if (item != null && item.itemData != null)
                {
                    Debug.Log($"  位置 ({x},{y}): ID={item.itemData.id}, Name={item.itemData.name}, Level={item.itemData.level}, MaxLevel={item.itemData.maxLevel}");
                    hasItems = true;
                }
            }
        }
        
        if (!hasItems)
        {
            Debug.Log("  网格中暂无物品");
        }
    }

    // 设置初始状态：在指定位置生成特定prefab，不触发合并
    public void SetInitialState(Vector2Int position, GameObject prefab)
    {
        // 检查位置是否有效
        if (position.x < 0 || position.x >= gridWidth || position.y < 0 || position.y >= gridHeight)
        {
            Debug.LogError($"位置 ({position.x}, {position.y}) 超出网格范围");
            return;
        }

        // 检查该位置是否已经有物品
        if (itemGrid[position.x, position.y] != null)
        {
            Debug.LogWarning($"位置 ({position.x}, {position.y}) 已有物品，将被替换");
            Destroy(itemGrid[position.x, position.y].gameObject);
        }

        // 获取世界坐标
        Vector3 worldPosition = GridToWorldPosition(position.x, position.y);
        
        // 实例化prefab
        GameObject itemObject = Instantiate(prefab, worldPosition, Quaternion.identity);
        
        // 获取ItemManager组件
        ItemManager itemManager = itemObject.GetComponent<ItemManager>();
        if (itemManager != null)
        {
            // 在GridManager中注册新item的位置
            SetItem(position.x, position.y, itemManager);
            Debug.Log($"在位置 ({position.x}, {position.y}) 生成初始物品: {prefab.name}");
        }
        else
        {
            Debug.LogError($"Prefab {prefab.name} 缺少 ItemManager 组件");
            Destroy(itemObject);
        }
    }

    // 批量设置初始状态
    public void SetInitialStates(Vector2Int[] positions, GameObject[] prefabs)
    {
        // 检查数组长度是否匹配
        if (positions.Length != prefabs.Length)
        {
            Debug.LogError("位置数组和Prefab数组长度不匹配");
            return;
        }

        // 依次设置每个位置的初始状态
        for (int i = 0; i < positions.Length; i++)
        {
            SetInitialState(positions[i], prefabs[i]);
        }
    }

    void OnDrawGizmos()
    {
        // 在Scene视图中显示网格轮廓
        Gizmos.color = gridLineColor;
        
        // 绘制水平线
        for (int y = 0; y <= gridHeight; y++)
        {
            float yPos = gamePosition.y + gridOffset.y + y * cellSize;
            Vector3 start = new Vector3(gamePosition.x + gridOffset.x, yPos, 0f);
            Vector3 end = new Vector3(gamePosition.x + gridOffset.x + gridWidth * cellSize, yPos, 0f);
            Gizmos.DrawLine(start, end);
        }

        // 绘制垂直线
        for (int x = 0; x <= gridWidth; x++)
        {
            float xPos = gamePosition.x + gridOffset.x + x * cellSize;
            Vector3 start = new Vector3(xPos, gamePosition.y + gridOffset.y, 0f);
            Vector3 end = new Vector3(xPos, gamePosition.y + gridOffset.y + gridHeight * cellSize, 0f);
            Gizmos.DrawLine(start, end);
        }
    }
}