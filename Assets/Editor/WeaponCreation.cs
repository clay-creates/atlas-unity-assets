using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WeaponCreation : BaseItemCreation<Weapon>
{
    [SerializeField]
    private WeaponType weaponType;
    [SerializeField]
    private float attackPower;
    [SerializeField]
    private float attackSpeed;
    [SerializeField]
    private float durability; // Durability of the weapon
    [SerializeField]
    private float range; // Range of the weapon
    [SerializeField]
    private float criticalHitChance; // Chance of a critical hit



    private void OnGUI()
    {
        DrawCommonFields();

        DrawCommonPropertySection();

        if (GUILayout.Button("Create Weapon"))
        {
            Weapon newWeapon = CreateInstance<Weapon>();

            // Assign weapon-specific values
            newWeapon.weaponType = weaponType;
            newWeapon.attackPower = attackPower;
            newWeapon.attackSpeed = attackSpeed;
            newWeapon.durability = durability;
            newWeapon.range = range;
            newWeapon.criticalHitChance = criticalHitChance;

            CreateItem(newWeapon);
        }
    }

    void DrawCommonPropertySection()
    {
        EditorGUILayout.LabelField("Weapon Properties", EditorStyles.boldLabel);

        weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);
        attackPower = EditorGUILayout.FloatField("Attack Power", attackPower);
        attackSpeed = EditorGUILayout.FloatField("Attack Speed", attackSpeed);
        durability = EditorGUILayout.FloatField("Durability", durability);
        range = EditorGUILayout.FloatField("Range", range);
        criticalHitChance = EditorGUILayout.FloatField("Critical Hit Chance", criticalHitChance);
    }
}
