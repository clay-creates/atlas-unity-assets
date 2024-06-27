using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.IO;

public class PotionDatabase : ItemDatabase<Potion>
{
    private string newItemName = "";
    private bool isDuplicateName = false;
    private bool hasInvalidCharacter = false;

    private Regex nameValidationRegex = new Regex(@"^[a-zA-Z0-9 \-']*$");

    private string searchQuery = "";
    private PotionEffect selectedPotionEffect = PotionEffect.None;

    private List<Potion> filteredPotions = new List<Potion>();

    protected override void DrawItemList()
    {
        DrawSearchBar();
        DrawFilters();
        DrawItemCount();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width - propertiesSectionWidth - 20), GUILayout.Height(position.height - 20));

        FilterPotions();

        if (filteredPotions.Count == 0)
        {
            EditorGUILayout.LabelField("No items match your search criteria.");
            if (GUILayout.Button("Reset Search"))
            {
                ResetFilters();
            }
        }
        else
        {
            foreach (Potion potion in filteredPotions)
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
        selectedPotionEffect = (PotionEffect)EditorGUILayout.EnumPopup(selectedPotionEffect);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawItemCount()
    {
        EditorGUILayout.LabelField($"Total Items: {filteredPotions.Count}");
    }

    private void FilterPotions()
    {
        string[] guids = AssetDatabase.FindAssets("t:Potion");
        filteredPotions.Clear();

        foreach (string guid in guids)
        {
            Potion potion = AssetDatabase.LoadAssetAtPath<Potion>(AssetDatabase.GUIDToAssetPath(guid));
            if (potion != null)
            {
                bool matchesSearchQuery = string.IsNullOrEmpty(searchQuery) || potion.itemName.ToLower().Contains(searchQuery.ToLower());
                bool matchesPotionEffect = selectedPotionEffect == PotionEffect.None || potion.potionEffect.HasFlag(selectedPotionEffect);

                if (matchesSearchQuery && matchesPotionEffect)
                {
                    filteredPotions.Add(potion);
                }
            }
        }
    }

    private void ResetFilters()
    {
        searchQuery = "";
        selectedPotionEffect = PotionEffect.None;
        FilterPotions();
    }

    protected override void DrawPropertiesSection()
    {
        if (selectedItem != null)
        {
            EditorGUILayout.LabelField("Potion Properties", EditorStyles.boldLabel);

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

            // Editable fields for the potion properties
            selectedItem.itemName = newItemName;
            selectedItem.description = EditorGUILayout.TextField("Description: ", selectedItem.description);
            selectedItem.baseValue = EditorGUILayout.FloatField("Base Value: ", selectedItem.baseValue);
            selectedItem.requiredLevel = EditorGUILayout.IntField("Required Level: ", selectedItem.requiredLevel);
            selectedItem.rarity = (Rarity)EditorGUILayout.EnumPopup("Rarity: ", selectedItem.rarity);
            selectedItem.equipSlot = (EquipSlot)EditorGUILayout.EnumPopup("Equip Slot: ", selectedItem.equipSlot);
            selectedItem.potionEffect = (PotionEffect)EditorGUILayout.EnumPopup("Potion Effect: ", selectedItem.potionEffect);
            selectedItem.effectPower = EditorGUILayout.FloatField("Effect Power: ", selectedItem.effectPower);
            selectedItem.duration = EditorGUILayout.FloatField("Duration: ", selectedItem.duration);
            selectedItem.cooldown = EditorGUILayout.FloatField("Cooldown: ", selectedItem.cooldown);
            selectedItem.isStackable = EditorGUILayout.Toggle("isStackable?", selectedItem.isStackable);

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
            EditorGUILayout.LabelField("Select a potion to see its properties.");
        }
    }

    protected override void ExportItemsToCSV()
    {
        string path = EditorUtility.SaveFilePanel("Export Potion Data to CSV", "", "PotionData.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                // Write header
                writer.WriteLine("Item Name\tIcon Path\tPotion Type\tAttack Power\tAttack Speed\tDurability\tRange\tCritical Hit Chance\tBase Value\tRarity\tRequired Level\tEquip Slot\tDescription");

                // Write potion data
                string[] guids = AssetDatabase.FindAssets("t:Potion");
                foreach (string guid in guids)
                {
                    Potion potion = AssetDatabase.LoadAssetAtPath<Potion>(AssetDatabase.GUIDToAssetPath(guid));
                    if (potion != null)
                    {
                        string line = $"{potion.itemName}\t" +
                                      $"{AssetDatabase.GetAssetPath(potion.icon)}\t" +
                                      $"{potion.potionEffect}\t" +
                                      $"{potion.effectPower}\t" +
                                      $"{potion.duration}\t" +
                                      $"{potion.cooldown}\t" +
                                      $"{potion.isStackable}\t" +
                                      $"{potion.baseValue}\t" +
                                      $"{potion.rarity}\t" +
                                      $"{potion.requiredLevel}\t" +
                                      $"{potion.equipSlot}\t" +
                                      $"{potion.description}";

                        writer.WriteLine(line);
                    }
                }
            }
            Debug.Log("Potion data successfully exported to CSV.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to export potion data to CSV: {e.Message}");
        }
    }

    protected override void ImportItemsFromCSV()
    {
        string path = EditorUtility.OpenFilePanel("Import Potion Data from CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines.Skip(1)) // Skip header line
            {
                string[] values = line.Split('\t'); // Using tab as delimiter

                Potion newPotion = ScriptableObject.CreateInstance<Potion>();
                newPotion.itemName = values[0];
                newPotion.icon = AssetDatabase.LoadAssetAtPath<Sprite>(values[1]);
                newPotion.potionEffect = (PotionEffect)Enum.Parse(typeof(PotionEffect), values[2]);
                newPotion.effectPower = float.Parse(values[3]);
                newPotion.duration = float.Parse(values[4]);
                newPotion.cooldown = float.Parse(values[5]);
                newPotion.isStackable = bool.Parse(values[6]);
                newPotion.baseValue = float.Parse(values[7]);
                newPotion.rarity = (Rarity)Enum.Parse(typeof(Rarity), values[8]);
                newPotion.requiredLevel = int.Parse(values[9]);
                newPotion.equipSlot = (EquipSlot)Enum.Parse(typeof(EquipSlot), values[10]);
                newPotion.description = values[11];

                string assetPath = $"Assets/Resources/Potions/{newPotion.itemName}.asset";
                AssetDatabase.CreateAsset(newPotion, assetPath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Potion data successfully imported from CSV.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to import potion data from CSV: {e.Message}");
        }
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
        string[] guids = AssetDatabase.FindAssets("t:Potion");
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

    private void OnInspectorUpdate()
    {
        Repaint();
    }
}
