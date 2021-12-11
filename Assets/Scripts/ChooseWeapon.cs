using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
// This is all shit!!!!

public class ChooseWeapon : MonoBehaviour {
    public static ChooseWeapon instance = null;
    public GameObject player;
    public GameObject content;
    public GameObject button;
    public GameObject weaponMenu;
    public GameObject confirmWeaponMenu;
    public GameObject confirmText;
    public Weapon defaultWeapon;
    [HideInInspector] public List<string> weaponNames;
    [HideInInspector] public Dictionary<string, Weapon> weaponList = new Dictionary<string, Weapon>();
    [HideInInspector] public Dictionary<string, int> weaponCount = new Dictionary<string, int>();
    [HideInInspector] public Dictionary<string, Text> weaponListContent = new Dictionary<string, Text>();
    private string confirmingName;

    private string split(string s) {
        for (int i = 0; i < s.Length; i++) {
            if (s[i] == 'X') {
                return s.Substring(0, i - 1);
            }
        }
        return s;
    }

    public bool menuIsOpen() {
        return weaponMenu.activeInHierarchy;
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this.gameObject);
            return;
        }
        AddWeapon(defaultWeapon, 1);
    }

    private void Update() {
        if (!ShopMenu.instance.menuIsOpen() && !GameController.instance.menuIsOpen() && Input.GetKeyDown(KeyCode.E) && !OptionMenuControl.instance.menuIsOpen() && !MessageBox.instance.hasBox && !GameController.instance.isShowingMessage) {
            if (weaponMenu.activeInHierarchy && !confirmWeaponMenu.activeInHierarchy) {
                GameController.instance.setContinue();
                weaponMenu.SetActive(false);
            } else {
                GameController.instance.setPause();
                weaponMenu.SetActive(true);
            }
            return;
        }
        if (!ShopMenu.instance.menuIsOpen() && !GameController.instance.menuIsOpen() && !OptionMenuControl.instance.menuIsOpen() && !MessageBox.instance.hasBox && !GameController.instance.isShowingMessage && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown((int) MouseButton.RightMouse))) {
            if (confirmWeaponMenu.activeInHierarchy) {
                confirmWeaponMenu.SetActive(false);
                return;
            }
            if (weaponMenu.activeInHierarchy) {
                GameController.instance.setContinue();
                weaponMenu.SetActive(false);
            }
        }
    }

    public void AddWeapon(Weapon w, int count) {
        if (weaponList.ContainsKey(w.weaponName)) {
            weaponCount[w.weaponName] += count;
            weaponListContent[w.weaponName].text = w.weaponName + " X " + weaponCount[w.weaponName];
            return;
        }
        weaponNames.Add(w.weaponName);
        weaponList.Add(w.weaponName, w);
        weaponCount.Add(w.weaponName, count);
        var tmp = Instantiate(button, transform.position, transform.rotation);
        tmp.GetComponent<WeaponButton>().text.text = w.weaponName + " X 1";
        tmp.GetComponent<Button>().onClick.AddListener(delegate() {
            ConfirmWeapon(tmp.GetComponent<WeaponButton>().text);
        });
        tmp.transform.SetParent(content.transform);
        weaponListContent.Add(w.weaponName, tmp.GetComponent<WeaponButton>().text);
    }

    public void ConfirmWeapon(Text TextName) {
        string Name = split(TextName.GetComponent<Text>().text);
        confirmText.GetComponent<Text>().text = Name + "\n\nDamage to enemies: \t" + weaponList[Name].damageToEnemies +
                                                 "\nDamage to walls: \t" + weaponList[Name].damageToWalls +
                                                 "\nAttack frequency: \t" + weaponList[Name].attackTime +
                                                 "s\nAttack range: \t\t" + weaponList[Name].sqrSwayRange +
                                                 "\n\nSwitch into this weapon?";
        confirmingName = Name;
        confirmWeaponMenu.SetActive(true);
    }

    public void ConfirmWeaponNo() {
        confirmWeaponMenu.SetActive(false);
    }

    public void ConfirmWeaponYes() {
        player.GetComponent<Player>().weapon = weaponList[confirmingName];
        confirmWeaponMenu.SetActive(false);
    }
}

