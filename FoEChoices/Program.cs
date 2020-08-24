using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FoEChoices
{

    public class Answer {

        public string Text { get; set; }
        public int UserInput { get; set; }
        public int ID { get; set; } // id format example: 2110048 chapter(2), quest(11), quest path(0), id(048)
        public int InstanceID { get; set; }
        public bool HasSpecialFunction { get; set; }
        public List<int> SpecialFunction { get; set; } // set to 1 or more to execute additional code. use with the functions below to use their code
        public int RepCheck { get; set; } // SpecialFunction: 1. set to the amount of rep points the player needs to have to pass the check.
        public string FactionName { get; set; } // SpecialFunction: 1. set to the faction's name you want to check the rep points. has to be used with RepCheck!
        public int QuestID { get; set; } // SpecialFuntion: 2. set to the ID of a quest to mark as completed

    }

    public class SuccessfulAnswer {

        public string Text { get; set; }
        public int UserInput { get; set; }
        public int ID { get; set; }
        public int InstanceID { get; set; }

    }

    public class Instance {

        public string Text { get; set; }
        public int ID { get; set; }
        public int AnswerID { get; set; }
        public bool HasSpecialFunction { get; set; } 
        public List<int> SpecialFunction { get; set; } // set to 1 or more to execute additional code. needs to be used with HasSpecialFunction. 1 = Finish; 2 = Death. syntax: new List<int>(new int[]{ 5 })
        public int NewWeaponID { get; set; } // SpecialFunction: 3. set to the corresponding WeaponID of a weapon to give to the player.
        public int DamageCheck { get; set; } // SpecialFunction: 4. set to the amount of damage the player's weapon needs to do to pass the check. default weapon makes 1 point of damage, so there's no point in having this set to 1.
        public string Faction { get; set; } // SpecialFunction: 5. set to the faction's name you want to add/remove rep points to/from
        public int RepPoints { get; set; } // SpecialFunction: 5. set to the amount of rep points to award or take from the player. has to be used with Faction!
        public int NewArmorID { get; set; } // SpecialFunction: 6. set to the corresponding ArmorID of an armor to give to the player.
        public int ArmorCheck { get; set; } // SpecialFunction: 7. set to the ArmorClass the player's armor needs to have to pass the check.
        public int QuestCheck { get; set; } // SpecialFunction: 8. set to the ID of a quest to check if it has been completed. !!!don't use twice without having answers in between, will break!!!
        public int RemoveAnswer { get; set; } // Specialfunction: 9. set to the ID of an answer to remove

    }

    public class Weapon {

        public string WeaponName { get; set; }
        public int WeaponID { get; set; }
        public int Damage { get; set; }

    }

    public class Faction {

        public string FactionName { get; set; }
        public int RepPoints { get; set; }

    }

    public class Armor {

        public string ArmorName { get; set; }
        public int ArmorID { get; set; }
        public int ArmorClass { get; set; }

    }

    public class Quest {

        public string Description { get; set; }
        public int QuestID { get; set; }
        public bool Completed { get; set; }

    }

    class Program
    {
        // list of all factions
        public static Faction Stable54 = new Faction
        {
            FactionName = "Stable 54",
            RepPoints = 0
        };

        public static int CurrentArmorID { get; set; }
        public static string CurrentArmorName { get; set; }
        public static int GameState { get; set; } // 0 = playing; 1 = game over

        public static List<SuccessfulAnswer> SuccessfulQuestAnswer = new List<SuccessfulAnswer>();

        public static List<Answer> AnswersList = new List<Answer>();

        public static List<Instance> InstanceList = new List<Instance>();

        public static List<Quest> QuestList = new List<Quest>();

        public static void Main(string[] args)
        {
            Console.WriteLine("\tLoading...");
// ------------------------------------------------------- QUEST LIST -------------------------------------------------------
            QuestList.Add(new Quest() { Description = "Have a constructive conversation with Light Harmony", QuestID = 0, Completed = false });
            QuestList.Add(new Quest() { Description = "Reset Cross Stitch's terminal's password and not lose your cool", QuestID = 1, Completed = false });
            QuestList.Add(new Quest() { Description = "Inspect the pistol in the Stable's armory", QuestID = 2, Completed = false });
            QuestList.Add(new Quest() { Description = "Mention Astral in your first conversation with Ardent", QuestID = 3, Completed = false });

            // ------------------------------------------------------- SUCCESSFUL QUEST ANSWERS -------------------------------------------------------
            SuccessfulQuestAnswer.Add(
                new SuccessfulAnswer
                {
                    Text = ") Placeholder",
                    UserInput = 99,
                    ID = 99,
                    InstanceID = 99
                }
                );

            RefreshTexts();

            int PassInstanceID = 0;
            int CurrentWeaponID = 0;
            int PassAnswerID = 0;
            GameState = 0;

            // var SoundPlayer = new System.Media.SoundPlayer();
            // SoundPlayer.SoundLocation = Environment.CurrentDirectory + "\\file-name.wav";
            // SoundPlayer.Load();
            Console.SetWindowSize(160, 40);
            Console.WriteLine("\t               ----------------------------------\n" +
                "\t            -------______________________-----------\n" +
                "\t         ----------\\	 		 \\-------------\n" +
                "\t      --------------\\  Fallout Equestria  \\---------------\n" +
                "\t      ---------------\\	     Choices	   \\--------------\n" +
                "\t         -------------\\_____________________\\----------\n" +
                "\t            -----------		      	     -------\n" +
                "\t               ----------------------------------");

            Console.WriteLine("\tDone.");
            Console.WriteLine("\tWelcome to Fallout Equestria: Choices. Before you start the game, please resize the console window so that this line of text fits on one line.");
            Console.WriteLine("\tIt's also recommended that you raise the font size a bit, so that the text is easier to read.");
            Console.WriteLine("\tRight click on the top part of the console window and choose \"Properties\". Click on the \"Font\" tab. Font size of 20 should be ok.");
            Console.WriteLine("\tPress Enter to start.");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            Console.Clear();

            Console.WriteLine("\tOnce upon a time, in the magical land of Equestria... A war broke out against the Zebra Empire, \n" +
                "\tas a consequence to multiple trade sanctions involving natural resources. While the Equestrian Nation had a monopoly \n" +
                "\ton magical gemstones, the Zebra Empire had a monopoly on coal, which was needed to keep the Equestrian Nation running. \n" +
                "\tBecause of the war, Equestria was forced into an industrial revolution, as an attempt to outdo the Zebra Empire in wartime technology. \n" +
                "\tPart of this was Stable-Tec, a corporation specialized in arcane science. One of their most known creations were the Stables. \n" +
                "\tThey were large fallout shelters, built all around Equestria in case of a megaspell holocaust. Your ancestors were selected \n" +
                "\tto become inhabitants of Stable 54. \n" +
                "\tAs neither side was able to make the other side surrender, the bombs eventually fell and engulfed the earth in fire and radiation, \n" +
                "\tsweeping it almost clean of life. \n" +
                "\n" +
                "\tYou are a unicorn mare named Silver Shift. You have been born and raised in Stable 54, which has now been functioning for nearly 200 years.\n" +
                "\tThis is where your story starts.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            Console.WriteLine();
            Console.WriteLine("\tYou wake up in your bed. You get up, and notice that your chest still hurts from yesterday's fight.\n" +
                "\tYou brush your mane, put on your Stable suit, and head to the cafeteria to get some breakfast.\n" +
                "\tOn your way there you run into Light Harmony, a young unicorn filly with white coat and dark blue mane.\n" +
                "\tAs she notices you, she tenses up, and looks a bit worried.");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            Console.WriteLine();
            Console.WriteLine("\t\"Oh, hey there Silver... Uhh, I was just about to leave...\" says the filly.");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

            while (GameState == 0) {

                PassAnswerID = GetAnswers(PassInstanceID, CurrentWeaponID);

                PassInstanceID = GetInstance(PassAnswerID, CurrentWeaponID);

            }

        }

        public static dynamic GetAnswers(int PassInstanceID, int WeaponID) {

            int PassedInstance = PassInstanceID;

            int CurrentWeaponID = 0;
            CurrentWeaponID = WeaponID;
            string CurrentWeaponName = WeaponName(CurrentWeaponID, 0);

            string NonParsedAnswer;
            int ParsedAnswer = 0;
            int AcceptedAnswer = 0;
            int PassAnswerID = 0;
            bool AnsWithinLimits = false;
            bool AnsSecondary = false;

            //arraylist query
            var AnswerEnum = AnswersList.OfType<Answer>();
            var CurrentAnswers =
                from answer in AnswerEnum
                where answer.InstanceID == PassInstanceID
                select answer;

            Console.WriteLine();
            foreach (var answer in CurrentAnswers)
            {
                Console.WriteLine("\t{0}) " + "{1}", answer.UserInput, answer.Text);
            }
            Console.Write("\t");
            NonParsedAnswer = Console.ReadLine();

            // try to parse the answer given by the player
            bool successfullyParsed = int.TryParse(NonParsedAnswer, out int IgnoreMe);
            if (successfullyParsed)
            {
                ParsedAnswer = Convert.ToInt32(NonParsedAnswer);
            }
            else
            {
                Console.WriteLine("\tUnacceptable answer. Please type a number that is a valid answer.");
                return GetAnswers(PassInstanceID, CurrentWeaponID);
            }

            var QueryAnswerID =
                from answer in AnswerEnum
                where answer.UserInput == ParsedAnswer && answer.InstanceID == PassInstanceID
                select answer;

            // check if the now parsed answer corresponds with the possible answers given
            foreach (var answer in QueryAnswerID)
            {
                if (ParsedAnswer == answer.UserInput)
                {
                    AcceptedAnswer = ParsedAnswer;
                    AnsWithinLimits = true;
                }
            }

            // check if the foreach was skipped
            if (AnsWithinLimits == false)
            {
                Console.WriteLine("\tUnacceptable answer. Please type a number that is a valid answer.");
                return GetAnswers(PassInstanceID, CurrentWeaponID);
            }
            else
            {
                AnsWithinLimits = false;
            }

            Console.WriteLine();

            QueryAnswerID =
                from answer in AnswerEnum
                where answer.UserInput == ParsedAnswer && answer.InstanceID == PassInstanceID
                select answer;

            if (AnsSecondary == false)
            {
                foreach (var answer in QueryAnswerID)
                {
                    PassAnswerID = answer.ID;

                    //check for any special functions
                    if (answer.HasSpecialFunction)
                    {
                        // add/remove rep points
                        if (answer.SpecialFunction.Contains(1))
                        {
                            int points = GetRepPoints(answer.FactionName, 0);

                            if (answer.RepCheck < points)
                            {
                                PassAnswerID += 1;
                            }
                        }
                        // set the quest to completed
                        if (answer.SpecialFunction.Contains(2))
                        {
                            GetQuest(answer.QuestID);
                        }
                    }
                }
            }

            return PassAnswerID;

        }

        public static dynamic GetInstance(int PassAnswerID, int WeaponID) {

            int CurrentWeaponID = WeaponID;
            string CurrentWeaponName = WeaponName(CurrentWeaponID, 0);

            int PassInstanceID = 0;
            int WeaponDamage;
            int ArmorClass;

            int Answered = Convert.ToInt32(PassAnswerID);

            bool TextUpdatePending = false;

            var InstanceEnum = InstanceList.OfType<Instance>();
            var CurrentInstances =
                from instance in InstanceEnum
                where instance.AnswerID == Answered
                select instance;

            foreach (var instance in CurrentInstances) {
                PassInstanceID = instance.ID;
                Console.WriteLine("\t{0}", instance.Text);
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

                if (instance.HasSpecialFunction)
                {
                    // check for any special functions
                    for (int i = 0; i < instance.SpecialFunction.Count; i++)
                    {

                        if (instance.SpecialFunction.Contains(1))
                        {
                            Finished();
                        }
                        if (instance.SpecialFunction.Contains(2))
                        {
                            Death();
                        }
                        // give the player a new weapon
                        if (instance.SpecialFunction.Contains(3))
                        {
                            CurrentWeaponID = instance.NewWeaponID;
                            CurrentWeaponName = WeaponName(CurrentWeaponID, 0);
                            TextUpdatePending = true;
                        }
                        // check if the player's weapon does enough damage to pass the check
                        if (instance.SpecialFunction.Contains(4))
                        {
                            WeaponDamage = WeaponName(CurrentWeaponID, 2);

                            if (WeaponDamage >= instance.DamageCheck) // does enough damage
                            {
                                Answered = instance.AnswerID + 2;
                                GetInstance(Answered, CurrentWeaponID);
                            }
                            else if (WeaponDamage < instance.DamageCheck) // does not do enough damage
                            {
                                Answered = instance.AnswerID + 1;
                                GetInstance(Answered, CurrentWeaponID);
                            }
                        }
                        if (instance.SpecialFunction.Contains(5)) // MAKE THIS DYNAMICALLY IF POSSIBLE!!!
                        {
                            if (instance.Faction == "Stable 54")
                            {
                                GetRepPoints("Stable 54", instance.RepPoints);
                            }
                        }
                        // give new armor to the player
                        if (instance.SpecialFunction.Contains(6)) {
                            CurrentArmorID = instance.NewArmorID;
                            CurrentArmorName = GetNewArmor(CurrentArmorID, 0);
                            TextUpdatePending = true;
                        }
                        // check if the players armor is strong enough to pass the check
                        if (instance.SpecialFunction.Contains(7))
                        {
                            ArmorClass = WeaponName(CurrentWeaponID, 2);

                            if (ArmorClass >= instance.ArmorCheck) // armor protects
                            {
                                Answered = instance.AnswerID + 2;
                                GetInstance(Answered, CurrentWeaponID);
                            }
                            else if (ArmorClass < instance.ArmorCheck) // armor does not protect
                            {
                                Answered = instance.AnswerID + 1;
                                GetInstance(Answered, CurrentWeaponID);
                            }
                        }
                        // check if a quest has been completed
                        if (instance.SpecialFunction.Contains(8))
                        {
                            if(QuestList[instance.QuestCheck].Completed == true)
                            {
                                Answered = instance.AnswerID + 2;
                                GetInstance(Answered, CurrentWeaponID);
                            }
                            else if (QuestList[instance.QuestCheck].Completed == false)
                            {
                                Answered = instance.AnswerID + 1;
                                GetInstance(Answered, CurrentWeaponID);
                            }
                        }
                        if(instance.SpecialFunction.Contains(9))
                        {
                            var item = AnswersList.SingleOrDefault(x => x.ID == PassAnswerID);

                            if(item != null)
                            {
                                AnswersList.Remove(item);
                            }
                        }
                    }

                }

            }

            if (TextUpdatePending) {
                RefreshTexts();
                TextUpdatePending = false;
            }

            return PassInstanceID;

        }

        public static void Death() {

            Console.WriteLine("You died. Restart? y/n");
            string RestartAnswer = Console.ReadLine();

            string[] args = null;

            if (RestartAnswer == "y")
            {
                Main(args);
            }
            else if (RestartAnswer == "n")
            {
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("That's not an acceptable answer.");
            }

        }

        public static void Finished() {

            Console.WriteLine("Congratulations, you won the game! Now, go outside. \n\r" +
                "You've probably been sitting on your ass all day.");
            Console.ReadKey();
            System.Environment.Exit(1);

        }

        public static dynamic GetRepPoints(string Faction, int SentRepPoints) {

            string FactionName = Faction;
            int RepPoints = SentRepPoints;
            int RepPointsToSend = 0;

            // MAKE THIS DYNAMICALLY IF POSSIBLE!!!
            if (FactionName == "Stable 54") {

                // add or remove the possible amount of points
                Stable54.RepPoints += RepPoints;

                // send the requested faction's rep points
                RepPointsToSend = Stable54.RepPoints;

            }

            return RepPointsToSend;

        }

        public static void GetQuest(int questid) {

            QuestList[questid].Completed = true;

        }

        public static dynamic WeaponName(int GetNewWeaponID, int InfoType) {

            string WeaponsName = "default";
            int RequestedInfo = InfoType; // 0 = WeaponName; 1 = WeaponID; 2 = Damage
            int WeaponDamage;

            ArrayList WeaponList = new ArrayList()
            {

                new Weapon
                {
                    WeaponName = "10mm Pistol",
                    WeaponID = 0,
                    Damage = 1
                },
                new Weapon
                {
                    WeaponName = "Double Barrel Shotgun",
                    WeaponID = 1,
                    Damage = 2
                }

            };

            int CurrentWeaponID = GetNewWeaponID;

            var WeaponEnum = WeaponList.OfType<Weapon>();
            var Weapons =
                from weapon in WeaponEnum
                where weapon.WeaponID == CurrentWeaponID
                select weapon;

            foreach (var weapon in Weapons)
            {
                WeaponsName = weapon.WeaponName;
                WeaponDamage = weapon.Damage;

                if (RequestedInfo == 0)
                {
                    return WeaponsName;
                }
                else if (RequestedInfo == 1)
                {
                    return CurrentWeaponID;
                }
                else if (RequestedInfo == 2)
                {
                    return WeaponDamage;
                }
                else
                {
                    Console.WriteLine("The request from the weapon was invalid or empty!");
                }

            }

            return null;

        }

        public static dynamic GetNewArmor(int GetNewArmorID, int InfoType) {

            string ArmorsName = "default";

            int RequestedInfo = InfoType; // 0 = ArmorName; 1 = ArmorID; 2 = ArmorClass
            int ArmorsClass;

            ArrayList ArmorList = new ArrayList()
            {

                new Armor
                {
                    ArmorName = "default",
                    ArmorID = 0,
                    ArmorClass = 0
                },
                new Armor
                {
                    ArmorName = "Strenghtened Stable Suit",
                    ArmorID = 1,
                    ArmorClass = 1
                }

            };

            CurrentArmorID = GetNewArmorID;

            var ArmorEnum = ArmorList.OfType<Armor>();
            var Armors =
                from armor in ArmorEnum
                where armor.ArmorID == CurrentArmorID
                select armor;
            
            foreach (var armor in Armors)
            {
                ArmorsName = armor.ArmorName;
                ArmorsClass = armor.ArmorClass;

                if (RequestedInfo == 0)
                {
                    return ArmorsName;
                }
                else if (RequestedInfo == 1)
                {
                    return CurrentArmorID;
                }
                else if (RequestedInfo == 2)
                {
                    return ArmorsClass;
                }
                else
                {
                    Console.WriteLine("The request from the armor was invalid or empty!");
                }

            }

            return null;

        }

        public static void RefreshTexts() {
            // ... don't judge my code
            AnswersList.Clear();
            InstanceList.Clear();

            // ------------------------------------------------------- ANSWER LIST -------------------------------------------------------
            AnswersList.Add(new Answer
            {
                Text = "Great. Even the kids shun me now.",
                UserInput = 1,
                ID = 0,
                InstanceID = 0
            });
            AnswersList.Add(new Answer
            {
                Text = "Hey now, no need to be scared. I won't hurt anypony.",
                UserInput = 2,
                ID = 1,
                InstanceID = 0
            });
            AnswersList.Add(new Answer
            {
                Text = "[jump in front of her] BOO!",
                UserInput = 3,
                ID = 2,
                InstanceID = 0
            });
            AnswersList.Add(new Answer
            {
                Text = "Yeah, you should be avoiding Astral, she's setting a bad example for you and other kids. But not me.",
                UserInput = 1,
                ID = 3,
                InstanceID = 1,
            });
            AnswersList.Add(new Answer
            {
                Text = "Oh please, your mom is overprotective. Don't let her tell you how to live your life.",
                UserInput = 2,
                ID = 4,
                InstanceID = 1,
            });
            AnswersList.Add(new Answer
            {
                Text = "I can see why.",
                UserInput = 3,
                ID = 5,
                InstanceID = 1,
            });
            AnswersList.Add(new Answer
            {
                Text = "Stupid kids.",
                UserInput = 1,
                ID = 6,
                InstanceID = 2
            });
            AnswersList.Add(new Answer
            {
                Text = "Sorry, didn't mean to scare you THAT badly!",
                UserInput = 2,
                ID = 6,
                InstanceID = 2
            });
            AnswersList.Add(new Answer
            {
                Text = "Of course not. Things just got a little out of hoof yesterday.",
                UserInput = 1,
                ID = 1,
                InstanceID = 3,
            });
            AnswersList.Add(new Answer
            {
                Text = "We'll see.",
                UserInput = 2,
                ID = 7,
                InstanceID = 3
            });
            AnswersList.Add(new Answer
            {
                Text = "No. Well, maybe Astral if she keeps being that annoying, but nopony else.",
                UserInput = 3,
                ID = 8,
                InstanceID = 3
            });
            AnswersList.Add(new Answer
            {
                Text = "*sigh* I know, but still.",
                UserInput = 1,
                ID = 9,
                InstanceID = 4
            });
            AnswersList.Add(new Answer
            {
                Text = "Well, you shouldn't.",
                UserInput = 2,
                ID = 9,
                InstanceID = 4
            });
            AnswersList.Add(new Answer
            {
                Text = "It means that your mom won't let you do what you want.",
                UserInput = 1,
                ID = 10,
                InstanceID = 5
            });
            AnswersList.Add(new Answer
            {
                Text = "It means that your mom doesn't want you to get hurt.",
                UserInput = 2,
                ID = 10,
                InstanceID = 5,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 0
            });
            AnswersList.Add(new Answer
            {
                Text = "Uh, nevermind.",
                UserInput = 3,
                ID = 10,
                InstanceID = 5
            });
            AnswersList.Add(new Answer
            {
                Text = "Oh, nevermind.",
                UserInput = 1,
                ID = 10,
                InstanceID = 6
            });
            AnswersList.Add(new Answer
            {
                Text = "Your mom just wants you to be safe, which is understandable.",
                UserInput = 2,
                ID = 10,
                InstanceID = 6,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 6 }),
                QuestID = 0
            });
            AnswersList.Add(new Answer
            {
                Text = "That's just how moms are.",
                UserInput = 3,
                ID = 10,
                InstanceID = 6
            });
            AnswersList.Add(new Answer
            {
                Text = "Yeah, whatever.",
                UserInput = 1,
                ID = 6,
                InstanceID = 8
            });
            AnswersList.Add(new Answer
            {
                Text = "Yeah, it was.",
                UserInput = 1,
                ID = 10,
                InstanceID = 9
            });
            AnswersList.Add(new Answer
            {
                Text = "No, really, if she doesn't stop being a prick, there will be consequences.",
                UserInput = 2,
                ID = 11,
                InstanceID = 9
            });
            AnswersList.Add(new Answer
            {
                Text = "I'll leave that up to you to decide.",
                UserInput = 3,
                ID = 11,
                InstanceID = 9
            });
            AnswersList.Add(new Answer
            {
                Text = "Later.",
                UserInput = 1,
                ID = 6,
                InstanceID = 10
            });
            AnswersList.Add(new Answer
            {
                Text = "Morning.",
                UserInput = 1,
                ID = 12,
                InstanceID = 7
            });
            AnswersList.Add(new Answer
            {
                Text = "[say nothing, start eating your breakfast]",
                UserInput = 2,
                ID = 13,
                InstanceID = 7
            });
            AnswersList.Add(new Answer
            {
                Text = "Something bothering you? You look a bit stressed.",
                UserInput = 1,
                ID = 14,
                InstanceID = 11
            });
            AnswersList.Add(new Answer
            {
                Text = "Don't tell me you too are scared of me because of yesterday.",
                UserInput = 2,
                ID = 15,
                InstanceID = 11
            });
            AnswersList.Add(new Answer
            {
                Text = "You know, a \"Good morning\" would have been a nice thing to say.",
                UserInput = 1,
                ID = 16,
                InstanceID = 12
            });
            AnswersList.Add(new Answer
            {
                Text = "Something bothering you?",
                UserInput = 2,
                ID = 14,
                InstanceID = 12
            });
            AnswersList.Add(new Answer
            {
                Text = "You know I'm bad with social skills, so if you could just tell me what's bothering you, that would be nice.",
                UserInput = 3,
                ID = 14,
                InstanceID = 12
            });
            AnswersList.Add(new Answer
            {
                Text = "Whatcha thinking about?",
                UserInput = 1,
                ID = 17,
                InstanceID = 15
            });
            AnswersList.Add(new Answer
            {
                Text = "[give him a warm smile] Thinking is not for you.",
                UserInput = 2,
                ID = 40,
                InstanceID = 15
            });
            AnswersList.Add(new Answer
            {
                Text = "Care to tell me? You're usually not this gloomy.",
                UserInput = 3,
                ID = 17,
                InstanceID = 15
            });
            AnswersList.Add(new Answer
            {
                Text = "[return the smile] Thanks, Ardent. So, what's on your mind then?",
                UserInput = 1,
                ID = 17,
                InstanceID = 14
            });
            AnswersList.Add(new Answer
            {
                Text = "Oh you, stop being so sweet. Well, if it's not that, then what is it?",
                UserInput = 2,
                ID = 17,
                InstanceID = 14
            });
            AnswersList.Add(new Answer
            {
                Text = "So what? It happens every year, and I'm pretty sure none of our friends or relatives are gonna get thrown out.",
                UserInput = 1,
                ID = 18,
                InstanceID = 13
            });
            AnswersList.Add(new Answer
            {
                Text = "You should be glad, I think Astral is going to get kicked out next.",
                UserInput = 2,
                ID = 19,
                InstanceID = 13,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 3
            });
            AnswersList.Add(new Answer
            {
                Text = "Wait, it's time for that again? Time sure does fly by.",
                UserInput = 3,
                ID = 20,
                InstanceID = 13
            });
            AnswersList.Add(new Answer
            {
                Text = "Oh. Well, it's gonna have to happen, there's nothing we can do about that.",
                UserInput = 4,
                ID = 21,
                InstanceID = 13
            });
            AnswersList.Add(new Answer
            {
                Text = "Well, no-",
                UserInput = 1,
                ID = 22,
                InstanceID = 16
            });
            AnswersList.Add(new Answer
            {
                Text = "Hmm. Now that you mention it, the whole Stable is getting along just fine.",
                UserInput = 2,
                ID = 22,
                InstanceID = 16
            });
            AnswersList.Add(new Answer
            {
                Text = "That's next year's problem, you shouldn't be stressing about that.",
                UserInput = 3,
                ID = 23,
                InstanceID = 16
            });
            AnswersList.Add(new Answer
            {
                Text = "I... Hadn't thought about that.",
                UserInput = 1,
                ID = 22,
                InstanceID = 17
            });
            AnswersList.Add(new Answer
            {
                Text = "You mean there's no more annoying ponies here? There's always somepony who annoys others.",
                UserInput = 2,
                ID = 24,
                InstanceID = 17
            });
            AnswersList.Add(new Answer
            {
                Text = "Geez, calm down. I was just joking.",
                UserInput = 1,
                ID = 25,
                InstanceID = 18
            });
            AnswersList.Add(new Answer
            {
                Text = "Uh, sorry. So, why are you stressed about that? It happens every year, and you're usually not that worried about it.",
                UserInput = 2,
                ID = 25,
                InstanceID = 18
            });
            AnswersList.Add(new Answer
            {
                Text = "No, I haven't. Why you ask?",
                UserInput = 1,
                ID = 26,
                InstanceID = 19
            });
            AnswersList.Add(new Answer
            {
                Text = "No, I like to live one year at a time.",
                UserInput = 2,
                ID = 26,
                InstanceID = 19
            });
            AnswersList.Add(new Answer
            {
                Text = "I hope not.",
                UserInput = 1,
                ID = 27,
                InstanceID = 20
            });
            AnswersList.Add(new Answer
            {
                Text = "Don't know. Nor do I care, really.",
                UserInput = 2,
                ID = 28,
                InstanceID = 20
            });
            AnswersList.Add(new Answer
            {
                Text = "Why? It will happen every year, we can't change that.",
                UserInput = 1,
                ID = 29,
                InstanceID = 21
            });
            AnswersList.Add(new Answer
            {
                Text = "Look, I couldn't care less for the voting. This year's OR the next.",
                UserInput = 2,
                ID = 28,
                InstanceID = 21
            });
            AnswersList.Add(new Answer
            {
                Text = "No, not really.",
                UserInput = 1,
                ID = 30,
                InstanceID = 22
            });
            AnswersList.Add(new Answer
            {
                Text = "If they are annoying and are aware of it, no.",
                UserInput = 2,
                ID = 31,
                InstanceID = 22
            });
            AnswersList.Add(new Answer
            {
                Text = "Well, when you put it that way... It kinda is.",
                UserInput = 3,
                ID = 32,
                InstanceID = 22
            });
            AnswersList.Add(new Answer
            {
                Text = "Oh, are you kidding me? I would be glad to get out of this Celestia-forsaken hole.",
                UserInput = 4,
                ID = 33,
                InstanceID = 22
            });
            AnswersList.Add(new Answer
            {
                Text = "Oh, are you kidding me? I would be glad to get to explore the outside world!",
                UserInput = 5,
                ID = 34,
                InstanceID = 22
            });
            AnswersList.Add(new Answer
            {
                Text = "What?! Are you crazy?",
                UserInput = 1,
                ID = 35,
                InstanceID = 23
            });
            AnswersList.Add(new Answer
            {
                Text = "I have a better idea. Let's NOT do anything that's dangerous for the entire Stable.",
                UserInput = 2,
                ID = 36,
                InstanceID = 23
            });
            AnswersList.Add(new Answer
            {
                Text = "Might as well overthrow the Overmare and tell Stable-Tec to go fuck themselves while we're at it.",
                UserInput = 3,
                ID = 37,
                InstanceID = 23
            });
            AnswersList.Add(new Answer
            {
                Text = "Hmm. That might not be a bad idea.",
                UserInput = 4,
                ID = 38,
                InstanceID = 23
            });
            AnswersList.Add(new Answer
            {
                Text = "I... *sigh* I don't know. Sorry, I guess I'm just a little tired.",
                UserInput = 1,
                ID = 32,
                InstanceID = 24
            });
            AnswersList.Add(new Answer
            {
                Text = "None of your business.",
                UserInput = 2,
                ID = 46,
                InstanceID = 24
            });
            AnswersList.Add(new Answer
            {
                Text = "No, not other ponies... I just... don't care about the vote. It happens only once a year, and nothing bad has\n" +
                "ever happened because of the vote.",
                UserInput = 1,
                ID = 44,
                InstanceID = 25
            });
            AnswersList.Add(new Answer
            {
                Text = "You think I'm the indifferent one here? What about the Overmare? Why do you think she hasn't made any objections\n" +
                "against the vote? Seems like as long as she can't be voted out, everything's just fine.",
                UserInput = 2,
                ID = 45,
                InstanceID = 25
            });
            AnswersList.Add(new Answer
            {
                Text = "Well now you know.",
                UserInput = 3,
                ID = 46,
                InstanceID = 25
            });
            AnswersList.Add(new Answer
            {
                Text = "Don't get me wrong, of course I would be happy to get rid of the vote.",
                UserInput = 1,
                ID = 47,
                InstanceID = 26
            });
            AnswersList.Add(new Answer
            {
                Text = "The less there are annoying ponies, the better the Stable will be, right?",
                UserInput = 2,
                ID = 30,
                InstanceID = 26
            });
            AnswersList.Add(new Answer
            {
                Text = "Hm, fair points.",
                UserInput = 1,
                ID = 27,
                InstanceID = 27
            });
            AnswersList.Add(new Answer
            {
                Text = "Surely there must be at least some life outside? I doubt the megaspells were able to wipe out literally everything.",
                UserInput = 2,
                ID = 50,
                InstanceID = 27
            });
            AnswersList.Add(new Answer
            {
                Text = "I don't care. If you think I'm going to go against the Overmare, you're out of your mind.",
                UserInput = 1,
                ID = 46,
                InstanceID = 28
            });
            AnswersList.Add(new Answer
            {
                Text = "Hm, I guess you're right. Tell you what, if you can get more ponies in, and come up with a decent plan, I'll join you.\n" +
                "\tBut right now I want to eat, let's talk about this some other time.",
                UserInput = 2,
                ID = 49,
                InstanceID = 28
            });
            AnswersList.Add(new Answer
            {
                Text = "Count me in. But right now I want to eat, let's talk about this more some other time.",
                UserInput = 1,
                ID = 49,
                InstanceID = 29,
            });
            AnswersList.Add(new Answer
            {
                Text = "Sounds good in theory, but what if the rebellion fails? The consequences might be really bad.",
                UserInput = 2,
                ID = 41,
                InstanceID = 29,
            });
            AnswersList.Add(new Answer
            {
                Text = "Yeah, I do. But we need to plan this well, I don't want this to turn out like the previous one.",
                UserInput = 1,
                ID = 54,
                InstanceID = 30
            });
            AnswersList.Add(new Answer
            {
                Text = "I said *might*. You know this is extremely dangerous for ourselves, AND the Stable, right?",
                UserInput = 2,
                ID = 41,
                InstanceID = 30
            });
            AnswersList.Add(new Answer
            {
                Text = "But right now I want to eat, can't plan anything with an empty stomach.",
                UserInput = 1,
                ID = 49,
                InstanceID = 37,
            });
            AnswersList.Add(new Answer
            {
                Text = "Alright, I'll think about it.",
                UserInput = 1,
                ID = 49,
                InstanceID = 32,
            });
            AnswersList.Add(new Answer
            {
                Text = "I'll join. But let's talk about it more some other time, I want to eat now.",
                UserInput = 2,
                ID = 49,
                InstanceID = 32,
            });
            AnswersList.Add(new Answer
            {
                Text = "Alright, I'll think about it.",
                UserInput = 1,
                ID = 49,
                InstanceID = 33,
            });
            AnswersList.Add(new Answer
            {
                Text = "Okay, I'm with you. But this needs to be planned well. Let's talk about it more later.",
                UserInput = 2,
                ID = 49,
                InstanceID = 33,
            });
            AnswersList.Add(new Answer
            {
                Text = "Yeah, I guess you're right. Sorry, I never really thinked about the voting since it has never affected me.",
                UserInput = 1,
                ID = 43,
                InstanceID = 34,
            });
            AnswersList.Add(new Answer
            {
                Text = "Whatever.",
                UserInput = 2,
                ID = 46,
                InstanceID = 34,
            });
            AnswersList.Add(new Answer
            {
                Text = "Or maybe she just wants to make us miserable.",
                UserInput = 1,
                ID = 23,
                InstanceID = 35,
            });
            AnswersList.Add(new Answer
            {
                Text = "Hm, might be.",
                UserInput = 2,
                ID = 32,
                InstanceID = 35,
            });
            AnswersList.Add(new Answer
            {
                Text = "[Go to the laundry]",
                UserInput = 1,
                ID = 0010000,
                InstanceID = 36,
            });
            AnswersList.Add(new Answer
            {
                Text = "[Go visit your dad]",
                UserInput = 2,
                ID = 0011000,
                InstanceID = 36,
            });
            AnswersList.Add(new Answer
            {
                Text = "[Go back to your room]",
                UserInput = 3,
                ID = 0012000,
                InstanceID = 36,
            });
            AnswersList.Add(new Answer
            {
                Text = "[Wave back] Hey Stitch. I have some laundry here.",
                UserInput = 1,
                ID = 0010001,
                InstanceID = 0010000,
            });
            AnswersList.Add(new Answer
            {
                Text = "Yeah hi. Here's some laundry for you.",
                UserInput = 2,
                ID = 0010001,
                InstanceID = 0010000,
            });
            AnswersList.Add(new Answer
            {
                Text = "Again? This is the third time this week.",
                UserInput = 1,
                ID = 0010002,
                InstanceID = 0010001,
            });
            AnswersList.Add(new Answer
            {
                Text = "Seriously? This again? I don't have time to always be here resetting the password for you.",
                UserInput = 2,
                ID = 0010003,
                InstanceID = 0010001,
            });
            AnswersList.Add(new Answer
            {
                Text = "Alright, let's go reset it.",
                UserInput = 1,
                ID = 0010004,
                InstanceID = 0010002,
            });
            AnswersList.Add(new Answer
            {
                Text = "Hmm, not this time. The lockout will end after 24 hours, that should be enough time for you to try and remember the password.",
                UserInput = 2,
                ID = 0010005,
                InstanceID = 0010002,
            });
            AnswersList.Add(new Answer
            {
                Text = "Seems to work. Try to keep the password in mind this time, would you?",
                UserInput = 1,
                ID = 0010006,
                InstanceID = 0010004,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 6 }),
                QuestID = 1
            });
            AnswersList.Add(new Answer
            {
                Text = "There we go. Same place, same time tomorrow?",
                UserInput = 2,
                ID = 0010007,
                InstanceID = 0010004,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 6 }),
                QuestID = 1 
            });
            AnswersList.Add(new Answer
            {
                Text = "This better be the last time I have to do this.",
                UserInput = 3,
                ID = 0010008,
                InstanceID = 0010004,
            });
            AnswersList.Add(new Answer
            {
                Text = "I see. Must start to feel a bit... monotonous after a while, huh?",
                UserInput = 1,
                ID = 0011001,
                InstanceID = 0011000,
            });
            AnswersList.Add(new Answer
            {
                Text = "I'd be bored to tartarus if I had to be here rearranging boxes all the time.",
                UserInput = 2,
                ID = 0011002,
                InstanceID = 0011000,
            });
            AnswersList.Add(new Answer
            {
                Text = "Yes please, the coffee they serve at the cafeteria tastes like the coffee grounds have been used a bit too many times.",
                UserInput = 1,
                ID = 0011003,
                InstanceID = 0011001,
            });
            AnswersList.Add(new Answer
            {
                Text = "No thanks, I think I'll be going now. Just wanted to stop by here.",
                UserInput = 2,
                ID = 0011004,
                InstanceID = 0011001,
            });
            AnswersList.Add(new Answer
            {
                Text = "[Leave the pistol alone and don't mention about it to dad]",
                UserInput = 1,
                ID = 0011006,
                InstanceID = 0011002,
            });
            AnswersList.Add(new Answer
            {
                Text = "[Inspect the pistol and show it to dad]",
                UserInput = 2,
                ID = 0011005,
                InstanceID = 0011002,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 2,
            });
            AnswersList.Add(new Answer
            {
                Text = "Do you know why it looks so different compared to the other pistols here?",
                UserInput = 1,
                ID = 0011010,
                InstanceID = 0011005,
            });
            AnswersList.Add(new Answer
            {
                Text = "Shouldn't it just be stored with the other weapons?",
                UserInput = 2,
                ID = 0011010,
                InstanceID = 0011005,
            });
            AnswersList.Add(new Answer
            {
                Text = "This pistol looks like it has a story behind it.",
                UserInput = 3,
                ID = 0011010,
                InstanceID = 0011005,
            });
            AnswersList.Add(new Answer
            {
                Text = "You should keep it as a memory of her. The gun probably meant a lot to her.",
                UserInput = 1,
                ID = 0011012,
                InstanceID = 0011006,
            });
            AnswersList.Add(new Answer
            {
                Text = "Can I keep it? It would be nice to have something to remember her by.",
                UserInput = 2,
                ID = 0011013,
                InstanceID = 0011006,
            });
            AnswersList.Add(new Answer
            {
                Text = "Do you know what the writing on the side means?",
                UserInput = 3,
                ID = 0011014,
                InstanceID = 0011006,
            });
            AnswersList.Add(new Answer
            {
                Text = "[Finish the coffee and leave]",
                UserInput = 1,
                ID = 0011015,
                InstanceID = 0011003,
            });
            AnswersList.Add(new Answer
            {
                Text = "How're you feeling about the voting?",
                UserInput = 2,
                ID = 0011016,
                InstanceID = 0011003,
            });
            AnswersList.Add(new Answer
            {
                Text = "Where's everypony else?",
                UserInput = 3,
                ID = 0011017,
                InstanceID = 0011003,
            });
            AnswersList.Add(new Answer
            {
                Text = "What do you want?",
                UserInput = 1,
                ID = 0012001,
                InstanceID = 0012000,
            });
            AnswersList.Add(new Answer
            {
                Text = "Good, good. And you?",
                UserInput = 2,
                ID = 0012003,
                InstanceID = 0012000,
            });
            AnswersList.Add(new Answer
            {
                Text = "Go away, I'm busy.",
                UserInput = 3,
                ID = 0012001,
                InstanceID = 0012000,
            });
            AnswersList.Add(new Answer
            {
                Text = "I was fine until you showed up.",
                UserInput = 4,
                ID = 0012004,
                InstanceID = 0012000,
            });
            AnswersList.Add(new Answer
            {
                Text = "What do you mean?",
                UserInput = 1,
                ID = 0012005,
                InstanceID = 0012002,
            });
            AnswersList.Add(new Answer
            {
                Text = "Good, I was really getting sick of you.",
                UserInput = 2,
                ID = 0012006,
                InstanceID = 0012002,
            });
            AnswersList.Add(new Answer
            {
                Text = "Fuck off.",
                UserInput = 3,
                ID = 0012007,
                InstanceID = 0012002,
            });


            // ------------------------------------------------------- INSTANCE LIST -------------------------------------------------------
            InstanceList.Add(new Instance
            {
                Text = "Wait, what? Did she hear your chat with Ardent earlier? No, you made sure she wasn't around during the conversation. Maybe she just knows\n" +
                "\tshe might get kicked this time, you quickly reason to yourself.",
                ID = 0012002,
                AnswerID = 0012003,
            });
            InstanceList.Add(new Instance
            {
                Text = "Wait, what? Does she think she's getting voted out? You try to think what she meant by that.",
                ID = 0012002,
                AnswerID = 0012002,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"What, can't I just come and say hi?\" she asks you with an exaggerated amount of disappointment in her voice. \"No.\" you simply\n" +
                "\treply. \"Well I just thought you might want to chat with me before...\" she steps closer to you. \"the vote comes. I heard there's a high\n" +
                "\tchance that you might finally get rid of me.\" she says with a menacing grin.\n",
                ID = 0012002,
                AnswerID = 0012001,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 3
            });
            InstanceList.Add(new Instance
            {
                Text = "Feeling rather lazy today, you decide to just go back to your room.\n",
                ID = 0012000,
                AnswerID = 0012000,
            });
            InstanceList.Add(new Instance
            {
                Text = "On your way to the living quarters, you start thinking about what you're going to do at work. There was one terminal you noticed yesterday\n" +
                "\tthat had been disconnected from the Stable's network for a couple days that you didn't have time to check. After that-\n",
                ID = 0012000,
                AnswerID = 0012000,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hey Silver.\" a familiar voice comes from behind you. You turn to see the white coat and dark yellow mane of none other than Astral Mist\n" +
                "\therself. The Stable's very own jackass bully. Just the mere sight of her smug grin makes your blood boil.\n",
                ID = 0012000,
                AnswerID = 0012000,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"How's it going?\" she asks in a strangely neutral tone.",
                ID = 0012000,
                AnswerID = 0012000,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, in a hurry? Well, it was nice seeing you.\" he says and looks a bit disappointed. \"Yeah, see ya.\" you say and start walking out.\n" +
                "\tWhy exactly did you leave so early? You don't really know.",
                ID = 0011009,
                AnswerID = 0011004,
            });
            InstanceList.Add(new Instance
            {
                Text = "You finish the coffee with a last sip. \"Leaving already?\" dad asks. \"Yeah, the network won't keep itself running.\" you say jokingly,\n" +
                "\tand get up. \"I guess it won't. Well, it was nice seeing you.\" he says.\n",
                ID = 0011008,
                AnswerID = 0011015,
            });
            InstanceList.Add(new Instance
            {
                Text = "You say your goodbyes, and start to walk towards the door. As you're leaving, three ponies come in. You recognise them as the other\n" +
                "\tworkers from the armory. \"Hey Spade.\" one of them says to your dad. \"I see you have a guest here, hope she left some coffee for the rest\n" +
                "\tof us.\" says another with a smirk. \"There's plenty of it left, don't worry.\" says dad with a calm voice. You give a little \"Hmph!\",\n" +
                "\tand leave the armory.\n",
                ID = 0011008,
                AnswerID = 0011015,
            });
            InstanceList.Add(new Instance
            {
                Text = "He doesn't answer right away. He seems to think about something for a while. \"The same most usually feel about it. Feel bad for the one\n" +
                "\tyou voted, and be scared about being voted yourself.\" he says after a moment. \"I wouldn't worry about it too much.\" you comfort him. He\n" +
                "\tdoesn't say anything.",
                ID = 0011003,
                AnswerID = 0011016,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 9 }),
                RemoveAnswer = 0011016
            });
            InstanceList.Add(new Instance
            {
                Text = "\"A couple guys went to the general depot, there was something that they needed help with. The rest are probably still sleeping, they\n" +
                "\ttend to come late to work since there's so little to do.\" he says. \"Well that's not very fair.\" you answer. If you had any co-workers, you'd\n" +
                "\tmake sure they'd be at work on time. \"Oh, it doesn't really matter to me, especially on slow days like this. If they feel like they have\n" +
                "\tbetter things to do than to be here, I won't stop them.\" he says. To each their own, you guess.",
                ID = 0011003,
                AnswerID = 0011017,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 9 }),
                RemoveAnswer = 0011017
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I'm sorry, but I'm not allowed to give away firearms to anypony but the security.\" he says. \"Oh, right. Well, I think you should keep\n" +
                "\tit on display here, or give it to somepony you trust from the security.\" you suggest.\n",
                ID = 0011007,
                AnswerID = 0011013,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yeah, I'll think about that. But hey, the coffee looks like it's ready.\" he says. He gets up, and gives you a cup from the cabinet. You\n" +
                "\tpour some coffee for the both of you. You sit down, take a sip of the coffee, and relax. So good, you think to yourself.",
                ID = 0011003,
                AnswerID = 0011013,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I would, but I'm not allowed to take firearms from the armory.\" he says. \"Oh. Well in that case I would just put it on display in one\n" +
                "\tof the racks here. Or give it to somepony you trust from the security.\" you suggest to him.\n",
                ID = 0011007,
                AnswerID = 0011012,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yeah, I'll think about that. But hey, the coffee looks like it's ready.\" he says. He gets up, and gives you a cup from the cabinet. You\n" +
                "\tpour some coffee for the both of you. You sit down, take a sip of the coffee, and relax. So good, you think to yourself. It's not everyday you\n" +
                "\tget to enjoy coffee this strong, since the Stable has to ration out the coffee so it lasts as long as possible. Dad apparently has a friend who\n" +
                "\tworks at the food depot, so he brings some extra coffee packages here every now and then.",
                ID = 0011003,
                AnswerID = 0011012,
            });
            InstanceList.Add(new Instance
            {
                Text = "He looks at the engraving for a minute. \"Not sure. My guess is it refers to the voting.\" he says.",
                ID = 0011006,
                AnswerID = 0011014,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 9 }),
                RemoveAnswer = 0011014
            });
            InstanceList.Add(new Instance
            {
                Text = "He sighs. \"Silver... this pistol once belonged to your mother.\" he says. You stare at the pistol for a while. Dad barely ever talks about\n" +
                "\tyour mom. You only know that she was the head of security, and that she died of some disease when you were only a couple years old. It never\n" +
                "\treally bothered you, probably because you don't even remember how she looked like.\n",
                ID = 0011006,
                AnswerID = 0011010,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh.\" you say after some time. You don't really know how to feel about it. \"The gun was modified by the gunsmith here. I thought it was\n" +
                "\tlost after her stuff was returned here. But now I found it, and I'm not sure what to do with it.\" he says.",
                ID = 0011006,
                AnswerID = 0011010,
            });
            InstanceList.Add(new Instance
            {
                Text = "You enter the break room. The room is small-ish and simple. There is only two tables in the room, with eight or so seats for each of the\n" +
                "\ttables. The coffee is brewing on the counter. Dad is sitting at one of the tables and gestures you to come sit next to him. You wonder where\n" +
                "\teverypony else is.\n",
                ID = 0011005,
                AnswerID = 0011007,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hey dad, I found this in one of the shelves. It looked really out of place, is it supposed to be there?\" you ask dad and set the gun\n" +
                "\ton the table in front of him. He eyes the pistol for a second.\n",
                ID = 0011005,
                AnswerID = 0011007,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, I found it while I was moving some stuff around, I was thinking what I should do with it so I left it there until I figure out\n" +
                "\twhat to do with it.\" he says. You can see that he looks a bit uncomfortable looking at the pistol.",
                ID = 0011005,
                AnswerID = 0011007,
            });
            InstanceList.Add(new Instance
            {
                Text = "You take the pistol in your magic and examine it. The weapon is lot shinier than the other pistols here, and it has matte black accents\n" +
                "\ton it. The left side of the barrel has the words \"The verdict of acquital comes for all\" engraved on it. The engraving is very roughly made.\n" +
                "\t\"Huh, I wonder what that means.\" you say to yourself.\n",
                ID = 0011005,
                AnswerID = 0011005,
            });
            InstanceList.Add(new Instance
            {
                Text = "You decide to take the pistol and give it back to dad. You continue to look for the shelf to put the boxes in. You find it, and lift the\n" +
                "\tboxes to the top of the shelf where there is space. You really should've trained your magic more when you were a filly, because lifting heavy\n" +
                "\tthings really takes a toll on your magic, you think to yourself. You then start walking towards the break room, eager to get some actual coffee.\n",
                ID = 0011005,
                AnswerID = 0011005,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 2,
            });
            InstanceList.Add(new Instance
            {
                Text = "You decide not to touch the pistol. It's probably meant to be there, dad wouldn't leave things like that lying in places they don't\n" +
                "\tbelong, you reason to yourself, and continue to search for the correct shelf.\n",
                ID = 0011003,
                AnswerID = 0011006,
            });
            InstanceList.Add(new Instance
            {
                Text = "You find the shelf, and lift the boxes to the top of the shelf where there is space. You mentally curse yourself for not practicing your\n" +
                "\ttelekinesis more when you were a filly, because using your telekinesis on heavy objects really takes a toll on you. You then start walking\n" +
                "\ttowards the break room, eager to get some actual coffee.\n",
                ID = 0011003,
                AnswerID = 0011006,
            });
            InstanceList.Add(new Instance
            {
                Text = "You enter the break room. The room is small-ish and simple. There is only two tables in the room, with eight or so seats for each of the\n" +
                "\ttables. Dad is sitting at one of the tables. You wonder where everypony else is.\n",
                ID = 0011003,
                AnswerID = 0011006,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Found the shelf?\" he asks, and you nod. \"Good, the coffee looks like it's just about ready.\" he says, gets up, and gives you a cup\n" +
                "\tfrom the cabinet. You pour some coffee for the both of you, and sit down at one of the tables. Taking a sip from your coffee, you relax.\n" +
                "\tIt's not everyday you get to enjoy coffee this strong, since the Stable has to ration out the coffee so it lasts as long as possible.\n" +
                "\tDad apparently has a friend who works at the food depot, so he brings some extra coffee packages here every now and then.",
                ID = 0011003,
                AnswerID = 0011006,
            });
            InstanceList.Add(new Instance
            {
                Text = "He chuckles. \"Yes, I suppose it does taste a bit... bland.\" he says. He then glances at the boxes he was carrying. \"Could you put\n" +
                "\tthese on the shelf over there?\" he says and points to the back left corner of the room. \"Shelf G5, put them next to the other boxes with\n" +
                "\tthese numbers.\" he says as he points to the boxes' side where there is a long string of numbers.\n",
                ID = 0011002,
                AnswerID = 0011003,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, of course.\" you say. \"Awesome, come to the break room afterwards, I'll go put the coffee brewing.\" he says and leaves.\n",
                ID = 0011002,
                AnswerID = 0011003,
            });
            InstanceList.Add(new Instance
            {
                Text = "You take a hold of the boxes with your magic. They are somewhat heavy. But then again, magic and telekinesis never were your strenghts,\n" +
                "\tas you never really cared to get better at them. Even as a filly you much preferred playing games on terminals to practicing your magic. You\n" +
                "\tgo to the back corner of the room, and look for the correct shelf. You then notice a glint in one of the shelves. On one of the boxes, there's\n" +
                "\ta pistol. It looks very out of place in there.",
                ID = 0011002,
                AnswerID = 0011003,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"It's not so bad. And to be honest, I'd be bored if I had to be sitting on a terminal all day.\" he says jokingly. \"Hey, there's\n" +
                "\ta lot more to my job than sitting!\" you say defensively. Why do others think your job is so simple? There's a lot of responsibility\n" +
                "\tin keeping the network and terminals running, you think to yourself.\n",
                ID = 0011001,
                AnswerID = 0011002,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yes, yes, I know. Just teasing.\" he says with a smile. \"Anyway, would you like some coffee before you leave?\" he asks.",
                ID = 0011001,
                AnswerID = 0011002,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"A bit maybe. But I do prefer this to most other jobs here. It's relaxing, and there's no hurry.\" he explains. \"Fair enough.\"\n" +
                "\tyou answer. Some ponies just like simpler jobs, it seems.\n",
                ID = 0011001,
                AnswerID = 0011001,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Say, would you like some coffee before you go?\" he asks.",
                ID = 0011001,
                AnswerID = 0011001,
            });
            InstanceList.Add(new Instance
            {
                Text = "You decide to go visit your dad at the Stable's armory. His job is to keep count of all the supplies in there whenever the\n" +
                "\tStable's security comes to change their equipment or take them for maintenance. Which happens quite seldom, so he usually doesn't\n" +
                "\thave much to do in there.\n",
                ID = 0011000,
                AnswerID = 0011000,
            });
            InstanceList.Add(new Instance
            {
                Text = "You open the door to the armory. The room is large, and there are countless lockers and boxes against the walls of the room.\n" +
                "\tThere are also a large number of different weapons ranging from pistols to assault rifles on weapon racks and on the walls. Why there\n" +
                "\tare so many guns in the Stable, you never knew.\n",
                ID = 0011000,
                AnswerID = 0011000,
            });
            InstanceList.Add(new Instance
            {
                Text = "Your dad comes from around the corner carrying a few boxes. \"Silver!\" he says as he notices you. \"Hey dad.\" you answer,\n" +
                "\tand give him a hug. \"What brings you here?\" he asks. \"Just thought I'd come and see how you're doing before I go to work.\" you\n" +
                "\tanswer. You don't usually see each other that often, so it's easy to forget to catch up on things every once in a while.\n",
                ID = 0011000,
                AnswerID = 0011000,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Same soup, just reheated, heh. Decided to rearrange some stuff here to pass the time.\" he says and gestures to the boxes he's\n" +
                "\tcarrying. You can't even begin to think about how many times he must have done that by now.",
                ID = 0011000,
                AnswerID = 0011000,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, um... Don't worry, this'll be the last time.\" she says in a sorry voice. \"Good.\" you say, and leave the laundry. You\n" +
                "\tthen head to the IT-department.",
                ID = 0010008,
                AnswerID = 0010008,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Pfft! No need, I'll remember it this time. Thanks a lot!\" she says. At the end of the day, helping others is pretty nice\n" +
                "\teven though it might sometimes feel like a waste of time, you think to yourself.\n",
                ID = 0010007,
                AnswerID = 0010007,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"No problem.\" you say, and leave the laundry. You then head to the IT-department.",
                ID = 0010007,
                AnswerID = 0010007,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Will do, thanks a ton!\" she beams. At the end of the day, helping others is pretty nice even though it might sometimes feel\n" +
                "\tlike a waste of time, you think to yourself.\n",
                ID = 0010006,
                AnswerID = 0010006,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"No problem.\" you say, and leave the laundry. You then head to the IT-department.",
                ID = 0010006,
                AnswerID = 0010006,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"B-but I need to-\" she starts saying, but you're already on your way out. \"I have better things to do than be resetting\n" +
                "\tpasswords all the time.\" you think to yourself.",
                ID = 0010005,
                AnswerID = 0010005,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Awesome, thanks!\" she says. You go to the terminal, power it up to the boot options, select the Stable's network, and login\n" +
                "\tas admin. You then go to the Stable's terminal list, select the laundry's terminal, and select Stitch's user.\n",
                ID = 0010004,
                AnswerID = 0010004,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Alright, enter your new password.\" Stitch starts typing. Really. Slowly. Sweet Celestia it's infuriating. You know that the\n" +
                "\tonly reason you can type so fast is because you spend most of your day on a terminal. Well, being a unicorn helps too. Still, you\n" +
                "\tcan't but find it annoying.\n",
                ID = 0010004,
                AnswerID = 0010004,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"There!\" she chirps. You confirm the change, change the boot option back to normal, and restart the terminal. She then logs in.",
                ID = 0010004,
                AnswerID = 0010004,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I knoooow. But hey, I remembered the password from two days ago, isn't that great?!\" she says.\n",
                ID = 0010002,
                AnswerID = 0010002,
            });
            InstanceList.Add(new Instance
            {
                Text = "You let out a heavy sigh. \"Is it really that difficult to remember a password without writing it down?\" you ask, not expecting\n" +
                "\tanything sensible as a response. You believe it's good to teach some basic rules about terminals to the Stable, but sometimes it just\n" +
                "\tdoesn't seem to be worth the trouble.\n",
                ID = 0010002,
                AnswerID = 0010002,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I'll try really hard this time, I promise!\" she tries to assure you.",
                ID = 0010002,
                AnswerID = 0010002,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Sorry... Can you do it just once more? I'll remember it this time. Promise.\" she assures you. For some reason, you don't think\n" +
                "\tthis is the last time you'll be doing this.",
                ID = 0010002,
                AnswerID = 0010003,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Alrighty, you can come pick them up this afternoon!\" she says in a cheery voice. How somepony can be so happy doing laundry and\n" +
                "\tsewing ripped clothes is beyond you.\n",
                ID = 0010001,
                AnswerID = 0010001,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Sure thing, I'll come by here then.\" you say, and start to walk away, but Stitch calls back to you from the counter.\n",
                ID = 0010001,
                AnswerID = 0010001,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hey Silver, just one tiny thing... I think I may have forgotten the password on the laundry's terminal... again. And now it has\n" +
                "\tlocked me out of it.\" she says while trying to sound as innocent as possible. And failing quite badly at that.",
                ID = 0010001,
                AnswerID = 0010001,
            });
            InstanceList.Add(new Instance
            {
                Text = "You decide to go do your laundry. You go to your room to pick up your dirty clothes. \"Would be nice to get some different color\n" +
                "\tclothes once in a while...\" you think to yourself as you eye the blue and yellow shirts. You head to the laundry room. The bland smell\n" +
                "\tof laundry detergent hits your nose as you open the door and step inside.\n",
                ID = 0010000,
                AnswerID = 0010000,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hey there Silver!\" an overly enthusiastic voice comes from the yellow earth pony over the counter. Cross Stitch waves her\n" +
                "\thoof to you. She's nice, but a bit too outgoing for your taste.",
                ID = 0010000,
                AnswerID = 0010000,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Sure thing.\" says the stallion. You and Ardent chat for a while about some lighter things. Ardent finishes eating before you,\n" +
                "\tand leaves to work in the maintenance office. Once you finish, you notice that you still have about an hour before your work starts.\n" +
                "\tYou are the Stable's network specialist and software engineer. Your job is to make sure the Stable's network is running fine,\n" +
                "\tand to fix any software related problems with terminals. A lot of work for one pony, since barely anypony is interested about\n" +
                "\tthat kind of stuff in the Stable. But that doesn't bother you, because you enjoy your job.\n",
                ID = 36,
                AnswerID = 49,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hmm, now would be a good time to go do the laundry. Or maybe I should I go visit my dad and see how he's doing at the armory.\"\n" +
                "\tyou ponder to yourself.",
                ID = 36,
                AnswerID = 49,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"You really seem to have something against the Overmare. Which might be good. Listen, I've been thinking about a rebellion.\n" +
                "\tI thought you might want to join, so we could get rid of the vote.\" says the stallion in a low voice.",
                ID = 23,
                AnswerID = 53,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I was a bit worried about what you might think. Glad you think the same way I do.\" says the stallion in a relieved tone.\n",
                ID = 36,
                AnswerID = 39,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Of course. But it needs to be planned well. And we need more ponies to join it. I don't even want to think about what happens if\n" +
                "\tthe rebellion fails.\" you say, remembering some stories about past rebellions.\n",
                ID = 36,
                AnswerID = 39,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Don't worry, I've got this. I know ponies who are willing to join. But hey, let's talk about it more some other time, I want to\n" +
                "\teat right now.\" reassures the stallion, taking a bite of his sandwich.\n",
                ID = 36,
                AnswerID = 39,
            });
            InstanceList.Add(new Instance
            {
                Text = "You and Ardent chat for a while about some lighter things. Ardent finishes eating before you, and leaves to work in the maintenance\n" +
                "\toffice. Once you finish, you notice that you still have about an hour before your work starts. You are the Stable's network specialist\n" +
                "\tand software engineer. Your job is to make sure the Stable's network is running fine, and to fix any software related problems with\n" +
                "\tterminals. A lot of work for one pony, since barely anypony is interested about that kind of stuff in the Stable. But that doesn't bother\n" +
                "\tyou, because you enjoy your job.\n",
                ID = 36,
                AnswerID = 39,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hmm, now would be a good time to go do the laundry. Or maybe I should I go visit my dad and see how he's doing at the armory.\"\n" +
                "\tyou ponder to yourself.",
                ID = 36,
                AnswerID = 39,
            });
            /*InstanceList.Add(new Instance
            {
                Text = "\"How else are we going to put an end to this? This is the only way, and you know it.\" says the stallion.",
                ID = 38,
                AnswerID = 51,
            });*/
            InstanceList.Add(new Instance
            {
                Text = "\"That's besides the point. Where I *was* going with this, is that I think we should attempt a new rebellion against the vote.\n" +
                "\tsays the stallion in a low voice.",
                ID = 23,
                AnswerID = 50,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Well in that case you might wanna hear about what I've been planning. I think we should attempt a new rebellion. Would you be willing" +
                "\tto join it?\" asks the stallion in a low voice.",
                ID = 23,
                AnswerID = 47,
            });
            InstanceList.Add(new Instance
            {
                Text = "He looks at you for a while in disbelief, huffs, and continues to eat his breakfast.\n",
                ID = 36,
                AnswerID = 46,
            });
            InstanceList.Add(new Instance
            {
                Text = "Neither of you say a word to each other. Ardent finishes breakfast before you, and leaves to work in the maintenance office. \n " +
                "\tOnce you finish, you notice that you still have about an hour before your work starts. You are the Stable's network specialist\n" +
                "\tand software engineer. Your job is to make sure the Stable's network is running fine, and to fix any software related problems with\n" +
                "\tterminals. A lot of work for one pony, since barely anypony is interested about that kind of stuff in the Stable. But that doesn't bother\n" +
                "\tyou, because you enjoy your job.\n",
                ID = 36,
                AnswerID = 46,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hmm, now would be a good time to go do the laundry. Or maybe I should I go visit my dad and see how he's doing at the armory.\"\n" +
                "\tyou ponder to yourself.",
                ID = 36,
                AnswerID = 46,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Maybe because she's afraid of what Stable-Tec might do?\" says the stallion.",
                ID = 35,
                AnswerID = 45,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Nothing bad? Are you crazy? Ponies have lost loved ones because of it, and it has caused countless fights and other shitstorms!\"\n" +
                "\tsays the stallion, getting slightly frustrated.",
                ID = 34,
                AnswerID = 44,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Trust me, this something that needs to be done.\" says the stallion in a reassuring voice.",
                ID = 33,
                AnswerID = 42,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yes, I'm aware of that. But I'm ready to take the risk, if it means we can stop the voting.\" says the stallion.",
                ID = 32,
                AnswerID = 41,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Of course. We need to convince others to join us, and make sure the Overmare doesn't hear about this.\" says the stallion.",
                ID = 37,
                AnswerID = 54,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I- wait, really? You think so?\" asks the stallion, a bit surprised.",
                ID = 30,
                AnswerID = 38,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Well, that might be what's gonna happen if the Overmare doesn't comply.\" says the stallion.\n",
                ID = 29,
                AnswerID = 37,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Are you actually thinking about a rebellion? You know that's a really risky move, right?\" you ask, not sure\n" +
                "\tif Ardent is being serious.\n",
                ID = 29,
                AnswerID = 37,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yeah, I think it's about time we do something about this. I've been planning this for some time now, \n" +
                "\tand if it means we - and everypony else - have a chance to live without the fear of getting cast out of the Stable...\n" +
                "\tI'm ready to take take the risk.\" says the stallion.",
                ID = 29,
                AnswerID = 37,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"This is the only way we can get rid of the vote, and you know it.\" he says in a firm voice.",
                ID = 28,
                AnswerID = 36,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"What, should we just wait and see what happens when everypony realizes they don't want to kick anypony out? Things are gonna\n" +
                "\tget a lot messier then.\" says the stallion.",
                ID = 28,
                AnswerID = 35,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I'm... just gonna assume that was a joke. Anyway, my point was, I think we should attempt a new rebellion.\"\n" +
                "\tsays the stallion with a low voice.",
                ID = 23,
                AnswerID = 34,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Ugh, sometimes I just don't understand you... Sure, the voting sucks and these hallways are pretty small, but at least we have\n" +
                "\tfood, water and a place to sleep. Who knows what lies on the other side of the Stable's door. For all we know, we might be the last \n" +
                "\tof our species!\" says the stallion with a hint of anger in his voice.",
                ID = 27,
                AnswerID = 33,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"That's what I thought. Anyway, my point was, because I think there's a fair number of ponies now who don't want the vote,\n" +
                "\twe should attempt a new rebellion.\" says the stallion in a low voice.",
                ID = 23,
                AnswerID = 32,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hm. I was hoping you'd be against the voting a bit more.\" says the stallion with a hint of disappointment in his voice.",
                ID = 26,
                AnswerID = 31,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Wow. I didn't know you cared so little for others.\" says the stallion in a disappointed voice.",
                ID = 25,
                AnswerID = 30,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Because I think it's time we attempted a new rebellion.\" says the stallion in a low voice.",
                ID = 23,
                AnswerID = 29,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"You what?! Since when have you been that inconsiderate towards other ponies?\" says the stallion with a surprised voice.",
                ID = 24,
                AnswerID = 28,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"So, I was thinking... Maybe it's time for a new rebellion against the voting.\" whispers the stallion.",
                ID = 23,
                AnswerID = 27,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Who do you think is getting kicked out next year? Is there anypony else than Astral you think is dangerous or annoying?\"\n" +
                "\tasks the stallion.",
                ID = 17,
                AnswerID = 26,
            });
            InstanceList.Add(new Instance
            {
                Text = "[he sighs] \"I just find this whole voting useless. Who do you think is going to get kicked next year? Is there anypony else than\n" +
                "\tAstral you think is dangerous or annoying?\" asks the stallion.",
                ID = 17,
                AnswerID = 25,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Don't you think it's a bit harsh to kick somepony out of here just because others think they're annoying? Who knows what\n" +
                "\tlies on the other side of the Stable's door.\" says the stallion.",
                ID = 22,
                AnswerID = 24,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"No, I think that's what everypony should be thinking right now.\" says the stallion in a firm voice.",
                ID = 21,
                AnswerID = 23,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Exactly. That's what I'm worried about. Will we start kicking random ponies out?\" says the stallion.",
                ID = 20,
                AnswerID = 22,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Maybe not this year. Listen, have you thought about what will happen in next year's voting?\" asks the stallion.",
                ID = 19,
                AnswerID = 21,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"YES! How can you not remember that?!\" yells the stallion in an uneasy voice. A few other ponies glance at your direction.",
                ID = 18,
                AnswerID = 20,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"And after that? What will happen in next year's voting?\" asks the stallion in a pressing voice.",
                ID = 17,
                AnswerID = 19,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"That's just it. All the ponies from that rebellion from 12 years ago have been kicked out already. What will happen in next \n" +
                "\tyear's voting? Is there anypony else than Astral who you think is annoying or dangerous?\" asks the stallion in a pressing voice.",
                ID = 16,
                AnswerID = 18,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Heh, I guess not. But jokes aside... I've been thinking about The Pariah Vote.\" Ardent says.\n",
                ID = 13,
                AnswerID = 40,
            });
            InstanceList.Add(new Instance
            {
                Text = "The Pariah Vote. It's an event that happens once per year. The whole Stable gathers together and everypony gets to vote for the \n" +
                "\tpony who they think is the most dangerous for the wellbeing of the Stable. The pony who gets the most votes will then be kicked out \n" +
                "\tof the Stable. You never put much thought into this, as you have always just voted for the pony who you thought was the most annoying \n" +
                "\tat the time. You have also been lucky enough to not lose any close relatives because of the vote.",
                ID = 13,
                AnswerID = 40,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Well, it's just that The Pariah Vote is just around the corner.\" says the stallion.\n",
                ID = 13,
                AnswerID = 17,
            });
            InstanceList.Add(new Instance
            {
                Text = "The Pariah Vote. It's an event that happens once per year. The whole Stable gathers together and everypony gets to vote for the \n" +
                "\tpony who they think is the most dangerous for the wellbeing of the Stable. The pony who gets the most votes will then be kicked out \n" +
                "\tof the Stable. You never put much thought into this, as you have always just voted for the pony who you thought was the most annoying \n" +
                "\tat the time. You have also been lucky enough to not lose any close relatives because of the vote.",
                ID = 13,
                AnswerID = 17,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hm? Oh sorry, I was just lost in my thoughts.\" says the stallion.",
                ID = 15,
                AnswerID = 16,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"What? Oh, don't worry. We've been friends for our whole lives, I know you are a kind pony.\" says the stallion with a warm smile.",
                ID = 14,
                AnswerID = 15,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, sorry. It's just that the voting for the next Pariah is just around the corner.\" says the stallion.\n",
                ID = 13,
                AnswerID = 14,
            });
            InstanceList.Add(new Instance
            {
                Text = "The Pariah Vote. It's an event that happens once per year. The whole Stable gathers together and everypony gets to vote for the \n" +
                "\tpony who they think is the most dangerous for the wellbeing of the Stable. The pony who gets the most votes will then be kicked out \n" +
                "\tof the Stable. You never put much thought into this, as you have always just voted for the pony who you thought was the most annoying \n" +
                "\tat the time. You have also been lucky enough to not lose any close relatives because of the vote.",
                ID = 13,
                AnswerID = 14,
            });
            InstanceList.Add(new Instance
            {
                Text = "[she relaxes a bit] \"Oh, okay. But mom said that I should avoid you and Astral.\" says the filly.",
                ID = 1,
                AnswerID = 1,
            });
            InstanceList.Add(new Instance
            {
                Text = "[she jumps in the air, let's out a high pitched scream and runs away]",
                ID = 2,
                AnswerID = 2,
                HasSpecialFunction = true,
                RepPoints = -1,
                SpecialFunction = new List<int>(new int[] { 5 }),
                Faction = "Stable 54"
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Um... You're not going to hurt me, are you? Or anypony else?\" asks the filly cautiously.",
                ID = 3,
                AnswerID = 0,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Aw, but I like Astral!\" says the filly.",
                ID = 4,
                AnswerID = 3,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Overproct... What does that mean?\" asks the filly.",
                ID = 5,
                AnswerID = 4,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"What do you mean?\" asks the filly.",
                ID = 6,
                AnswerID = 5,
            });
            InstanceList.Add(new Instance
            {
                Text = "You make your way to the cafeteria. Once you get there, you spot your friend's, Ardent's, light gray coat and bright red mane \n" +
                "\tfrom the crowd. You take a sandwich and some water from the counter, and go sit next to Ardent. You notice that he has \n" +
                "\ta worried expression on his face.",
                ID = 7,
                AnswerID = 6,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, um, okay... Well, I've gotta go now, so...\" stammers the filly.",
                ID = 8,
                AnswerID = 7,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Haha, good joke... It was a joke, right?\" asks the filly warily.",
                ID = 9,
                AnswerID = 8,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hmm. Well, I'm gonna go. Seeya!\" says the filly.",
                ID = 10,
                AnswerID = 9,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, okay. Well, I've gotta go. Seeya!\" says the filly.",
                ID = 10,
                AnswerID = 10,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, umm... Well, I've gotta go now. Uh, seeya!\" says the filly.",
                ID = 10,
                AnswerID = 11,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Good morning.\" answers the stallion.",
                ID = 11,
                AnswerID = 12,
            });
            InstanceList.Add(new Instance
            {
                Text = "You both eat for a while without saying anything.",
                ID = 12,
                AnswerID = 13,
            });

        }


        public static void GetMap() {

            Console.WriteLine("	  \\__					    ^ Crystal Mountains   	    				       '-._ \n" +
                "	     '-_							    		    			           \\\n" +
                "      		\\							    	    				       ____/\n" +
                "  North		 \\						    	    			              /\n" +
                "    Luna     /-. |  * Vanhoover		    	    						       ___    |\n" +
                "      Ocean  | | |    					    	    		       Manehattan *   /   \\___/\n" +
                "             |/  |					    		        	         .---'  .__  \n" +
                "		 /							    	                 |      \\_ \\\n" +
                "	 _______/						    		    	         \\__	  ''\n" +
                "--._____/							    		    	            ''--___\n" +
                "						Canterlot *		    	    		            \\\n" +
                "									    Fillydelphia *     	            |\n" +
                "					    Ponyville *	    			        	       	_--''\n" +
                "						           # Everfree Forest           	               |\n" +
                "						  		            			    	\\\n" +
                "								       	    	        Baltimare * 	 ''--_\n" +
                "	              _____					        				    __    \\\n" +
                "		     /     \\									    _-''  '''\\_\\	Celestial\n" +
                "		 __-'       \\ * Las Pegasus				* ###########		   |		             Sea\n" +
                "       --.___---'            \\_				 * Appleloosa	  N3W APPL3L00SA	    \\_\n" +
                "\\     /                        '-.								      '-.___\n" +
                " \\___/				  |									    __ /\n" +
                "				 /			^ Macintosh Hills				    \\ \\| \n" +
                "		      .-'\\	/									    | \n" +
                "		      |   \\    /									    \\_\n" +
                "		     /     \\__/										  '-.\n" +
                "		    /											        |\n" +
                "		    \\											        |\n" +
                "     South	     \\_											        |_\n" +
                "	Luna	       '-.__											      \\\n" +
                "	   Ocean	    \\											       \\\n" +
                "			     \\___										        \\\n" +
                "	   	 		 '''--._									         |\n" +
                "	      				\\									         |\n");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

        }

    }

}
