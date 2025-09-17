using UnityEngine;

[System.Serializable]
public class ItemClass
{
    public int id;
    public string name;
    public Sprite sprite;
    // 可以添加更多属性
    public int level = 1;
    public int value = 0;
    public int maxLevel = 3; // 当前ID最大可以合成的level值
    //如果是 equipment，则上述属性均无效
    public string itemType = "item";    
    // 默认构造函数
    public ItemClass() { }

    // 带参数的构造函数
    public ItemClass(int id, string name, Sprite sprite, int level = 1, int value = 0, int maxLevel = 10, string itemType = "item")
    {
        this.id = id;
        this.name = name;
        this.sprite = sprite;
        this.level = level;
        this.value = value;
        this.maxLevel = maxLevel;
        this.itemType = itemType;
    }

    // 克隆方法
    public ItemClass Clone()
    {
        return new ItemClass(id, name, sprite, level, value, maxLevel, itemType);
    }
}