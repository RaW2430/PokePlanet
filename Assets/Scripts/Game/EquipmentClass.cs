using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipment", menuName = "MergeGame/Equipment")]
public class EquipmentClass : ScriptableObject
{
    [Header("Equipment Properties")]
    public int id; // Equipment的唯一标识
    public string equipmentName; // Equipment的名称

    [Header("Generation Settings")]
    public GameObject itemToGenerate; // 点击时要生成的Item Prefab
    public int maxGenerateCount = 4; // 最大可生成数量（默认为4个方向）
    
    [TextArea(3, 5)]
    public string description; // Equipment的描述
}