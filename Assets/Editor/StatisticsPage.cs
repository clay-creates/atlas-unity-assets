using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StatisticsPage : EditorWindow
{
    [MenuItem("Window/Item Manager/Statistics Page")]
    public static void ShowWindow()
    {
        GetWindow<StatisticsPage>("Statistics Page");
    }

    private Vector2 scrollPosition;
    private bool showGeneralStats = true;
    private bool showWeaponStats = true;
    private bool showArmorStats = true;
    private bool showPotionStats = true;

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

        EditorGUILayout.LabelField("Item Statistics Page", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This page provides statistics about the different types of items in the database.", MessageType.Info);

        EditorGUILayout.Space();
        showGeneralStats = EditorGUILayout.Foldout(showGeneralStats, "General Item Statistics");
        if (showGeneralStats)
        {
            DrawGeneralStats();
        }

        EditorGUILayout.Space();
        showWeaponStats = EditorGUILayout.Foldout(showWeaponStats, "Weapon Statistics");
        if (showWeaponStats)
        {
            DrawWeaponStats();
        }

        EditorGUILayout.Space();
        showArmorStats = EditorGUILayout.Foldout(showArmorStats, "Armor Statistics");
        if (showArmorStats)
        {
            DrawArmorStats();
        }

        EditorGUILayout.Space();
        showPotionStats = EditorGUILayout.Foldout(showPotionStats, "Potion Statistics");
        if (showPotionStats)
        {
            DrawPotionStats();
        }

        GUILayout.EndScrollView();
    }

    private void DrawGeneralStats()
    {
        int totalItems = AssetDatabase.FindAssets("t:BaseItem").Length;
        int totalWeapons = AssetDatabase.FindAssets("t:Weapon").Length;
        int totalPotions = AssetDatabase.FindAssets("t:Potion").Length;
        int totalArmors = AssetDatabase.FindAssets("t:Armor").Length;

        GUILayout.Label($"Total Items: {totalItems}");
        GUILayout.Label($"Total Weapons: {totalWeapons}");
        GUILayout.Label($"Total Potions: {totalPotions}");
        GUILayout.Label($"Total Armors: {totalArmors}");
    }

    private void DrawWeaponStats()
    {
        var weapons = GetAllItems<Weapon>();
        if (weapons.Count == 0)
        {
            GUILayout.Label("No weapons found.");
            return;
        }

        GUILayout.Label($"Total Weapons: {weapons.Count}");
        GUILayout.Label($"Average Attack Power: {weapons.Average(w => w.attackPower):0.00}");
        GUILayout.Label($"Average Base Value: {weapons.Average(w => w.baseValue):0.00}");
        GUILayout.Label($"Average Attack Speed: {weapons.Average(w => w.attackSpeed):0.00}");

        var rarityGroups = weapons.GroupBy(w => w.rarity)
            .Select(g => new { Rarity = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count);

        GUILayout.Label("Weapons by Rarity:");
        foreach (var group in rarityGroups)
        {
            GUILayout.Label($"{group.Rarity}: {group.Count}");
        }

        var mostCommonWeapon = weapons.GroupBy(w => w.itemName)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (mostCommonWeapon != null)
        {
            GUILayout.Label($"Most Common Weapon: {mostCommonWeapon.Key} ({mostCommonWeapon.Count()})");
        }

        var weaponEfficiency = weapons.Select(w => new { w.itemName, Efficiency = w.attackPower / w.baseValue })
            .OrderByDescending(w => w.Efficiency)
            .FirstOrDefault();

        if (weaponEfficiency != null)
        {
            GUILayout.Label($"Most Efficient Weapon: {weaponEfficiency.itemName} (Efficiency: {weaponEfficiency.Efficiency:0.00})");
        }
    }

    private void DrawArmorStats()
    {
        var armors = GetAllItems<Armor>();
        if (armors.Count == 0)
        {
            GUILayout.Label("No armors found.");
            return;
        }

        GUILayout.Label($"Total Armors: {armors.Count}");
        GUILayout.Label($"Average Defense Power: {armors.Average(a => a.defensePower):0.00}");
        GUILayout.Label($"Average Base Value: {armors.Average(a => a.baseValue):0.00}");

        var rarityGroups = armors.GroupBy(a => a.rarity)
            .Select(g => new { Rarity = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count);

        GUILayout.Label("Armors by Rarity:");
        foreach (var group in rarityGroups)
        {
            GUILayout.Label($"{group.Rarity}: {group.Count}");
        }

        var mostCommonArmor = armors.GroupBy(a => a.itemName)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (mostCommonArmor != null)
        {
            GUILayout.Label($"Most Common Armor: {mostCommonArmor.Key} ({mostCommonArmor.Count()})");
        }
    }

    private void DrawPotionStats()
    {
        var potions = GetAllItems<Potion>();
        if (potions.Count == 0)
        {
            GUILayout.Label("No potions found.");
            return;
        }

        GUILayout.Label($"Total Potions: {potions.Count}");
        GUILayout.Label($"Average Duration: {potions.Average(p => p.duration):0.00}");
        GUILayout.Label($"Average Cooldown: {potions.Average(p => p.cooldown):0.00}");
        GUILayout.Label($"Average Base Value: {potions.Average(p => p.baseValue):0.00}");

        var rarityGroups = potions.GroupBy(p => p.rarity)
            .Select(g => new { Rarity = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count);

        GUILayout.Label("Potions by Rarity:");
        foreach (var group in rarityGroups)
        {
            GUILayout.Label($"{group.Rarity}: {group.Count}");
        }

        var mostCommonPotion = potions.GroupBy(p => p.itemName)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (mostCommonPotion != null)
        {
            GUILayout.Label($"Most Common Potion: {mostCommonPotion.Key} ({mostCommonPotion.Count()})");
        }
    }

    private List<T> GetAllItems<T>() where T : BaseItem
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        List<T> items = new List<T>();

        foreach (string guid in guids)
        {
            T item = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            if (item != null)
            {
                items.Add(item);
            }
        }

        return items;
    }
}
