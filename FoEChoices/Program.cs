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
        public int QuestID { get; set; } // SpecialFuntion: 2. set to the ID of a quest to mark as completed.

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
        public string Faction { get; set; } // SpecialFunction: 5/11. set to the faction's name you want to add/remove rep points to/from, or to check the Faction's rep points.
        public int RepPoints { get; set; } // SpecialFunction: 5. set to the amount of rep points to award or take from the player. has to be used with Faction!
        public int NewArmorID { get; set; } // SpecialFunction: 6. set to the corresponding ArmorID of an armor to give to the player.
        public int ArmorCheck { get; set; } // SpecialFunction: 7. set to the ArmorClass the player's armor needs to have to pass the check.
        public int QuestCheck { get; set; } // SpecialFunction: 8. set to the ID of a quest to check if it has been completed. !!!don't use twice without having answers in between, will break!!! <-- not sure if fixed? <-- yeah, it's fixed
        public int RemoveAnswer { get; set; } // Specialfunction: 9. set to the ID of an answer to remove.
        public int RedirectInstance { get; set; } // Specialfunction: 10. set to the AnswerID of an instance to redirect the current instance chain to.
        public int RepCheck { get; set; } // SpecialFunction: 11. set to the amount of rep points the player needs to have to pass the check. has to be used with Faction!

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
        public static Faction Security = new Faction
        {
            FactionName = "Security",
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
            QuestList.Add(new Quest() { Description = "Reset Cross Stitch's terminal's password without losing your cool", QuestID = 1, Completed = false });
            QuestList.Add(new Quest() { Description = "Inspect the pistol in the Stable's armory", QuestID = 2, Completed = false });
            QuestList.Add(new Quest() { Description = "Mention Astral in your first conversation with Ardent", QuestID = 3, Completed = false });
            QuestList.Add(new Quest() { Description = "Choose Cross Stitch's path", QuestID = 4, Completed = false });
            QuestList.Add(new Quest() { Description = "Annoy the Overmare during your first conversation", QuestID = 5, Completed = true });
            QuestList.Add(new Quest() { Description = "Agree to join the rebellion", QuestID = 6, Completed = true });
            QuestList.Add(new Quest() { Description = "Get apples from the orchard", QuestID = 7, Completed = false });
            QuestList.Add(new Quest() { Description = "Get some Sparkle-Cola from the bar", QuestID = 8, Completed = false });
            QuestList.Add(new Quest() { Description = "Meet Astral during the first day", QuestID = 9, Completed = true });

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

            bool DebugMode = true; // skip the intro and set starting point
            if (DebugMode) PassInstanceID = 0030005;

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
            Console.WriteLine("\tWelcome to Fallout Equestria: Choices. Before you start the game, please make sure this window is wide enough to fit the line below on one row.");
            Console.WriteLine("\t<---------------------------------------------------------------------------------------------------------------------------------------------------->");
            Console.WriteLine("\tIt's also recommended that you raise the font size a bit, so that the text is easier to read.");
            Console.WriteLine("\tRight click on the top part of the console window and choose \"Properties\". Click on the \"Font\" tab. Font size of 20 or 24 should be ok.");
            Console.WriteLine("\tPress Enter to start.");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            Console.Clear();

            if (!DebugMode) {
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
                    "\tYou brush your mane, put on your Stable suit, and head to the cafeteria to get some breakfast.\n");
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
                Console.WriteLine("\tYou step into the gray hallway. As you turn the first corner, you notice there's some posters that have been hung on the walls.\n" +
                    "\tPosters with the words such as \"Keep The Stable safe!\" and \"Every vote counts!\" are plastered on the walls. \"Oh, it's that time again.\"\n" +
                    "\tyou say to yourself.\n");
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
                Console.WriteLine("\tOn your way to the cafeteria you run into Light Harmony, a young unicorn filly with white coat and dark blue mane.\n" +
                    "\tAs she notices you, she tenses up, and looks a bit worried.");
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
                Console.WriteLine();
                Console.WriteLine("\t\"Oh, hey there Silver... Uhh, I was just about to leave...\" says the filly.");
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            }

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
            if (!AnsWithinLimits)
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

            if (!AnsSecondary)
            {
                foreach (var answer in QueryAnswerID)
                {
                    PassAnswerID = answer.ID;

                    //check for any special functions
                    if (answer.HasSpecialFunction)
                    {
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
            bool InstanceRedirected = false;

            var InstanceEnum = InstanceList.OfType<Instance>();
            var CurrentInstances =
                from instance in InstanceEnum
                where instance.AnswerID == Answered
                select instance;

            foreach (var instance in CurrentInstances) {
                PassInstanceID = instance.ID;
                Console.WriteLine("\t{0}", instance.Text);
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

                if (instance.HasSpecialFunction) // check for any SpecialFunctions
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
                            Answered = PassAnswerID + 2;
                            PassInstanceID = GetInstance(Answered, CurrentWeaponID);
                            InstanceRedirected = true;
                        }
                        else if (WeaponDamage < instance.DamageCheck) // does not do enough damage
                        {
                            Answered = PassAnswerID + 1;
                            PassInstanceID = GetInstance(Answered, CurrentWeaponID);
                            InstanceRedirected = true;
                        }
                    }
                    // add/remove rep points
                    if (instance.SpecialFunction.Contains(5)) // TODO: MAKE THIS DYNAMIC!!!
                    {
                        if (instance.Faction == "Stable 54")
                        {
                            GetRepPoints("Stable 54", instance.RepPoints);
                        }
                        if (instance.Faction == "Security")
                        {
                            GetRepPoints("Security", instance.RepPoints);
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
                        ArmorClass = GetNewArmor(CurrentArmorID, 2);

                        if (ArmorClass >= instance.ArmorCheck) // armor protects
                        {
                            Answered = PassAnswerID + 2;
                            PassInstanceID = GetInstance(Answered, CurrentWeaponID);
                            InstanceRedirected = true;
                        }
                        else if (ArmorClass < instance.ArmorCheck) // armor does not protect
                        {
                            Answered = PassAnswerID + 1;
                            PassInstanceID = GetInstance(Answered, CurrentWeaponID);
                            InstanceRedirected = true;
                        }
                    }
                    // check if a quest has been completed
                    if (instance.SpecialFunction.Contains(8))
                    {
                        if(QuestList[instance.QuestCheck].Completed)
                        {
                            Answered = PassAnswerID + 2;
                            PassInstanceID = GetInstance(Answered, CurrentWeaponID);
                            InstanceRedirected = true;
                        }
                        else if (!QuestList[instance.QuestCheck].Completed)
                        {
                            Answered = PassAnswerID + 1;
                            PassInstanceID = GetInstance(Answered, CurrentWeaponID);
                            InstanceRedirected = true;
                        }
                    }
                    // remove answer
                    if(instance.SpecialFunction.Contains(9))
                    {
                        var item = AnswersList.SingleOrDefault(x => x.ID == PassAnswerID);

                        if(item != null)
                        {
                            AnswersList.Remove(item);
                        }
                    }
                    // redirect instance
                    if (instance.SpecialFunction.Contains(10))
                    {
                        Answered = instance.RedirectInstance;
                        PassInstanceID = GetInstance(Answered, CurrentWeaponID);
                        InstanceRedirected = true;
                    }
                    // check reputation
                    if (instance.SpecialFunction.Contains(11))
                    {
                        int points = GetRepPoints(instance.Faction, 0);

                        if (instance.RepCheck <= points) {

                            Answered = PassAnswerID + 2;
                            PassInstanceID = GetInstance(Answered, CurrentWeaponID);
                            InstanceRedirected = true;

                        }
                        else {

                            Answered = PassAnswerID + 1;
                            PassInstanceID = GetInstance(Answered, CurrentWeaponID);
                            InstanceRedirected = true;

                        }
                    }
                }
                if (InstanceRedirected) { InstanceRedirected = false; break; }
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

            // TODO: MAKE THIS DYNAMIC!!!
            if (FactionName == "Stable 54") {
                // add or remove the possible amount of points
                Stable54.RepPoints += RepPoints;
                // send the requested faction's rep points
                RepPointsToSend = Stable54.RepPoints;
            }
            if (FactionName == "Security")
            {
                Security.RepPoints += RepPoints;
                RepPointsToSend = Security.RepPoints;
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

            List<Weapon> WeaponList = new List<Weapon>()
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
                InstanceID = 28,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 6
            });
            AnswersList.Add(new Answer
            {
                Text = "Count me in. But right now I want to eat, let's talk about this more some other time.",
                UserInput = 1,
                ID = 49,
                InstanceID = 29,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 6
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
                InstanceID = 30,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 6
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
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 6
            });
            AnswersList.Add(new Answer
            {
                Text = "Actually, no. If you think I'm going to go against the Overmare, you're wrong.",
                UserInput = 1,
                ID = 46,
                InstanceID = 32,
            });
            AnswersList.Add(new Answer
            {
                Text = "I'll join. But let's talk about it more some other time, I want to eat now.",
                UserInput = 2,
                ID = 49,
                InstanceID = 32,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 6
            });
            AnswersList.Add(new Answer
            {
                Text = "Actually, no. If you think I'm going to go against the Overmare, you're wrong.",
                UserInput = 1,
                ID = 46,
                InstanceID = 33,
            });
            AnswersList.Add(new Answer
            {
                Text = "Alright, I'm with you. But this needs to be planned well. Let's talk about it more later, okay?",
                UserInput = 2,
                ID = 49,
                InstanceID = 33,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 6
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
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 4,
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
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 9
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
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 1
            });
            AnswersList.Add(new Answer
            {
                Text = "There we go. Same place, same time tomorrow?",
                UserInput = 2,
                ID = 0010007,
                InstanceID = 0010004,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
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
                ID = 0012004,
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
                ID = 0012005,
                InstanceID = 0012000,
            });
            AnswersList.Add(new Answer
            {
                Text = "What do you mean?",
                UserInput = 1,
                ID = 0012006,
                InstanceID = 0012003,
            });
            AnswersList.Add(new Answer
            {
                Text = "Good, I was really getting sick of you.",
                UserInput = 2,
                ID = 0012007,
                InstanceID = 0012003,
            });
            AnswersList.Add(new Answer
            {
                Text = "Fuck off.",
                UserInput = 3,
                ID = 0012011,
                InstanceID = 0012003,
            });
            AnswersList.Add(new Answer
            {
                Text = "True. Good thing I have a dad to hang out with.",
                UserInput = 1,
                ID = 0012008,
                InstanceID = 0012005,
            });
            AnswersList.Add(new Answer
            {
                Text = "How about you stop threatening me and my dad, and fuck off before I call the security?",
                UserInput = 2,
                ID = 0012009,
                InstanceID = 0012005,
            });
            AnswersList.Add(new Answer
            {
                Text = "Too bad ponies don't have anything against me.",
                UserInput = 1,
                ID = 0012010,
                InstanceID = 0012008,
            });
            AnswersList.Add(new Answer
            {
                Text = "Oh, I don't have to worry about that. You on the other hoof...",
                UserInput = 2,
                ID = 0012010,
                InstanceID = 0012008,
            });
            AnswersList.Add(new Answer
            {
                Text = "[go lie down in your bed]",
                UserInput = 1,
                ID = 0012013,
                InstanceID = 0012007,
            });
            AnswersList.Add(new Answer
            {
                Text = "[go to the terminal]",
                UserInput = 2,
                ID = 0012014,
                InstanceID = 0012007,
            });
            AnswersList.Add(new Answer
            {
                Text = "[inspect the contents of the shelf]",
                UserInput = 3,
                ID = 0012015,
                InstanceID = 0012007,
            });
            AnswersList.Add(new Answer
            {
                Text = "Morning. You're here earlier than normal.",
                UserInput = 1,
                ID = 0020000,
                InstanceID = 0020000,
            });
            AnswersList.Add(new Answer
            {
                Text = "Already working at full steam, I see.",
                UserInput = 2,
                ID = 0020000,
                InstanceID = 0020000,
            });
            AnswersList.Add(new Answer
            {
                Text = "Uh, sorry?",
                UserInput = 1,
                ID = 0020005,
                InstanceID = 0020004,
            });
            AnswersList.Add(new Answer
            {
                Text = "Well, technically it is both since we have to keep all the terminal hardware here because they don't have a place elsewhere.",
                UserInput = 2,
                ID = 0020006,
                InstanceID = 0020004,
            });
            AnswersList.Add(new Answer
            {
                Text = "Soo, what do you want me to do?",
                UserInput = 1,
                ID = 0020008,
                InstanceID = 0020006,
            });
            AnswersList.Add(new Answer
            {
                Text = "I assume this is the part where I come in?",
                UserInput = 2,
                ID = 0020008,
                InstanceID = 0020006,
            });
            AnswersList.Add(new Answer
            {
                Text = "Alright, I'll start looking into it.",
                UserInput = 1,
                ID = 0020009,
                InstanceID = 0020007,
            });
            AnswersList.Add(new Answer
            {
                Text = "And if I refuse?",
                UserInput = 2,
                ID = 0020012,
                InstanceID = 0020007,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 5
            });
            AnswersList.Add(new Answer
            {
                Text = "The voting starts in a week, correct?",
                UserInput = 3,
                ID = 0020013,
                InstanceID = 0020007,
            });
            AnswersList.Add(new Answer
            {
                Text = "Hey Scan. What're you doing in here?",
                UserInput = 1,
                ID = 0020015,
                InstanceID = 0020008,
            });
            AnswersList.Add(new Answer
            {
                Text = "Oh, hey there. Miss being at work so much you had to come here on a holiday?",
                UserInput = 2,
                ID = 0020015,
                InstanceID = 0020008,
            });
            AnswersList.Add(new Answer
            {
                Text = "I, uhh, no. Just came here before you. Yup. Also, wanted to try a new manestyle.",
                UserInput = 1,
                ID = 0020016,
                InstanceID = 0020009,
            });
            AnswersList.Add(new Answer
            {
                Text = "Maybe...?",
                UserInput = 2,
                ID = 0020017,
                InstanceID = 0020009,
            });
            AnswersList.Add(new Answer
            {
                Text = "Knock knock!",
                UserInput = 1,
                ID = 0020019,
                InstanceID = 0020010,
            });
            AnswersList.Add(new Answer
            {
                Text = "Uh, hello?",
                UserInput = 2,
                ID = 0020019,
                InstanceID = 0020010,
            });
            AnswersList.Add(new Answer
            {
                Text = "Yes, I did as you instructed and sent the mails at midday.",
                UserInput = 1,
                ID = 0020020,
                InstanceID = 0020011,
            });
            AnswersList.Add(new Answer
            {
                Text = "Yeah, I made it in time. Barely.",
                UserInput = 2,
                ID = 0020021,
                InstanceID = 0020011,
            });
            AnswersList.Add(new Answer
            {
                Text = "[vote for Astral]",
                UserInput = 1,
                ID = 0020026,
                InstanceID = 0020012,
            });
            AnswersList.Add(new Answer
            {
                Text = "[vote Astral]",
                UserInput = 2,
                ID = 0020026,
                InstanceID = 0020012,
            });
            AnswersList.Add(new Answer
            {
                Text = "[vote Astral out]",
                UserInput = 3,
                ID = 0020026,
                InstanceID = 0020012,
            });
            AnswersList.Add(new Answer
            {
                Text = "G'morning.",
                UserInput = 1,
                ID = 0020029,
                InstanceID = 0020013,
            });
            AnswersList.Add(new Answer
            {
                Text = "You look a bit tired. Are you nervous about the vote?",
                UserInput = 2,
                ID = 0020030,
                InstanceID = 0020013,
            });
            AnswersList.Add(new Answer
            {
                Text = "Morning. Did you sleep well last night?",
                UserInput = 3,
                ID = 0020031,
                InstanceID = 0020013,
            });
            AnswersList.Add(new Answer
            {
                Text = "Apology accepted.",
                UserInput = 1,
                ID = 0020032,
                InstanceID = 0020015,
            });
            AnswersList.Add(new Answer
            {
                Text = "No need to apologize. I think I got a bit too heated during our conversation.",
                UserInput = 2,
                ID = 0020032,
                InstanceID = 0020015,
            });
            AnswersList.Add(new Answer
            {
                Text = "Morning. Yeah, I've been busy.",
                UserInput = 1,
                ID = 0020034,
                InstanceID = 0020014,
            });
            AnswersList.Add(new Answer
            {
                Text = "I've been pretty much living in the IT-department for the past week.",
                UserInput = 2,
                ID = 0020034,
                InstanceID = 0020014,
            });
            AnswersList.Add(new Answer
            {
                Text = "That's what happens when somepony gives me a deadline that's way too short.",
                UserInput = 3,
                ID = 0020034,
                InstanceID = 0020014,
            });
            AnswersList.Add(new Answer
            {
                Text = "Of course. But can I really refuse the Overmare's orders?",
                UserInput = 1,
                ID = 0020037,
                InstanceID = 0020017,
            });
            AnswersList.Add(new Answer
            {
                Text = "The Overmare asked me to do it. Would you have refused to do it in that situation?",
                UserInput = 2,
                ID = 0020037,
                InstanceID = 0020017,
            });
            AnswersList.Add(new Answer
            {
                Text = "The Overmare ordered me to do it. I couldn't have refused it even if I wanted to.",
                UserInput = 1,
                ID = 0020037,
                InstanceID = 0020018,
            });
            AnswersList.Add(new Answer
            {
                Text = "Listen. The Overmare quite literally threatened to kick me out of the Stable if I didn't do it.",
                UserInput = 2,
                ID = 0020038,
                InstanceID = 0020018,
            });
            AnswersList.Add(new Answer
            {
                Text = "That's it?",
                UserInput = 1,
                ID = 0020042,
                InstanceID = 0020019,
            });
            AnswersList.Add(new Answer
            {
                Text = "Sweet Celestia, this is not going to work.",
                UserInput = 2,
                ID = 0020043,
                InstanceID = 0020019,
            });
            AnswersList.Add(new Answer
            {
                Text = "Alright, worth a shot. I guess we don't have much of a choice.",
                UserInput = 3,
                ID = 0020044,
                InstanceID = 0020019,
            });
            AnswersList.Add(new Answer
            {
                Text = "The problem is you, not the program. If you would type your code correctly the first time, you wouldn't have such troubles with the system.",
                UserInput = 1,
                ID = 0020046,
                InstanceID = 0020020,
            });
            AnswersList.Add(new Answer
            {
                Text = "Sorry for the trouble with the system. Are you sure you typed the code correctly? Was there any error messages when you tried to vote?",
                UserInput = 2,
                ID = 0020047,
                InstanceID = 0020020,
            });
            AnswersList.Add(new Answer
            {
                Text = "Fuck you. I'm tired of idiots like you not knowing how to use a simple piece of software. Seriously, fuck you.",
                UserInput = 3,
                ID = 0020048,
                InstanceID = 0020020,
            });
            AnswersList.Add(new Answer
            {
                Text = "You say that everytime you see me.",
                UserInput = 1,
                ID = 0020051,
                InstanceID = 0020022,
            });
            AnswersList.Add(new Answer
            {
                Text = "And you've gotten stronger since the last time, it seems.",
                UserInput = 2,
                ID = 0020052,
                InstanceID = 0020022,
            });
            AnswersList.Add(new Answer
            {
                Text = "Yeah, yeah. Hey, you got any spare apples for me?",
                UserInput = 3,
                ID = 0020053,
                InstanceID = 0020022,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 8
            });
            AnswersList.Add(new Answer
            {
                Text = "Alright, let's go take a look at it.",
                UserInput = 1,
                ID = 0020055,
                InstanceID = 0020025,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 2 }),
                QuestID = 7
            });
            AnswersList.Add(new Answer
            {
                Text = "Are you sure you typed your code correctly?",
                UserInput = 2,
                ID = 0020056,
                InstanceID = 0020025,
            });
            AnswersList.Add(new Answer
            {
                Text = "And you even reserved a seat for me. I appreciate it.",
                UserInput = 1,
                ID = 0030002,
                InstanceID = 0030000,
            });
            AnswersList.Add(new Answer
            {
                Text = "Good call. I probably would've missed the whole event if the Overmare didn't announce the reminder over the PA system.",
                UserInput = 2,
                ID = 0030003,
                InstanceID = 0030000,
            });
            AnswersList.Add(new Answer
            {
                Text = "Where are we going?",
                UserInput = 1,
                ID = 0030005,
                InstanceID = 0030001,
            });
            AnswersList.Add(new Answer
            {
                Text = "There has to be a mistake, there's no way I had the most votes!",
                UserInput = 2,
                ID = 0030006,
                InstanceID = 0030001,
            });
            AnswersList.Add(new Answer
            {
                Text = "Yeah, kinda caught me off guard too.",
                UserInput = 1,
                ID = 0030009,
                InstanceID = 0030002,
            });
            AnswersList.Add(new Answer
            {
                Text = "Oh well, what can you do, right?",
                UserInput = 2,
                ID = 0030009,
                InstanceID = 0030002,
            });
            AnswersList.Add(new Answer
            {
                Text = "Weren't you at the announcement? I thought everypony goes there to hear the results.",
                UserInput = 3,
                ID = 0030008,
                InstanceID = 0030002,
            });
            AnswersList.Add(new Answer
            {
                Text = "Uh, it's okay, really.",
                UserInput = 1,
                ID = 00300012,
                InstanceID = 0030004,
            });
            AnswersList.Add(new Answer
            {
                Text = "[hug her back]",
                UserInput = 2,
                ID = 0030013,
                InstanceID = 0030004,
            });
            AnswersList.Add(new Answer
            {
                Text = "Could I get some privacy, please?",
                UserInput = 1,
                ID = 0030015,
                InstanceID = 0030005,
            });
            AnswersList.Add(new Answer
            {
                Text = "Did your mother never teach you not to barge in a girl's room uninvited?",
                UserInput = 2,
                ID = 0030016,
                InstanceID = 0030005,
            });
            AnswersList.Add(new Answer
            {
                Text = "I'll quickly go check the votes.",
                UserInput = 1,
                ID = 0030031,
                InstanceID = 0030007,
            });
            AnswersList.Add(new Answer
            {
                Text = "I'll check the votes after we're done with the rebellion.",
                UserInput = 2,
                ID = 0030035,
                InstanceID = 0030007,
            });
            AnswersList.Add(new Answer
            {
                Text = "Why are you doing this? We can put an end to this madness!",
                UserInput = 1,
                ID = 0030039,
                InstanceID = 0030008,
            });
            AnswersList.Add(new Answer
            {
                Text = "[Bluff] Stop waving that thing around, we both know you don't have what it takes to shoot one of your own!",
                UserInput = 2,
                ID = 0030040,
                InstanceID = 0030008,
            });
            AnswersList.Add(new Answer
            {
                Text = "Let's just calm down for a second, we can work this out without anypony else getting hurt!",
                UserInput = 3,
                ID = 0030041,
                InstanceID = 0030008,
            });

            // ------------------------------------------------------- INSTANCE LIST -------------------------------------------------------


            InstanceList.Add(new Instance
            {
                Text = "As you're walking, you notice how empty the hallways are. Most ponies are probably at the top level of the Stable. Exactly how many rebels are in\n" +
                "\tthe revolt? You hope it's the majority of the Stable, as it would be a lot easier to pressure the Overmare to abandon the vote.\n",
                ID = 0030008,
                AnswerID = 0030043,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030038
            });
            InstanceList.Add(new Instance
            {
                Text = "With one final glance back at Ardent, you whisper your last words to him.\n" +
                "\t\"Goodbye, friend.\"\n",
                ID = 0030009,
                AnswerID = 0030042,
            });
            InstanceList.Add(new Instance
            {
                Text = "You step outside the Stable, and in to the darkness.\n",
                ID = 0030009,
                AnswerID = 0030042,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"There's nothing to work out, this is the way it's going to have to be!\" the Overmare yells at you.\n",
                ID = 0030009,
                AnswerID = 0030041,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"No, it doesn't have to! We can abandon the vote, find other ways to keep us safe!\" you try to reason to her, but they seem to fall on deaf ears.\n",
                ID = 0030009,
                AnswerID = 0030041,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"You don't understand the situation! If there were any alternatives, I would have taken them! Just go away!\" she yells back at you.\n",
                ID = 0030009,
                AnswerID = 0030041,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Please, think about what you're d-\"\n",
                ID = 0030009,
                AnswerID = 0030041,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Leave, before I do something we'll both regret!\" she screams at you. She just doesn't want to cooperate. You glance at Ardent, who seems to be\n" +
                "\tfrozen in fear by the pistol pointed at you.\n",
                ID = 0030009,
                AnswerID = 0030041,
            });
            InstanceList.Add(new Instance
            {
                Text = "You look at the Overmare for a moment longer. She seems to be on the verge of a complete meltdown. Seemingly left with no choice, you turn around,\n" +
                "\tand start walking towards the open door.\n",
                ID = 0030009,
                AnswerID = 0030041,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030042
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I don't?\" she asks. You two stare at each others' eyes for what feels like an eternity.\n",
                ID = 0030009,
                AnswerID = 0030040,
            });
            InstanceList.Add(new Instance
            {
                Text = "She then moves the pistol to point towards Ardent. *BANG*\n",
                ID = 0030009,
                AnswerID = 0030040,
            });
            InstanceList.Add(new Instance
            {
                Text = "Ardent drops to the ground, not moving. Your blood runs cold. Another wave of screams can be heard from the crowd.\n",
                ID = 0030009,
                AnswerID = 0030040,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"You motherfucking piece of shit monster,\" you growl at the Overmare. Deep hatred burns within you, like never before.\n",
                ID = 0030009,
                AnswerID = 0030040,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Now get out, before you meet a similar fate,\" the Overmare hisses back at you, the smoking barrel of the pistol once again pointed at you.\n",
                ID = 0030009,
                AnswerID = 0030040,
            });
            InstanceList.Add(new Instance
            {
                Text = "Never before have you felt so much anger and fear. You stare at the Overmare for a moment longer. Seemingly with no choice left, you turn around,\n" +
                "\tand start walking towards the open door.\n",
                ID = 0030009,
                AnswerID = 0030040,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030042
            });
            InstanceList.Add(new Instance
            {
                Text = "\"You don't understand what's at stake here! I'm doing this to protect our Stable, whether your small brain can comprehend it or not!\" she yells\n" +
                "\tat you angrily.\n",
                ID = 0030009,
                AnswerID = 0030039,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"This is not the way to keep our Stable safe, there are better ways to do it! We can help you!\" you try to reason to her.\n",
                ID = 0030009,
                AnswerID = 0030039,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Fuck you, if there were any other options, I would have taken them! Like I said, You don't understand the situation we're in! Now get the fuck out,\n" +
                "\tbefore I'll do something we'll both regret!\" she screams desperately. She is clearly on the verge of a complete meltdown.\n",
                ID = 0030009,
                AnswerID = 0030039,
            });
            InstanceList.Add(new Instance
            {
                Text = "You glance at Ardent, who seems to be frozen in fear by the pistol pointed at you.\n",
                ID = 0030009,
                AnswerID = 0030039,
            });
            InstanceList.Add(new Instance
            {
                Text = "Seemingly having no choice left, you turn around, and start walking towards the open door.\n",
                ID = 0030008,
                AnswerID = 0030039,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030042
            });
            InstanceList.Add(new Instance
            {
                Text = "When you get to the top level of the Stable, you are immediately greeted by a crowd of ponies. The top level is small-ish, the only notable things\n" +
                "\tin the room being the giant metal gear-shaped door with the number 54 printed on it, and the control panel for the door on the right side of the room.\n" +
                "\tYou can't even remember the last time you were here.\n",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "The security ponies take you to the front of the crowd. You're feeling really uncomfortable about the amount of ponies watching you, but do your\n" +
                "\tbest to ignore them. The Overmare is standing next to the door's control panel. She's looking at you with a seemingly neutral expression. She then\n" +
                "\tspeaks up, silencing the ponies in the room.\n",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"The time has come once again to exile the Pariah from our home,\" she starts her speech. You notice Ardent and dad arriving in the back of\n" +
                "\tthe crowd. You feel your heart start beating faster, as you wait for the moment the rebellion starts.\n",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"We thank you for the effort you've put into the wellbeing of our Stable, but unfortunately the votes don't lie.\" she continues. You somehow doubt\n" +
                "\tthat last statement. \"May Luna watch over you,\" she says, and hits a button on the control panel. Alarms start blaring, and red lights start flashing.\n",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "A huge hydraulic arm comes down from the ceiling, and latches itself on the middle of the door. Hydraulic hisses can be heard from the arm as it pulls\n" +
                "\tthe door inwards, and starts rolling it slowly to the left along the tracks on the floor. You glance at the crowd of ponies. Most of them are staring\n" +
                "\tsilently at the door.\n",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "Once the door comes to a halt, the alarms and lights also stop.\n" +
                "\t\"Silver Shift. Please m-\"\n",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "*BANG*\n",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "Chaos breaks loose. The crowd starts screaming in panic. Some ponies try rushing for the stairs, while some are clashing with the security ponies\n" +
                "\tin the room. Was that a gun shot? Wasn't this supposed to be a peaceful rebellion, what the hell happened?!\n",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"What the hell is going on?!\" the Overmare screams to nopony in particular, mirroring your thoughts pretty well. You then notice somepony lying in\n" +
                "\ta heap, unmoving in the crowd. Oh no. They're not dead, right? You can't make out whether they're security or not from this distance.\n",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "Ardent then emerges from the chaos, rushing towards you.\n" +
                "\t\"Come on Silver, let's go!\" he motions for you to come forward.\n",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"If you take a single step back towards the Stable, you will regret it deeply!\" the Overmare yells at you. She's holding a pistol in her magic,\n" +
                "\tpointed towards you.",
                ID = 0030008,
                AnswerID = 0030038,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"You came here just to make fun of the situation?\" you ask her, not in the mood for a chat.\n",
                ID = 0030008,
                AnswerID = 0030036,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Nah, I was just passing through. But I do have to admit, it is kinda funny seeing you being dragged away,\" she says. She glances at the security\n" +
                "\tponies behind you, and gives you a smirk. \"I see you're running short on time, so I'll leave you be. Goodbye~\" she says, and flicks her tail at you\n" +
                "\tas she turns to leave. You glare at her for a moment before continuing on your way.",
                ID = 0030008,
                AnswerID = 0030036,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yeah, what a coincidence, right? You certainly didn't have anything to do with me getting voted out, did you?\" you quip at her sarcastically.\n",
                ID = 0030008,
                AnswerID = 0030037,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"What, me? I would never do such a thing. Although, I do have to say, it is kinda funny seeing you being dragged away,\" she responds.\n" +
                "\t\"And I guess this means the admin's job is now vacant. I hope the Stable finds a suitable replacement for it~\" she continues.\n",
                ID = 0030008,
                AnswerID = 0030037,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Wha- That's the reason for all this? My job? You're sick, you know that?\" you say, appalled.\n",
                ID = 0030008,
                AnswerID = 0030037,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Serves you right, if you ask me. Always acting like you're better than the rest of us, and getting handed everything on a silver platter.\n" +
                "\tWarms my heart to see you get the short end of the stick,\" she says, clearly taking great pleasure from the situation.\n",
                ID = 0030008,
                AnswerID = 0030037,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"But it looks like you're running short on time, so I'll leave you to it. Goodbye, Silver Shift,\" she says, and flicks her tail at you as she\n" +
                "\tturns to leave. You can barely contain your anger at this point.\n",
                ID = 0030008,
                AnswerID = 0030037,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030038
            });
            InstanceList.Add(new Instance
            {
                Text = "\"You seeing this? Isn't it obvious she rigged the vote?\" you ask the security ponies. They just shrug in response.\n",
                ID = 0030008,
                AnswerID = 0030037,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Not my problem, really. And besides, I didn't really see any concrete evidence about her rigging the vote,\" they older pony says. You can't\n" +
                "\tbelieve what you're hearing. Since when has the security been so useless? Feeling utterly defeated, you sigh and continue on your way.\n",
                ID = 0030008,
                AnswerID = 0030037,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030038
            });
            InstanceList.Add(new Instance
            {
                Text = "You don't want to keep the Overmare waiting. The logs aren't going anywhere, you can check them later.\n" +
                "\t\"Alright. We're going to check up on a couple of friends, and then we'll come to the Stable's door, okay?\" dad says to you. You nod in response.\n" +
                "\tArdent and dad then get up, and leave. After a moment, you pick up your saddlebags, and leave too.\n",
                ID = 0030008,
                AnswerID = 0030035,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Finally. Let's go,\" the younger security pony says as you step out from the room. You ignore the comment and continue walking towards the top\n" +
                "\tlevel of the Stable.\n",
                ID = 0030008,
                AnswerID = 0030035,
            });
            InstanceList.Add(new Instance
            {
                Text = "As you're walking, you notice how empty the hallways are. Most ponies are probably at the top level of the Stable. Exactly how many rebels are in\n" +
                "\tthe revolt? You hope it's the majority of the Stable, as it would be a lot easier to pressure the Overmare to abandon the vote.\n",
                ID = 0030008,
                AnswerID = 0030035,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Fancy seeing you here, Silver,\" comes the voice of none other than the bitch herself, Astral. Fucking great. You turn to look at her smug face.\n",
                ID = 0030008,
                AnswerID = 0030035,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 9,
            });
            InstanceList.Add(new Instance
            {
                Text = "You then hear the younger security pony walk to the office.\n" +
                "\t\"There you are! Maybe don't run away like that, huh?\" the stallion says. Feeling utterly defeated, you decide to just shut down the terminal\n" +
                "\tand start walking towards the door.\n",
                ID = 0030008,
                AnswerID = 0030032,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yeah, yeah. Let's just go,\" you say to him. You're already dreading about the amount of work it's going to take to restore your accounts.\n" +
                "\tIf you're lucky, it's going to be easily restored with backups. If you're unlucky, however... You're too tired to even think about that.\n",
                ID = 0030008,
                AnswerID = 0030032,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030043
            });
            InstanceList.Add(new Instance
            {
                Text = "You then hear the younger security pony rush to the office.\n" +
                "\t\"There you are, you little prick!\" he growls at you. He starts walking towards you... with a baton in his magic. Oh no.\n" +
                "\t\"I've just about had enough of you for today!\" he says, and pulls the chair from under you with his magic. You fall onto the floor in a heap.\n",
                ID = 0030008,
                AnswerID = 0030033,
            });
            InstanceList.Add(new Instance
            {
                Text = "You start to get up, but are interrupted by a strike to your side from the baton. Yelping in pain, you raise your forehooves to protect you.\n" +
                "\t\"Maybe this'll teach you a little respect, bitch!\" the stallion says, as he keeps beating down on you.\n",
                ID = 0030008,
                AnswerID = 0030033,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Alright, that's enough. We need to get going,\" comes the voice of the older stallion from the door. The younger pony stops his strikes.\n" +
                "\t\"Get up,\" he says to you. Hurting all over, you slowly get up. You almost collapse again when you try to put weight on your left forehoof.\n",
                ID = 0030008,
                AnswerID = 0030033,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oww... What was that for?\" you ask the stallion.\n" +
                "\t\"You know it already. Maybe start showing some respect to the ponies around you, huh?\" he spats at you. You almost go for an insult, but\n" +
                "\tdecide against it. The three of you then start walking towards the top level of the Stable. You try to put as little weight on your left\n" +
                "\tforehoof as you walk. This day just keeps getting shittier.\n",
                ID = 0030008,
                AnswerID = 0030033,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030043
            });
            InstanceList.Add(new Instance
            {
                Text = "It would be a huge help in the rebellion if we could catch the Overmare messing with the votes, you reason to yourself.\n" +
                "\t\"Alright, see you at the door,\" you say as you pick up your saddlebags, and leave the room. You don't have much time, so you start running\n" +
                "\tfor the IT-department.\n",
                ID = 0030008,
                AnswerID = 0030031,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 5 }),
                Faction = "Security",
                RepPoints = 1,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hey, wait up!\" you hear the security ponies say behind you. You don't slow down however, and instead keep running.\n",
                ID = 0030008,
                AnswerID = 0030031,
            });
            InstanceList.Add(new Instance
            {
                Text = "The hallways are almost empty, you notice. Most ponies are probably at the top level of the Stable. Exactly how many rebels are in\n" +
                "\tthe revolt? You hope it's the majority of the Stable, as it would be a lot easier to pressure the Overmare to abandon the vote\n.",
                ID = 0030008,
                AnswerID = 0030031,
            });
            InstanceList.Add(new Instance
            {
                Text = "Once you get to the lower level, you see Scanline leave the IT-department, much to your surprise. She then notices you.\n" +
                "\t\"Silver? what are you doing here?\" she asks, also surprised.\n",
                ID = 0030008,
                AnswerID = 0030031,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I could ask you the same,\" you respond to her.\n" +
                "\t\"Something urgent came up, had to take care of it. Aren't you supposed to be, uh, leaving?\" she asks. You open your mouth to respond, but\n" +
                "\tnotice that Scanline seems to have... a black eye?\n",
                ID = 0030008,
                AnswerID = 0030031,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I was just gonna check if the votes I got are legit. Are you okay? What happened to your eye?\" you ask her.\n" +
                "\t\"Oh, I tripped yesterday, don't worry,\" she says. You're not sure if you believe her, but let it be for now.\n",
                ID = 0030008,
                AnswerID = 0030031,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Well, be sure to hurry. I was on my way to the top level to, you know... say goodbye,\" she says after a few seconds of silence.\n" +
                "\t\"I'll be quick,\" you say to her, and continue walking to the IT-department.\n",
                ID = 0030008,
                AnswerID = 0030031,
            });
            InstanceList.Add(new Instance
            {
                Text = "You step into the office, turn on your terminal, and log in. But instead of letting you in, the machine gives you an error.\n" +
                "\t'The username or password is incorrect.' You type them again, but get the same response. A third time, you type slowly to not make any typos.\n" +
                "\tStill no luck.\n",
                ID = 0030008,
                AnswerID = 0030031,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"FUCK!\" you yell in frustration. Nothing is ever easy, is it? You realize that the program automatically deleted the Pariah's account\n" +
                "\tfrom the system. And since your personal account is tied to the admin account, it got deleted too. This mess is going to take ages to fix...\n" +
                "\tShould've put more thought into edge cases. You yell a few more profanities to let some of the stress out.\n",
                ID = 0030008,
                AnswerID = 0030031,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 11 }),
                Faction = "Security",
                RepCheck = 3,
            });
            InstanceList.Add(new Instance
            {
                Text = "The three of you then fall silent for a moment. You can clearly see how nervous Ardent and dad are getting, despite their best\n" +
                "\tattempts at hiding it. Not that you can blame them, this is the biggest event in the Stable since... well, since the last revolt attempt.\n" +
                "\tArdent then speaks up, breaking the silence.\n",
                ID = 0030007,
                AnswerID = 0030029,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I can't help but think how the Overmare acted during the announcement. She didn't seem the least bit surprised that you got\n" +
                "\tthe most votes. Isn't that kinda suspicious to you too?\" he asks you.\n" +
                "\t\"That's true. Or maybe she just doesn't care about anypony else than herself,\" you respond. Although, the Overmare would have an easy\n" +
                "\ttime rigging the votes because of the program. A realization then hits you.\n",
                ID = 0030007,
                AnswerID = 0030029,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I could go check the logs from the voting program to see if the votes are legit! Goddesses, why didn't I think of it sooner...\"\n" +
                "\tyou say, disappointed in yourself. The lack of sleep recently really has taken a toll on you.\n" +
                "\t\"You're gonna have to hurry if you want to do that, we need to be at the Stable's door soon,\" dad says.",
                ID = 0030007,
                AnswerID = 0030029,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, before I forget... Here, I think you should have this,\" dad says, and pulls out something from his saddlebags. Is that...\n" +
                "\t\"A gun?\" you ask, trying to keep your voice down.\n" +
                "\t\"It's the pistol you found last week. You should keep it for now, just in case things get ugly during the rebellion,\" he says. You can\n" +
                "\tbarely believe what you're hearing. You've never even handled a gun before.\n",
                ID = 0030007,
                AnswerID = 0030030,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"B-but I don't even know how to use it!\" you say.\n" +
                "\t\"Oh, it's easy. Remove the safety, load it, point at the target, and pull the trigger. Nothing more to it. And they did teach how to use\n" +
                "\ta gun in classes when you were younger, correct?\" dad says like it's no big deal.\n",
                ID = 0030007,
                AnswerID = 0030030,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yeah, but it was only in theory. We didn't actually get to use them,\" you say.\n" +
                "\t\"And theory has always been your strength,\" dad says encouragingly. Seemingly having no room for an argument, you decide to take the pistol,\n" +
                "\tand put it in your saddlebags. Here's hoping you won't have to use it.\n",
                ID = 0030007,
                AnswerID = 0030030,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"What happened to the whole 'not allowed to give guns to anypony but the security' thing?\" you ask dad playfully.\n" +
                "\t\"Well as it turns out, this pistol wasn't actually even listed in our system, which is quite weird. But it means I'm not technically doing\n" +
                "\tanything wrong by giving it to you, since there's no documentation of it,\" dad explains. Not documented?\n",
                ID = 0030007,
                AnswerID = 0030030,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Really? You'd think the Stable would keep a good record of all firearms,\" you say.\n" +
                "\t\"Yeah. And what's even weirder is that it doesn't seem to have a serial number on it,\" dad says. Either it's worn off, of it's a custom made,\n" +
                "\tyou think to yourself.\n",
                ID = 0030007,
                AnswerID = 0030030,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030029
            });
            InstanceList.Add(new Instance
            {
                Text = "You then remember a particular event that happened a week ago. You ran into Astral during one morning, and then she proceeded to make some\n" +
                "\tambiguous threats towards you. Could it be that she's somehow behind all this? What reason would she have to take such drastic measures?\n" +
                "\t\"I guess I have one theory. I think it might have been Astral. I ran into her some time ago, and it sounded like she was planning something,\"\n" +
                "\tyou explain to Ardent.\n",
                ID = 0030007,
                AnswerID = 0030027,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Sounds a bit far fetched to me. Why would she have even done it?\" Ardent asks sceptically.\n" +
                "\t\"I don't know. I just wish she would leave me alone,\" you answer.\n",
                ID = 0030007,
                AnswerID = 0030027,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030026
            });
            InstanceList.Add(new Instance
            {
                Text = "\"It doesn't matter right now, we can ask her about it later. What matters now is making sure we stop the vote from happening in the future,\"\n" +
                "\tdad chimes in. You and Ardent both agree.\n",
                ID = 0030007,
                AnswerID = 0030026,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030028
            });
            InstanceList.Add(new Instance
            {
                Text = "\"So, I guess the next thing to do is to make sure Silver doesn't get thrown out. We've already planned everything with the others who\n" +
                "\tjoined us in the rebellion, so we shouldn't have any problems with that,\" Ardent explains. \"You just make sure not to leave the Stable, alright?\"\n" +
                "\the adds, poking you in the chest playfully.\n" +
                "\t\"I'll try to remember that,\" you answer.\n",
                ID = 0030007,
                AnswerID = 0030028,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 2,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"But seriously, what happened? There's no way you actually had the most votes. Are you sure the program you made works as it's\n" +
                "\tsupposed to?\" Ardent asks. You act shocked by the question.\n" +
                "\t\"Are you suggesting I'm bad at my job?\" you ask in a playful manner. Now it's Ardent's turn to roll his eyes.\n",
                ID = 0030007,
                AnswerID = 0030025,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 9,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"And it looks like you will be part of the rebellion too, whether you wanted to or not,\" Ardent says to you, half-joking.\n" +
                "\t\"I guess so,\" you reply. You really didn't want to get involved in it, but it seems like fate had other ideas.\n",
                ID = 0030007,
                AnswerID = 0030024,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030025
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Silver!\" dad exclaims, and leaps in for a hug. \"Well, that was an unexpected turn of events,\" he continues after he lets you go.\n" +
                "\t\"Not really,\" you almost say aloud, but stop at the last second. Wait, what? You shake your head to clear your thoughts.\n" +
                "\t\"Yeah,\" you reply simply.\n",
                ID = 0030007,
                AnswerID = 0030020,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 1,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Going for a new look, huh? Where'd you get that jumpsuit?\" dad asks after getting a closer look at you.\n" +
                "\t\"Oh, Cross Stitch gave it to me. Said it would keep me warm,\" you answer. He seems impressed about the outfit.\n",
                ID = 0030007,
                AnswerID = 0030022,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030021
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Alright, what the fuck is going on? How did you manage get the most votes? I don't think most ponies here even know you exist!\" Ardent says\n" +
                "\tafter closing the door behind him. You raise an eyebrow at him.\n" +
                "\t\"Oh, I uh- meant that in a good way, heh,\" he then says, somewhat embarrassed. You roll your eyes at that.\n" +
                "\t\"Your guess is as good as mine,\" you respond.\n",
                ID = 0030007,
                AnswerID = 0030021,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030023
            });
            InstanceList.Add(new Instance
            {
                Text = "\"It's a good thing we decided to do the rebellion this year,\" dad says, lowering his voice.\n" +
                "\t\"Oh, Ardent recruited you to the rebellion too?\" you ask him.\n" +
                "\t\"He asked me to join some time ago, actually,\" dad replies. You nod in acknowledgement.\n",
                ID = 0030007,
                AnswerID = 0030023,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 6,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Alright, what should I pack with me?\" you think aloud. Maybe some food in case the rebellion fails and you actually get kicked out? No,\n" +
                "\tdon't think like that. But just in case? You grab your saddlebags from the corner, and go over to the shelf at the back of the room. You admire the\n" +
                "\tcollection of Sparkle-Cola merch for a moment, before taking the six bottles from the shelf and putting them into the saddlebags.\n",
                ID = 0030007,
                AnswerID = 0030018,
            });
            InstanceList.Add(new Instance
            {
                Text = "You then open your drawer and pull out a box of sugar apple bombs. What, can't a mare have some snacks hidden away for those lazy days when\n" +
                "\tthey don't want to leave the room?\n",
                ID = 0030007,
                AnswerID = 0030018,
            });
            InstanceList.Add(new Instance
            {
                Text = "After placing the box of sugary goodness in your saddlebags, you hear the door open behind you. Sighing heavily, you turn around ready to give\n" +
                "\ta few carefully chosen words to the security ponies. But to your pleasant surprise, you see Ardent and your dad walk into the room.\n",
                ID = 0030007,
                AnswerID = 0030018,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030020
            });
            InstanceList.Add(new Instance
            {
                Text = "You then take off your stable suit and toss it on the bed, and start putting on the jumpsuit. Instead of only taking up the top half of your\n" +
                "\tbody like the normal stable suits do, the jumpsuit covers your whole body. Stitch was right, this is pretty warm. There is also quite a few pockets\n" +
                "\ton the jumpsuit. Stitch really has put the effort in to this.\n",
                ID = 0030007,
                AnswerID = 0030019,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030018
            });
            InstanceList.Add(new Instance
            {
                Text = "You start thinking what you're going to pack up. You don't want to take too much stuff to haul around the Stable, but not too little to be\n" +
                "\tsuspicious. Wait, how much stuff are you allowed to even take with you? You decide to ask the security ponies.\n" +
                "\t\"Hey, is there a limit to how much stuff I can take with me?\" you ask.\n" +
                "\t\"We don't have a strict limit to it, but try not to take half the Stable with you,\" the older stallion says. You give a small \"hmm\" in response.\n",
                ID = 0030005,
                AnswerID = 0030010,
            });
            InstanceList.Add(new Instance
            {
                Text = "You finally get to your room. As you step into your room, so do the security ponies, much to your dismay. You would much prefer to not have\n" +
                "\tthe security snooping around in your room.",
                ID = 0030005,
                AnswerID = 0030010,
            });
            InstanceList.Add(new Instance
            {
                Text = "You then collapse on the bed. This day is already more stressful than the past week. Sighing deeply, you decide to just lay there for a minute.\n",
                ID = 0030006,
                AnswerID = 0030017,
            });
            InstanceList.Add(new Instance
            {
                Text = "After getting up from your bed, you take a moment to look at yourself on the mirror. A mare with a silver coat and a dark gray and red mane looks\n" +
                "\tback at you with tired, pink eyes. There is a picture of a shiny shift key is on the mare's flank. Overall a really boring and average pony, you think\n" +
                "\tto yourself.\n",
                ID = 0030006,
                AnswerID = 0030017,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 1,
            });
            InstanceList.Add(new Instance
            {
                Text = "The security ponies look at each other for a moment. The older pony then shrugs.\n" +
                "\t\"Fine. But no funny business, got it?\" he says. You nod in response. They then back out of the room, and close the door. You sigh in relief, already\n" +
                "\tgetting tired of being watched by those two.\n",
                ID = 0030006,
                AnswerID = 0030015,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030017
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I wouldn't act so bitchy if I were in your spot,\" the younger stallion says.\n" +
                "\t\"Uh-huh. Well, could you please leave me alone for a moment?\" you ask in a mocking tone. The two look at each other for a moment. The older pony then\n" +
                "\trolls his eyes with a sigh, and backs out of the room. The other one leaves too, and closes the door behind him. You sigh in relief, already getting\n" +
                "\ttired of being watched by those two.\n",
                ID = 0030006,
                AnswerID = 0030016,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 5, 10 }),
                RedirectInstance = 0030017,
                Faction = "Security",
                RepPoints = 1,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I just wanted to say that I really appreciated the work you did for the Stable. I know it must have been annoying having to come help me so often,\"\n" +
                "\tshe says. She's not wrong, you think to yourself, but decide not to say it aloud.\n" +
                "\t\"So I wanted to give you something in return,\" she says, and pulls out a neatly folded Stable Suit from her saddlebags. \"It's a maintenance jumpsuit\n" +
                "\tthat I modified a little in my spare time,\" she says.\n",
                ID = 0030005,
                AnswerID = 0030014,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh wow. You didn't have to,\" you reply.\n" +
                "\t\"Oh don't give me any of that. I just hope it at least keeps you warm when you leave. I don't know if it's cold outside of the Stable, so better safe\n" +
                "\tthan sorry, right?\" she says, and holds the jumpsuit out for you. You pick it up with your magic. It's surprisingly heavy, and on closer inspection it\n" +
                "\tseems to have been reinforced with leather in some places.\n",
                ID = 0030005,
                AnswerID = 0030014,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Thanks. It means a lot to me,\" you say to her. She gives a small smile.\n" +
                "\t\"No problem. I won't hold you up any longer, so uh... goodbye,\" she says. You say your goodbyes, and continue walking towards your room.\n",
                ID = 0030005,
                AnswerID = 0030014,
            });
            InstanceList.Add(new Instance
            {
                Text = "As you get to your room and step inside, so do the security ponies, much to your dismay.",
                ID = 0030005,
                AnswerID = 0030014,
            });
            InstanceList.Add(new Instance
            {
                Text = "Usually you don't really enjoy hugging random ponies, but this time you can't help but get a warm feeling in your chest as you hug Stitch back.\n" +
                "\t\"It shouldn't go like this. Good ponies getting voted out,\" she says. Sounds like she's trying to hold back tears.\n",
                ID = 0030005,
                AnswerID = 0030013,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030014
            });
            InstanceList.Add(new Instance
            {
                Text = "\"No it's not! You don't deserve to get thrown out!\" she wails. You can't really argue with that.\n",
                ID = 0030005,
                AnswerID = 0030012,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030014
            });
            InstanceList.Add(new Instance
            {
                Text = "As you get to the living quarters, you hear somepony yelling to get your attention. You turn around and see Cross Stitch running to catch up with you.\n" +
                "\t\"Silver! There you are!\" she says as she walks next to you, out of breath. She then throws her hooves around your neck and gives you a tight hug.\n" +
                "\t\"I'm sorry,\" she says, sounding sorrowful.",
                ID = 0030004,
                AnswerID = 0030011,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"No need for me to be there. The Pariah will always come here to get their PipBuck disconnected from the network, so I'll see the 'winner' anyway,\"\n" +
                "\tshe explains. Makes sense, you figure.\n",
                ID = 0030003,
                AnswerID = 0030008,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030009
            });
            InstanceList.Add(new Instance
            {
                Text = "She looks at you for a few more moments before continuing.\n" +
                "\t\"Well, should we get started? Don't wanna keep you here for too long, I imagine you have other things to take care of. Sit down, please,\" she says.\n" +
                "\tYou do just that. She then connects your PipBuck to her terminal via a cord, and starts typing something. You sit in silence for a minute.\n" +
                "\t\"Stable-Tec has instructed us to unlock all of the spells of the PipBuck once we leave the Stable, so that's what I've been doing to all of the\n" +
                "\tPariahs too. This might feel a little weird,\" she finally says. You raise an eyebrow at that. What might feel w-\n",
                ID = 0030003,
                AnswerID = 0030009,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Woah!\" you exclaim, as colors start to move in your vision. You blink several times, and bring a hoof to rub at your eyes.\n" +
                "\t\"That's your Eyes Forward Sparkle, or E.F.S. for short. It's like a compass in your vision. It's also able to identify any nearby living creatures,\"\n" +
                "\tshe explains. Once the colors stop shifting, you open your eyes. And sure enough, you can see a compass-like bar at the bottom of your vision.\n" +
                "\tThere's a yellow dot in the middle of the bar, indicating the direction where Chip is.\n" +
                "\t\"Huh, pretty neat. I didn't know these had more functionality than just the basics,\" you say while looking at your PipBuck.\n",
                ID = 0030003,
                AnswerID = 0030009,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"That's not even the best part. Try tensing up your body,\" she encourages. You do as instructed, and... time seems to come to a halt. You\n" +
                "\tcan't seem to move, or even blink. You almost start to panic, but then hear a mare's voice in your head.\n",
                ID = 0030003,
                AnswerID = 0030009,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Welcome to Stable-Tec Arcane Targeting Spell! This handy little spell is designed to help you survive in the post-apocalyptic Equestria, by\n" +
                "\thelping you combat any hostiles you may encounter in your endeavour to rebuild Equestria to it's former glory!\" the voice declares. \"Everything's\n" +
                "\tcontrolled by your mind, so just think about choosing your target, the number of attacks you want to do, and then just activate it! The rest will\n" +
                "\tbe handled by the spell. Warning! Stable-Tec is not responsible for any injuries the spell may inflict on you or your loved ones if used\n" +
                "\tincorrectly. Have a great day!\" the voice finishes. Time starts to resume, and you inhale sharply.\n",
                ID = 0030003,
                AnswerID = 0030009,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"It's great, isn't it? The spell doesn't actually stop time, it just accelerates your senses to the point where it feels like the world comes\n" +
                "\tto a stop,\" Chip says.\n" +
                "\t\"Gonna take some getting used to, that's for sure,\" you answer. She presses a few more keys on the terminal, and then disconnects the cord from\n" +
                "\tyour PipBuck.\n" +
                "\t\"And that's that. You can go now,\" she says.\n" +
                "\t\"Thanks, Chip. And uh, goodbye, I guess,\" you say. She nods in response. You hope it isn't actually a goodbye. You then leave the service\n" +
                "\tstation, the security ponies following close behind.\n",
                ID = 0030003,
                AnswerID = 0030009,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Where to next?\" you ask the two.\n" +
                "\t\"Up to you. I'd spend the time wisely if I were you,\" one of them says.\n" +
                "\t\"I guess I'll go pack my stuff,\" you say, and start walking towards the elevator. The security ponies follow. It's already starting to annoy you.\n",
                ID = 0030003,
                AnswerID = 0030009,
            });
            InstanceList.Add(new Instance
            {
                Text = "As you're walking the hallways, you get sad looks from most ponies that pass by you. The extra attention you're getting is slightly uncomfortable,\n" +
                "\tbut you do your best to ignore them.\n",
                ID = 0030003,
                AnswerID = 0030009,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 1,
            });
            InstanceList.Add(new Instance
            {
                Text = "The rest of the way is made in silence. You take the elevator to the third level. Now that you've had a little time to process the situation,\n" +
                "\tyou find some relief in the fact that the rebellion happened to be this year. The only thing left to do now is to hope that it'll be successful.\n" +
                "\tBecause if it isn't... Well, better not to think about that.\n",
                ID = 0030002,
                AnswerID = 0030007,
            });
            InstanceList.Add(new Instance
            {
                Text = "You arrive at the service station. The door slides open with a hydraulic hiss, revealing a small workshop with various tools on the wall,\n" +
                "\tPipBucks on a shelf in one corner, and a terminal on a small table in the middle of the room. The Stable's PipBuck technician, named Chip Set,\n" +
                "\tis sitting at the terminal. She has a green coat with a darker green mane. She then notices you.\n" +
                "\t\"Silver?\" she asks, clearly surprised to see you.\n" +
                "\t\"Hey Chip,\" you answer simply. She stares at you for a moment.\n" +
                "\t\"Sorry, I just... wasn't expecting to see you here,\" she finally says with a sad smile.",
                ID = 0030002,
                AnswerID = 0030007,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"You have no idea how many times I've heard that same line,\" says the pony in front of you with a small chuckle.\n" +
                "\t\"B-but I need to talk to the Overmare, I'm sure that-\"\n" +
                "\t\"Shut up and keep walking. You can go annoy the Overmare after we've been to the PipBuck service station,\" the pony behind you says. You sigh,\n" +
                "\tand keep going.\n",
                ID = 0030002,
                AnswerID = 0030006,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 5, 10 }),
                RedirectInstance = 0030007,
                Faction = "Security",
                RepPoints = 1,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"To the PipBuck service station,\" the pony in front of you says.\n" +
                "\t\"Wait, am I getting my PipBuck removed?!\" you ask. At this point, you can't even imagine being without the little device. It's just way too useful.\n" +
                "\t\"No, they'll just disconnect it from the Stable's network and wipe any sensitive information it may have in its memory,\" the pony behind you\n" +
                "\texplains. Well that's a relief. But then you remember the situation you're in, and all feeling of relief is instantly replaced with dread.\n",
                ID = 0030002,
                AnswerID = 0030005,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030007
            });
            InstanceList.Add(new Instance
            {
                Text = "You start munching on your hayburger. Pretty good, you think to yourself. Seems like the workers at the cafeteria wanted to make today's menu\n" +
                "\tmore special than usual.\n" +
                "\t\"Can't wait to see what's for dinner. It's not everyday the cafeteria puts this much effort into the food,\" you say after swallowing. Ardent nods.\n" +
                "\t\"Yeah. I hope it's carrot soup, we haven't had that for quite some time,\" he says. You roll your eyes at that.\n" +
                "\t\"We had that last week,\" you say.\n" +
                "\t\"I know,\" he says with a grin. You give a little chuckle.\n",
                ID = 0030001,
                AnswerID = 0030004,
            });
            InstanceList.Add(new Instance
            {
                Text = "You both eat for a while without saying anything. You take a moment to gaze over the ponies in the cafeteria. You can sense some tension in\n" +
                "\tthe air, which is no surprise. Although some groups don't seem that concerned, and are instead talking and laughing like normal. You then notice\n" +
                "\ta group of security ponies walking towards the platform. About 20 of them, you estimate. In the middle of the ponies you can see the Overmare.\n" +
                "\tShe's carrying a small saddlebag on her back. As she gets to the platform, she pulls out a few papers from her saddlebags on to the small podium\n" +
                "\tin front of her. You glance at Ardent, who seems to look at the Overmare with no small amount of hatred. Not that you can blame him.\n",
                ID = 0030001,
                AnswerID = 0030004,
            });
            InstanceList.Add(new Instance
            {
                Text = "The security ponies then spread out evenly in front of the platform, as the Overmare prepares for her speech.\n" +
                "\t\"Good day, residents of Stable 54,\" the Overmare starts speaking to the microphone. The ponies in the cafeteria fall silent. \"Welcome to the\n" +
                "\tannouncement of this year's Pariah. Now, those of you who know the history of our Stable know that this year marks the 120th anniversary of the vote,\"\n" +
                "\tthe Overmare continues. Now that the Overmare mentioned the anniversary, you remember Ardent talking about it some time back.\n" +
                "\t\"A great tradition started by my family to keep our Stable a safe place for ponykind to survive, by weeding out those who could harm our\n" +
                "\tway of life, and by extension, the chances of our survival as a species,\" the Overmare says. You roll your eyes. The Overmare makes it sound way too\n" +
                "\tdramatic.\n",
                ID = 0030001,
                AnswerID = 0030004,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"So, for this anniversary I wanted to improve the voting system to be as quick and non-disruptive as possible. After some planning, I came to\n" +
                "\tthe conclusion that it would be easiest to hold the voting via our terminals. And from now on, all future votes will be held the same way,\"\n" +
                "\tthe Overmare explains.\n" +
                "\t\"Gee, not even a thanks for the quick work?\" you mumble under your breath as you start eating your piece of cake. You don't even remember\n" +
                "\twhen was the last time the Stable had cake. But when there is cake, it just seems to taste better and better each time.\n",
                ID = 0030001,
                AnswerID = 0030004,
            });
            InstanceList.Add(new Instance
            {
                Text = "The Overmare continues her speech for what feels like hours but is actually only 15 minutes according to your PipBuck.\n" +
                "\t\"And with that, I think it's time we hear this year's Pariah,\" the Overmare finally says. That seems to immediately make the air thick with\n" +
                "\tanticipation. The quiet chatter dies down completely as everypony turns their gazes to the Overmare. She rips open an envelope, and pulls out a piece\n" +
                "\tof paper. She eyes the paper for a moment before continuing.\n" +
                "\t\"With 128 votes, I hereby announce this years Pariah to be,\" she pauses for a second.\n",
                ID = 0030001,
                AnswerID = 0030004,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Silver Shift.\"\n",
                ID = 0030001,
                AnswerID = 0030004,
            });
            InstanceList.Add(new Instance
            {
                Text = "You feel your heart skip a beat. You glance at Ardent, who seems just as appalled as you.\n" +
                "\t\"Wh-what...\" you try to utter out. A few ponies look at your way, while most others seem either releaved or confused.\n" +
                "\t\"Please gather your belongings, and prepare to leave. You have 30 minutes,\" the Overmare orders. You see two of the security ponies start walking\n" +
                "\ttowards you. You can feel your heart racing in your chest, as you try to comprehend the situation. Then Ardent pulls your hoof towards him, getting\n" +
                "\tyour attention.\n" +
                "\t\"Do as they say, pack your stuff as if you're going to leave. We're not gonna let you go, trust me,\" he quickly whispers to you. You nod in response.\n",
                ID = 0030001,
                AnswerID = 0030004,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Let's go Miss Shift,\" one of the security ponies says as they approach you. Ardent gives you an encouraging nod. You shakily get up from your\n" +
                "\tseat. You can feel everypony staring at you, which just makes you even more nervous. You then follow one of the security ponies out of the cafeteria,\n" +
                "\twhile the second one keeps tail.",
                ID = 0030001,
                AnswerID = 0030004,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I know you refuse to sit anywhere else but here,\" he says with a grin.\n" +
                "\t\"Hey, it's the best place in the cafeteria,\" you say with a smile of your own.\n",
                ID = 0030001,
                AnswerID = 0030002,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030004
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Let me guess, you were so absorbed in something you were doing at your terminal that you didn't notice the time?\" he asks with a knowing smile.\n" +
                "\t\"What, me? Never,\" you say, trying to sound innocent.\n",
                ID = 0030001,
                AnswerID = 0030003,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0030004
            });
            InstanceList.Add(new Instance
            {
                Text = "Time flies by while playing Wastebuck. You're in the middle of a zebra ambush when the Stable's PA system crackles to life.\n" +
                "\t\"Good day, residents of Stable 54,\" comes the sound of the Overmare over the speakers. \"The voting has now been closed, and the results will be\n" +
                "\tannounced at the cafeteria in half an hour,\" says the Overmare. You didn't even notice that lunch has already started. You save the game, turn off\n" +
                "\tthe terminal, and start making your way towards the cafeteria.\n",
                ID = 0030000,
                AnswerID = 0020057,
            });
            InstanceList.Add(new Instance
            {
                Text = "You take note on how empty the hallways are. But you already know that most ponies will be in the cafeteria, which means the place will be even\n" +
                "\tmore packed than usual. And sure enough, as you get to there, it's just as full as you had expected. You also notice that a small platform has been set\n" +
                "\tup at the back wall of the room, where the Overmare will make the announcement.\n",
                ID = 0030000,
                AnswerID = 0020057,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 6,
            });
            InstanceList.Add(new Instance
            {
                Text = "You go to the counter, pick up a glass of apple juice, a hayburger and... cake! The cafeteria is really putting the effort in. You then start\n" +
                "\tlooking for a free seat. You go check behind the corner if your usual spot is taken, and see Ardent sitting there with a free seat opposite of him.\n" +
                "\t\"Hey Silver,\" he says as you sit down.\n" +
                "\t\"Hey. Kinda crowded in here, huh?\" you say.\n" +
                "\t\"Yeah. That's why I came here a bit early, to get our usual spot,\" he says.",
                ID = 0030000,
                AnswerID = 0020058,
            });
            InstanceList.Add(new Instance
            {
                Text = "You go to the counter, pick up a glass of apple juice, a hayburger and... cake! The cafeteria is really putting the effort in. You then start looking\n" +
                "\tfor Ardent in the crowd. You go check behind the corner if your usual spot is taken, and see Ardent sitting there with a free seat opposite of him.\n" +
                "\t\"Hey Silver,\" he says as you sit down.\n" +
                "\t\"Hey. Kinda crowded in here, huh?\" you say.\n" +
                "\t\"Yeah. That's why I came here a bit early, to get our usual spot,\" he says.",
                ID = 0030002,
                AnswerID = 0020059,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Wait, I can't leave the orchard, I need to stay here!\" she says. You look at her quizzically.\n" +
                "\t\"You can use the orchard's terminal, right?\" you ask her.\n" +
                "\t\"But don't I need to be on my machine to vote?\" she asks dumbfounded. You facehoof.\n" +
                "\t\"You can login to your account on any of the terminals in the Stable.\" you explain to her. It's infuriating to explain basic things about tech to some\n" +
                "\tponies. She looks puzzled a moment.\n" +
                "\t\"Huh, alright then,\" she says. You sigh in response, and start walking towards the orchard's small office.\n",
                ID = 0020026,
                AnswerID = 0020055,
            });
            InstanceList.Add(new Instance
            {
                Text = "At the office, you boot up the terminal. There's a post-it note on the side of the terminal with the orchard's username and passoword written on it.\n" +
                "\tYou frown upon seeing it, but decide to not go on a rant about cybersecurity. The terminal then shows the login page.\n" +
                "\t\"Here, enter your username and password,\" you say. Autumn does that, very slowly. That's another thing you find annoying about some ponies. Once she's\n" +
                "\tlogged in, she goes to her inbox, and opens the message that has her code and the link to the program.\n",
                ID = 0020026,
                AnswerID = 0020055,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Alright, your code is A113. You can copy that like this,\" you say as you copy the code to the clipboard and go to the link, \"and paste it like this,\"\n" +
                "\tyou finish as you paste the code to the program. You then press enter and... an error pops up on the screen. \"This code has already been used.\"\n" +
                "\tYou raise an eyebrow at that.\n" +
                "\t\"You've already voted. You can't vote twice.\" you say to Autumn.\n" +
                "\t\"But it said the same thing when I first tried it,\" she says. You somehow doubt that.\n",
                ID = 0020026,
                AnswerID = 0020055,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Is that so? Well, I guess I can go check that for you from the database,\" you say.\n" +
                "\t\"That would be great, thanks! Here, let me give you those apples,\" she says as she goes to a nearby basket filled with apples. She takes a small\n" +
                "\tplastic bag from a nearby table, and puts four apples in to the bag.\n" +
                "\t\"Here you go,\" she says as she hands the bag over to you. You lift it with your magic.\n" +
                "\t\"Thanks. I'll swing by when I know what the problem is,\" you say to her, and turn to leave.\n",
                ID = 0020026,
                AnswerID = 0020055,
            });
            InstanceList.Add(new Instance
            {
                Text = "After leaving the orchard, you briefly think about going to the IT-department to check the database for Autumn's vote, but come to the conclusion that\n" +
                "\tyou can't be bothered to do any more work related stuff on your holiday. You can always tell Autumn that the problem was hard to find, and you couldn't\n" +
                "\tget it sorted out in time, you think to yourself. You then start heading towards your room. Once you're back on the second level, you take notice to\n" +
                "\tthe growing number of ponies in he hallways and cafeteria. You quickly weave through the ponies towards the living quarters, as the crowd is making\n" +
                "\tyou feel uncomfortable.\n",
                ID = 0020026,
                AnswerID = 0020055,
            });
            InstanceList.Add(new Instance
            {
                Text = "Finally at your room, you sit down at your terminal, grab one of the apples from the bag with your magic, and take a bite. Even better than the one\n" +
                "\tyou ate at the cafeteria, you think to yourself. You munch on the apple for a few more moments, after which you start up Wastebuck on your terminal.\n" +
                "\tNowadays you don't have nearly as much time to play videogames as when you were a filly.\n",
                ID = 0020026,
                AnswerID = 0020055,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020057
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I'm pretty sure. But knowing my hooves, maybe I did mess it up,\" she says with a small chuckle. You roll your eyes at that.",
                ID = 0020025,
                AnswerID = 0020056,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 9 }),
                RemoveAnswer = 0020056
            });
            InstanceList.Add(new Instance
            {
                Text = "\"So, what brings you here?\" she asks.\n" +
                "\t\"I thought I'd come check if you have any apples to spare? You know, because of the holiday,\" you say hopefully. She scrunches her muzzle in\n" +
                "\tthought for a moment.\n" +
                "\t\"Well... There wasn't enough spare to go for the entire Stable. Only the workers here at the orchard got some. But I could give you some of my\n" +
                "\town, if you help me with something,\" she says.\n",
                ID = 0020025,
                AnswerID = 0020054,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Uh, sure. What do you need help with?\" you ask, hoping that it isn't any physical work.\n" +
                "\t\"You made the new voting system, right? I'm having some troubles with it,\" she says.\n" +
                "\t\"Oh, what kind of trouble?\" you ask curiously.\n" +
                "\t\"It keeps saying something about my number, and I'm not sure what to do about it,\" she explains. You can already guess that most likely she's just\n" +
                "\tentering her code incorrectly, which makes you feel a bit frustrated.",
                ID = 0020025,
                AnswerID = 0020054,
            });
            InstanceList.Add(new Instance
            {
                Text = "She scrunches her face for a moment.\n" +
                "\t\"I'm sorry dear, but all of the apples we've harvested have gone straight to the storage,\" she says.\n" +
                "\t\"Really? Damn, getting only one in the cafeteria really just makes you want more,\" you say, disappointed.\n" +
                "\t\"Yes, but you know how it is. Need to keep the food stock up, we only give them out if we have any excess,\" she explains.\n" +
                "\t\"Oh well. Guess I'll go then,\" you say and turn to leave.\n",
                ID = 0020024,
                AnswerID = 0020053,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Alright, feel free to visit again sometime!\" Autumn says as you leave the orchard. Well that was a wasted effort, you think to yourself. Feeling a\n" +
                "\tlittle disappointed, you decide to go get a bottle of Sparkle-Cola from the bar. You take the elevator back to the second level, and make your way to the\n" +
                "\tbar. This isn't a place you like to be, since there's always so many ponies around. But this is the only place in the Stable where they have\n" +
                "\tSparkle-Cola, so you're forced to come here when you run out of them. And it looks like this place is even more packed than usual. Some ponies start\n" +
                "\tdrinking quite early, it seems.\n",
                ID = 0020024,
                AnswerID = 0020053,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hey there gorgeous,\" says the bartender over the counter. He has a red coat and a slightly darker red mane. His a cutiemark is a drinking glass.\n" +
                "\t\"Hello, Cherry. Give me six Sparkle-Colas, please,\" you say to the stallion.\n" +
                "\t\"What, no alcohol even on a holiday?\" he teases.\n" +
                "\t\"You know I don't drink,\" you say to him, not in the mood for a chat.\n",
                ID = 0020024,
                AnswerID = 0020053,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"How about this, you come over to my place this evening, and we'll taste some of the better drinks I have stashed away?\" he asks, clearly hopeful.\n" +
                "\t\"Look, I'm flattered. But I really don't feel like going on a date, especially today. Besides, surely you can get a better marefriend than me?\" you ask\n" +
                "\thim. You're not sure if he likes you, or if he just flirts with every mare that comes to the bar.\n",
                ID = 0020024,
                AnswerID = 0020053,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Ah, don't sell yourself short. Some other time then, huh? Here's the colas,\" he says, and gives you the bottles. You thank him, and head to your\n" +
                "\troom. \"Should've taken my saddlebags,\" you mumble to yourself as you carry the bottles in your magic. As you walk, you notice that the hallways are\n" +
                "\tgetting even more crowded. You almost lose the grip on your bottles when a group of ponies accidentally bumps into you.\n" +
                "\t\"Watch where you're going!\" one of the ponies says to you. You don't pay them any mind and just continue walking, not wanting any more social\n" +
                "\tinteraction.\n",
                ID = 0020024,
                AnswerID = 0020053,
            });
            InstanceList.Add(new Instance
            {
                Text = "Back in your room, you decide to spend the time sipping on your Sparkle-Cola and playing some Wastebuck. Nowadays you don't have as much time to play\n" +
                "\tvideogames as when you were a filly. Life was so simple back then, you think to yourself.",
                ID = 0020024,
                AnswerID = 0020053,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020057
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Well, I do try to stay in shape. Exercise is good for your health, you know. And speaking of health, have you remembered to get up from your machine\n" +
                "\tevery once in a while? It's not good for you to sit around so much,\" she says as she takes your right forehoof in her magical grip and examines it.\n" +
                "\t\"Yes, Autumn, I have,\" you sigh and pull back your hoof.\n",
                ID = 0020023,
                AnswerID = 0020052,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020054
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Do I? Well it's true. I remember when you were just a tiny filly,\" she says and ruffles your mane. She's always been sort of grandma figure for\n" +
                "\tthe Stable, likely because she doesn't have any grandchildren of her own.\n",
                ID = 0020023,
                AnswerID = 0020051,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020054
            });
            InstanceList.Add(new Instance
            {
                Text = "You take the elevator down to the orchard. During the day the voting ends, the orchard sometimes gives out apples to the residents of the Stable.\n" +
                "\tUsually all of the apples go to the cafeteria to be then used sparingly to make them last as long as possible. However, the orchard tries to set aside\n" +
                "\ta small amount of them for special occasions, like today.\n",
                ID = 0020022,
                AnswerID = 0020050,
            });
            InstanceList.Add(new Instance
            {
                Text = "As you step inside the orchard, you immediately feel the air get a bit warmer. The room is about as big as the cafeteria, with several rows of\n" +
                "\tapple trees lined up on long garden plots. There are also specialized lights that simulate sunlight over each tree, giving a yellow-ish tint to the\n" +
                "\twhole room.\n" +
                "\t\"Well hello there, Silver!\" comes the voice of Autumn Morning from between the rows of trees. She's one of the ponies who work at the orchard. She's\n" +
                "\ta bit older, only a couple years until she retires if you remember correctly.\n" +
                "\t\"Hey Autumn,\" you say as she walks over to you and gives you a bone-crushing hug.\n" +
                "\t\"My my, you've grown since the last time I saw you,\" she says as she lets you go.",
                ID = 0020022,
                AnswerID = 0020050,
            });
            InstanceList.Add(new Instance
            {
                Text = "With that out of the way, you close the message board. Your job would be so much easier if everypony had even basic knowledge about terminals,\n" +
                "\tyou think to yourself. You then decide to play some games on your terminal to pass the time. Nowadays you don't have much time to do that, unlike\n" +
                "\twhen you were a filly. Life was so simple back then, you think to yourself.\n",
                ID = 0020021,
                AnswerID = 0020049,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020057
            });
            InstanceList.Add(new Instance
            {
                Text = "You press send. Short and straight to the point, that should get the message across.\n",
                ID = 0020021,
                AnswerID = 0020046,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020049
            });
            InstanceList.Add(new Instance
            {
                Text = "You press send. You keep the frustration down and try your best to reply professionally.\n",
                ID = 0020021,
                AnswerID = 0020047,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020049
            });
            InstanceList.Add(new Instance
            {
                Text = "You press send. You are really not in the mood to deal with these kinds of ponies.\n",
                ID = 0020021,
                AnswerID = 0020048,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020049
            });
            InstanceList.Add(new Instance
            {
                Text = "\"If you say so. I just don't want anyone to get hurt. Or worse... Who knows how much force the security will try to use,\" you wonder. You\n" +
                "\tdistantly remember somepony getting shot after they refused to leave the Stable after getting voted out when you were young.\n" +
                "\t\"As long as we don't get violent, the security will do the same. I have a few friends in security that agreed to join the rebellion. I asked them\n" +
                "\tabout how much the security has been authorised to use force in situations like this,\" he says, almost as if he was expecting you to ask that.\n",
                ID = 0020020,
                AnswerID = 0020045,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"That's good. At least some peace of mind,\" you say, feeling a little relief. Maybe the rebellion will go smoothly, and you're just worrying\n" +
                "\ttoo much. Ardent sure seems to have things under control. Just like he always has. He's the type to always keep his cool in stressful situations.\n" +
                "\t\"Anyways, I think I'll leave you be. I have some stuff to do in the maintenance wing,\" he says as he stands up from the bed.\n" +
                "\t\"But don't you have a day off today?\" you ask.\n",
                ID = 0020020,
                AnswerID = 0020045,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I was supposed to have one, yeah. But something came up this morning. The Stable doesn't care that it's my day off, heh,\" he says jokingly.\n" +
                "\t\"Looks like I'm not the only one who has to work overtime,\" you observe.\n" +
                "\t\"I guess not. Hey, should we meet up for lunch today? We could listen the Overmare's speech together,\" he asks, stopping at your door.\n" +
                "\t\"Sure, I don't really have anyone else to hang out with,\" you answer.\n" +
                "\t\"Alright, see you then,\" he says, and leaves.\n",
                ID = 0020020,
                AnswerID = 0020045,
            });
            InstanceList.Add(new Instance
            {
                Text = "You watch the door close. You then turn on your terminal, almost instictively. You've gotten so used to the hum and soft glow of terminals that it\n" +
                "\tfeels weird to be in your room with your terminal off. You then check your mail, somewhat surprised to see there's no messages other than two\n" +
                "\tautomated ones from the message board you created a few years back. One of the messages is a weekly log of data you use to detect any outages in the\n" +
                "\tsystem among other things, and the other one is a report on the automated time-outs of users. Nothing interesting in either of those.\n",
                ID = 0020020,
                AnswerID = 0020045,
            });
            InstanceList.Add(new Instance
            {
                Text = "You decide to take a look to see if there's any interesting new threads on the board. It's quite rare, but every once in a while you can find an\n" +
                "\tinteresting thread. You open the message board, only to see a bunch of threads about the vote. Gossip, propaganda, and other boring stuff. As you\n" +
                "\tscroll down however, you notice a thread with a topic called \"new voting program sucks\". You open the thread made by a user called \"shadow1x\",\n" +
                "\tand start reading it.\n",
                ID = 0020020,
                AnswerID = 0020045,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"anypony else hate the new voting system? i kept trying to enter my code when the machine asked for it, but it always refused it. there\n" +
                "\twasnt anything wrong with the old system, so why try to fix something that isnt broken?\". Seriously? Messages like this are extremely infuriating for\n" +
                "\tyou. Very vague descriptions of problems, not appreciating the effort that went in to the project, and the whole \"old is better\" mentality of ponies.\n" +
                "\tYou feel the need to respond to the message, so you start typing.",
                ID = 0020020,
                AnswerID = 0020045,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yeah. Depending on how the Overmare'll react to the riot, we may have to overthrow her and pick a new leader for the Stable,\" he says. You can\n" +
                "\thear it in his voice that he clearly wants it to happen.\n" +
                "\t\"That would likely prove to be difficult,\" you say, thinking about how the Stable would react to being without an Overmare, even for a short time.\n" +
                "\t\"Difficult, yes, but we would manage,\" he says, sounding confident.\n",
                ID = 0020020,
                AnswerID = 0020044,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020045
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I was hoping you'd try to be a bit more supportive. I know it seems like a bad idea, but do we really have any choice if we want to end this\n" +
                "\tinsanity?\" he says disappointingly.\n" +
                "\t\"I guess we don't. But it still is dangerous, and I'm really worried about what happens if...\" you trail off.\n" +
                "\t\"That won't happen, I promise,\" he says comfortingly.\n",
                ID = 0020020,
                AnswerID = 0020043,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020045
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Pretty much, yeah. We'll have to see how the Overmare'll react. In the worst case, we'll have to overthrow her and pick a new leader for the\n" +
                "\tStable,\" he says matter-of-factly. You have a hard time trying to wrap your head around this whole thing. It's not everyday you face events this big.\n" +
                "\t\"How well would that even work out? I imagine there would be some unrest among everyone when there's no Overmare,\" you ponder out loud.\n" +
                "\t\"Hey, don't worry so much. Me and a couple others have things already planned out,\" he says comfortingly.\n",
                ID = 0020020,
                AnswerID = 0020042,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020045
            });
            InstanceList.Add(new Instance
            {
                Text = "You both continue to eat as you catch up on things. You learn that the Stable's reactor had an extensive maintenance done to it during the\n" +
                "\tpast week. Ardent explains that the reactor is divided into three different sectors and that it's able to run for a short period of time even\n" +
                "\tif two of the sectors are offline. That way the maintenance crew can work on the reactor while still providing power to the Stable.\n",
                ID = 0020019,
                AnswerID = 0020039,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 6,
            });
            InstanceList.Add(new Instance
            {
                Text = "After you've finished your breakfast, Ardent leaves to do some last minute preparations for the rebellion. You can't help but be worried about how\n" +
                "\tit will go. After a minute, you shove those thoughts aside. You then look at the applecore on your plate. \"Maybe the orchard has some spare apples.\n" +
                "\tMight as well visit the place while I wait for the vote to end,\" you think to yourself. You start heading to the orchard, which is located at the\n" +
                "\tfifth level of the Stable.\n",
                ID = 0020019,
                AnswerID = 0020040,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020050
            });
            InstanceList.Add(new Instance
            {
                Text = "After you've finished your breakfast, you two decide to go to your room to discuss the rebellion.\n" +
                "\t\"I've already given the run-down for the others who I've asked to join. We have about a quarter of the Stable's inhabitants with us on this,\"\n" +
                "\tArdent says to you while you're walking towards the living quarters. Part of you is worried that somepony has ratted out the plan to the Overmare,\n" +
                "\tbut you shove those thoughts aside.\n",
                ID = 0020019,
                AnswerID = 0020041,
            });
            InstanceList.Add(new Instance
            {
                Text = "Once you get to your room, you sit down at your terminal. Ardent sits down on your bed and seems to look at all the Sparkle-Cola merch on\n" +
                "\tyour shelf for a while.\n" +
                "\t\"I don't think I've said it before, but I really like your collection,\" he says while looking at the bottles. You smile proudly.\n" +
                "\t\"Thanks, I take pride in it. It was quite difficult to get a hold of some of those things,\" you explain to him, feeling good that somepony took\n" +
                "\tnotice to your collection.\n",
                ID = 0020019,
                AnswerID = 0020041,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"So. The plan is pretty simple. Once the time comes to throw The Pariah outside, we prevent them from going out, and demand the Overmare\n" +
                "\tto stop the vote and let The Pariah stay with us,\" he says.",
                ID = 0020019,
                AnswerID = 0020041,
            });
            InstanceList.Add(new Instance
            {
                Text = "He looks shocked by your statement.\n" +
                "\t\"Really? That's... quite alarming, to put it lightly. Have you told anypony else?\" he asks you.\n" +
                "\t\"No. Well, Scanline was there when she said it. But what does it matter? I doubt anything good is gonna come if I try to tell about it,\"\n" +
                "\tyou reason to him. He seems to think about it for a while.\n" +
                "\t\"I guess you're right,\" he admits in defeat, clearly wanting something to accuse the Overmare of.\n",
                ID = 0020019,
                AnswerID = 0020038,
            });
            InstanceList.Add(new Instance
            {
                Text = "Wanting to change the subject, you pick up your apple from the table with your magic.\n" +
                "\t\"Apples, huh? I guess Carrot Leaf wanted to treat us today,\" he says. You take a bite of the apple in your magic. It's a tad sour, but still good.\n" +
                "\t\"Yup. I wonder if she's planned something special for lunch and dinner, too,\" you say to him.\n",
                ID = 0020019,
                AnswerID = 0020038,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020039
            });
            InstanceList.Add(new Instance
            {
                Text = "He looks at you for a second, and sighs.\n" +
                "\t\"You're right. Sorry, I'm just frustrated about the vote,\" he says apologetically.\n" +
                "\t\"It's fine. But can we talk about something else now? I'm getting tired of the vote,\" you say to him.\n",
                ID = 0020019,
                AnswerID = 0020037,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020032
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Doesn't it bother you contributing to the voting process?\" he asks with a hint of disappointment in his voice.",
                ID = 0020018,
                AnswerID = 0020036,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I know that, but... doesn't it bother you? Making the voting process easier?\" he asks with a hint of disappointment in his voice.",
                ID = 0020017,
                AnswerID = 0020035,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Ah, I see,\" he says, and bites down on his bread. \"Wait, you created the new voting system, right?\" he continues after a few seconds.\n" +
                "\t\"Well yeah, I don't really know anypony else with enough knowledge of programming to do it,\" you answer.\n",
                ID = 0020017,
                AnswerID = 0020034,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 5,
            });
            InstanceList.Add(new Instance
            {
                Text = "Ardent smiles at you, and changes the subject.\n" +
                "\t\"Apples, huh? I guess Carrot Leaf wanted to treat us today,\" he says and gestures to the apples on the table. You take a hold of your apple with\n" +
                "\tyour magic and take a bite. It's a tad sour, but still good.\n" +
                "\t\"Yup. I wonder if she's planned something special for lunch and dinner, too,\" you say to him.\n",
                ID = 0020016,
                AnswerID = 0020032,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020039
            });
            InstanceList.Add(new Instance
            {
                Text = "\"No, not really. I've been planning the rebellion pretty much non-stop for the past week,\" he says, and yawns.\n" +
                "\t\"Trust me, I know how that feels like,\" you say to him, knowing full well how he must feel like.\n" +
                "\t\"Listen, I didn't mean to come off as hostile when I asked you about the rebellion. I know it's a dangerous thing, and I should've just accepted\n" +
                "\tyour response when you said you didn't want to join in. And I'm sorry for that,\" he apologizes to you.",
                ID = 0020015,
                AnswerID = 0020031,
            });
            InstanceList.Add(new Instance
            {
                Text = "He yawns. \"Yeah, I guess I am. And to be fair, I'm kinda nervous about the rebellion too. It has to work. If it doesn't...\" he trails off.\n" +
                "\tYou nod understandingly. \"Listen, I didn't mean to come off as hostile when I asked you about the rebellion. I know it's a dangerous thing, and\n" +
                "\tI should've just accepted your response when you said you didn't want to join in. And I'm sorry for that,\" he apologizes to you.",
                ID = 0020015,
                AnswerID = 0020030,
            });
            InstanceList.Add(new Instance
            {
                Text = "He seems to think about something for a few minutes while you two eat in silence.\n" +
                "\t\"Listen, I didn't mean to come off as hostile when I asked you about the rebellion. I know it's a dangerous thing, and I should've just accepted\n" +
                "\tyour response when you said you didn't want to join in. So... I'm sorry,\" he apologizes.",
                ID = 0020015,
                AnswerID = 0020029,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Morning! Haven't seen you in a while,\" he says to you, and sits down on the other side of the table. He looks a bit tired.",
                ID = 0020014,
                AnswerID = 0020028,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Morning,\" he says and sits on the other side of the table. He looks somewhat tired.",
                ID = 0020013,
                AnswerID = 0020027,
            });
            InstanceList.Add(new Instance
            {
                Text = "You decide to vote for Astral, surprise surprise. For the first time in quite a while, you don't actually feel like sitting at a terminal.\n" +
                "\tYou go lie down at your bed, and turn on the radio on your PipBuck. \"---vote. If y'all been livin' under ah rock, there's ah new votin' system\n" +
                "\tin test this time aroun'. All y'all need to do to vote is to open your intramail, go to the link in the mail an' input the included code when\n" +
                "\tthe machine asks for it. Then jus' select the name y'all want to vote for. It's that easy!\" explains Dust over the radio with that strange\n" +
                "\taccent of his. It's nice to hear that he likes the program.\n",
                ID = 0020013,
                AnswerID = 0020026,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"And next, some Sweetie Belle for y'all. Get Happy!\" he says, and a cheery song comes on. One of your favorites. \"Forget your troubles,\n" +
                "\tcome on, get happy! You'd better chase all your cares away!...\" the sweet sounds carry on from your PipBuck. Even though you've heard all of\n" +
                "\tthe songs countless times already, they're still good. The last time the radio got a new song was about 40 years ago, when there still was a\n" +
                "\tmusician in the Stable. But apparently she was voted out, due to some drama. Or so you've heard.\n",
                ID = 0020013,
                AnswerID = 0020026,
            });
            InstanceList.Add(new Instance
            {
                Text = "You slowly drift to sleep while listening to the radio, as the past week's lack of sleep finally catches up with you.\n",
                ID = 0020013,
                AnswerID = 0020026,
            });
            InstanceList.Add(new Instance
            {
                Text = "Next day...\n",
                ID = 0020013,
                AnswerID = 0020026,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Good mornin', my lil' ponies!\" you hear as you wake up. You look around to see where the voice came from, and glance at your PipBuck.\n" +
                "\tLooks like the radio was on for the whole night. \"Today's the day! This year's Pariah will be chosen today after lunch. Make sure y'all are\n" +
                "\tat the cafeteria by then!\" says Dust. You turn off the radio, and yawn deeply. You look at the time, and notice that you slept about 18 hours.\n" +
                "\t\"Huh, I guess I was a bit tired.\" you mumble to yourself. After laying on the bed for a few more minutes you get up, brush your mane and tail,\n" +
                "\tand head to the cafeteria to get some breakfast. You're feeling quite hungry.\n",
                ID = 0020013,
                AnswerID = 0020026,
            });
            InstanceList.Add(new Instance
            {
                Text = "As you come around the corner to the cafeteria, you see tons of decorations that have been hung up on the walls and the ceiling, in\n" +
                "\taddition to a few banners saying \"Happy judgement day!\". You never really understood why the Stable makes this day such a big thing that they\n" +
                "\thave to put decorations up for it. You go to the counter to get some water and bread, like you usually do. To your pleasant surprise, in addition\n" +
                "\tto the usual selection of bread and water, there are apples! The Stable's orchard is relatively small, meaning apples are quite the rare treat.\n" +
                "\tYou pick one up along with the bread and water, and go sit at your usual spot. It's a nice, quiet place in a corner, where nopony else usually\n" +
                "\tgoes to sit.\n",
                ID = 0020013,
                AnswerID = 0020026,
            });
            InstanceList.Add(new Instance
            {
                Text = "As you munch on your bread, you eye over the cafeteria and the ponies in there. The cafeteria is more packed than normally, likely because\n" +
                "\tof the day off. Most of the ponies seem to go about their day as usual, although you can see a few who look a bit stressed. You then notice\n" +
                "\tArdent walking your way.\n",
                ID = 0020013,
                AnswerID = 0020026,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 6,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Please do,\" she says and motions for you to come to her terminal. You access the database and tell her that the database can only be\n" +
                "\taccessed from the IT-department and from here. You then show her where she can see the total vote count, votes for individual ponies, as\n" +
                "\twell as a few other tidbits she might want to know.\n" +
                "\t\"Alright, that should cover all you need to know about the program. If something comes up, just send me an intramail,\" you say to her.\n" +
                "\tYou then start to leave to go monitor the program for some more time.\n" +
                "\t\"Don't forget to vote!\" the Overmare calls back from the table. You smile and give a little nod to her, and leave. Huh, Crystal was in a\n" +
                "\tbetter mood than you'd first imagined. You guess the program really facilitates her job.\n",
                ID = 0020012,
                AnswerID = 0020022,
            });
            InstanceList.Add(new Instance
            {
                Text = "You head back to the IT-department to monitor the voting for a little bit more. After that, you plan on taking it easy for the rest of\n" +
                "\tthe day. Holidays are way too rare, you think to yourself.\n",
                ID = 0020012,
                AnswerID = 0020022,
            });
            InstanceList.Add(new Instance
            {
                Text = "Back at the office you log back in to your terminal. You take a look at the database. Total votes are currently at 104. According to the\n" +
                "\tpaper the Overmare gave you a few days ago, the final vote count should be 311 out of the 512 ponies in the Stable. The rest are under the\n" +
                "\tage of 17, and cannot vote or be voted. You then check the log file for any sign of problems with the program. No warnings or errors, that's\n" +
                "\tgood. One thing however catches your eye. At least a five votes have come from a single subaddress. Is that from one of the common areas? Is\n" +
                "\tthere multiple terminals at some room you're forgetting? Or have ponies just moved terminals to the same room to play games together? You\n" +
                "\tthink about that for a moment.\n",
                ID = 0020012,
                AnswerID = 0020022,
            });
            InstanceList.Add(new Instance
            {
                Text = "Normally you would examine the cause more closely, but right now you really don't feel like doing that. Instead, you decide to log out\n" +
                "\tfrom the terminal, and head to your room. You really need a break from the vote and the program. On your way to your room, you start to think\n" +
                "\tabout Ardent and how the rebellion is going. You haven't really had the time to see him for the past week.\n",
                ID = 0020012,
                AnswerID = 0020022,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 6,
            });
            InstanceList.Add(new Instance
            {
                Text = "Back in your room, you're still thinking about the rebellion. You're really worried about the safety of the ponies who decide to join in.\n" +
                "\tEspecially Ardent's. You just hope for the best.\n",
                ID = 0020012,
                AnswerID = 0020023,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020025
            });
            InstanceList.Add(new Instance
            {
                Text = "Back in your room, you're still thinking about the rebellion. You're really worried about the safety of the ponies who decide to join in.\n" +
                "\tEspecially Ardent's and yours. You can only hope it will go smoothly.\n",
                ID = 0020012,
                AnswerID = 0020024,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020025
            });
            InstanceList.Add(new Instance
            {
                Text = "You turn on your terminal to drop off your vote and to distract yourself from the rebellion. \"Hmm, who to vote...\" you think to yourself.",
                ID = 0020012,
                AnswerID = 0020025,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I knew you would get it done in time,\" she says with a small reassuring smile.\n" +
                "\t\"I can show you how to access the database to see the votes.\" you say to her.\n",
                ID = 0020012,
                AnswerID = 0020021,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020022
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Good, good,\" she says, looking pleased.\n" +
                "\t\"I can show you how to access the database to see the votes.\" you suggest to her.\n",
                ID = 0020012,
                AnswerID = 0020020,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020022
            });
            InstanceList.Add(new Instance
            {
                Text = "She turns towards you.\n" +
                "\t\"Ah, Silver. Come in,\" she says and motions for you to sit on the chair on the other side of the big, half circle table. \"I assume the\n" +
                "\tvoting program is up and running?\" she asks you.",
                ID = 0020011,
                AnswerID = 0020019,
            });
            InstanceList.Add(new Instance
            {
                Text = "You get ready to launch the script. The final minute feels really long. As the clock on your PipBuck hits 12:00, you send the mails.\n" +
                "\t\"There we go,\" you say. You then open the database to check if the votes are coming through. A minute passes. Then two. Total vote count is\n" +
                "\tstill at 0. \"Okay, no need to panic. Let's wait a couple minutes more. Not everypony will vote immediately. But surely there must be at\n" +
                "\tleast a few who want to be done with the vote as quickly as possible, right?\" you try to reason to yourself as you hit the refresh button.\n" +
                "\tAs you're about to go check if the mails were actually sent, the total vote count goes to 3. Then 5, and then 9. You let out a sigh of relief.\n",
                ID = 0020010,
                AnswerID = 0020018,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"See? I told you it would work out fine,\" Scan says and pats you on the back. \"Anyway, I'm gonna go enjoy my day off. Seeya!\" she says\n" +
                "\tand waves goodbye.\n" +
                "\t\"Alright, later,\" you say and wave back. You refresh the database a few more times to make sure it's working, and decide to go tell the\n" +
                "\tOvermare the program is working fine and to show her how to access the database.\n",
                ID = 0020010,
                AnswerID = 0020018,
            });
            InstanceList.Add(new Instance
            {
                Text = "You log out of your terminal and leave the IT-department. You take the elevator to second level where the cafeteria, living quarters and\n" +
                "\tOvermare's office is. The hallways are now filled with voting propaganda and instructions on how to vote with the new system. You really hate the\n" +
                "\thassle that's going on during the voting. Everypony's stressed and nervous. More ponies are in the cafeteria than normal, causing a lot of noise\n" +
                "\tin the hallways. You can't wait for the vote to end.\n",
                ID = 0020010,
                AnswerID = 0020018,
            });
            InstanceList.Add(new Instance
            {
                Text = "You take the stairs to the upper level of the cafeteria, where the common area is. There's a few pool tables, a bar, couches, and some other\n" +
                "\tstuff to pass the time. You rarely visit this place. You walk past the common area, towards the Overmare's office. You mentally prepare for the\n" +
                "\tdiscussion with the Overmare. She's usually pretty irritable during the voting. The door to the office is open, and you can see the Overmare\n" +
                "\tsitting in her chair, looking out of the round window to the cafeteria.",
                ID = 0020010,
                AnswerID = 0020018,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, alright,\" she says. You look at each other for a few seconds with straight faces. You both then burst out laughing. You ruffle your\n" +
                "\tmane to straighten it out a bit.\n",
                ID = 0020010,
                AnswerID = 0020016,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020018
            });
            InstanceList.Add(new Instance
            {
                Text = "She just chuckles and shakes her head. You try to ruffle your mane with your hoof to straighten it out a bit.\n",
                ID = 0020010,
                AnswerID = 0020017,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020018
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Just wanted to see how you're doing with the project,\" she answers.\n" +
                "\t\"Well I finished it yesterday night and it seems to work just fine. I'll send the messages in a few minutes. Then we'll see if it works for\n" +
                "\treal,\" you say to her. She looks at you for a while with a small smile.\n" +
                "\t\"Have you been here for the whole night? Judging by those dark bags under your eyes and that mane of yours, that's the case,\" she asks you.",
                ID = 0020009,
                AnswerID = 0020015,
            });
            InstanceList.Add(new Instance
            {
                Text = "You go sit at your terminal and log back in. \"Well, I guess I better get to work.\" you say to yourself. You start off by thinking how\n" +
                "\tthe voting process will work. You settle on sending everypony an intramail with a unique code and a link to the program that'll be hosted on\n" +
                "\tan encrypted virtual server. Ponies will use their code to vote, after which the code will become invalid. Once you're done with the initial\n" +
                "\tplanning, you start to write the program.\n",
                ID = 0020008,
                AnswerID = 0020014,
            });
            InstanceList.Add(new Instance
            {
                Text = "The next week is a blur of planning, coding, coffee, and sleepless nights. Part of you enjoys the late nights and problem solving.\n" +
                "\tIt's been some time since your last big project. On the other hoof, one week is a really short time to create a program of this caliber.\n" +
                "\tBut, as the week finally passes...\n",
                ID = 0020008,
                AnswerID = 0020014,
            });
            InstanceList.Add(new Instance
            {
                Text = "You wake up on the floor. \"Nnnhhg, fucking headache...\" you groan. You get up and take a look at your surroundings. You're at the\n" +
                "\tIT-department. Your heart skips a beat. \"Fuck, shit, what's the time!?\" you say to yourself and take a look at your PipBuck. 11:23.\n" +
                "\tOkay, half an hour left until the vote. You sit down at your terminal and check what you were doing last night. Yesterday's kinda fuzzy,\n" +
                "\tyou can't seem to recall much what you did. Looks like you finished setting up the mailing script. That means you've done it? You\n" +
                "\tcompleted the project? You quickly do a test by sending yourself a code. Okay, that works. You access the link in the mail,\n" +
                "\tand do a test vote. You check if the vote appears in the database. It works.\n",
                ID = 0020008,
                AnswerID = 0020014,
            });
            InstanceList.Add(new Instance
            {
                Text = "A feeling of relief washes over you. Some things are implemented a bit crudely in the program, but it works so that's good enough for\n" +
                "\tnow. You can improve it for next year's voting. The voting starts in a moment. You get ready to start the mailing script. You normally\n" +
                "\twould've automated that too, but because of time restraints you decided to left that out. In addition to that, you want to minimize the risk\n" +
                "\tof automation failing.\n",
                ID = 0020008,
                AnswerID = 0020014,
            });
            InstanceList.Add(new Instance
            {
                Text = "You hear the door open to the office. Scanline steps in.\n" +
                "\t\"Hey Silver,\" she says. You're surprised to see her here today, because the two days during the voting are holidays for most of the\n" +
                "\tStable's residents.",
                ID = 0020008,
                AnswerID = 0020014,
            });
            InstanceList.Add(new Instance
            {
                Text = "You look at Scanline. She still looks a bit nervous.\n" +
                "\t\"Well then. A week to write a program for the most important event of the year,\" you say to her, already feeling exhausted just by thinking\n" +
                "\tabout the project.\n" +
                "\t\"I'm sure you'll manage. I can help you a bit, although my programming skills are not quite on par with yours,\" she says comfortingly.\n",
                ID = 0020008,
                AnswerID = 0020010,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020014
            });
            InstanceList.Add(new Instance
            {
                Text = "You look at Scanline. She seems very disturbed by your conversation with the Overmare.\n" +
                "\t\"I uh, knew the Overmare wasn't the nicest pony around, but that was straight up threatening,\" she says after a moment.\n" +
                "\t\"Yeah, no kidding,\" you answer, the Overmare's threat still lingering in your mind.\n",
                ID = 0020008,
                AnswerID = 0020011,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020014
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Good. I trust it'll be ready by next week,\" she says and starts walking towards the door. She gives a last look at you and Scanline\n" +
                "\tat the door, then leaves.\n",
                ID = 0020008,
                AnswerID = 0020009,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 5,
            });
            InstanceList.Add(new Instance
            {
                Text = "She slowly walks closer to you. \"Now, why would you do that?\" she says in a strangely disappointed tone. She gets right next to you.\n" +
                "\t\"If you refuse, I'll personally make sure you'll win the voting,\" she hisses at you. You just gulp and nod. O-kay, that escalated quickly.\n" +
                "\t\"I'm happy we understand each other,\" she says coldly, and takes a step back from you.",
                ID = 0020007,
                AnswerID = 0020012,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 9 }),
                RemoveAnswer = 0020012
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Correct. I trust you can make it in time,\" she says like it's no big deal. She really has high expectations of you, it seems.",
                ID = 0020007,
                AnswerID = 0020013,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 9 }),
                RemoveAnswer = 0020013
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I want you to write a program and distribute it to every terminal in the living quarters, so ponies can vote from there. That way we\n" +
                "\tdon't have to count the votes one at a time. Should be simple enough, right?\" she says.",
                ID = 0020007,
                AnswerID = 0020008,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Anyway, that wasn't what I came here for. Now, as you're aware, the Pariah Vote is around the corner. I've thought about how we can\n" +
                "\timprove the voting process. The classic pen and paper method is a relatively slow process, as counting the votes manually takes a lot of\n" +
                "\ttime. So, I thought it's time to make the voting process faster,\" she explains while walking around the office. She seems to think about\n" +
                "\tsomething for a while.\n",
                ID = 0020006,
                AnswerID = 0020007,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"...and reduce the risk of ponies trying to tamper with the votes...\" you hear her say under her breath. You and Scanline glance at\n" +
                "\teach other. She seems a bit unsettled about the Overmare's presence.",
                ID = 0020006,
                AnswerID = 0020007,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I mean, this place looks a mess. Clean it up,\" she says in a deadpan tone. A mess? It's called organized chaos! You open your mouth\n" +
                "\tto protest, but decide against it and just nod in submission.\n",
                ID = 0020005,
                AnswerID = 0020005,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020007
            });
            InstanceList.Add(new Instance
            {
                Text = "She looks unamused by your comment. \"Clean this place up,\" she simply says, not in the mood for jokes. Clean it up? It's organized\n" +
                "\tchaos! \"But-\" you begin to say, but she just raises a hoof to silence you.\n",
                ID = 0020005,
                AnswerID = 0020006,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020007
            });
            InstanceList.Add(new Instance
            {
                Text = "Before you get to the door, a unicorn with a beige coat and hazel brown mane steps into the office. \"Good morning,\" says the Overmare.\n" +
                "\t\"Oh! Good morning, Crystal Sand. What brings you here?\" you say to her. It's not everyday the Overmare visits the lower level of the\n" +
                "\tStable.\n",
                ID = 0020004,
                AnswerID = 0020004,
            });
            InstanceList.Add(new Instance
            {
                Text = "She looks around the room for a bit before saying anything. \"Is this an office or a scrapyard?\" she says and looks at you quizzically.",
                ID = 0020004,
                AnswerID = 0020004,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yeah, I think I figured out what's wrong with this motherboard while I was at the shower yesterday.\" she says.\n" +
                "\t\"Yeah, that's the best place to do debugging too.\" you answer with a chuckle. You then go sit at your terminal and log on to it.\n" +
                "\tYou first go over the normal routine, checking the Stable's server status. After that you check the intramail.\n",
                ID = 0020001,
                AnswerID = 0020000,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 8 }),
                QuestCheck = 4,
            });
            InstanceList.Add(new Instance
            {
                Text = "No new messages. After that, you check your todo list. There was that one terminal that was disconnected from the Stable's network,\n" +
                "\tyou should go and check that. It looks to be in the living quarters, room 87. You lock the terminal, and start walking towards the door.\n",
                ID = 0020002,
                AnswerID = 0020002,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020004
            });
            InstanceList.Add(new Instance
            {
                Text = "There's one new message, it's from the general depot. You open the message. It says that the laundry's terminal has been locked\n" +
                "\tbecause to Cross Stitch forgot the password to it. Seriously, it's the third time this week. You can't help but facehoof hard.\n" +
                "\t\"Let me guess, another forgotten password?\" says Scanline with a smile.\n" +
                "\t\"Yup,\" you answer with a sigh. You lock the terminal and start walking towards the door.\n",
                ID = 0020003,
                AnswerID = 0020001,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0020004
            });
            InstanceList.Add(new Instance
            {
                Text = "You walk to the shelf. Half a dozen glass bottles of Sparkle-Cola are sitting neatly in a row. The only drink you like more\n" +
                "\tthan coffee is Sparkle-Cola. You have collected most of the different variations of the drink over the years. Sparkle-Cola Quantum,\n" +
                "\tSparkle-Cola Cherry, Sparkle-Light, Sparkle-Cola Orange, Sparkle-Cola Cranberry, and of course a classic one. Only two are missing,\n" +
                "\twhich you probably won't even see in your lifetime, are Sparkle-Grape and Rum & Sparkle. To your knowledge, the Stable doesn't even\n" +
                "\thave them. You've only seen them mentioned on a promotional poster you have hung up on the wall. They were probably released just\n" +
                "\tbefore the bombs fell, and didn't get widespread enough. Kinda bummer, but it is what it is.\n",
                ID = 0012007,
                AnswerID = 0012015,
            });
            InstanceList.Add(new Instance
            {
                Text = "There's also some other Sparkle-Cola themed stuff on the shelf, such as a toy carriage, a scarf, and glasses. Some say that\n" +
                "\tcollecting stuff like that is foalish, but you don't mind that. It gives something to do, other than to sit at a terminal.",
                ID = 0012007,
                AnswerID = 0012015,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 9 }),
                RemoveAnswer = 0012015
            });
            InstanceList.Add(new Instance
            {
                Text = "You decide to turn on your trusty terminal. You should open it up and clean some of the dust from it, but haven't gotten around\n" +
                "\tto do it. The machine gives a soft hum as the screen lights up. You check the intramail. Just a message from the Overmare reminding\n" +
                "\tponies to vote. You then look at a folder named \"Projects\". You've been writing a small game on your freetime. You should probably\n" +
                "\tstart writing it more, or it's never going to get finished, you think to yourself. But, like usual, you decide against it and\n" +
                "\tstart a game of solitaire to pass the time.\n",
                ID = 0012009,
                AnswerID = 0012014,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0012016
            });
            InstanceList.Add(new Instance
            {
                Text = "Wow, feeling really lazy today. You decide to just lie on the bed until the work starts.\n",
                ID = 0012008,
                AnswerID = 0012013,
            });
            InstanceList.Add(new Instance
            {
                Text = "As you lie in the bed, your mind wanders to various things, including the vote, Astral, and life in the Stable in general. It gets\n" +
                "\tpretty boring here sometimes, even for somepony like you, who likes peace and quiet. But then you remember all those educational videos\n" +
                "\tfrom Stable-Tec saying it's for the good of the ponykind for you to stay in the Stable until the surface is habitable again.\n",
                ID = 0012008,
                AnswerID = 0012013,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0012016
            });
            InstanceList.Add(new Instance
            {
                Text = "After a while, you get up and start heading to the IT-department. You take the elevator to the third level, where the IT-department,\n" +
                "\tmaintenance, and PipBuck service station are located.\n",
                ID = 0012010,
                AnswerID = 0012016,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0012017
            });
            InstanceList.Add(new Instance
            {
                Text = "The door to the IT-department slides open with a hydraulic hiss. The office isn't that big. A couple of terminals are sitting on\n" +
                "\ttables, a small fridge is on the right side of the room, and a glass door is on the left side of the room, leading to the server room.\n" +
                "\tAn earth pony with a red coat and orange mane is sitting at a table, tinkering with a circuit board. Fitting, as her cutie mark is a\n" +
                "\tcircuit pattern with screwdriver next to it.\n",
                ID = 0020000,
                AnswerID = 0012017,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Morning Silver,\" says Scanline. She's also working at the IT. Her job leans more towards the hardware side of terminals. You\n" +
                "\tlike her, as she's a lot like you. Doesn't talk too much, and prefers to be alone. That's not to say you two don't like talking to\n" +
                "\teach other. She's one of the few ponies you can discuss tech stuff with.",
                ID = 0020000,
                AnswerID = 0012017,
            });
            InstanceList.Add(new Instance
            {
                Text = "As you walk to your room, you try hard to forget Astral, but fail miserably. This day got ruined before it even started, you\n" +
                "\tthink to yourself. You open the door to your room and step inside. The room is designed for three ponies, like most other rooms in\n" +
                "\tthe living quarters. There's three beds on the left side of the room. Your personal terminal is on the far right corner of the room.\n" +
                "\tYou're usually there when you're not working. There's a shelf on the back wall, filled with different Sparkle-Cola bottles. You've\n" +
                "\tbeen lucky so far, as nopony else has been assigned to this room other than you. You like being alone.\n",
                ID = 0012007,
                AnswerID = 0012012,
            });
            InstanceList.Add(new Instance
            {
                Text = "You look at the Sparkle-Cola themed clock on the wall. There's still some time before your work starts.",
                ID = 0012007,
                AnswerID = 0012012,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Now now, no need to be so rude. I just wanted to remind you that we should enjoy what we have here. We often don't appreciate\n" +
                "\tit enough... until it's all gone,\" she says.\n",
                ID = 0012007,
                AnswerID = 0012011,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I'll keep that in mind. Now, if you're done blabbering, I have places to be,\" you say to her, not in the mood for this.\n" +
                "\t\"Yeah. See you around,\" she says and starts walking the other way. You will never understand how one mare can be so annyoing.\n",
                ID = 0012007,
                AnswerID = 0012011,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0012012
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I could say the same. But that wouldn't be really nice now, would it?\" she says. Sweet Celestia she's annoying. \"Anyway, I'll\n" +
                "\tleave you to it. Be seeing you,\" she says and walks away. You really hope she gets kicked out in the next voting.\n",
                ID = 0012007,
                AnswerID = 0012007,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0012012
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, I think you'll find out soon enough. Anyway... see you around,\" she says with a smirk as she starts walking her own way.\n" +
                "\tShe's up to something, you think to yourself.\n",
                ID = 0012007,
                AnswerID = 0012006,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0012012
            });
            InstanceList.Add(new Instance
            {
                Text = "She gives a little smile. \"We'll see. Anyway... have a good day,\" she says as she starts walking away. You really wish you could\n" +
                "\tjust slap the sass out of her.\n",
                ID = 0012007,
                AnswerID = 0012010,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0012012
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Aw, don't be so mean. I just wanted to check on you, see how you're doing,\" she says. You don't quite believe that.\n" +
                "\t\"Oh yeah? Why's that?\" you ask, not really in the mood for this.\n" +
                "\t\"Well, the voting's soon, I thought we should have chat before that... In case one of us will be voted out,\" she says with a hint of\n" +
                "\tmalice in her voice. Your heart skips a beat.",
                ID = 0012008,
                AnswerID = 0012005,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh I'm not threatening you. I'm just giving some... friendly advice,\" she says with a devious smile. \"Anyway... be seeing you,\"\n" +
                "\tshe says as she starts walking away. Oh how you want to teach her some manners someday.\n",
                ID = 0012007,
                AnswerID = 0012009,
            });
            InstanceList.Add(new Instance
            {
                Text = "She gives a little humph. \"Yeah... Be sure to visit him. Oh, and tell him I said hi,\" she says with a smirk, and starts walking away.\n" +
                "\tShe's just trying to taunt you, you think to yourself.\n",
                ID = 0012006,
                AnswerID = 0012008,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I'm fine,\" she says. Something about the tone of her voice makes you a bit nervous. \"How's your dad?\" she asks.\n" +
                "\t\"As good as ever,\" you answer. Why's she talking about your dad, you wonder.\n",
                ID = 0012005,
                AnswerID = 0012004,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"That's nice. You know, spending time with family is important. We don't quite appreciate it enough... until it's too late,\" she says.\n" +
                "\tIs she talking about the vote?",
                ID = 0012005,
                AnswerID = 0012004,
            });
            InstanceList.Add(new Instance
            {
                Text = "Wait, what? Did she hear your chat with Ardent earlier? No, you made sure she wasn't around during the conversation. Maybe she just knows\n" +
                "\tshe might get kicked this time, you quickly reason to yourself.",
                ID = 0012003,
                AnswerID = 0012003,
            });
            InstanceList.Add(new Instance
            {
                Text = "Wait, what? Does she think she's getting voted out? You try to think what she meant by that.",
                ID = 0012003,
                AnswerID = 0012002,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"What, can't I just come and say hi?\" she asks you with an exaggerated amount of disappointment in her voice.\n" +
                "\t\"No,\" you simply reply.\n" +
                "\t\"Well I just thought you might want to chat with me before...\" she steps closer to you. \"the vote comes. I heard there's a high chance that\n" +
                "\tyou might finally get rid of me,\" she says with a menacing grin.\n",
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
                "\tthat has been disconnected from the Stable's network for a couple days that you didn't have time to check. After that-\n",
                ID = 0012000,
                AnswerID = 0012000,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hey Silver,\" A familiar voice comes from behind you. You turn to see the white coat and dark yellow mane of none other than Astral Mist\n" +
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
                Text = "\"Oh, in a hurry? Well, it was nice seeing you,\" he says and looks a bit disappointed.\n" +
                "\t\"Yeah, see ya,\" you say and start walking out. Why exactly did you leave so early? You don't really know.\n",
                ID = 0011009,
                AnswerID = 0011004,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0011018
            });
            InstanceList.Add(new Instance
            {
                Text = "You finish the coffee with a last sip.\n" +
                "\t\"Leaving already?\" dad asks.\n" +
                "\t\"Yeah, the network won't keep itself running,\" you say jokingly, and get up.\n" +
                "\t\"I guess it won't. Well, it was nice seeing you,\" he says.\n",
                ID = 0011008,
                AnswerID = 0011015,
            });
            InstanceList.Add(new Instance
            {
                Text = "You say your goodbyes, and start to walk towards the door. As you're leaving, three ponies come in. You recognise them as the other\n" +
                "\tworkers from the armory.\n" +
                "\t\"Hey Spade,\" one of them says to your dad.\n" +
                "\t\"I see you have a guest here, hope she left some coffee for the rest of us,\" says another with a smirk.\n" +
                "\t\"There's plenty of it left, don't worry,\" says dad with a calm voice. You give a little \"Hmph!\" and leave the armory.\n",
                ID = 0011008,
                AnswerID = 0011015,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0011018
            });
            InstanceList.Add(new Instance
            {
                Text = "You take the elevator to the third level, where the IT-department, maintenance and PipBuck service station is.\n",
                ID = 0011008,
                AnswerID = 0011018,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0012017
            });
            InstanceList.Add(new Instance
            {
                Text = "He doesn't answer right away. He seems to think about something for a while.\n" +
                "\t\"The same most usually feel about it. Feel bad for the one you voted, and be scared about being voted yourself,\" he says after a moment.\n" +
                "\t\"I wouldn't worry about it too much,\" you comfort him. He doesn't say anything.",
                ID = 0011003,
                AnswerID = 0011016,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 9 }),
                RemoveAnswer = 0011016
            });
            InstanceList.Add(new Instance
            {
                Text = "\"A couple guys went to the general depot, there was something that they needed help with. The rest are probably still sleeping, they\n" +
                "\ttend to come late to work since there's so little to do,\" he says.\n" +
                "\t\"Well that's not very fair,\" you answer. If Scanline did that, you'd give her a few carefully chosen words.\n" +
                "\t\"Oh, it doesn't really matter to me, especially on slow days like this. If they feel like they have better things to do than to be here,\n" +
                "\tI won't stop them,\" he says. To each their own, you guess.",
                ID = 0011003,
                AnswerID = 0011017,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 9 }),
                RemoveAnswer = 0011017
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I'm sorry, but I'm not allowed to give away firearms to anypony but the security,\" he says.\n" +
                "\t\"Oh, right. Well, I think you should keep it on display here, or give it to somepony you trust from the security,\" you suggest.\n",
                ID = 0011007,
                AnswerID = 0011013,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yeah, I'll think about that. But hey, the coffee looks like it's ready,\" he says. He gets up, and gives you a cup from the cabinet. You\n" +
                "\tpour some coffee for the both of you. You sit down, take a sip of the coffee, and relax. So good, you think to yourself.",
                ID = 0011003,
                AnswerID = 0011013,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I would, but I'm not allowed to take firearms from the armory,\" he says.\n" +
                "\t\"Oh. Well in that case I would just put it on display in one of the racks here. Or give it to somepony you trust from the security,\" you\n" +
                "\tsuggest to him.\n",
                ID = 0011007,
                AnswerID = 0011012,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yeah, I'll think about that. But hey, the coffee looks like it's ready,\" he says. He gets up, and gives you a cup from the cabinet. You\n" +
                "\tpour some coffee for the both of you. You sit down, take a sip of the coffee, and relax. So good, you think to yourself. It's not everyday you\n" +
                "\tget to enjoy coffee this strong, since the Stable has to ration out the coffee so it lasts as long as possible. Dad apparently has a friend who\n" +
                "\tworks at the food depot, so he brings some extra coffee packages here every now and then.",
                ID = 0011003,
                AnswerID = 0011012,
            });
            InstanceList.Add(new Instance
            {
                Text = "He looks at the engraving for a minute. \"Not sure. My guess is it refers to the voting,\" he says.",
                ID = 0011006,
                AnswerID = 0011014,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 9 }),
                RemoveAnswer = 0011014
            });
            InstanceList.Add(new Instance
            {
                Text = "He sighs. \"Silver... this pistol once belonged to your mother,\" he says. You stare at the pistol for a while. Dad barely ever talks about\n" +
                "\tyour mom. You only know that she was the head of security, and that she died of some disease when you were only a couple years old. It never\n" +
                "\treally bothered you, probably because you don't even remember how she looked like.\n",
                ID = 0011006,
                AnswerID = 0011010,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh,\" you say after some time. You don't really know how to feel about it.\n" +
                "\t\"The gun was modified by the gunsmith here. I thought it was lost after her stuff was returned here. But now I found it, and I'm not sure\n" +
                "\twhat to do with it,\" he says.",
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
                "\twhat to do with it,\" he says. You can see that he looks a bit uncomfortable looking at the pistol.",
                ID = 0011005,
                AnswerID = 0011007,
            });
            InstanceList.Add(new Instance
            {
                Text = "You take the pistol in your magic and examine it. The weapon is lot shinier than the other pistols here, and it has matte black accents\n" +
                "\ton it. The left side of the barrel has the words \"The verdict of acquital comes for all\" engraved on it. The engraving is very roughly made.\n" +
                "\t\"Huh, I wonder what that means,\" you say to yourself.\n",
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
                Text = "\"Found the shelf?\" he asks, and you nod. \"Good, the coffee looks like it's just about ready,\" he says, gets up, and gives you a cup\n" +
                "\tfrom the cabinet. You pour some coffee for the both of you, and sit down at one of the tables. Taking a sip from your coffee, you relax.\n" +
                "\tIt's not everyday you get to enjoy coffee this strong, since the Stable has to ration out the coffee so it lasts as long as possible.\n" +
                "\tDad apparently has a friend who works at the food depot, so he brings some extra coffee packages here every now and then.",
                ID = 0011003,
                AnswerID = 0011006,
            });
            InstanceList.Add(new Instance
            {
                Text = "He chuckles. \"Yes, I suppose it does taste a bit... bland,\" he says. He then glances at the boxes he was carrying. \"Could you put\n" +
                "\tthese on the shelf over there?\" he says and points to the back left corner of the room. \"Shelf G5, put them next to the other boxes with\n" +
                "\tthese numbers,\" he says as he points to the boxes' side where there is a long string of numbers.\n",
                ID = 0011002,
                AnswerID = 0011003,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, of course,\" you say.\n" +
                "\t\"Awesome, come to the break room afterwards, I'll go put the coffee brewing,\" he says and leaves.\n",
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
                Text = "\"It's not so bad. And to be honest, I'd be bored if I had to be sitting in front of a terminal all day,\" he says jokingly.\n" +
                "\t\"Hey, there's a lot more to my job than sitting!\" you say defensively. Why do others think your job is so simple? There's a lot of\n" +
                "\tresponsibility in keeping the network and terminals running, you think to yourself.\n",
                ID = 0011001,
                AnswerID = 0011002,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Yes, yes, I know. Just teasing,\" he says with a smile. \"Anyway, would you like some coffee before you leave?\" he asks.",
                ID = 0011001,
                AnswerID = 0011002,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"A bit maybe. But I do prefer this to most other jobs here. It's relaxing, and there's no hurry,\" he explains.\n" +
                "\t\"Fair enough,\" you answer. Some ponies just like simpler jobs, it seems.\n",
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
                "\thave much to do in there. You take the elevator to the fourth level of the Stable, where the armory and general depot are located.\n",
                ID = 0011000,
                AnswerID = 0011000,
            });
            InstanceList.Add(new Instance
            {
                Text = "You open the door to the armory. The room is large, and there are countless lockers and boxes arranged neatly against the walls.\n" +
                "\tAbout a dozen rows of shelves are in the middle of the room, each row full of boxes stacked to fit as much stuff as possible. There\n" +
                "\tare also a large number of different weapons ranging from pistols to assault rifles on weapon racks and on the walls. You wonder\n" +
                "\twhy the Stable needs so many guns.\n",
                ID = 0011000,
                AnswerID = 0011000,
            });
            InstanceList.Add(new Instance
            {
                Text = "Your dad comes from around the corner carrying a few boxes.\n" +
                "\t\"Silver!\" he says with a smile as he notices you.\n" +
                "\t\"Hey dad,\" you answer, and give him a hug.\n" +
                "\t\"What brings you here?\"\t" +
                "\t\"Just thought I'd come and see how you're doing before I go to work,\" you answer. You don't usually see each other that often,\n" +
                "\tso it's easy to forget to catch up on things every once in a while.\n",
                ID = 0011000,
                AnswerID = 0011000,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Same soup, just reheated, heh. Decided to rearrange some stuff here to pass the time,\" he says and gestures to the boxes he's\n" +
                "\tcarrying. You can't even begin to think about how many times he must have done that by now.",
                ID = 0011000,
                AnswerID = 0011000,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, um... Don't worry, this'll be the last time,\" she says in a sorry voice.\n" +
                "\t\"Good,\" you say, and leave the laundry. You then head to the IT-department. You take the elevator to the lower level, where the\n" +
                "\tIT-department, maintenance and PipBuck service station are located.\n",
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
                Text = "\"No problem,\" you say, wave her goodbye, and leave the laundry. You then head to the IT-department. You take the elevator\n" +
                "\tto the lower level, where the IT-department, maintenance and PipBuck service station are located.\n",
                ID = 0010007,
                AnswerID = 0010007,
                HasSpecialFunction = true,
                SpecialFunction = new List<int>(new int[] { 10 }),
                RedirectInstance = 0012017
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
                Text = "\"No problem,\" you say, and leave the laundry. You then head to the IT-department.",
                ID = 0010006,
                AnswerID = 0010006,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"B-but I need to-\" she starts saying, but you're already on your way out. You have better things to do than be resetting\n" +
                "\tpasswords all the time, you think to yourself.",
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
                "\tcan't help but find it annoying.\n",
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
                "\tanything sensible as a response. You believe it's good to teach some basic rules about cybersecurity to the Stable, but sometimes it\n" +
                "\tjust doesn't seem to be worth the trouble.\n",
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
                Text = "\"Sorry... Can you do it just once more? I'll remember it this time. Promise,\" she assures you. For some reason, you don't think\n" +
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
                Text = "\"Sure thing, I'll come by here then,\" you say, and start to walk away, but Stitch calls back to you from the counter.\n",
                ID = 0010001,
                AnswerID = 0010001,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hey Silver, just one tiny thing... I think I may have forgotten the password on the laundry's terminal... again. And now it has\n" +
                "\tlocked me out of it,\" she says while trying to sound as innocent as possible. And failing quite badly at that.",
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
                Text = "\"Sure thing,\" says the stallion. You and Ardent chat for a while about some lighter things. Ardent finishes eating before you,\n" +
                "\tand leaves to work in the maintenance office. Once you finish, you notice that you still have about an hour before your work starts.\n" +
                "\tYou are the Stable's network specialist and software engineer. Your job is to make sure the Stable's network is running fine,\n" +
                "\tand to fix any software related problems with terminals. A lot of work for one pony, since barely anypony is interested about\n" +
                "\tthat kind of stuff in the Stable. But that doesn't bother you, because you enjoy your job.\n",
                ID = 36,
                AnswerID = 49,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hmm, now would be a good time to go do the laundry. Or maybe I should I go visit my dad and see how he's doing at the armory,\"\n" +
                "\tyou ponder to yourself.",
                ID = 36,
                AnswerID = 49,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"You really seem to have something against the Overmare. Which might be good. Listen, I've been thinking about a rebellion.\n" +
                "\tI thought you might want to join, so we could get rid of the vote,\" says the stallion in a low voice.",
                ID = 23,
                AnswerID = 53,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I was a bit worried about what you might think. Glad you think the same way I do,\" says the stallion in a relieved tone.\n",
                ID = 36,
                AnswerID = 39,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Of course. But it needs to be planned well. And we need more ponies to join it. I don't even want to think about what happens if\n" +
                "\tthe rebellion fails,\" you say, remembering some stories about past rebellions.\n",
                ID = 36,
                AnswerID = 39,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Don't worry, I've got this. I know ponies who are willing to join. But hey, let's talk about it more some other time, I want to\n" +
                "\teat right now,\" reassures the stallion, taking a bite of his sandwich.\n",
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
                Text = "\"Hmm, now would be a good time to go do the laundry. Or maybe I should I go visit my dad and see how he's doing at the armory,\"\n" +
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
                Text = "\"That's besides the point. Where I *was* going with this, is that I think we should attempt a new rebellion against the vote,\n" +
                "\tsays the stallion in a low voice.",
                ID = 23,
                AnswerID = 50,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Well in that case you might wanna hear about what I've been planning. I think we should attempt a new rebellion. Would you be willing\n" +
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
                Text = "\"Hmm, now would be a good time to go do the laundry. Or maybe I should I go visit my dad and see how he's doing at the armory,\"\n" +
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
                Text = "\"Yes, I'm aware of that. But I'm ready to take the risk, if it means we can stop the voting,\" says the stallion.",
                ID = 32,
                AnswerID = 41,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Of course. We need to convince others to join us, and make sure the Overmare doesn't hear about this,\" says the stallion.",
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
                "\tI'm ready to take take the risk,\" says the stallion.",
                ID = 29,
                AnswerID = 37,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"This is the only way we can get rid of the vote, and you know it,\" he says in a firm voice.",
                ID = 28,
                AnswerID = 36,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"What, should we just wait and see what happens when everypony realizes they don't want to kick anypony out? Things are gonna\n" +
                "\tget a lot messier then,\" says the stallion.",
                ID = 28,
                AnswerID = 35,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"I'm... just gonna assume that was a joke. Anyway, my point was, I think we should attempt a new rebellion,\"\n" +
                "\tsays the stallion with a low voice.",
                ID = 23,
                AnswerID = 34,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Ugh, sometimes I just don't understand you... Sure, the voting sucks and these hallways are pretty small, but at least we have\n" +
                "\tfood, water and a place to sleep. Who knows what lies on the other side of the Stable's door. For all we know, we might be the last\n" +
                "\tof our species!\" says the stallion with a hint of anger in his voice.",
                ID = 27,
                AnswerID = 33,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"That's what I thought. Anyway, my point was, because I think there's a fair number of ponies now who don't want the vote,\n" +
                "\twe should attempt a new rebellion,\" says the stallion in a low voice.",
                ID = 23,
                AnswerID = 32,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Hm. I was hoping you'd be against the voting a bit more,\" says the stallion with a hint of disappointment in his voice.",
                ID = 26,
                AnswerID = 31,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Wow. I didn't know you cared so little for others,\" says the stallion in a disappointed voice.",
                ID = 25,
                AnswerID = 30,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Because I think it's time we attempted a new rebellion,\" says the stallion in a low voice.",
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
                Text = "\"So, I was thinking... Maybe it's time for a new rebellion against the voting,\" whispers the stallion.",
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
                Text = "He sighs. \"I just find this whole voting useless. Who do you think is going to get kicked next year? Is there anypony else than\n" +
                "\tAstral you think is dangerous or annoying?\" asks the stallion.",
                ID = 17,
                AnswerID = 25,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Don't you think it's a bit harsh to kick somepony out of here just because others think they're annoying? Who knows what\n" +
                "\tlies on the other side of the Stable's door,\" says the stallion.",
                ID = 22,
                AnswerID = 24,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"No, I think that's what everypony should be thinking right now,\" says the stallion in a firm voice.",
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
                Text = "\"Heh, I guess not. But jokes aside... I've been thinking about The Pariah Vote,\" Ardent says.\n",
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
                Text = "\"Well, it's just that The Pariah Vote is just around the corner,\" says the stallion.\n",
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
                Text = "\"Hm? Oh sorry, I was just lost in my thoughts,\" says the stallion.",
                ID = 15,
                AnswerID = 16,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"What? Oh, don't worry. We've been friends for our whole lives, I know you are a kind pony,\" says the stallion with a warm smile.",
                ID = 14,
                AnswerID = 15,
            });
            InstanceList.Add(new Instance
            {
                Text = "\"Oh, sorry. It's just that the voting for the next Pariah is just around the corner,\" says the stallion.\n",
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
                Text = "She relaxes a bit. \"Oh, okay. But mom said that I should avoid you and Astral,\" says the filly.",
                ID = 1,
                AnswerID = 1,
            });
            InstanceList.Add(new Instance
            {
                Text = "She jumps in the air, let's out a high pitched scream and runs away.",
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
