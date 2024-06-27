using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class WeaponDatabase : ItemDatabase<Weapon>
{
    private new Weapon selectedItem;
    private string newItemName = "";
    private bool isDuplicateName = false;
    private bool hasInvalidCharacter = false;

    private Regex nameValidationRegex = new Regex(@"^[a-zA-Z0-9 \-']*$");

    private string searchQuery = "";
    private WeaponType selectedWeaponType = WeaponType.None;

    private List<Weapon> filteredWeapons = new List<Weapon>();

    protected override void DrawItemList()
    {
        DrawSearchBar();
        DrawFilters();
        DrawItemCount();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width - propertiesSectionWidth - 20), GUILayout.Height(position.height - 20));

        FilterWeapons();

        if (filteredWeapons.Count == 0)
        {
            EditorGUILayout.LabelField("No items match your search criteria.");
            if (GUILayout.Button("Reset Filters"))
            {
                ResetFilters();
            }
        }
        else
        {
            foreach (Weapon weapon in filteredWeapons)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Box(weapon.icon ? weapon.icon.texture : Texture2D.grayTexture, GUILayout.Width(50), GUILayout.Height(50));
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(weapon.itemName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Base Value: " + weapon.baseValue.ToString());
                EditorGUILayout.LabelField("Rarity: " + weapon.rarity.ToString());
                EditorGUILayout.EndVertical();
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    selectedItem = weapon;
                    newItemName = selectedItem.itemName; // Keep track of the selected item name
                    Repaint();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.EndScrollView();
    }

    private void DrawSearchBar()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
        searchQuery = EditorGUILayout.TextField(searchQuery);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawFilters()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Type:", GUILayout.Width(50));
        selectedWeaponType = (WeaponType)EditorGUILayout.EnumPopup(selectedWeaponType);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawItemCount()
    {
        EditorGUILayout.LabelField($"Total Items: {filteredWeapons.Count}");
    }

    private void FilterWeapons()
    {
        string[] guids = AssetDatabase.FindAssets("t:Weapon");
        filteredWeapons.Clear();

        foreach (string guid in guids)
        {
            Weapon weapon = AssetDatabase.LoadAssetAtPath<Weapon>(AssetDatabase.GUIDToAssetPath(guid));
            if (weapon != null)
            {
                bool matchesSearchQuery = string.IsNullOrEmpty(searchQuery) || weapon.itemName.ToLower().Contains(searchQuery.ToLower());
                bool matchesWeaponType = selectedWeaponType == WeaponType.None || weapon.weaponType.HasFlag(selectedWeaponType);

                if (matchesSearchQuery && matchesWeaponType)
                {
                    filteredWeapons.Add(weapon);
                }
            }
        }
    }

    private void ResetFilters()
    {
        searchQuery = "";
        selectedWeaponType = WeaponType.None;
        FilterWeapons();
    }

    protected override void DrawPropertiesSection()
    {
        if (selectedItem != null)
        {
            EditorGUILayout.LabelField("Weapon Properties", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name: ");
            newItemName = EditorGUILayout.TextField(newItemName);
            EditorGUILayout.EndHorizontal();

            hasInvalidCharacter = !nameValidationRegex.IsMatch(newItemName);
            isDuplicateName = CheckForDuplicateName(newItemName, selectedItem);

            if (hasInvalidCharacter || isDuplicateName || string.IsNullOrWhiteSpace(newItemName))
            {
                GUI.color = Color.red;
            }
            else
            {
                GUI.color = Color.white;
            }

            if (hasInvalidCharacter)
            {
                EditorGUILayout.HelpBox("Item name contains invalid characters.", MessageType.Error);
            }
            else if (isDuplicateName)
            {
                EditorGUILayout.HelpBox("Item name is a duplicate.", MessageType.Error);
            }
            else if (string.IsNullOrWhiteSpace(newItemName))
            {
                EditorGUILayout.HelpBox("Item name cannot be empty.", MessageType.Error);
            }

            GUI.color = Color.white;

            // Editable fields for the weapon properties
            selectedItem.itemName = newItemName;
            selectedItem.description = EditorGUILayout.TextField("Description: ", selectedItem.description);
            selectedItem.baseValue = EditorGUILayout.FloatField("Base Value: ", selectedItem.baseValue);
            selectedItem.requiredLevel = EditorGUILayout.IntField("Required Level: ", selectedItem.requiredLevel);
            selectedItem.rarity = (Rarity)EditorGUILayout.EnumPopup("Rarity: ", selectedItem.rarity);
            selectedItem.equipSlot = (EquipSlot)EditorGUILayout.EnumPopup("Equip Slot: ", selectedItem.equipSlot);
            selectedItem.weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type: ", selectedItem.weaponType);
            selectedItem.attackPower = EditorGUILayout.FloatField("Attack Power: ", selectedItem.attackPower);
            selectedItem.attackSpeed = EditorGUILayout.FloatField("Attack Speed: ", selectedItem.attackSpeed);
            selectedItem.durability = EditorGUILayout.FloatField("Durability: ", selectedItem.durability);
            selectedItem.range = EditorGUILayout.FloatField("Range: ", selectedItem.range);
            selectedItem.criticalHitChance = EditorGUILayout.FloatField("Critical Hit Chance: ", selectedItem.criticalHitChance);

            if (GUILayout.Button("Apply Changes"))
            {
                if (string.IsNullOrWhiteSpace(newItemName) || hasInvalidCharacter || isDuplicateName)
                {
                    EditorUtility.DisplayDialog("Invalid Input", "Please fix the errors before applying changes.", "OK");
                }
                else
                {
                    EditorUtility.SetDirty(selectedItem);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("Select a weapon to see its properties.");
        }
    }

    protected override void ExportItemsToCSV()
    {
        string path = EditorUtility.SaveFilePanel("Export Weapon Data to CSV", "", "WeaponData.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                // Write header
                writer.WriteLine("Item Name,Icon Path,Weapon Type,Attack Power,Attack Speed,Durability,Range,Critical Hit Chance,Base Value,Rarity,Required Level,Equip Slot,Description");

                // Write weapon data
                string[] guids = AssetDatabase.FindAssets("t:Weapon");
                foreach (string guid in guids)
                {
                    Weapon weapon = AssetDatabase.LoadAssetAtPath<Weapon>(AssetDatabase.GUIDToAssetPath(guid));
                    if (weapon != null)
                    {
                        string line = $"{EscapeString(weapon.itemName)}," +
                                      $"{EscapeString(AssetDatabase.GetAssetPath(weapon.icon))}," +
                                      $"{weapon.weaponType}," +
                                      $"{weapon.attackPower}," +
                                      $"{weapon.attackSpeed}," +
                                      $"{weapon.durability}," +
                                      $"{weapon.range}," +
                                      $"{weapon.criticalHitChance}," +
                                      $"{weapon.baseValue}," +
                                      $"{weapon.rarity}," +
                                      $"{weapon.requiredLevel}," +
                                      $"{weapon.equipSlot}," +
                                      $"{EscapeString(weapon.description)}";

                        writer.WriteLine(line);
                    }
                }
            }
            Debug.Log("Weapon data successfully exported to CSV.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to export weapon data to CSV: {e.Message}");
        }
    }


    private string EscapeString(string str)
    {
        if (str.Contains("\t") || str.Contains("\n") || str.Contains("\""))
        {
            str = str.Replace("\"", "\"\"");
            str = $"\"{str}\"";
        }
        return str;
    }

    protected override void ImportItemsFromCSV()
    {
        string path = EditorUtility.OpenFilePanel("Import Weapon Data from CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines.Skip(1)) // Skip header line
            {
                string[] values = ParseCSVLine(line); // Using custom parsing

                if (values.Length != 13)
                {
                    Debug.LogError("CSV line does not have exactly 13 values. Skipping line.");
                    continue;
                }

                Weapon newWeapon = ScriptableObject.CreateInstance<Weapon>();
                newWeapon.itemName = values[0];
                newWeapon.icon = AssetDatabase.LoadAssetAtPath<Sprite>(values[1]);
                newWeapon.weaponType = (WeaponType)Enum.Parse(typeof(WeaponType), values[2]);
                newWeapon.attackPower = float.Parse(values[3]);
                newWeapon.attackSpeed = float.Parse(values[4]);
                newWeapon.durability = float.Parse(values[5]);
                newWeapon.range = float.Parse(values[6]);
                newWeapon.criticalHitChance = float.Parse(values[7]);
                newWeapon.baseValue = float.Parse(values[8]);
                newWeapon.rarity = (Rarity)Enum.Parse(typeof(Rarity), values[9]);
                newWeapon.requiredLevel = int.Parse(values[10]);
                newWeapon.equipSlot = (EquipSlot)Enum.Parse(typeof(EquipSlot), values[11]);
                newWeapon.description = values[12];

                string assetPath = $"Assets/Resources/Weapons/{newWeapon.itemName}.asset";
                AssetDatabase.CreateAsset(newWeapon, assetPath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Weapon data successfully imported from CSV.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to import weapon data from CSV: {e.Message}");
        }
    }

    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField += '"';
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    currentField += c;
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == '\t')
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }
        }

        result.Add(currentField);
        return result.ToArray();
    }

    protected override void DeleteSelectedItem()
    {
        if (selectedItem != null)
        {
            string itemName = selectedItem.itemName;
            bool delete = EditorUtility.DisplayDialog($"Delete {itemName}?", $"Are you sure you want to delete {itemName}?", "Yes", "No");
            if (delete)
            {
                string selectedItemPath = AssetDatabase.GetAssetPath(selectedItem);
                AssetDatabase.DeleteAsset(selectedItemPath);
                selectedItem = null;
                AssetDatabase.Refresh();
            }
        }
    }

    protected override void DuplicateSelectedItem()
    {
        if (selectedItem != null)
        {
            string path = AssetDatabase.GetAssetPath(selectedItem);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CopyAsset(path, newPath);
            AssetDatabase.Refresh();
        }
    }

    protected override void DrawTopLeftOptions()
    {
        GUILayout.Label("Database Admin Functions:", EditorStyles.boldLabel);
        if (GUILayout.Button("Export to CSV")) ExportItemsToCSV();
        if (GUILayout.Button("Import from CSV")) ImportItemsFromCSV();
        if (GUILayout.Button("Delete Selected Item")) DeleteSelectedItem();
        if (GUILayout.Button("Duplicate Selected Item")) DuplicateSelectedItem();
        if (GUILayout.Button("Create Weapon"))
        {
            WeaponCreation wc = (WeaponCreation)EditorWindow.GetWindow(typeof(WeaponCreation), false, "WeaponCreation");
        }
    }

    private bool CheckForDuplicateName(string name, Weapon currentWeapon)
    {
        string[] guids = AssetDatabase.FindAssets("t:Weapon");
        foreach (string guid in guids)
        {
            Weapon weapon = AssetDatabase.LoadAssetAtPath<Weapon>(AssetDatabase.GUIDToAssetPath(guid));
            if (weapon != null && weapon != currentWeapon && weapon.itemName == name)
            {
                return true;
            }
        }
        return false;
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(position.width - propertiesSectionWidth - 20));
        DrawTopLeftOptions();
        DrawItemList();
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(propertiesSectionWidth));
        DrawPropertiesSection();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }
}
