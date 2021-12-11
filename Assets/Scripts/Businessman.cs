using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Businessman: MovingObj {
    public int money = 300;
    public float walkingSpeed = 1f;
    public float deltaTime = 2f;
    public float movingTime = 1f;
    public float attackTime = 1f;
    public int damgeToPlayer = 30;
    public int healthPoint = 100;
    public Animator thisAnimator;
    public LayerMask layer;
    public AudioClip[] attackSound;
    public bool isAgressive;
    public GameObject[] weaponsToSell;
    public bool forceStartChatting = false;
    private List<int> toSell = new List<int>();
    private OptionMenuControl selectMenu;
    private float lastAttackTimePoint;
    private float lastChange;
    private float angle;
    private RaycastHit2D[] hit;
    private GameObject player;
    private static readonly int BusinessmanChop = Animator.StringToHash("businessmanChop");
    private static readonly int PlayerHit = Animator.StringToHash("playerHit");
    private static int moneyForBoost = 20;
    private static int moneyForEnergySaving = 20;
    private static int moneyForDefense = 20;

    private new void Awake() {
        isAgressive = false;
        lastChange = Time.time;
        lastAttackTimePoint = Time.time;
        angle = Random.Range(0, (float) Math.PI * 2f);
        player = GameObject.FindWithTag("Player");
        selectMenu = GameObject.Find("OptionMenu").GetComponent<OptionMenuControl>();
        if (GameController.instance.level == 1) {
            moneyForBoost = 20;
            moneyForEnergySaving = 20;
            moneyForDefense = 20;
        }
        int sellNumber = Random.Range(1, 4);
        Dictionary<int, bool> hasGenerated = new Dictionary<int, bool>();
        for (int i = 0; i < sellNumber; i++) {
            int p;
            do {
                p = Random.Range(0, weaponsToSell.Length);
            } while (hasGenerated.ContainsKey(p));
            hasGenerated.Add(p, true);
            toSell.Add(p);
        }
        base.Awake();
    }
    protected override Vector2 Move() {
        if (isAgressive) {
            player.GetComponent<BoxCollider2D>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
            hit = Physics2D.LinecastAll(player.transform.position, transform.position, layer);
            player.GetComponent<BoxCollider2D>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = true;
            bool hasWall = false;
            for (int i = 0; i < hit.Length; i++) {
                if (hit[i].transform.GetComponent<Wall>() != null) {
                    hasWall = true;
                    break;
                }
            }
            if (!hasWall) {
                lastChange = Time.time - movingTime;
                return Time.deltaTime * walkingSpeed * 4 * ((player.transform.position - transform.position) / (player.transform.position - transform.position).magnitude);
            }
            if (Time.time - lastChange >= movingTime + deltaTime) {
                angle = Random.Range(0, (float) Math.PI * 2f);
                lastChange = Time.time;
                return new Vector2(0f, 0f);
            }
            if (Time.time - lastChange <= movingTime) {
                return new Vector2((float) Math.Cos(angle) * walkingSpeed * Time.deltaTime, (float) Math.Sin(angle) * walkingSpeed * Time.deltaTime);
            } else {
                return new Vector2(0f, 0f);
            }
        } else {
            if (Time.time - lastChange >= movingTime + deltaTime) {
                angle = Random.Range(0, (float) Math.PI * 2f);
                lastChange = Time.time;
                return new Vector2(0f, 0f);
            }

            if (Time.time - lastChange <= movingTime) {
                return new Vector2((float) Math.Cos(angle) * walkingSpeed * Time.deltaTime,
                    (float) Math.Sin(angle) * walkingSpeed * Time.deltaTime);
            }
            else {
                return new Vector2(0f, 0f);
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Player")) {
            if (isAgressive) {
                if (Time.time - lastAttackTimePoint >= attackTime) {
                    SoundManager.instance.RandomPlay(attackSound);
                    other.gameObject.GetComponent<Player>().healthPoint -= damgeToPlayer - other.gameObject.GetComponent<Player>().defense;
                    lastAttackTimePoint = Time.time;
                    thisAnimator.SetTrigger(BusinessmanChop);
                    other.gameObject.GetComponent<Animator>().SetTrigger(PlayerHit);
                }
            }
        }
    }
    
    public void PlayerSell() {
        ShopMenu.instance.CleanButtons();
        ShopMenu.instance.SetTitle("What do you want to sell?");
        //GameController.instance.setPause();
        foreach (var s in ChooseWeapon.instance.weaponNames) {
            ShopMenu.instance.AddButton(s + " X " + ChooseWeapon.instance.weaponCount[s], "Do you want to sell all the extra " + s + "s?", delegate {
                int count = ChooseWeapon.instance.weaponCount[s];
                if (count <= 1) {
                    MessageBox.instance.ShowMessage("There's no extra " + s + "s for you to sell.", 1f);
                } else {
                    int getCoins = (count - 1) * ChooseWeapon.instance.weaponList[s].value;
                    player.GetComponent<Player>().coins += getCoins;
                    MessageBox.instance.ShowMessage("You get " + getCoins + " coins for your " + (count - 1) + " " + s+ "s.", 1f);
                    ChooseWeapon.instance.AddWeapon(ChooseWeapon.instance.weaponList[s], -(count - 1));
                    ShopMenu.instance.Buttons[s].GetComponent<WeaponButton>().text.text = s + " X 1";
                }
            });
        }
        ShopMenu.instance.ShowShop();
    }

    public void PlayerBuy() {
        ShopMenu.instance.CleanButtons();
        ShopMenu.instance.SetTitle("What do you want to buy?");
        //GameController.instance.setPause();
        foreach (var p in toSell) {
            string s = weaponsToSell[p].GetComponent<Weapon>().weaponName;
            int val = weaponsToSell[p].GetComponent<Weapon>().value * 2;
            ShopMenu.instance.AddButton(s, "Do you want to buy a " + s + " with " + val + " coins?", delegate {
                if (player.GetComponent<Player>().coins < val) {
                    MessageBox.instance.ShowMessage("You don't have enough money to buy a " + s + ".", 1f);
                } else {
                    player.GetComponent<Player>().coins -= val;
                    MessageBox.instance.ShowMessage("You spend " + val + " coins for a " + s + ".", 1f);
                    ChooseWeapon.instance.AddWeapon(weaponsToSell[p].GetComponent<Weapon>(), 1);
                }
            });
        }
        ShopMenu.instance.AddButton("* Boost", "Do you want to spend " + moneyForBoost + " coins to boost your character?", delegate {
            if (player.GetComponent<Player>().coins < moneyForBoost) {
                MessageBox.instance.ShowMessage("You don't have enough money to boost your character.", 1f);
            } else if (player.GetComponent<Player>().speed > 10f) {
                MessageBox.instance.ShowMessage("Your speed has reached the limit.", 1f);
            } else {
                player.GetComponent<Player>().coins -= moneyForBoost;
                moneyForBoost += 20;
                player.GetComponent<Player>().speed += 0.4f;
                if (player.GetComponent<Player>().speed > 10f) {
                    MessageBox.instance.ShowMessage( "You spend " + moneyForBoost + " coins to boost your character.\nAnd now your speed has reached the limit.", 1f);
                }
            }
            ShopMenu.instance.confirmMessage["* Boost"] = "Do you want to spend " + moneyForBoost + " coins to boost your character?";
        });
        ShopMenu.instance.AddButton("* Energy saving", "Do you want to spend " + moneyForEnergySaving + " coins to let your character have a better resistance of hunger?", delegate {
            if (player.GetComponent<Player>().coins < moneyForEnergySaving) {
                MessageBox.instance.ShowMessage("You don't have enough money to strengthen your character.", 1f);
            } else {
                player.GetComponent<Player>().coins -= moneyForEnergySaving;
                MessageBox.instance.ShowMessage("You spend " + moneyForEnergySaving + " coins to strengthen your character.", 1f);
                moneyForEnergySaving += 20;
                player.GetComponent<Player>().starvingSpeed *= 0.8f;
            }
            ShopMenu.instance.confirmMessage["* Energy saving"] = "Do you want to spend " + moneyForEnergySaving + " coins to let your character have a better resistance of hunger?";
        });
        ShopMenu.instance.AddButton("* Defense", "Do you want to spend " + moneyForDefense + " coins to let your character have a better defense of attacks?", delegate {
            if (player.GetComponent<Player>().coins < moneyForDefense) {
                MessageBox.instance.ShowMessage("You don't have enough money to strengthen your character.", 1f);
            } else {
                player.GetComponent<Player>().coins -= moneyForDefense;
                MessageBox.instance.ShowMessage("You spend " + moneyForDefense + " coins to strengthen your character.", 1f);
                moneyForDefense += 20;
                player.GetComponent<Player>().defense += 2;
            }
            ShopMenu.instance.confirmMessage["* Defense"] = "Do you want to spend " + moneyForDefense + " coins to let your character have a better defense of attacks?";
        });
        ShopMenu.instance.ShowShop();
    }

    private int tutorialProgress = 0;

    public void ReChat() {
        forceStartChatting = true;
    }
    
    public void StartChat() {
        if (GameController.instance.level == 0 && tutorialProgress != 3) {
            if (tutorialProgress == 0) {
                MessageBox.instance.ShowPassage(false,
                    new Sentence("Businessman", "Welcome, player!\nI'm your guide for this game!"),
                    new Sentence("If you are not interested in this, just press Ctrl to skip.")
                );
                tutorialProgress = 1;
            }
            if (tutorialProgress == 1) {
                MessageBox.instance.ShowPassage(true,
                    new Sentence("Businessman", "First, you can see your status above."),
                    new Sentence( "The 'Food' part shows how much food you have currently.\nYour food will keep losing, unless you are chatting with someone.\nCollecting food on the ground can add to your food points, and you'll lose food points when attacked by an enemy."),
                    new Sentence( "After being attacked by an enemy, your starving speed will increase a little."),
                    new Sentence( "The 'Bullets' part shows how many bullets you have now.\nYou can use bullets to shoot at your enemies or destroying walls.\nBullets can be collected by picking up guns on the ground."),
                    new Sentence( "The 'Coins' part shows how much money you have.\nYou can get money by defeating enemies or selling the extra weapons you get.\nMoney can be used to buy weapons or strengthen your character when you are chatting with a businessman."),
                    new Sentence( "The 'Weapon' part shows the name of the weapon you are using.\nDifferent weapons have different damages and speed.\nWeapons are placed randomly on the ground."),
                    new Sentence( "When you left click on an enemy or a wall block, you will use your weapon to attack it when it's in the attack range of your weapon, and you will shoot at it when it's out of range and you have a bullet"),
                    new Sentence( "When you right click and you have a bullet, you will shoot at the direction you click, even if there's an enemy you can attack using your weapon."),
                    new Sentence("To switch a weapon, just press 'E'."),
                    new Sentence( "Now, let's have a try. You won't die during this period.\nWhen you feel it's enough, go back and start chatting with me again.")
                );
                tutorialProgress = 2;
                MessageBox.instance.StartActionAfterMessage(delegate {
                    MessageBox.instance.CloseMessageBox();
                    MapGenerator.instance.GenerateObject(MapGenerator.instance.foods, new MapGenerator.Range(4, 4));
                    MapGenerator.instance.GenerateObject(MapGenerator.instance.guns, new MapGenerator.Range(4, 4));
                    MapGenerator.instance.GenerateObject(MapGenerator.instance.enemies, new MapGenerator.Range(2, 2));
                    MapGenerator.instance.GenerateObject(MapGenerator.instance.weapons, new MapGenerator.Range(6, 6));
                    MapGenerator.instance.GenerateObject(MapGenerator.instance.walls, new MapGenerator.Range(10, 10));
                    player.transform.position = new Vector3(0, 0, 0);
                    this.gameObject.transform.position = new Vector3(0, 1, 0);
                    tutorialProgress = 2;
                });
            } else if (tutorialProgress == 2) {
                MessageBox.instance.ShowPassage(false, new Sentence("Businessman", "What else do you want to know?"));
                selectMenu.ShowMenu(4, new ButtonContent("Please explain it again", () => {
                                                                MapGenerator.instance.TutorialClean();
                                                                MessageBox.instance.CloseMessageBox();
                                                                tutorialProgress = 1;
                                                                ReChat();
                                                            }),
                                                            new ButtonContent("Why is my guide a businessman?", () => {
                                                                MessageBox.instance.ShowPassage(true,
                                                                    new Sentence("Businessman", "The reason is actually quite simple..."),
                                                                    new Sentence("Because there's no more character's model to use."));
                                                                tutorialProgress = 2;
                                                                MessageBox.instance.StartActionAfterMessage(delegate () {
                                                                    ReChat();
                                                                });
                                                            }),
                                                            new ButtonContent("Move on to the next step", () => {
                                                                MapGenerator.instance.TutorialClean();
                                                                MapGenerator.instance.GenerateExitToBonus();
                                                                MessageBox.instance.ShowPassage(true, new Sentence("Businessman", "Now, let's talk about exits. There's one exit for each level, stepping upon it will lead you to the next level."),
                                                                                                                                    new Sentence("The exit is usually at the top-right corner. The exit for this level has already being generated now."),
                                                                                                                                    new Sentence("Then let's talk about businessmen."),
                                                                                                                                    new Sentence("Usually, by chatting with businessman like me, you can buy and sell many kinds of items, and you can even strengthen your character."),
                                                                                                                                    new Sentence("And, there is only one more thing I didn't tell you."),
                                                                                                                                    new Sentence("There will be some 'bonus stages', which is an easy level for you to earn food points."),
                                                                                                                                    new Sentence("You can first try to chat with me to learn what it feels like to interact with a businessman, and then go the exit to have a try of two different kinds of bonus stages."),
                                                                                                                                    new Sentence("After two bonus stages, you'll enter level 1.")
                                                                                                                                    );
                                                                tutorialProgress = 3;
                                                            }),
                                                            new ButtonContent("Never mind", () => {
                                                                MessageBox.instance.CloseMessageBox();
                                                                tutorialProgress = 2;
                                                            }));
            }
            return;
        }
        MessageBox.instance.ShowPassage(false, new Sentence("Businessman", "What do you want?"));
        selectMenu.ShowMenu(4, new ButtonContent("Buy", () => {MessageBox.instance.CloseMessageBox(); GameController.instance.setPause(); PlayerBuy();}),
                                                    new ButtonContent("Sell", () => {MessageBox.instance.CloseMessageBox(); GameController.instance.setPause(); PlayerSell();}),
                                                    new ButtonContent("Give me all you have!", () => {MessageBox.instance.ShowMessage("Businessman","Fuck off!", 1f); isAgressive = true; }),
                                                    new ButtonContent("Never mind", () => {MessageBox.instance.ShowMessage("Businessman", "Good luck!", 1f);}));
    }
    
    private void LateUpdate() {
        if (forceStartChatting) {
            forceStartChatting = false;
            StartChat();
        }
        if (!GameController.instance.isPaused() && player.GetComponent<Player>().hasKilledBusinessman) {
            MessageBox.instance.ShowMessage("Businessman", "I'll revenge for my friend!", 2f);
            player.GetComponent<Player>().hasKilledBusinessman = false;
            isAgressive = true;
        }
        if (healthPoint <= 0) {
            player.GetComponent<Player>().coins += money;
            MessageBox.instance.ShowMessage("Businessman", "You'll regret for what you've done!", 2f);
            player.GetComponent<Player>().hasKilledBusinessman = true;
            Destroy(this.gameObject);
        }
    }
}
