using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System;

public class WeaponDatabase : ItemDatabase<Weapon>
{
 
    private string newItemName = "";
    private bool isDuplicateName = false;
    private bool hasInvalidCharacter = false;

    private Regex nameValidationRegex = new Regex(@"^[a-zA-Z0-9 \-']*$");

    private string searchQuery = "";
    private WeaponType[] weaponTypes; // Array to hold all weapon types
    private int selectedWeaponTypeIndex = 0;

    private List<Weapon> filteredWeapons = new List<Weapon>();

    protected override void DrawItemList()
    {
        DrawSearchBar();
        DrawFilters();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width - propertiesSectionWidth - 20), GUILayout.Height(position.height - 20));

        FilterWeapons();

        if (filteredWeapons.Count == 0)
        {
            EditorGUILayout.LabelField("No items match your search criteria.");
            if (GUILayout.Button("Reset Search"))
            {
                searchQuery = "";
                selectedWeaponTypeIndex = 0;
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
                    selectedItem = weapon; // Use the base class's selectedItem
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
        selectedWeaponTypeIndex = EditorGUILayout.Popup(selectedWeaponTypeIndex, GetWeaponTypeNames());
        EditorGUILayout.EndHorizontal();
    }

    private string[] GetWeaponTypeNames()
    {
        if (weaponTypes == null)
        {
            weaponTypes = (WeaponType[])Enum.GetValues(typeof(WeaponType));
        }

        string[] names = new string[weaponTypes.Length + 1];
        names[0] = "All";
        for (int i = 0; i < weaponTypes.Length; i++)
        {
            names[i + 1] = weaponTypes[i].ToString();
        }

        return names;
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
                bool matchesSearchQuery = weapon.itemName.ToLower().Contains(searchQuery.ToLower());
                bool matchesWeaponType = selectedWeaponTypeIndex == 0 || weapon.weaponType.HasFlag(weaponTypes[selectedWeaponTypeIndex - 1]);

                // Debug logs to see the values
                Debug.Log($"Weapon: {weapon.itemName}, Search Query Match: {matchesSearchQuery}, Weapon Type Match: {matchesWeaponType}, Weapon Type: {weapon.weaponType}, Selected Type: {(selectedWeaponTypeIndex == 0 ? "All" : weaponTypes[selectedWeaponTypeIndex - 1].ToString())}");

                if (matchesSearchQuery && matchesWeaponType)
                {
                    filteredWeapons.Add(weapon);
                }
            }
        }
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
        // Export functionality implementation
    }

    protected override void ImportItemsFromCSV()
    {
        // Import functionality implementation
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
