using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PotionCreation : BaseItemCreation<Potion>
{
    [SerializeField]
    private PotionEffect potionEffect;
    [SerializeField]
    private float effectPower;
    [SerializeField]
    private float duration; // Duration of effect
    [SerializeField]
    private float cooldown; // Cooldown for using again
    [SerializeField]
    private bool isStackable; // Whether the effect is stackable



    private void OnGUI()
    {
        DrawCommonFields();

        DrawCommonPropertySection();

        if (GUILayout.Button("Create Potion"))
        {
            Potion newPotion = CreateInstance<Potion>();

            // Assign weapon-specific values
            newPotion.potionEffect = potionEffect;
            newPotion.effectPower = effectPower;
            newPotion.duration = duration;
            newPotion.cooldown = cooldown;
            newPotion.isStackable = isStackable;

            CreateItem(newPotion);
        }
    }

    void DrawCommonPropertySection()
    {
        EditorGUILayout.LabelField("Weapon Properties", EditorStyles.boldLabel);

        potionEffect = (PotionEffect)EditorGUILayout.EnumPopup("Potion Effect", potionEffect);
        effectPower = EditorGUILayout.FloatField("Effect Power", effectPower);
        duration = EditorGUILayout.FloatField("Duration", duration);
        cooldown = EditorGUILayout.FloatField("Cooldown", cooldown);
        isStackable = EditorGUILayout.Toggle("isStackable?", isStackable);
    }
}
