using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public bool inPlayerInventory = false;

    private Player player;
    private WeaponComponents[] weaponComponents;
    private bool weaponUsed = false; // used to trigger the swing animation

    // adds a weapon to the inventory
    public void AcquireWeapon() {
        player = GetComponentInParent<Player>();
        weaponComponents = GetComponentsInChildren<WeaponComponents>();
    }

    // starts the weapon swing animation
    public void UseWeapon() 
    {
        EnableSpriteRender(true);
        weaponUsed = true;
    }


    public void EnableSpriteRender(bool isEnabled) 
    {
        foreach(WeaponComponents component in weaponComponents)
        {
            component.GetSpriteRenderer().enabled = isEnabled;
        }
    }

    // informs player about the weapon that is carried
    public Sprite GetComponentImage(int index)
    {
        return weaponComponents[index].GetSpriteRenderer().sprite;
    }

    // runs aniamtion
    private void Update()
    {
        if (!inPlayerInventory)
            return;

        transform.position = player.transform.position;
        // if it is, runs animation cycle
        if(weaponUsed == true)
        {
            float degreeY = 0, degreeZ = -90f, degreeZMax = 275f;
            Vector3 returnVector = Vector3.zero;

            if(Player.isFacingRight)
            {
                degreeY = 0;
                returnVector = Vector3.zero;
            }
            if (!Player.isFacingRight)
            {
                degreeY = 180;
                returnVector = new Vector3(0, 180, 0);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, degreeY, degreeZ), Time.deltaTime * 20f);
            if(transform.eulerAngles.z <= degreeZMax)
            {
                transform.eulerAngles = returnVector;
                weaponUsed = false;
                EnableSpriteRender(false);
            }
        }
    }

}
