using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ArmorCreation : BaseItemCreation<Armor>
{
    [SerializeField]
    private ArmorType armorType;
    [SerializeField]
    private float defensePower;
    [SerializeField]
    private float resistance; // Magical resistance
    [SerializeField]
    private float weight; // Weight of the armor
    [SerializeField]
    private float movementSpeedModifier; // Affects movement speed



    private void OnGUI()
    {
        DrawCommonFields();

        DrawCommonPropertySection();

        if (GUILayout.Button("Create Armor"))
        {
            Armor newArmor = CreateInstance<Armor>();

            // Assign weapon-specific values
            newArmor.armorType = armorType;
            newArmor.defensePower = defensePower;
            newArmor.resistance = resistance;
            newArmor.weight = weight;
            newArmor.movementSpeedModifier = movementSpeedModifier;

            CreateItem(newArmor);
        }
    }

    void DrawCommonPropertySection()
    {
        EditorGUILayout.LabelField("Weapon Properties", EditorStyles.boldLabel);

        armorType = (ArmorType)EditorGUILayout.EnumPopup("Armor Type", armorType);
        defensePower = EditorGUILayout.FloatField("Defense Power", defensePower);
        resistance = EditorGUILayout.FloatField("Resistance", resistance);
        weight = EditorGUILayout.FloatField("Weight", weight);
        movementSpeedModifier = EditorGUILayout.FloatField("Movement Speed Modifier", movementSpeedModifier);
    }
}
