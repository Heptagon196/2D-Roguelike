using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
// This is all shit!!!!

public class ShopMenu: MonoBehaviour {
    public static ShopMenu instance = null;
    public GameObject title;
    public GameObject content;
    public GameObject button;
    public GameObject shopMenu;
    public GameObject confirmShopMenu;
    public GameObject confirmText;
    [HideInInspector] public Dictionary<string, GameObject> Buttons = new Dictionary<string, GameObject>();
    public Dictionary<string, string> confirmMessage = new Dictionary<string, string>();
    private string confirmingName;
    private Dictionary<string, UnityAction> actions = new Dictionary<string, UnityAction>();

    public bool menuIsOpen() {
        return shopMenu.activeInHierarchy;
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this.gameObject);
            return;
        }
    }

    public void SetTitle(string titletext) {
        title.GetComponent<Text>().text = titletext;
    }

    public void ShowShop() {
        GameController.instance.setPause();
        shopMenu.SetActive(true);
    }

    public string split(string s) {
        for (int i = 0; i < s.Length; i++) {
            if (s[i] == 'X') {
                return s.Substring(0, i - 1);
            }
        }
        return s;
    }

    public void CleanButtons() {
        var buttons = new List<Transform>();
        foreach (Transform child in content.transform) {
            buttons.Add(child);
        }
        for (int i = 0; i < buttons.Count; i++) {
            Destroy(buttons[i].gameObject);
        }
        Buttons.Clear();
    }

    private void Update() {
        if (!ChooseWeapon.instance.menuIsOpen() && !GameController.instance.menuIsOpen() && !OptionMenuControl.instance.menuIsOpen() && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown((int) MouseButton.RightMouse))) {
            if (confirmShopMenu.activeInHierarchy) {
                confirmShopMenu.SetActive(false);
                return;
            }
            if (shopMenu.activeInHierarchy) {
                GameController.instance.setContinue();
                shopMenu.SetActive(false);
            }
        }
    }

    public void AddButton(string Text, string ConfirmMessage, UnityAction Action) {
        confirmMessage[split(Text)] = ConfirmMessage;
        var tmp = Instantiate(button, transform.position, transform.rotation);
        tmp.GetComponent<WeaponButton>().text.text = Text;
        tmp.GetComponent<Button>().onClick.AddListener(delegate() {
            ConfirmWeapon(tmp.GetComponent<WeaponButton>().text);
        });
        tmp.transform.SetParent(content.transform);
        Buttons.Add(split(Text), tmp);
        actions[split(Text)] = Action;
    }

    public void ConfirmWeapon(Text TextName) {
        string Name = split(TextName.GetComponent<Text>().text);
        confirmText.GetComponent<Text>().text = confirmMessage[Name];
        confirmingName = Name;
        confirmShopMenu.SetActive(true);
    }

    public void ConfirmWeaponNo() {
        confirmShopMenu.SetActive(false);
    }

    public void ConfirmWeaponYes() {
        actions[confirmingName].Invoke();
        confirmShopMenu.SetActive(false);
    }
}
