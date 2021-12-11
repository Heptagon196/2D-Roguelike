using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diary: MonoBehaviour {
    public AudioClip[] pickSound;
    private static List<int> diariesleft = new List<int>();
    private static int progress;
    private bool destoryed = false;
    
    private readonly Sentence[][][] diaries = new [] {
        new [] {
            new [] {
                new Sentence("", "You pick up the diary on the ground, and start to read."),
                new Sentence("Diary", "October 29th, 2019"),
                new Sentence("Another day of being bullied\nWhy are they all so powerful, so knowledgeable?"),
                new Sentence("I once thought we were all beginners!\nBut it seems that the only beginner is me."),
                new Sentence("When and how, can I be as strong as them?"),
            },
            new [] {
                new Sentence("", "It's ... a book. It's titled 'C Programming: A Modern Approach'"), 
                new Sentence("Why it's here?"), 
                new Sentence("You are not touching it."), 
            },
            new [] {
                new Sentence("", "You pick up the diary on the ground, and start to read."),
                new Sentence("Diary", "November 13th, 2019"),
                new Sentence("'Pigeon death'? What a funny name. Maybe they are just too lazy."),
                new Sentence("Well, it will undoubtedly be blocked out of this country."),
                new Sentence("Thanks to the Great Biological Wall."),
            },
            new [] {
                new Sentence("", "You pick up the diary on the ground, and start to read."),
                new Sentence("Diary", "December 9th, 2019"),
                new Sentence("America has lost. Although a few escaped.\nIt's up to us now."),
                new Sentence("But I believe in our Party and our scientists."),
            },
        },
        new [] {
            new [] {
                new Sentence("", "You pick up the diary on the ground, and start to read."),
                new Sentence("", "There's an extract from a newspaper in it."),
                new Sentence("Newspaper", "WE ARE ALL SINNERS"),
                new Sentence("We were all wrong."),
                new Sentence("The source of the disease is never pigeon."),
                new Sentence("It's US. It's us who give life to it."),
                new Sentence("", "The extract is ended. You try to find the rest part of the newspaper, but it's a vain attempt."), 
            },
            new [] {
                new Sentence("", "You find a piece of paper. There's only one word on it."),
                new Sentence("'JUDGEMENT'"),
            },
            new [] {
                new Sentence("", "You find a piece of paper. There's only one word on it."),
                new Sentence("'BLESSEDNESS'"),
            },
            new [] {
                new Sentence("", "You find a piece of paper. There's only three words on it."),
                new Sentence("'THE PROMISED LAND'"),
            },
            new [] {
                new Sentence("", "It's a brochure titled 'JOIN THE RESISTANCE'"),
                new Sentence("You can hardly recognize other words. What you can see is 'AN EYE FOR AN EYE'"),
            },
            new [] {
                new Sentence("", "You pick up the book on the ground.\nIt's titled 'The Hitch-hiker's Guide to the Pigeon Dungeon'"),
                new Sentence("You have a quick look into it. It's all nonsense."),
                new Sentence("But a strange idea jumped into your mind when you touched its cover."),
                new Sentence("You realized that you actually cannot recall ANYTHING."),
                new Sentence("Why are you wandering in this dungeon? Who are you? What is this dungeon after all?"),
                new Sentence("It bothers you deeply.\nBut all you can do now, which is branded in your mind, is to SURVIVE."),
            },
            new [] {
                new Sentence("", "You pick up the diary on the ground, and start to read."),
                new Sentence("Diary", "February 13th, 2020"),
                new Sentence("Well, let this be the last diary entry of mine."),
                new Sentence("I'm still writing, just because I feel I have the obligation to explain to you what exactly is 'Pigeon Death', as a virologist who have studied on that 'disease'."),
                new Sentence("It's ... perfect. You don't have to do anything."),
                new Sentence("No pain."),
                new Sentence("No sorrow."),
                new Sentence("NO DEADLINE."),
                new Sentence("I'm writing for those wandering miserable souls who might accidentally come across with this diary."),
                new Sentence("IT'S HEAVEN"),
                new Sentence("", "The diary ends at here."),
                new Sentence("You are puzzled."),
                new Sentence("Why are you still wandering? What's the meaning of that?"),
                new Sentence("What's the difference of being a pigeon or a human? Are you killing someone that is still alive?"),
                new Sentence("But, although you don't know why, an idea is still branded deep in you mind: SURVIVE"),
            },
        },
        new [] {
            new [] {
                new Sentence("", "You find a diary on the ground. You open it and start to read."), 
                new Sentence("Diary", "January 8th, 2020"), 
                new Sentence("They are all MAD!"), 
                new Sentence("I do understand how horrible it is to see a human act like a monster. But this is too much!"), 
                new Sentence("Wars, riots, vandalism, what else couldn't those conspiracy theorists and religious fanatics do?"), 
                new Sentence("And there are tons of FAKE NEWS."), 
                new Sentence("THIS IS THE REAL CATACLYSM"), 
            },
            new [] {
                new Sentence("", "There's a picture on the cover of that book ..."), 
                new Sentence("It's YOU."), 
                new Sentence("You pick up the book, but it's written in another language. You can't understand."), 
            },
        },
        new [] {
            new [] {
                new Sentence("", "The same book again ... with you on the cover."), 
                new Sentence("And you suddenly realized, the BUSINESSMAN is also on it."), 
                new Sentence("Why haven't you realized it before?\nThere's something strange about him ..."), 
                new Sentence("Why isn't he attacked?"), 
                new Sentence("What's wrong with your mind?"), 
                new Sentence("You couldn't recall anything.\nThe only thought in your mind, is to SURVIVE."), 
            }
        },
        new [] {
            new [] {
                new Sentence("", "The same book again ..."), 
                new Sentence("But now, you are filled with determination ..."), 
            },
            new [] {
                new Sentence("", "The same book again ..."), 
                new Sentence("But now, you are full of determination ..."), 
            },
        },
    };

    private void Awake() {
        if (GameController.instance.level == 1) {
            progress = 0;
            diariesleft.Clear();
            for (int i = 0; i < diaries[0].Length; i++) {
                diariesleft.Add(i);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player") && !destoryed) {
            SoundManager.instance.RandomPlay(pickSound);
            /*
            Debug.Log(diariesleft.Count);
            Debug.Log(progress);
            */
            if (diariesleft.Count == 0) {
                if (progress != diaries.Length - 1) {
                    progress = (progress + 1) % diaries.Length;
                }
                for (int i = 0; i < diaries[progress].Length; i++) {
                    diariesleft.Add(i);
                }
            }
            int p = UnityEngine.Random.Range(0, diariesleft.Count);
            MessageBox.instance.ShowPassage(true, diaries[progress][diariesleft[p]]);
            diariesleft.RemoveAt(p);
            destoryed = true;
        }
    }

    private void LateUpdate() {
        if (destoryed) {
            Destroy(this.gameObject);
        }
    }
}
