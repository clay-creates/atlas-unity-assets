using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/BaseItem")]
public abstract class BaseItem : ScriptableObject
{
    [Header("General Properties")]
    [SerializeField]
    public string itemName;
    [SerializeField]
    public Sprite icon;
    [SerializeField]
    public string description;
    [SerializeField]
    public float baseValue;
    [SerializeField]
    public Rarity rarity;
    [SerializeField]
    public int requiredLevel; // Required level to use the item
    [SerializeField]
    public EquipSlot equipSlot; // Where the item can be equipped
}

