using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ConsoleApp2
{

    public class Answer {

        public string Text { get; set; }
        public int UserInput { get; set; }
        public int ID { get; set; }
        public int InstanceID { get; set; }
        public bool HasSpecialFunction { get; set; }
        public List<int> SpecialFunction { get; set; } // set to 1 or more to execute additional code. use with the functions below to use their code
        public int RepCheck { get; set; } // SpecialFunction: 1. set to the amount of rep points the player needs to have to pass the check.
        public string FactionName { get; set; } // SpecialFunction: 1. set to the faction's name you want to check the rep points. has to be used with RepCheck!
        public int QuestID { get; set; } // no SpecialFunction required. set to the ID of a quest to mark as completed

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
        public int QuestCheck { get; set; } // no SpecialFunction required. set to the ID of a quest to check if it has been completed.

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

            string Name;
            int PassInstanceID = 0;
            int CurrentWeaponID = 0;
            int PassAnswerID = 0;
            GameState = 0;

            // var SoundPlayer = new System.Media.SoundPlayer();
            // SoundPlayer.SoundLocation = Environment.CurrentDirectory + "\\file-name.wav";
            // SoundPlayer.Load();
            Console.SetWindowSize(160, 40);
            Console.WriteLine("\t               ----------------------------------\n\r" +
                "\t            -------______________________-----------\n\r" +
                "\t         ----------\\	 		 \\-------------\n\r" +
                "\t      --------------\\  Fallout Equestria  \\---------------\n\r" +
                "\t      ---------------\\	     Choices	   \\--------------\n\r" +
                "\t         -------------\\_____________________\\----------\n\r" +
                "\t            -----------		      	     -------\n\r" +
                "\t               ----------------------------------");

            Console.WriteLine("\tDone.");
            Console.WriteLine("\tWelcome to Fallout Equestria: Choices. Before you start the game, please resize the console window so that this line of text fits on one line.");
            Console.WriteLine("\tIt's also recommended that you raise the font size a bit, so that the text is easier to read.");
            Console.WriteLine("\tRight click on the top part of the console window and choose \"Properties\". Click on the \"Font\" tab. Font size of 20 should be ok.");
            Console.WriteLine("\tPress Enter to start.");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            Console.Clear();
            Console.WriteLine("\tPlease type the name you want your character to be referred as in the game:");

            Console.Write("\t");
            Name = Console.ReadLine();
            Console.Clear();

            Console.WriteLine("\tOnce upon a time, in the magical land of Equestria... A war broke out against the Zebra Empire, \n\r" +
                "\tas a consequence to multiple trade sanctions involving natural resources. While the Equestrian Nation had a monopoly \n\r" +
                "\ton magical gemstones, the Zebra Empire had a monopoly on coal, which was needed to keep the Equestrian Nation running. \n\r" +
                "\tBecause of the war, Equestria was forced into an industrial revolution, as an attempt to outdo the Zebra Empire in wartime technology. \n\r" +
                "\tPart of this was Stable-Tec, a corporation specialized in arcane science. One of their most known creations were the Stables. \n\r" +
                "\tThey were large fallout shelters, built all around Equestria in case of a megaspell holocaust. Your ancestors were selected \n\r" +
                "\tto become inhabitants of Stable 54. Unfortunately, not all Stables were built just for protection. \n\r" +
                "\tAs neither side was able to make the other side surrender, the bombs eventually fell and engulfed the earth in fire and radiation, \n\r" +
                "\tsweeping it almost clean of life. \n\r" +
                "\n\r" +
                "\tStable 54 has now been functioning for nearly 200 years. You have been born and raised in the Stable. This is where your story starts.");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            Console.WriteLine();
            Console.WriteLine("\tYou wake up in your bed. You get up, and notice that your chest still hurts from yesterday's fight.\n\r" +
                "\tYou brush your mane, put on your Stable suit, and head to the cafeteria to get some breakfast.\n\r" +
                "\tOn your way there you run into Light Harmony, a young unicorn filly with white coat and dark blue mane.\n\r" +
                "\tAs she notices you, she tenses up, and looks a bit worried.");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            Console.WriteLine();
            Console.WriteLine("\t\"Oh, hey there " + Name + "... Uhh, I was just about to leave...\" says the filly.");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

            while (GameState == 0) {

                PassAnswerID = GetAnswers(PassInstanceID, Name, CurrentWeaponID);

                PassInstanceID = GetInstance(PassAnswerID, Name, CurrentWeaponID);

            }

        }

        public static dynamic GetAnswers(int PassInstanceID, string Name, int WeaponID) {

            string PlayerName = Name;

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

            var QAnswerEnum = SuccessfulQuestAnswer.OfType<SuccessfulAnswer>();
            var CurrentQAnswers =
                from qanswer in QAnswerEnum
                where qanswer.InstanceID == PassInstanceID
                select qanswer;
            Console.WriteLine();
            foreach (var answer in CurrentAnswers)
            {
                Console.WriteLine("\t{0}" + "{1}", answer.UserInput, answer.Text);

                // if there's a quest and it has been completed, print out an additional answer
                if (QuestList[answer.QuestID].Completed == true)
                {
                    foreach (var qanswer in CurrentQAnswers)
                    {
                        Console.WriteLine("\t{0}" + "{1}", qanswer.UserInput, qanswer.Text);
                    }
                }
                
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
                Console.WriteLine("Unacceptable answer. Please type a number that is a valid answer.");
                return GetAnswers(PassInstanceID, PlayerName, CurrentWeaponID);
            }

            var QueryAnswerID =
                from answer in AnswerEnum
                where answer.UserInput == ParsedAnswer && answer.InstanceID == PassInstanceID
                select answer;

            var CurrentQAnswersID =
                from qanswer in QAnswerEnum
                where qanswer.InstanceID == PassInstanceID
                select qanswer;

            // check if the now parsed answer corresponds with the possible answers given
            foreach (var answer in QueryAnswerID)
            {
                if (ParsedAnswer == answer.UserInput)
                {
                    AcceptedAnswer = ParsedAnswer;
                    AnsWithinLimits = true;
                }
            }

            foreach (var qanswer in CurrentQAnswersID)
            {
                if (ParsedAnswer == qanswer.UserInput)
                {
                    AcceptedAnswer = ParsedAnswer;
                    AnsWithinLimits = true;
                    AnsSecondary = true;
                }
            }

            // check if the foreach was skipped
            if (AnsWithinLimits == false)
            {
                Console.WriteLine("Unacceptable answer. Please type a number that is a valid answer.");
                return GetAnswers(PassInstanceID, PlayerName, CurrentWeaponID);
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

            CurrentQAnswersID =
                from qanswer in QAnswerEnum
                where qanswer.InstanceID == PassInstanceID
                select qanswer;

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
            else {

                foreach (var qanswer in CurrentQAnswersID) {
                    PassAnswerID = qanswer.ID;

                    AnsSecondary = false;

                }
            }

            return PassAnswerID;

        }

        public static dynamic GetInstance(int PassAnswerID, string Name, int WeaponID) {

            int CurrentWeaponID = WeaponID;
            string CurrentWeaponName = WeaponName(CurrentWeaponID, 0);

            string PlayerName = Name;
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
                                GetInstance(Answered, PlayerName, CurrentWeaponID);
                            }
                            else if (WeaponDamage < instance.DamageCheck) // does not do enough damage
                            {
                                Answered = instance.AnswerID + 1;
                                GetInstance(Answered, PlayerName, CurrentWeaponID);
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
                                GetInstance(Answered, PlayerName, CurrentWeaponID);
                            }
                            else if (ArmorClass < instance.ArmorCheck) // armor does not protect
                            {
                                Answered = instance.AnswerID + 1;
                                GetInstance(Answered, PlayerName, CurrentWeaponID);
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
                Text = ") Great. Even the kids shun me now.",
                UserInput = 1,
                ID = 0,
                InstanceID = 0
            });
            AnswersList.Add(new Answer
            {
                Text = ") Hey now, no need to be scared. I won't hurt anypony.",
                UserInput = 2,
                ID = 1,
                InstanceID = 0
            });
            AnswersList.Add(new Answer
            {
                Text = ") [jump in front of her] BOO!",
                UserInput = 3,
                ID = 2,
                InstanceID = 0
            });
            AnswersList.Add(new Answer
            {
                Text = ") Yeah, you should be avoiding Astral, she's setting a bad example for you and other kids. But not me.",
                UserInput = 1,
                ID = 3,
                InstanceID = 1,
            });
            AnswersList.Add(new Answer
            {
                Text = ") Oh please, your mom is overprotective. Don't let her tell you how to live your life.",
                UserInput = 2,
                ID = 4,
                InstanceID = 1,
            });
            AnswersList.Add(new Answer
            {
                Text = ") I can see why.",
                UserInput = 3,
                ID = 5,
                InstanceID = 1,
            });
            AnswersList.Add(new Answer
            {
                Text = ") Stupid kids.",
                UserInput = 1,
                ID = 6,
                InstanceID = 2
            });
            AnswersList.Add(new Answer
            {
                Text = ") Sorry, didn't mean to scare you THAT badly!",
                UserInput = 2,
                ID = 6,
                InstanceID = 2
            });
            AnswersList.Add(new Answer
            {
                Text = ") Of course not. Things just got a little out of hoof yesterday.",
                UserInput = 1,
                ID = 1,
                InstanceID = 3,
            });
            AnswersList.Add(new Answer
            {
                Text = ") We'll see.",
                UserInput = 2,
                ID = 7,
                InstanceID = 3
            });
            AnswersList.Add(new Answer
            {
                Text = ") No. Well, maybe Astral if she keeps being that annoying, but nopony else.",
                UserInput = 3,
                ID = 8,
                InstanceID = 3
            });
            AnswersList.Add(new Answer
            {
                Text = ") *sigh* I know, but still.",
                UserInput = 1,
                ID = 9,
                InstanceID = 4
            });
            AnswersList.Add(new Answer
            {
                Text = ") Well, you shouldn't.",
                UserInput = 2,
                ID = 9,
                InstanceID = 4
            });
            AnswersList.Add(new Answer
            {
                Text = ") It means that your mom won't let you do what you want.",
                UserInput = 1,
                ID = 10,
                InstanceID = 5
            });
            AnswersList.Add(new Answer
            {
                Text = ") It means that your mom doesn't want you to get hurt.",
                UserInput = 2,
                ID = 10,
                InstanceID = 5,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 0
            });
            AnswersList.Add(new Answer
            {
                Text = ") Uh, nevermind.",
                UserInput = 3,
                ID = 10,
                InstanceID = 5
            });
            AnswersList.Add(new Answer
            {
                Text = ") Oh, nevermind.",
                UserInput = 1,
                ID = 10,
                InstanceID = 6
            });
            AnswersList.Add(new Answer
            {
                Text = ") Your mom just wants you to be safe, which is understandable.",
                UserInput = 2,
                ID = 10,
                InstanceID = 6,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 6 }),
                QuestID = 0
            });
            AnswersList.Add(new Answer
            {
                Text = ") That's just how moms are.",
                UserInput = 3,
                ID = 10,
                InstanceID = 6
            });
            AnswersList.Add(new Answer
            {
                Text = ") Yeah, whatever.",
                UserInput = 1,
                ID = 6,
                InstanceID = 8
            });
            AnswersList.Add(new Answer
            {
                Text = ") Yeah, it was.",
                UserInput = 1,
                ID = 10,
                InstanceID = 9
            });
            AnswersList.Add(new Answer
            {
                Text = ") No, really, if she doesn't stop being a prick, there will be consequences.",
                UserInput = 2,
                ID = 11,
                InstanceID = 9
            });
            AnswersList.Add(new Answer
            {
                Text = ") I'll leave that up to you to decide.",
                UserInput = 3,
                ID = 11,
                InstanceID = 9
            });
            AnswersList.Add(new Answer
            {
                Text = ") Later.",
                UserInput = 1,
                ID = 6,
                InstanceID = 10
            });
            AnswersList.Add(new Answer
            {
                Text = ") Morning.",
                UserInput = 1,
                ID = 12,
                InstanceID = 7
            });
            AnswersList.Add(new Answer
            {
                Text = ") [say nothing, start eating your breakfast]",
                UserInput = 2,
                ID = 13,
                InstanceID = 7
            });
            AnswersList.Add(new Answer
            {
                Text = ") Something bothering you? You look a bit stressed.",
                UserInput = 1,
                ID = 14,
                InstanceID = 11
            });
            AnswersList.Add(new Answer
            {
                Text = ") Don't tell me you too are scared of me because of yesterday.",
                UserInput = 2,
                ID = 15,
                InstanceID = 11
            });
            AnswersList.Add(new Answer
            {
                Text = ") You know, a \"Good morning\" would have been a nice thing to say.",
                UserInput = 1,
                ID = 16,
                InstanceID = 12
            });
            AnswersList.Add(new Answer
            {
                Text = ") Something bothering you?",
                UserInput = 2,
                ID = 14,
                InstanceID = 12
            });
            AnswersList.Add(new Answer
            {
                Text = ") You know I'm bad with social skills, so if you could just tell me what's bothering you, that would be nice.",
                UserInput = 3,
                ID = 14,
                InstanceID = 12
            });
            AnswersList.Add(new Answer
            {
                Text = ") Whatcha thinking about?",
                UserInput = 1,
                ID = 17,
                InstanceID = 15
            });
            AnswersList.Add(new Answer
            {
                Text = ") [give him a warm smile] Thinking is not for you.",
                UserInput = 2,
                ID = 40,
                InstanceID = 15
            });
            AnswersList.Add(new Answer
            {
                Text = ") Care to tell me? You're usually not this gloomy.",
                UserInput = 3,
                ID = 17,
                InstanceID = 15
            });
            AnswersList.Add(new Answer
            {
                Text = ") [return the smile] Thanks, Ardent. So, what's on your mind then?",
                UserInput = 1,
                ID = 17,
                InstanceID = 14
            });
            AnswersList.Add(new Answer
            {
                Text = ") Oh you, stop being so sweet. Well, if it's not that, then what is it?",
                UserInput = 2,
                ID = 17,
                InstanceID = 14
            });
            AnswersList.Add(new Answer
            {
                Text = ") So what? It happens every year, and I'm pretty sure none of our friends or relatives are gonna get thrown out.",
                UserInput = 1,
                ID = 18,
                InstanceID = 13
            });
            AnswersList.Add(new Answer
            {
                Text = ") You should be glad, I think Astral is going to get kicked out next.",
                UserInput = 2,
                ID = 19,
                InstanceID = 13
            });
            AnswersList.Add(new Answer
            {
                Text = ") Wait, it's time for that again? Time sure does fly by.",
                UserInput = 3,
                ID = 20,
                InstanceID = 13
            });
            AnswersList.Add(new Answer
            {
                Text = ") Oh. Well, it's gonna have to happen, there's nothing we can do about that.",
                UserInput = 4,
                ID = 21,
                InstanceID = 13
            });
            AnswersList.Add(new Answer
            {
                Text = ") Well, no-",
                UserInput = 1,
                ID = 22,
                InstanceID = 16
            });
            AnswersList.Add(new Answer
            {
                Text = ") Hmm. Now that you mention it, the whole Stable is getting along just fine.",
                UserInput = 2,
                ID = 22,
                InstanceID = 16
            });
            AnswersList.Add(new Answer
            {
                Text = ") That's next year's problem, you shouldn't be stressing about that.",
                UserInput = 3,
                ID = 23,
                InstanceID = 16
            });
            AnswersList.Add(new Answer
            {
                Text = ") I... Hadn't thought about that.",
                UserInput = 1,
                ID = 22,
                InstanceID = 17
            });
            AnswersList.Add(new Answer
            {
                Text = ") You mean there's no more annoying ponies here? There's always somepony who annoys others.",
                UserInput = 2,
                ID = 24,
                InstanceID = 17
            });
            AnswersList.Add(new Answer
            {
                Text = ") Geez, calm down. I was just joking.",
                UserInput = 1,
                ID = 25,
                InstanceID = 18
            });
            AnswersList.Add(new Answer
            {
                Text = ") Uh, sorry. So, why are you stressed about that? It happens every year, and you're usually not that worried about it.",
                UserInput = 2,
                ID = 25,
                InstanceID = 18
            });
            AnswersList.Add(new Answer
            {
                Text = ") No, I haven't. Why you ask?",
                UserInput = 1,
                ID = 26,
                InstanceID = 19
            });
            AnswersList.Add(new Answer
            {
                Text = ") No, I like to live one year at a time.",
                UserInput = 2,
                ID = 26,
                InstanceID = 19
            });
            AnswersList.Add(new Answer
            {
                Text = ") I hope not.",
                UserInput = 1,
                ID = 27,
                InstanceID = 20
            });
            AnswersList.Add(new Answer
            {
                Text = ") Don't know. Nor do I care, really.",
                UserInput = 2,
                ID = 28,
                InstanceID = 20
            });
            AnswersList.Add(new Answer
            {
                Text = ") Why? It will happen every year, we can't change that.",
                UserInput = 1,
                ID = 29,
                InstanceID = 21
            });
            AnswersList.Add(new Answer
            {
                Text = ") Look, I couldn't care less for the voting. This year's OR the next.",
                UserInput = 2,
                ID = 28,
                InstanceID = 21
            });
            AnswersList.Add(new Answer
            {
                Text = ") No, not really.",
                UserInput = 1,
                ID = 30,
                InstanceID = 22
            });
            AnswersList.Add(new Answer
            {
                Text = ") If they are annoying and are aware of it, no.",
                UserInput = 2,
                ID = 31,
                InstanceID = 22
            });
            AnswersList.Add(new Answer
            {
                Text = ") Well, when you put it that way... It kinda is.",
                UserInput = 3,
                ID = 32,
                InstanceID = 22
            });
            AnswersList.Add(new Answer
            {
                Text = ") Oh, are you kidding me? I would be glad to get out of this Celestia-forsaken hole.",
                UserInput = 4,
                ID = 33,
                InstanceID = 22
            });
            AnswersList.Add(new Answer
            {
                Text = ") Oh, are you kidding me? I would be glad to get to explore the outside world!",
                UserInput = 5,
                ID = 34,
                InstanceID = 22
            });
            AnswersList.Add(new Answer
            {
                Text = ") What?! Are you crazy?",
                UserInput = 1,
                ID = 35,
                InstanceID = 23
            });
            AnswersList.Add(new Answer
            {
                Text = ") I have a better idea. Let's NOT do anything that's dangerous for the entire Stable.",
                UserInput = 2,
                ID = 36,
                InstanceID = 23
            });
            AnswersList.Add(new Answer
            {
                Text = ") Might as well overthrow the Overmare and tell Stable-Tec to go fuck themselves while we're at it.",
                UserInput = 3,
                ID = 37,
                InstanceID = 23
            });
            AnswersList.Add(new Answer
            {
                Text = ") Hmm. That might not be a bad idea.",
                UserInput = 4,
                ID = 38,
                InstanceID = 23
            });
            AnswersList.Add(new Answer
            {
                Text = ") Yeah, I do. But we need to plan this well, I don't want this to turn out like the previous one.",
                UserInput = 1,
                ID = 39,
                InstanceID = 31
            });
            AnswersList.Add(new Answer
            {
                Text = ") I said *might*. You know this is extremely dangerous for ourselves, AND the Stable, right?",
                UserInput = 2,
                ID = 41,
                InstanceID = 31
            });
            AnswersList.Add(new Answer
            {
                Text = ") Actually... Gah, I don't know! I'm not used to making decisions this important!",
                UserInput = 3,
                ID = 42,
                InstanceID = 31
            });
            AnswersList.Add(new Answer
            {
                Text = ") I... *sigh* I don't know. Sorry, I guess I'm just a little tired.",
                UserInput = 1,
                ID = 32,
                InstanceID = 24
            });
            AnswersList.Add(new Answer
            {
                Text = ") None of your business.",
                UserInput = 2,
                ID = 43,
                InstanceID = 24
            });
            AnswersList.Add(new Answer
            {
                Text = ") No, not other ponies... I just... don't care about the vote. It happens only once a year, and nothing bad has\n\r" +
                "ever happened because of the vote.",
                UserInput = 1,
                ID = 44,
                InstanceID = 25
            });
            AnswersList.Add(new Answer
            {
                Text = ") You think I'M the indifferent one here? What about the Overmare? Why do you think she hasn't made any objections\n\r" +
                "against the vote? Seems like as long as she can't be voted out, everything's just fine.",
                UserInput = 2,
                ID = 45,
                InstanceID = 25
            });
            AnswersList.Add(new Answer
            {
                Text = ") Well now you know.",
                UserInput = 3,
                ID = 46,
                InstanceID = 25
            });
            AnswersList.Add(new Answer
            {
                Text = ") Don't get me wrong, of course I would be happy to get rid of the vote.",
                UserInput = 1,
                ID = 47,
                InstanceID = 26
            });
            AnswersList.Add(new Answer
            {
                Text = ") The less there are annoying ponies, the better the Stable will be, right?",
                UserInput = 2,
                ID = 48,
                InstanceID = 26
            });
            AnswersList.Add(new Answer
            {
                Text = ") Hm, fair points.",
                UserInput = 1,
                ID = 49,
                InstanceID = 27
            });
            AnswersList.Add(new Answer
            {
                Text = ") Surely there must be at least some life outside? I doubt the megaspells were able to wipe out literally everything.",
                UserInput = 2,
                ID = 50,
                InstanceID = 27
            });
            AnswersList.Add(new Answer
            {
                Text = ") I don't care. If you think I'm going to go against the Overmare, you're out of your mind.",
                UserInput = 1,
                ID = 51,
                InstanceID = 28
            });
            AnswersList.Add(new Answer
            {
                Text = ") I see where you're coming from, but... Don't you think a rebellion is a bit overkill?",
                UserInput = 2,
                ID = 52,
                InstanceID = 28
            });
            AnswersList.Add(new Answer
            {
                Text = ") Hm, I guess you're right.",
                UserInput = 3,
                ID = 53,
                InstanceID = 28
            });
            AnswersList.Add(new Answer
            {
                Text = ") Wait, you're actually thinking about a rebellion? Count me in!",
                UserInput = 1,
                ID = 54,
                InstanceID = 29,
            });
            AnswersList.Add(new Answer
            {
                Text = ") Wait, you're actually thinking about a rebellion? I want nothing to do with it.",
                UserInput = 2,
                ID = 55,
                InstanceID = 29
            });



            // ------------------------------------------------------- INSTANCE LIST -------------------------------------------------------
            InstanceList.Add(new Instance
            {
                Text = "Neither of you say a word to each other. Ardent finishes breakfast before you, and leaves to work in the maintenance office. \n\r " +
                "Once you finish, you notice that you still have ",
                ID = 37,
                AnswerID = 46,
            });
            InstanceList.Add(new Instance
            {
                Text = "[He looks at you for a while in disbelief, huffs, and continues to eat his breakfast]",
                ID = 37,
                AnswerID = 46,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Maybe because she's afraid of what Stable-Tec might do?\" says the stallion.",
                ID = 36,
                AnswerID = 45,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Nothing bad? Are you crazy? Ponies have lost loved ones because of it, and it has caused countless fights and other shitstorms!\"" +
                "says the stallion, getting slightly frustrated.",
                ID = 35,
                AnswerID = 44,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Is that so? Well suit yourself then.\" says the stallion, a bit hurt.",
                ID = 34,
                AnswerID = 43,
            });
            InstanceList.Add(new Instance
            {
                Text = "You eat the rest of your breakfast without saying a word to each other.",
                ID = 34,
                AnswerID = 43,
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
                ID = 31,
                AnswerID = 39,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I- wait, really? You think so?\" asks the stallion, a bit surprised.",
                ID = 30,
                AnswerID = 38,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Well, that might be what's gonna happen if the Overmare doesn't comply.\" says the stallion.",
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
                Text = "\"What, should we just wait and see what happens when everypony realizes they don't want to kick anypony out? Things are gonna" +
                "get a lot messier then.\" says the stallion.",
                ID = 28,
                AnswerID = 35,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I'm... just gonna assume that was a joke. Anyway, my point was, I think we should attempt a new rebellion.\"\n\r" +
                "says the stallion with a low voice.",
                ID = 23,
                AnswerID = 34,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Ugh, sometimes I just don't understand you... Sure, the voting sucks and these hallways are pretty small, but at least we have\n\r" +
                "food, water and a place to sleep. Who knows what lies on the other side of the Stable's door. For all we know, we might be the last \n\r" +
                "of our species!\" says the stallion with a hint of anger in his voice.",
                ID = 27,
                AnswerID = 33,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"That's what I thought. Anyway, my point was, because I think there's a fair number of ponies now who don't want the vote,\n\r" +
                "we should attempt a new rebellion.\" says the stallion in a low voice.",
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
                Text = "\"Who do you think is getting kicked out next year? Is there anypony else than Astral you think is dangerous or annoying?\"\n\r" +
                "asks the stallion.",
                ID = 17,
                AnswerID = 26,
            });
            InstanceList.Add(new Instance
            {
                Text = "[he sighs] \"I just find this whole voting useless. Who do you think is going to get kicked next year? Is there anypony else than\n\r" +
                "Astral you think is dangerous or annoying?\" asks the stallion.",
                ID = 17,
                AnswerID = 25,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Don't you think it's a bit harsh to kick somepony out of here just because others think they're annoying? Who knows what lies on\n\r" +
                "the other side of the Stable's door.\" says the stallion.",
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
                Text = "\"That's just it. All the ponies from that rebellion from 12 years ago have been kicked out already. What will happen in next \n\r" +
                "year's voting? Is there anypony else than Astral who you think is annoying or dangerous?\" asks the stallion in a pressing voice.",
                ID = 16,
                AnswerID = 18,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Heh, I guess not. But jokes aside... I've been thinking about The Pariah Vote.\" Ardent says.",
                ID = 13,
                AnswerID = 40,
            });
            InstanceList.Add(new Instance
            {
                Text = "The Pariah Vote. It's an event that happens once per year. The whole Stable gathers together and everypony gets to vote for the \n\r" +
                "pony who they think is the most dangerous for the wellbeing of the Stable. The pony who gets the most votes will then be kicked out \n\r" +
                "of the Stable. You never put much thought into this, as you have always just voted for the pony who you thought was the most annoying \n\r" +
                "at the time. You have also been lucky enough to not lose any close relatives because of the vote.",
                ID = 13,
                AnswerID = 40,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Well, it's just that The Pariah Vote is just around the corner.\" says the stallion.",
                ID = 13,
                AnswerID = 17,
            });
            InstanceList.Add(new Instance
            {
                Text = "The Pariah Vote. It's an event that happens once per year. The whole Stable gathers together and everypony gets to vote for the \n\r" +
                "pony who they think is the most dangerous for the wellbeing of the Stable. The pony who gets the most votes will then be kicked out \n\r" +
                "of the Stable. You never put much thought into this, as you have always just voted for the pony who you thought was the most annoying \n\r" +
                "at the time. You have also been lucky enough to not lose any close relatives because of the vote.",
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
                Text = "\"Oh, sorry. It's just that the voting for the next Pariah is just around the corner.\" says the stallion.\n\r",
                ID = 13,
                AnswerID = 14,
            });
            InstanceList.Add(new Instance
            {
                Text = "The Pariah Vote. It's an event that happens once per year. The whole Stable gathers together and everypony gets to vote for the \n\r" +
                "pony who they think is the most dangerous for the wellbeing of the Stable. The pony who gets the most votes will then be kicked out \n\r" +
                "of the Stable. You never put much thought into this, as you have always just voted for the pony who you thought was the most annoying \n\r" +
                "at the time. You have also been lucky enough to not lose any close relatives because of the vote.",
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
                Text = "You make your way to the cafeteria. Once you get there, you spot your friend's, Ardent's, light gray coat and bright red mane \n\r" +
                "from the crowd. You take a sandwich and some water from the counter, and go sit next to Ardent. You notice that he has \n\r" +
                "a worried expression on his face.",
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

            Console.WriteLine("	  \\__					    ^ Crystal Mountains   	    				       '-._ \n\r" +
                "	     '-_							    		    			           \\\n\r" +
                "      		\\							    	    				       ____/\n\r" +
                "  North		 \\						    	    			              /\n\r" +
                "    Luna     /-. |  * Vanhoover		    	    						       ___    |\n\r" +
                "      Ocean  | | |    					    	    		       Manehattan *   /   \\___/\n\r" +
                "             |/  |					    		        	         .---'  .__  \n\r" +
                "		 /							    	                 |      \\_ \\\n\r" +
                "	 _______/						    		    	         \\__	  ''\n\r" +
                "--._____/							    		    	            ''--___\n\r" +
                "						Canterlot *		    	    		            \\\n\r" +
                "									    Fillydelphia *     	            |\n\r" +
                "					    Ponyville *	    			        	       	_--''\n\r" +
                "						           # Everfree Forest           	               |\n\r" +
                "						  		            			    	\\\n\r" +
                "								       	    	        Baltimare * 	 ''--_\n\r" +
                "	              _____					        				    __    \\\n\r" +
                "		     /     \\									    _-''  '''\\_\\	Celestial\n\r" +
                "		 __-'       \\ * Las Pegasus				* ###########		   |		             Sea\n\r" +
                "       --.___---'            \\_				 * Appleloosa	  N3W APPL3L00SA	    \\_\n\r" +
                "\\     /                        '-.								      '-.___\n\r" +
                " \\___/				  |									    __ /\n\r" +
                "				 /			^ Macintosh Hills				    \\ \\| \n\r" +
                "		      .-'\\	/									    | \n\r" +
                "		      |   \\    /									    \\_\n\r" +
                "		     /     \\__/										  '-.\n\r" +
                "		    /											        |\n\r" +
                "		    \\											        |\n\r" +
                "     South	     \\_											        |_\n\r" +
                "	Luna	       '-.__											      \\\n\r" +
                "	   Ocean	    \\											       \\\n\r" +
                "			     \\___										        \\\n\r" +
                "	   	 		 '''--._									         |\n\r" +
                "	      				\\									         |\n\r");

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

        }

    }

}
