using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public class PotionDatabase : ItemDatabase<Potion>
{
    private new Potion selectedItem;
    private string newItemName = "";
    private bool isDuplicateName = false;
    private bool hasInvalidCharacter = false;

    private Regex nameValidationRegex = new Regex(@"^[a-zA-Z0-9 \-']*$");

    protected override void DrawItemList()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width - propertiesSectionWidth - 20), GUILayout.Height(position.height - 20));

        string[] guids = AssetDatabase.FindAssets("t:potion");
        foreach (string guid in guids)
        {
            Potion potion = AssetDatabase.LoadAssetAtPath<Potion>(AssetDatabase.GUIDToAssetPath(guid));
            if (potion != null)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Box(potion.icon ? potion.icon.texture : Texture2D.grayTexture, GUILayout.Width(50), GUILayout.Height(50));
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(potion.itemName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Base Value: " + potion.baseValue.ToString());
                EditorGUILayout.LabelField("Rarity: " + potion.rarity.ToString());
                EditorGUILayout.EndVertical();
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    selectedItem = potion;
                    newItemName = selectedItem.itemName; // Keep track of the selected item name
                    Repaint();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.EndScrollView();
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
            selectedItem.potionEffect = (PotionEffect)EditorGUILayout.EnumPopup("Potion Effect: ", selectedItem.potionEffect);
            selectedItem.duration = EditorGUILayout.FloatField("Duration: ", selectedItem.duration);
            selectedItem.cooldown = EditorGUILayout.FloatField("Cooldown: ", selectedItem.cooldown);
            selectedItem.isStackable= EditorGUILayout.Toggle("Stackable?", selectedItem.isStackable);

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
        if (GUILayout.Button("Create potion"))
        {
            PotionCreation wc = (PotionCreation)EditorWindow.GetWindow(typeof(PotionCreation), false, "potionCreation");
        }
    }

    private bool CheckForDuplicateName(string name, Potion currentpotion)
    {
        string[] guids = AssetDatabase.FindAssets("t:potion");
        foreach (string guid in guids)
        {
            Potion potion = AssetDatabase.LoadAssetAtPath<Potion>(AssetDatabase.GUIDToAssetPath(guid));
            if (potion != null && potion != currentpotion && potion.itemName == name)
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
}