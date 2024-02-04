using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace MegaMariPrac
{
    public partial class MainForm : Form
    {
        #region global variables
        static string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string configpath = appdata + @"\MegaMariPrac\";
        string configfilename = "config.cfg";
        string hotkeyfilename = "hotkey.cfg";
        string savestatesfilename = "savestates.cfg";

        Dictionary<int, string> dictStage = new Dictionary<int, string>();
        Dictionary<string, int> dictWeapon = new Dictionary<string, int>();
        Dictionary<int, short> dictMarisaSprite = new Dictionary<int, short>();
        Dictionary<int, short> dictAliceSprite = new Dictionary<int, short>();

        SaveState ss = new SaveState();

        static string hotkeyVersion = "v1.0";
        static int numberHotkeys = 6;
        KeyboardKeys keybKeys = new KeyboardKeys();

        List<int> lstHotkeys = new List<int>();

        short curCharacter = 0, bossHP = 0;
        int screenType = 0, stageID = 255, state = 255,
            flagBroom = 255, flagDoll = 255, flagReimu = 255, flagCirno = 255, flagSakuya = 255,
            flagRemilia = 255, flagYoumu = 255, flagYuyuko = 255, flagReisen = 255, flagEirin = 255,
            flagTank1 = 0, flagTank2 = 0, flagTank3 = 0, flagTank4 = 0;
        float XF = 0, YF = 0;
        uint screenTimer = 0;
        #endregion

        #region memory stuff
        static ProcessMemory pm = new ProcessMemory();

        //first offsets - these are added to "megamari.exe" when reading/writing
        int FIRST_OFFSET = 0xDB0D4; //general first offset
        int FIRST_OFFSET_BOSS_HP = 0xCF708; //for some reason, boss hp requires a different first offset

        //static addresses
        int STATE = 0xDD6C0;
        const int READY = 0, PLAYING = 1, TRANSITION_LEFT = 2, TRANSITION_UP = 4, TRANSITION_DOWN = 5, TRANSITION_RIGHT = 14,
                  WIN_FANFARE = 6, WIN_NO_FANFARE_TELEPORT = 7, WIN_NO_FANFARE_NO_TELEPORT = 8,
                  REFILL_FULL_HP = 9, REFILL_FULL_AMMO = 9, MENU = 11, DEAD = 12;

        int STAGE_ID = 0xDD6C4;
        const int REIMU_STAGE = 0, CIRNO_STAGE = 1, SAKUYA_STAGE = 2, REMILIA_STAGE = 3,
                  YOUMU_STAGE = 4, YUYUKO_STAGE = 5, REISEN_STAGE = 6, EIRIN_STAGE = 7,
                  PATCHY_1 = 8, PATCHY_2 = 9, PATCHY_3 = 12, PATCHY_4 = 13, PATCHY_5 = 10, PATCHY_6 = 11, CREDITS = 15, ELSEWHERE = 255;

        int SCREEN_TYPE = 0xE3614;
        const int TITLE_SCREEN = 0, STAGE_SELECT = 1, STAGE_LOADING = 2, STAGE = 3, WEAPON_GET = 4, GAME_OVER = 6, CONTINUE = 8;

        int DELAY = 0xDD6D4, SCREEN_TIMER = 0xDD6D8;

        //second offsets added to the result of "megamari.exe" + first offset, resulting in a pointer to certain a certain value
        int X_OFFSET = 0x70, Y_OFFSET = 0x74;
        int CAMERA_X_1_OFFSET = 0xF480, CAMERA_Y_1_OFFSET = 0xF484, CAMERA_X_2_OFFSET = 0xF488, CAMERA_Y_2_OFFSET = 0xF48C;
        int CAMERA_VIEW_X_OFFSET = 0xF3B8, CAMERA_VIEW_Y_OFFSET = 0xF3BC;

        int CHECKPOINT_OFFSET = 0xF478;
        const int CHECKPOINT_START = 0, CHECKPOINT = 1, CHECKPOINT_BOSS = 2;

        int MENU_TANK_SLOT_1_OFFSET = 0xDD, MENU_TANK_SLOT_2_OFFSET = 0xDE, MENU_TANK_SLOT_3_OFFSET = 0xDF, MENU_TANK_SLOT_4_OFFSET = 0xE0;
        const int NO_TANK = 0, ETANK = 1, STAR_TANK = 2, DOUBLE_ETANK = 3;
        int MENU_PANE_OFFSET = 0xDC, MENU_TANKS_OFFSET = 0xDD, MENU_CURSOR_OFFSET = 0x12C;

        int LIVES_OFFSET = 0xD4, IFRAMES_OFFSET = 0x128, SPEED_OFFSET = 0xB8,
            MARISA_HP_OFFSET = 0xCC, ALICE_HP_OFFSET = 0xD0;
        const int FULL_HP = 28;

        int REIMU_FLAG_OFFSET = 0xE1, REMILIA_FLAG_OFFSET = 0xE2, YOUMU_FLAG_OFFSET = 0xE3, REISEN_FLAG_OFFSET = 0xE4,
            CIRNO_FLAG_OFFSET = 0xE5, SAKUYA_FLAG_OFFSET = 0xE6, YUYUKO_FLAG_OFFSET = 0xE7, EIRIN_FLAG_OFFSET = 0xE8;
        const int ON_MARISA = 0, ON_ALICE = 1, ON_NOBODY = 255;

        int BROOM_FLAG_OFFSET = 0xE9, DOLL_FLAG_OFFSET = 0xEA;
        const int SPECIAL_WEAPON_ON = 0, SPECIAL_WEAPON_OFF = 255;

        int REIMU_AMMO_OFFSET = 0xEC, REMILIA_AMMO_OFFSET = 0xF0, YOUMU_AMMO_OFFSET = 0xF4, REISEN_AMMO_OFFSET = 0xF8,
            CIRNO_AMMO_OFFSET = 0xFC, SAKUYA_AMMO_OFFSET = 0x100, YUYUKO_AMMO_OFFSET = 0x104, EIRIN_AMMO_OFFSET = 0x108,
            BROOM_AMMO_OFFSET = 0x10C, DOLL_AMMO_OFFSET = 0x110;
        const int FULL_AMMO = 112;

        int CHARACTER_OFFSET = 0xC9;
        const int MARISA = 0, ALICE = 1;

        int CHARACTER_SPRITE_OFFSET = 0x68;
        const short MARISA_SPRITE_NORMAL = 3376, MARISA_SPRITE_BROOM = 4096,
                    MARISA_SPRITE_REIMU = 3456, MARISA_SPRITE_REMILIA = 3536, MARISA_SPRITE_YOUMU = 3616, MARISA_SPRITE_REISEN = 3696,
                    MARISA_SPRITE_CIRNO = 3776, MARISA_SPRITE_SAKUYA = 3856, MARISA_SPRITE_YUYUKO = 3936, MARISA_SPRITE_EIRIN = 4016,
                    ALICE_SPRITE_NORMAL = 4176, ALICE_SPRITE_DOLL = 4896,
                    ALICE_SPRITE_REIMU = 4256, ALICE_SPRITE_REMILIA = 4336, ALICE_SPRITE_YOUMU = 4416, ALICE_SPRITE_REISEN = 4496,
                    ALICE_SPRITE_CIRNO = 4576, ALICE_SPRITE_SAKUYA = 4656, ALICE_SPRITE_YUYUKO = 4736, ALICE_SPRITE_EIRIN = 4816;

        int CHARACTER_WEAPON_OFFSET = 0x131;
        const int NORMAL_WEAPON = 0, SPECIAL_WEAPON = 9,
                  REIMU_WEAPON = 1, REMILIA_WEAPON = 2, YOUMU_WEAPON = 3, REISEN_WEAPON = 4,
                  CIRNO_WEAPON = 5, SAKUYA_WEAPON = 6, YUYUKO_WEAPON = 7, EIRIN_WEAPON = 8;
        #endregion

        #region form
        public MainForm()
        {
            InitializeComponent();
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            LoadHotkeys();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(appdata + @"\MegaMariPrac"))
                Directory.CreateDirectory(appdata + @"\MegaMariPrac");

            string x, y;
            if (File.Exists(configpath + configfilename)) //checks if config.cfg exists
            {
                using (StreamReader sr = File.OpenText(configpath + configfilename))
                {
                    x = sr.ReadLine();
                    y = sr.ReadLine();
                }
                Location = new Point(int.Parse(x), int.Parse(y)); //places the app at the same position it was in the last time
            }

            toolTip.AutoPopDelay = 20000; toolTip.InitialDelay = 200; toolTip.ReshowDelay = 100;
            toolTip.ShowAlways = true; //force the ToolTip text to be displayed whether or not the form is active
            toolTip.SetToolTip(checkFreezeAll, "Checks all weapon checkboxes below and forces ammo for all of them at maximum.");
            toolTip.SetToolTip(checkHealth, "Forces health for both Marisa and Alice at maximum.");
            toolTip.SetToolTip(checkLives, "Freezes lives at 2.");
            toolTip.SetToolTip(checkIframes, "Gives infinite invincibility frames.");
            toolTip.SetToolTip(buttonDie, "Die.");
            toolTip.SetToolTip(buttonGameOver, "Instantly go to the game over screen.");
            toolTip.SetToolTip(buttonWin, "Initiates the winning fanfare and brings the user to the stage select screen.\nIf used outside Patchouli stages, the currently used character will receive the stage's boss weapon.");
            toolTip.SetToolTip(buttonCheckpoint, "Cycles through each checkpoint of the stage in the following order: Starting point -> Checkpoint -> Boss.");
            toolTip.SetToolTip(buttonStore, "Stores the following values into memory: Coordinates, HP, Lives, Tanks and Weapon flags/ammo.");
            toolTip.SetToolTip(buttonLoad, "Loads previously stored values from memory.");
            toolTip.SetToolTip(buttonSave, "Stores values into the savestates.cfg file, memory and the dropdown list below.");
            toolTip.SetToolTip(buttonDelete, "Deletes a save state from the savestates.cfg file and the dropwdown list below.");
            toolTip.SetToolTip(comboSaves, "Selecting an entry from this dropdown list will load all values attached to it.");
            toolTip.SetToolTip(checkEarlyBroom, "Checking this will load the early broom route into the drowndown list on the right.");
            toolTip.SetToolTip(buttonWarp, "Loads the selected stage from the dropdown list. Requires the user to select 'Continue' afterwards.\nAlso gives characters their appropriate weapons based on the speedrun route.");

            if (!File.Exists(configpath + savestatesfilename)) //checks if savestatesfilename.cfg exists
            {
                using (StreamWriter sw = File.CreateText(configpath + savestatesfilename)) //creates the save state file template
                {
                    sw.WriteLine("[Reimu-0]\n"); sw.WriteLine("[Cirno-1]\n");
                    sw.WriteLine("[Sakuya-2]\n"); sw.WriteLine("[Remilia-3]\n");
                    sw.WriteLine("[Youmu-4]\n"); sw.WriteLine("[Yuyuko-5]\n");
                    sw.WriteLine("[Reisen-6]\n"); sw.WriteLine("[Eirin-7]\n");
                    sw.WriteLine("[Patchouli 1-8]\n"); sw.WriteLine("[Patchouli 2-9]\n");
                    sw.WriteLine("[Patchouli 3-12]\n"); sw.WriteLine("[Patchouli 4-13]\n");
                    sw.WriteLine("[Patchouli 5-10]\n"); sw.WriteLine("[Patchouli 6-11]\n");
                }
            }
            else //if savestates.cfg exists, run a quick cleanup to remove empty lines that might be in the wrong spots
            {
                List<string> lst_lines = new List<string>();
                using (StreamReader sr = File.OpenText(configpath + savestatesfilename))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line.Length > 0)
                        {
                            if (line.Contains("["))
                            {
                                lst_lines.Add(""); //add an empty line before each stage ID
                                lst_lines.Add(line);
                            }
                            else
                                lst_lines.Add(line); //a save state line
                        }
                    }
                }
                lst_lines.RemoveAt(0); //removes the very first line which is empty
                File.WriteAllLines(configpath + savestatesfilename, lst_lines.ToArray()); //write the new lines to the file
            }

            dictStage.Add(REIMU_STAGE, "Reimu"); dictStage.Add(CIRNO_STAGE, "Cirno"); dictStage.Add(SAKUYA_STAGE, "Sakuya");
            dictStage.Add(YOUMU_STAGE, "Youmu"); dictStage.Add(YUYUKO_STAGE, "Yuyuko"); dictStage.Add(REISEN_STAGE, "Reisen");
            dictStage.Add(REMILIA_STAGE, "Remilia"); dictStage.Add(EIRIN_STAGE, "Eirin");
            dictStage.Add(PATCHY_1, "Patchouli 1"); dictStage.Add(PATCHY_2, "Patchouli 2");
            dictStage.Add(PATCHY_3, "Patchouli 3"); dictStage.Add(PATCHY_4, "Patchouli 4"); dictStage.Add(PATCHY_5, "Patchouli 5");
            dictStage.Add(PATCHY_6, "Patchouli 6"); dictStage.Add(CREDITS, "Credits");

            dictWeapon.Add("Broom", ON_NOBODY); dictWeapon.Add("Doll", ON_NOBODY);
            dictWeapon.Add("Reimu", ON_NOBODY); dictWeapon.Add("Remilia", ON_NOBODY); dictWeapon.Add("Youmu", ON_NOBODY); dictWeapon.Add("Reisen", ON_NOBODY);
            dictWeapon.Add("Cirno", ON_NOBODY); dictWeapon.Add("Sakuya", ON_NOBODY); dictWeapon.Add("Yuyuko", ON_NOBODY); dictWeapon.Add("Eirin", ON_NOBODY);

            dictMarisaSprite.Add(NORMAL_WEAPON, MARISA_SPRITE_NORMAL); dictMarisaSprite.Add(SPECIAL_WEAPON, MARISA_SPRITE_BROOM);
            dictMarisaSprite.Add(REIMU_WEAPON, MARISA_SPRITE_REIMU); dictMarisaSprite.Add(REMILIA_WEAPON, MARISA_SPRITE_REMILIA);
            dictMarisaSprite.Add(YOUMU_WEAPON, MARISA_SPRITE_YOUMU); dictMarisaSprite.Add(REISEN_WEAPON, MARISA_SPRITE_REISEN);
            dictMarisaSprite.Add(CIRNO_WEAPON, MARISA_SPRITE_CIRNO); dictMarisaSprite.Add(SAKUYA_WEAPON, MARISA_SPRITE_SAKUYA);
            dictMarisaSprite.Add(YUYUKO_WEAPON, MARISA_SPRITE_YUYUKO); dictMarisaSprite.Add(EIRIN_WEAPON, MARISA_SPRITE_EIRIN);

            dictAliceSprite.Add(NORMAL_WEAPON, ALICE_SPRITE_NORMAL); dictAliceSprite.Add(SPECIAL_WEAPON, ALICE_SPRITE_DOLL);
            dictAliceSprite.Add(REIMU_WEAPON, ALICE_SPRITE_REIMU); dictAliceSprite.Add(REMILIA_WEAPON, ALICE_SPRITE_REMILIA);
            dictAliceSprite.Add(YOUMU_WEAPON, ALICE_SPRITE_YOUMU); dictAliceSprite.Add(REISEN_WEAPON, ALICE_SPRITE_REISEN);
            dictAliceSprite.Add(CIRNO_WEAPON, ALICE_SPRITE_CIRNO); dictAliceSprite.Add(SAKUYA_WEAPON, ALICE_SPRITE_SAKUYA);
            dictAliceSprite.Add(YUYUKO_WEAPON, ALICE_SPRITE_YUYUKO); dictAliceSprite.Add(EIRIN_WEAPON, ALICE_SPRITE_EIRIN);

            //enables controls when playing and disables them when title screen/stage select
            new Thread(ManageControls) { IsBackground = true }.Start();
            //this thread will read values from the game
            new Thread(ReadValues) { IsBackground = true }.Start();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            using (StreamWriter sw = File.CreateText(configpath + configfilename)) //saving application's position upon closing
            {
                sw.WriteLine(Location.X);
                sw.WriteLine(Location.Y);
            }
        }
        #endregion

        #region threads
        private void ManageControls()
        {
            bool inStage = false;
            while (true)
            {
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (screenType == STAGE) //if marisa is in a stage
                        {
                            if (curCharacter == MARISA)
                            {
                                labelStatus.ForeColor = Color.Gold;
                                labelStatus.Text = "Marisa is in " + dictStage[stageID] + "'s stage";
                            }
                            else
                            {
                                labelStatus.ForeColor = Color.FromArgb(130, 115, 255);
                                labelStatus.Text = "Alice is in " + dictStage[stageID] + "'s stage";
                            }
                            if (!inStage)
                            {
                                foreach (Control group in Controls)
                                {
                                    if (group is GroupBox)
                                    {
                                        switch (group.Name)
                                        {
                                            case "groupCoordinates":
                                                foreach (Control c in group.Controls)
                                                {
                                                    if (c is Button || c is TextBox) c.Enabled = true;
                                                    if (c is TextBox) c.Text = string.Empty;
                                                }
                                                break;
                                            default:
                                                foreach (Control c in group.Controls) c.Enabled = true; break;
                                        }
                                    }
                                }

                                UpdateComboSaveStates(true);
                                StoreValues();

                                new Thread(Coordinates) { IsBackground = true }.Start();
                                new Thread(Timers) { IsBackground = true }.Start();
                                new Thread(Freeze) { IsBackground = true }.Start();
                                new Thread(Track) { IsBackground = true }.Start();
                                new Thread(EnableWeaponTankIcons) { IsBackground = true }.Start();
                                new Thread(Hotkeys) { IsBackground = true }.Start();
                                inStage = true;
                            }
                        }
                        else
                        {
                            if (screenType == STAGE_SELECT)
                            {
                                labelStatus.Text = "Stage select...";
                                labelStatus.ForeColor = Color.Cyan;
                            }
                            else if (screenType == STAGE_LOADING)
                                pm.WriteStatic(SCREEN_TYPE, BitConverter.GetBytes(STAGE)); //forces the stage to show up right away
                            else
                            {
                                labelStatus.Text = "Marisa is on the title screen...";
                                labelStatus.ForeColor = Color.LightGreen;
                            }
                            if (inStage)
                            {
                                weaponBoxBroom.Image = Properties.Resources.broom_off; weaponBoxDoll.Image = Properties.Resources.doll_off;
                                weaponBoxReimu.Image = Properties.Resources.reimu_off; weaponBoxRemilia.Image = Properties.Resources.remi_off;
                                weaponBoxYoumu.Image = Properties.Resources.youmu_off; weaponBoxReisen.Image = Properties.Resources.reisen_off;
                                weaponBoxCirno.Image = Properties.Resources.cirno_off; weaponBoxSakuya.Image = Properties.Resources.sakuya_off;
                                weaponBoxYuyuko.Image = Properties.Resources.yuyuko_off; weaponBoxEirin.Image = Properties.Resources.eirin_off;
                                tankBox1.Image = Properties.Resources.tank_off; tankBox2.Image = Properties.Resources.tank_off;
                                tankBox3.Image = Properties.Resources.tank_off; tankBox4.Image = Properties.Resources.tank_off;

                                foreach (Control group in Controls)
                                {
                                    if (group is GroupBox)
                                    {
                                        switch (group.Name)
                                        {
                                            case "groupCoordinates":
                                                foreach (Control c in group.Controls)
                                                {
                                                    if (c is Button || c is TextBox) c.Enabled = false;
                                                    if (c is TextBox) c.Text = string.Empty;
                                                }
                                                break;
                                            default:
                                                foreach (Control c in group.Controls) c.Enabled = false; break;
                                        }
                                    }
                                }

                                comboSaves.Items.Clear();
                                labelX.Text = "X:"; labelY.Text = "Y:"; labelStoredX.Text = "X:"; labelStoredY.Text = "Y:";
                                labelScreenTime.Text = "00:00:00"; labelLastScreenTime.Text = "00:00:00";
                                inStage = false;
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        Console.WriteLine(ex.Message);
                }
                Thread.Sleep(100);
            }
        }

        private void Hotkeys()
        {
            byte[] modifier1 = new byte[1], modifier2 = new byte[1], modifier3 = new byte[1],
                   modifier4 = new byte[1], modifier5 = new byte[1], modifier6 = new byte[1],
                   key1 = new byte[1], key2 = new byte[1], key3 = new byte[1],
                   key4 = new byte[1], key5 = new byte[1], key6 = new byte[1];
           
            while (true)
            {
                if (lstHotkeys[0] != 0) modifier1 = pm.ReadStatic(lstHotkeys[0], modifier1); else modifier1[0] = 128;
                if (lstHotkeys[2] != 0) modifier2 = pm.ReadStatic(lstHotkeys[2], modifier2); else modifier2[0] = 128;
                if (lstHotkeys[4] != 0) modifier3 = pm.ReadStatic(lstHotkeys[4], modifier3); else modifier3[0] = 128;
                if (lstHotkeys[6] != 0) modifier4 = pm.ReadStatic(lstHotkeys[6], modifier4); else modifier4[0] = 128;
                if (lstHotkeys[8] != 0) modifier5 = pm.ReadStatic(lstHotkeys[8], modifier5); else modifier5[0] = 128;
                if (lstHotkeys[10] != 0) modifier6 = pm.ReadStatic(lstHotkeys[10], modifier6); else modifier6[0] = 128;

                key1 = pm.ReadStatic(lstHotkeys[1], key1); key2 = pm.ReadStatic(lstHotkeys[3], key2);
                key3 = pm.ReadStatic(lstHotkeys[5], key3); key4 = pm.ReadStatic(lstHotkeys[7], key4);
                key5 = pm.ReadStatic(lstHotkeys[9], key5); key6 = pm.ReadStatic(lstHotkeys[11], key6);

                bool isHotkey1Pressed = modifier1[0] == 128 && key1[0] == 128;
                bool isHotkey2Pressed = modifier2[0] == 128 && key2[0] == 128;
                bool isHotkey3Pressed = modifier3[0] == 128 && key3[0] == 128;
                bool isHotkey4Pressed = modifier4[0] == 128 && key4[0] == 128;
                bool isHotkey5Pressed = modifier5[0] == 128 && key5[0] == 128;
                bool isHotkey6Pressed = modifier6[0] == 128 && key6[0] == 128;

                if (isHotkey1Pressed) { StoreValues(); comboSaves.SelectedIndex = -1; }
                if (isHotkey2Pressed) { LoadStoredValues(); }
                if (isHotkey3Pressed) { LoadNextSaveState(); }
                if (isHotkey4Pressed) { buttonDie_Click(null, null); }
                if (isHotkey5Pressed) { buttonCheckpoint_Click(null, null); }
                if (isHotkey6Pressed) { LoadNextStage(); }

                Thread.Sleep(75);

                if (screenType == TITLE_SCREEN || screenType == STAGE_SELECT || screenType == STAGE_LOADING)
                {
                    print("Exiting thread " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    break;
                }
            }
        }

        private void ReadValues()
        {
            while (true)
            {
                byte[] buffer = pm.Read(FIRST_OFFSET, X_OFFSET); XF = BitConverter.ToSingle(buffer, 0); //convert to float
                buffer = pm.Read(FIRST_OFFSET, Y_OFFSET); YF = BitConverter.ToSingle(buffer, 0); //convert to float

                buffer = pm.ReadStatic(STATE, buffer); state = buffer[0];
                buffer = pm.ReadStatic(SCREEN_TYPE, buffer); screenType = buffer[0];
                buffer = pm.ReadStatic(STAGE_ID, buffer); stageID = buffer[0];
                buffer = pm.ReadStatic(SCREEN_TIMER, buffer); screenTimer = BitConverter.ToUInt32(buffer, 0);

                buffer = pm.Read(FIRST_OFFSET, CHARACTER_OFFSET); curCharacter = BitConverter.ToInt16(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, BROOM_FLAG_OFFSET); flagBroom = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, DOLL_FLAG_OFFSET); flagDoll = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, REIMU_FLAG_OFFSET); flagReimu = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, CIRNO_FLAG_OFFSET); flagCirno = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, SAKUYA_FLAG_OFFSET); flagSakuya = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, REMILIA_FLAG_OFFSET); flagRemilia = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, YOUMU_FLAG_OFFSET); flagYoumu = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, YUYUKO_FLAG_OFFSET); flagYuyuko = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, REISEN_FLAG_OFFSET); flagReisen = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, EIRIN_FLAG_OFFSET); flagEirin = buffer[0];

                buffer = pm.Read(FIRST_OFFSET, MENU_TANK_SLOT_1_OFFSET); flagTank1 = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, MENU_TANK_SLOT_2_OFFSET); flagTank2 = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, MENU_TANK_SLOT_3_OFFSET); flagTank3 = buffer[0];
                buffer = pm.Read(FIRST_OFFSET, MENU_TANK_SLOT_4_OFFSET); flagTank4 = buffer[0];

                buffer = pm.Read(FIRST_OFFSET_BOSS_HP, 0x0); bossHP = BitConverter.ToInt16(buffer, 0);

                int sleep;
                if (screenType == TITLE_SCREEN || screenType == STAGE_SELECT || screenType == STAGE_LOADING) sleep = 100;
                else sleep = 1;
                Thread.Sleep(sleep);
            }
        }

        private void Coordinates()
        {
            while (true)
            {
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        labelX.Text = "X: " + XF.ToString("0.000");
                        labelY.Text = "Y: " + YF.ToString("0.000");
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        print(ex.Message);
                }
                if (screenType == TITLE_SCREEN || screenType == STAGE_SELECT || screenType == STAGE_LOADING)
                {
                    print("Exiting thread " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    break;
                }
                Thread.Sleep(25);
            }
        }

        private void Timers()
        {
            TimeSpan t = new TimeSpan();
            uint tempTimer = 0;
            while (true)
            {
                t = TimeSpan.FromSeconds((double)screenTimer / 60);
                string stringScreenTimer = t.Minutes.ToString("D2") + ":" + t.Seconds.ToString("D2") + "." + t.Milliseconds.ToString("D3");
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (tempTimer > screenTimer)
                        {
                            t = TimeSpan.FromSeconds((double)tempTimer / 60);
                            string stringLastScreenTime = t.Minutes.ToString("D2") + ":" + t.Seconds.ToString("D2") + "." + t.Milliseconds.ToString("D3");
                            labelLastScreenTime.Text = stringLastScreenTime;
                        }
                        labelScreenTime.Text = stringScreenTimer;
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        print(ex.Message);
                }
                tempTimer = screenTimer;
                if (screenType == TITLE_SCREEN || screenType == STAGE_SELECT || screenType == STAGE_LOADING)
                {
                    print("Exiting thread " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    break;
                }
                Thread.Sleep(25);
            }
        }

        private void Freeze()
        {
            while (true)
            {
                if (weaponCheckBroom.Checked) pm.Write(FIRST_OFFSET, BROOM_AMMO_OFFSET, BitConverter.GetBytes(FULL_AMMO));
                if (weaponCheckDoll.Checked) pm.Write(FIRST_OFFSET, DOLL_AMMO_OFFSET, BitConverter.GetBytes(FULL_AMMO));
                if (weaponCheckReimu.Checked) pm.Write(FIRST_OFFSET, REIMU_AMMO_OFFSET, BitConverter.GetBytes(FULL_AMMO));
                if (weaponCheckRemilia.Checked) pm.Write(FIRST_OFFSET, REMILIA_AMMO_OFFSET, BitConverter.GetBytes(FULL_AMMO));
                if (weaponCheckYoumu.Checked) pm.Write(FIRST_OFFSET, YOUMU_AMMO_OFFSET, BitConverter.GetBytes(FULL_AMMO));
                if (weaponCheckReisen.Checked) pm.Write(FIRST_OFFSET, REISEN_AMMO_OFFSET, BitConverter.GetBytes(FULL_AMMO));
                if (weaponCheckCirno.Checked) pm.Write(FIRST_OFFSET, CIRNO_AMMO_OFFSET, BitConverter.GetBytes(FULL_AMMO));
                if (weaponCheckSakuya.Checked) pm.Write(FIRST_OFFSET, SAKUYA_AMMO_OFFSET, BitConverter.GetBytes(FULL_AMMO));
                if (weaponCheckYuyuko.Checked) pm.Write(FIRST_OFFSET, YUYUKO_AMMO_OFFSET, BitConverter.GetBytes(FULL_AMMO));
                if (weaponCheckEirin.Checked) pm.Write(FIRST_OFFSET, EIRIN_AMMO_OFFSET, BitConverter.GetBytes(FULL_AMMO));

                if (checkHealth.Checked)
                {
                    pm.Write(FIRST_OFFSET, MARISA_HP_OFFSET, BitConverter.GetBytes(FULL_HP));
                    pm.Write(FIRST_OFFSET, ALICE_HP_OFFSET, BitConverter.GetBytes(FULL_HP));
                }

                if (checkLives.Checked)
                    pm.Write(FIRST_OFFSET, LIVES_OFFSET, BitConverter.GetBytes(2));

                if (checkIframes.Checked)
                    pm.Write(FIRST_OFFSET, IFRAMES_OFFSET, BitConverter.GetBytes(200));

                if (screenType == TITLE_SCREEN || screenType == STAGE_SELECT || screenType == STAGE_LOADING)
                {
                    print("Exiting thread " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    break;
                }
                Thread.Sleep(100);
            }
        }

        private void Track()
        {
            while (true)
            {
                if (screenType == STAGE && state == DEAD) //track death
                {
                    //fast respawn
                    pm.WriteStatic(STATE, BitConverter.GetBytes(DEAD));
                    pm.WriteStatic(DELAY, BitConverter.GetBytes(240));
                    Thread.Sleep(100);
                    pm.WriteStatic(STATE, BitConverter.GetBytes(READY));
                    pm.WriteStatic(DELAY, BitConverter.GetBytes(120));
                    LoadStoredValues();
                }

                try //track boss hp
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (bossHP >= 0 && bossHP <= 280)
                        {
                            barBossHP.Value = bossHP;
                            labelBossHp.Text = bossHP.ToString();
                        }
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        print(ex.Message);
                }

                if (screenType == TITLE_SCREEN || screenType == STAGE_SELECT || screenType == STAGE_LOADING)
                {
                    print("Exiting thread " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    break;
                }
                Thread.Sleep(100);
            }
        }

        private void EnableWeaponTankIcons()
        {
            while (true)
            {
                dictWeapon["Broom"] = flagBroom; dictWeapon["Doll"] = flagDoll;
                dictWeapon["Reimu"] = flagReimu; dictWeapon["Remilia"] = flagRemilia; dictWeapon["Youmu"] = flagYoumu; dictWeapon["Reisen"] = flagReisen;
                dictWeapon["Cirno"] = flagCirno; dictWeapon["Sakuya"] = flagSakuya; dictWeapon["Yuyuko"] = flagYuyuko; dictWeapon["Eirin"] = flagEirin;

                EnableIcon(flag: flagBroom, character: "broom", box: weaponBoxBroom, regularWeapon: false);
                EnableIcon(flag: flagDoll, character: "doll", box: weaponBoxDoll, regularWeapon: false);
                EnableIcon(flag: flagReimu, character: "reimu", box: weaponBoxReimu);
                EnableIcon(flag: flagRemilia, character: "remi", box: weaponBoxRemilia);
                EnableIcon(flag: flagYoumu, character: "youmu", box: weaponBoxYoumu);
                EnableIcon(flag: flagReisen, character: "reisen", box: weaponBoxReisen);
                EnableIcon(flag: flagCirno, character: "cirno", box: weaponBoxCirno);
                EnableIcon(flag: flagSakuya, character: "sakuya", box: weaponBoxSakuya);
                EnableIcon(flag: flagYuyuko, character: "yuyuko", box: weaponBoxYuyuko);
                EnableIcon(flag: flagEirin, character: "eirin", box: weaponBoxEirin);

                EnableIcon(flag: flagTank1, box: tankBox1, isTank: true); EnableIcon(flagTank2, box: tankBox2, isTank: true);
                EnableIcon(flag: flagTank3, box: tankBox3, isTank: true); EnableIcon(flagTank4, box: tankBox4, isTank: true);

                if (screenType == TITLE_SCREEN || screenType == STAGE_SELECT || screenType == STAGE_LOADING)
                {
                    print("Exiting thread " + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    break;
                }
                Thread.Sleep(250);
            }
        }

        private void EnableIcon(int flag = 0, string character = "", PictureBox box = null, bool regularWeapon = true, bool isTank = false)
        {
            try
            {
                if (!isTank)
                {
                    if (regularWeapon)
                    {
                        switch (flag)
                        {
                            case ON_NOBODY:
                                object test = Properties.Resources.ResourceManager.GetObject(character + "_off");
                                if (test is Image)
                                    box.Image = (Image)Properties.Resources.ResourceManager.GetObject(character + "_off");
                                break;
                            case ON_MARISA:
                                test = Properties.Resources.ResourceManager.GetObject(character + "_on_marisa");
                                if (test is Image)
                                    box.Image = (Image)Properties.Resources.ResourceManager.GetObject(character + "_on_marisa");
                                break;
                            case ON_ALICE:
                                test = Properties.Resources.ResourceManager.GetObject(character + "_on_alice");
                                if (test is Image)
                                    box.Image = (Image)Properties.Resources.ResourceManager.GetObject(character + "_on_alice");
                                break;
                        }
                    }
                    else
                    {
                        switch (flag)
                        {
                            case SPECIAL_WEAPON_OFF:
                                object test = Properties.Resources.ResourceManager.GetObject(character + "_off");
                                if (test is Image)
                                    box.Image = (Image)Properties.Resources.ResourceManager.GetObject(character + "_off");
                                break;
                            case SPECIAL_WEAPON_ON:
                                test = Properties.Resources.ResourceManager.GetObject(character + "_on");
                                if (test is Image)
                                    box.Image = (Image)Properties.Resources.ResourceManager.GetObject(character + "_on");
                                break;
                        }
                    }
                }
                else
                {
                    switch (flag)
                    {
                        case NO_TANK: box.Image = Properties.Resources.tank_off; break;
                        case ETANK: box.Image = Properties.Resources.etank; break;
                        case STAR_TANK: box.Image = Properties.Resources.startank; break;
                        case DOUBLE_ETANK: box.Image = Properties.Resources.doubletank; break;
                    }
                }
            }
            catch (InvalidOperationException ex) { print(ex.Message); }
        }
        #endregion

        #region buttons
        private void buttonStore_Click(object sender, EventArgs e)
        {
            if (screenType == STAGE && state == PLAYING)
                StoreValues();
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (screenType == STAGE && state == PLAYING)
                LoadStoredValues();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (File.Exists(configpath + savestatesfilename)) //checks if savestates.cfg exists
            {
                int lineNumber = 0;
                //find the line number of the line to delete
                using (StreamReader sr = File.OpenText(configpath + savestatesfilename))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        //leave the loop once the line to remove is found
                        if (line == comboSaves.Text)
                            break;
                        lineNumber++;
                    }
                }
                //read all save state lines into memory
                var lines = File.ReadAllLines(configpath + savestatesfilename).ToList();
                lines.RemoveAt(lineNumber);
                File.WriteAllLines(configpath + savestatesfilename, lines); //write the new lines to the file
                UpdateComboSaveStates(true);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            using (SaveStateName ssname = new SaveStateName())
            {
                if (ssname.ShowDialog() == DialogResult.OK)
                {
                    bool sectionFound = false;
                    int lineNumber = 0;

                    if (File.Exists(configpath + savestatesfilename)) //checks if savestates.cfg exists
                    {
                        //find the line number to which the new line needs to be inserted
                        using (StreamReader sr = File.OpenText(configpath + savestatesfilename))
                        {
                            while (!sr.EndOfStream)
                            {
                                string line = sr.ReadLine();

                                if (line.Length > 0)
                                    if (line.Contains("[") && sectionFound)
                                        break;
                                if (line.Contains(dictStage[stageID] + "-" + stageID))
                                    sectionFound = true;
                                lineNumber++;
                            }
                        }
                        //read all save state lines into memory
                        var lines = File.ReadAllLines(configpath + savestatesfilename).ToList();
                        //insert the desired line at the number found - 1
                        StoreValues();
                        lines.Insert(lineNumber - 1, ssname.name + " | " + ss.ToString());
                        File.WriteAllLines(configpath + savestatesfilename, lines); //write the new lines to the file
                        UpdateComboSaveStates(false);
                    }
                }
            }
        }

        private void buttonGameOver_Click(object sender, EventArgs e)
        {
            //fast respawn
            checkLives.Checked = false;
            pm.Write(FIRST_OFFSET, LIVES_OFFSET, BitConverter.GetBytes(0));
            pm.WriteStatic(STATE, BitConverter.GetBytes(DEAD));
            pm.WriteStatic(DELAY, BitConverter.GetBytes(240));
            for (int i = 0; i < 5000; i++)
                pm.WriteStatic(SCREEN_TYPE, BitConverter.GetBytes(CONTINUE));
        }

        private void buttonWin_Click(object sender, EventArgs e)
        {
            pm.WriteStatic(STATE, BitConverter.GetBytes(WIN_FANFARE));
            Thread.Sleep(250);
            pm.WriteStatic(DELAY, BitConverter.GetBytes(220));
            Thread.Sleep(250);
            pm.WriteStatic(DELAY, BitConverter.GetBytes(150));
            for (int i = 0; i < 5000; i++)
                pm.WriteStatic(SCREEN_TYPE, BitConverter.GetBytes(WEAPON_GET));
        }

        private void buttonDie_Click(object sender, EventArgs e)
        {
            //fast respawn
            pm.Write(FIRST_OFFSET, LIVES_OFFSET, BitConverter.GetBytes(3));
            pm.WriteStatic(STATE, BitConverter.GetBytes(DEAD));
            pm.WriteStatic(DELAY, BitConverter.GetBytes(240));
            Thread.Sleep(100);
            pm.WriteStatic(STATE, BitConverter.GetBytes(READY));
            pm.WriteStatic(DELAY, BitConverter.GetBytes(120));
        }

        private void buttonCheckpoint_Click(object sender, EventArgs e)
        {
            //fast respawn
            int checkpoint = pm.Read(FIRST_OFFSET, CHECKPOINT_OFFSET)[0];
            switch (checkpoint)
            {
                case CHECKPOINT_START: pm.Write(FIRST_OFFSET, CHECKPOINT_OFFSET, new byte[1] { CHECKPOINT }); break;
                case CHECKPOINT: pm.Write(FIRST_OFFSET, CHECKPOINT_OFFSET, new byte[1] { CHECKPOINT_BOSS }); break;
                case CHECKPOINT_BOSS: pm.Write(FIRST_OFFSET, CHECKPOINT_OFFSET, new byte[1] { CHECKPOINT_START }); break;
            }
            pm.Write(FIRST_OFFSET, LIVES_OFFSET, BitConverter.GetBytes(3));
            pm.WriteStatic(STATE, BitConverter.GetBytes(DEAD));
            pm.WriteStatic(DELAY, BitConverter.GetBytes(240));
            Thread.Sleep(100);
            pm.WriteStatic(STATE, BitConverter.GetBytes(READY));
            pm.WriteStatic(DELAY, BitConverter.GetBytes(120));
        }

        private void weaponBox_Click(object sender, EventArgs e)
        {
            var s = sender as PictureBox;
            switch (s.Name)
            {
                case "weaponBoxBroom": SetWeapon(BROOM_FLAG_OFFSET, flagBroom, false); break;
                case "weaponBoxDoll": SetWeapon(DOLL_FLAG_OFFSET, flagDoll, false); break;
                case "weaponBoxReimu": SetWeapon(REIMU_FLAG_OFFSET, flagReimu, true); break;
                case "weaponBoxRemilia": SetWeapon(REMILIA_FLAG_OFFSET, flagRemilia, true); break;
                case "weaponBoxYoumu": SetWeapon(YOUMU_FLAG_OFFSET, flagYoumu, true); break;
                case "weaponBoxReisen": SetWeapon(REISEN_FLAG_OFFSET, flagReisen, true); break;
                case "weaponBoxCirno": SetWeapon(CIRNO_FLAG_OFFSET, flagCirno, true); break;
                case "weaponBoxSakuya": SetWeapon(SAKUYA_FLAG_OFFSET, flagSakuya, true); break;
                case "weaponBoxYuyuko": SetWeapon(YUYUKO_FLAG_OFFSET, flagYuyuko, true); break;
                case "weaponBoxEirin": SetWeapon(EIRIN_FLAG_OFFSET, flagEirin, true); break;
            }
        }

        private void tankBox_Click(object sender, EventArgs e)
        {
            var s = sender as PictureBox;
            switch (s.Name)
            {
                case "tankBox1": SetTank(MENU_TANK_SLOT_1_OFFSET, tankBox1); break;
                case "tankBox2": SetTank(MENU_TANK_SLOT_2_OFFSET, tankBox2); break;
                case "tankBox3": SetTank(MENU_TANK_SLOT_3_OFFSET, tankBox3); break;
                case "tankBox4": SetTank(MENU_TANK_SLOT_4_OFFSET, tankBox4); break;
            }
        }
        #endregion

        #region menustrip
        private void applicationFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", configpath);
        }

        private void helpAboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
        }

        private void hotkeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HotkeyDialog hd = new HotkeyDialog();
            hd.ShowDialog();
            LoadHotkeys();
        }
        #endregion

        #region states
        private void UpdateComboSaveStates(bool first)
        {
            if (dictStage.ContainsKey(stageID))
            {
                bool sectionFound = false;
                if (File.Exists(configpath + savestatesfilename)) //checks if savestates.cfg exists
                {
                    comboSaves.Items.Clear();
                    using (StreamReader sr = File.OpenText(configpath + savestatesfilename))
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();

                            //skip empty lines
                            if (line.Length > 0)
                            {
                                //if another section is reached after the desired one is parsed
                                if (line.Contains("[") && sectionFound)
                                    break;

                                //if flag is true then analyze the line to check its screenID
                                if (sectionFound)
                                    comboSaves.Items.Add(line);

                                //if reached the desired section -> set flag to true
                                if (line.Contains(dictStage[stageID] + "-" + stageID))
                                    sectionFound = true;
                            }
                        }
                        if (first) comboSaves.SelectedIndex = -1;
                        else comboSaves.SelectedIndex = comboSaves.Items.Count - 1; //this removes the empty entry
                    }
                }
            }
        }

        private void comboSaves_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string[] split = comboSaves.Text.Split('|');
            ss = new SaveState(split[1]);

            labelStoredX.Text = "X: " + ss._XF.ToString("0.000"); //X
            labelStoredY.Text = "Y: " + ss._YF.ToString("0.000"); //Y

            //weapon flags
            dictWeapon["Broom"] = ss._BroomFlag; dictWeapon["Doll"] = ss._DollFlag;
            dictWeapon["Reimu"] = ss._ReimuFlag; dictWeapon["Remilia"] = ss._RemiliaFlag;
            dictWeapon["Youmu"] = ss._YoumuFlag; dictWeapon["Reisen"] = ss._ReisenFlag;
            dictWeapon["Cirno"] = ss._CirnoFlag; dictWeapon["Sakuya"] = ss._SakuyaFlag;
            dictWeapon["Yuyuko"] = ss._YuyukoFlag; dictWeapon["Eirin"] = ss._EirinFlag;

            LoadStoredValues();
        }
        #endregion

        #region hotkeys
        private void WriteDefaultHotkeyConfig()
        {
            TextWriter writer = new StreamWriter(configpath + hotkeyfilename);
            writer.WriteLine(hotkeyVersion + "\nLAlt\nD1\nLAlt\nD2\nLAlt\nD3\nLAlt\nD4\nLAlt\nD5\nLAlt\nD6");
            writer.Close();
        }

        private void LoadHotkeys()
        {
            if (!Directory.Exists(configpath)) Directory.CreateDirectory(configpath);
            if (!File.Exists(configpath + hotkeyfilename))
                WriteDefaultHotkeyConfig();
            if (File.Exists(configpath + hotkeyfilename)) //checks if hotkey.cfg exists
            {
                if (File.ReadLines(configpath + hotkeyfilename).First().Contains(hotkeyVersion))
                {
                    using (StreamReader sr = File.OpenText(configpath + hotkeyfilename))
                    {
                        int modifier = 0;
                        lstHotkeys.Clear();
                        sr.ReadLine(); //skip the first line containing the version number
                        for (int i = 2; i <= numberHotkeys * 2 + 1; i++)
                        {
                            if (i % 2 == 0) //if the line number is even then it's a modifier
                            {
                                modifier = keybKeys.dictModifierKeys[sr.ReadLine()]; //this variable will hold the address of the modifier
                                lstHotkeys.Add(modifier);
                            }
                            else //if the line number is odd then it's a hotkey
                            {
                                lstHotkeys.Add(keybKeys.dictKeys[sr.ReadLine()]);
                            }
                        }
                    }
                    //foreach (int key in lstHotkeys) print(key.ToString("x"));
                }
                else
                {
                    MessageBox.Show("Some changes have been made to hotkeys. They have been set back to defaults.\n",
                                    "Hotkeys changed",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    WriteDefaultHotkeyConfig();
                }
            }
        }
        #endregion

        #region actions
        private void StoreValues()
        {   
            if (screenType == STAGE && state == PLAYING)
            {
                byte[] xPos = pm.Read(FIRST_OFFSET, X_OFFSET); //read x speed value
                byte[] yPos = pm.Read(FIRST_OFFSET, Y_OFFSET); //read y speed value

                ss = new SaveState(
                    X: BitConverter.ToInt32(pm.Read(FIRST_OFFSET, X_OFFSET), 0), Y: BitConverter.ToInt32(pm.Read(FIRST_OFFSET, Y_OFFSET), 0),
                    XF: BitConverter.ToSingle(pm.Read(FIRST_OFFSET, X_OFFSET), 0), YF: BitConverter.ToSingle(pm.Read(FIRST_OFFSET, Y_OFFSET), 0),
                    Camera1X: BitConverter.ToInt32(pm.Read(FIRST_OFFSET, CAMERA_X_1_OFFSET), 0),
                    Camera1Y: BitConverter.ToInt32(pm.Read(FIRST_OFFSET, CAMERA_Y_1_OFFSET), 0),
                    Camera2X: BitConverter.ToInt32(pm.Read(FIRST_OFFSET, CAMERA_X_2_OFFSET), 0),
                    Camera2Y: BitConverter.ToInt32(pm.Read(FIRST_OFFSET, CAMERA_Y_2_OFFSET), 0),
                    CameraViewX: pm.Read(FIRST_OFFSET, CAMERA_VIEW_X_OFFSET)[0], CameraViewY: pm.Read(FIRST_OFFSET, CAMERA_VIEW_Y_OFFSET)[0],
                    MarisaHP: pm.Read(FIRST_OFFSET, MARISA_HP_OFFSET)[0], AliceHP: pm.Read(FIRST_OFFSET, ALICE_HP_OFFSET)[0],
                    Character: BitConverter.ToInt16(pm.Read(FIRST_OFFSET, CHARACTER_OFFSET), 0),
                    CharacterWeapon: pm.Read(FIRST_OFFSET, CHARACTER_WEAPON_OFFSET)[0],
                    CharacterSprite: BitConverter.ToInt16(pm.Read(FIRST_OFFSET, CHARACTER_SPRITE_OFFSET), 0),
                    BroomAmmo: pm.Read(FIRST_OFFSET, BROOM_AMMO_OFFSET)[0], BroomFlag: pm.Read(FIRST_OFFSET, BROOM_FLAG_OFFSET)[0],
                    CirnoAmmo: pm.Read(FIRST_OFFSET, CIRNO_AMMO_OFFSET)[0], CirnoFlag: pm.Read(FIRST_OFFSET, CIRNO_FLAG_OFFSET)[0],
                    DollAmmo: pm.Read(FIRST_OFFSET, DOLL_AMMO_OFFSET)[0], DollFlag: pm.Read(FIRST_OFFSET, DOLL_FLAG_OFFSET)[0],
                    EirinAmmo: pm.Read(FIRST_OFFSET, EIRIN_AMMO_OFFSET)[0], EirinFlag: pm.Read(FIRST_OFFSET, EIRIN_FLAG_OFFSET)[0],
                    ReimuAmmo: pm.Read(FIRST_OFFSET, REIMU_AMMO_OFFSET)[0], ReimuFlag: pm.Read(FIRST_OFFSET, REIMU_FLAG_OFFSET)[0],
                    ReisenAmmo: pm.Read(FIRST_OFFSET, REISEN_AMMO_OFFSET)[0], ReisenFlag: pm.Read(FIRST_OFFSET, REISEN_FLAG_OFFSET)[0],
                    RemiliaAmmo: pm.Read(FIRST_OFFSET, REMILIA_AMMO_OFFSET)[0], RemiliaFlag: pm.Read(FIRST_OFFSET, REMILIA_FLAG_OFFSET)[0],
                    SakuyaAmmo: pm.Read(FIRST_OFFSET, SAKUYA_AMMO_OFFSET)[0], SakuyaFlag: pm.Read(FIRST_OFFSET, SAKUYA_FLAG_OFFSET)[0],
                    YoumuAmmo: pm.Read(FIRST_OFFSET, YOUMU_AMMO_OFFSET)[0], YoumuFlag: pm.Read(FIRST_OFFSET, YOUMU_FLAG_OFFSET)[0],
                    YuyukoAmmo: pm.Read(FIRST_OFFSET, YUYUKO_AMMO_OFFSET)[0], YuyukoFlag: pm.Read(FIRST_OFFSET, YUYUKO_FLAG_OFFSET)[0],
                    MenuCursor: BitConverter.ToInt32(pm.Read(FIRST_OFFSET, MENU_CURSOR_OFFSET), 0),
                    Tanks: BitConverter.ToInt32(pm.Read(FIRST_OFFSET, MENU_TANKS_OFFSET), 0),
                    Lives: pm.Read(FIRST_OFFSET, LIVES_OFFSET)[0]);

                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        labelStoredX.Text = "X: " + ss._XF.ToString("0.000");
                        labelStoredY.Text = "Y: " + ss._YF.ToString("0.000");
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        print(ex.Message);
                }
            }
        }

        private void LoadStoredValues()
        {
            if ((screenType == STAGE && state == PLAYING) && (ss._X != 1 && ss._Y != 1))
            {
                pm.Write(FIRST_OFFSET, X_OFFSET, BitConverter.GetBytes(ss._X));
                pm.Write(FIRST_OFFSET, Y_OFFSET, BitConverter.GetBytes(ss._Y));

                pm.Write(FIRST_OFFSET, CAMERA_VIEW_X_OFFSET, BitConverter.GetBytes(ss._CameraViewX));
                pm.Write(FIRST_OFFSET, CAMERA_VIEW_Y_OFFSET, BitConverter.GetBytes(ss._CameraViewY));
                pm.Write(FIRST_OFFSET, CAMERA_X_1_OFFSET, BitConverter.GetBytes(ss._Camera1X));
                pm.Write(FIRST_OFFSET, CAMERA_Y_1_OFFSET, BitConverter.GetBytes(ss._Camera1Y));
                pm.Write(FIRST_OFFSET, CAMERA_X_2_OFFSET, BitConverter.GetBytes(ss._Camera2X));
                pm.Write(FIRST_OFFSET, CAMERA_Y_2_OFFSET, BitConverter.GetBytes(ss._Camera2Y));

                pm.Write(FIRST_OFFSET, MARISA_HP_OFFSET, new byte[1] { (byte)ss._MarisaHP });
                pm.Write(FIRST_OFFSET, ALICE_HP_OFFSET, new byte[1] { (byte)ss._AliceHP });

                pm.Write(FIRST_OFFSET, CHARACTER_OFFSET, BitConverter.GetBytes(ss._Character));
                pm.Write(FIRST_OFFSET, CHARACTER_WEAPON_OFFSET, new byte[1] { (byte)ss._CharacterWeapon });
                pm.Write(FIRST_OFFSET, CHARACTER_SPRITE_OFFSET, BitConverter.GetBytes(ss._CharacterSprite));

                pm.Write(FIRST_OFFSET, BROOM_FLAG_OFFSET, new byte[1] { (byte)dictWeapon["Broom"] });
                pm.Write(FIRST_OFFSET, DOLL_FLAG_OFFSET, new byte[1] { (byte)dictWeapon["Doll"] });
                pm.Write(FIRST_OFFSET, REIMU_FLAG_OFFSET, new byte[1] { (byte)dictWeapon["Reimu"] });
                pm.Write(FIRST_OFFSET, REMILIA_FLAG_OFFSET, new byte[1] { (byte)dictWeapon["Remilia"] });
                pm.Write(FIRST_OFFSET, YOUMU_FLAG_OFFSET, new byte[1] { (byte)dictWeapon["Youmu"] });
                pm.Write(FIRST_OFFSET, REISEN_FLAG_OFFSET, new byte[1] { (byte)dictWeapon["Reisen"] });
                pm.Write(FIRST_OFFSET, CIRNO_FLAG_OFFSET, new byte[1] { (byte)dictWeapon["Cirno"] });
                pm.Write(FIRST_OFFSET, SAKUYA_FLAG_OFFSET, new byte[1] { (byte)dictWeapon["Sakuya"] });
                pm.Write(FIRST_OFFSET, YUYUKO_FLAG_OFFSET, new byte[1] { (byte)dictWeapon["Yuyuko"] });
                pm.Write(FIRST_OFFSET, EIRIN_FLAG_OFFSET, new byte[1] { (byte)dictWeapon["Eirin"] });

                pm.Write(FIRST_OFFSET, BROOM_AMMO_OFFSET, BitConverter.GetBytes(ss._BroomAmmo));
                pm.Write(FIRST_OFFSET, DOLL_AMMO_OFFSET, BitConverter.GetBytes(ss._DollAmmo));
                pm.Write(FIRST_OFFSET, REIMU_AMMO_OFFSET, BitConverter.GetBytes(ss._ReimuAmmo));
                pm.Write(FIRST_OFFSET, REMILIA_AMMO_OFFSET, BitConverter.GetBytes(ss._RemiliaAmmo));
                pm.Write(FIRST_OFFSET, YOUMU_AMMO_OFFSET, BitConverter.GetBytes(ss._YoumuAmmo));
                pm.Write(FIRST_OFFSET, REISEN_AMMO_OFFSET, BitConverter.GetBytes(ss._ReisenAmmo));
                pm.Write(FIRST_OFFSET, CIRNO_AMMO_OFFSET, BitConverter.GetBytes(ss._CirnoAmmo));
                pm.Write(FIRST_OFFSET, SAKUYA_AMMO_OFFSET, BitConverter.GetBytes(ss._SakuyaAmmo));
                pm.Write(FIRST_OFFSET, YUYUKO_AMMO_OFFSET, BitConverter.GetBytes(ss._YuyukoAmmo));
                pm.Write(FIRST_OFFSET, EIRIN_AMMO_OFFSET, BitConverter.GetBytes(ss._EirinAmmo));

                pm.Write(FIRST_OFFSET, MENU_CURSOR_OFFSET, BitConverter.GetBytes(ss._MenuCursor));
                pm.Write(FIRST_OFFSET, MENU_TANKS_OFFSET, BitConverter.GetBytes(ss._Tanks));
                pm.Write(FIRST_OFFSET, LIVES_OFFSET, new byte[1] { (byte)ss._Lives });
            }
        }

        private void LoadNextSaveState()
        {
            try
            {
                Invoke((MethodInvoker)delegate //using this because thread
                {
                    if (comboSaves.Items.Count > 0)
                    {
                        if (comboSaves.SelectedIndex < comboSaves.Items.Count - 1)
                            comboSaves.SelectedIndex += 1;
                        else if (comboSaves.SelectedIndex == comboSaves.Items.Count - 1 || comboSaves.SelectedIndex == -1)
                            comboSaves.SelectedIndex = 0;
                        comboSaves_SelectionChangeCommitted(null, null);
                    }
                });
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException || ex is InvalidOperationException)
                    print(ex.Message);
            }
        }
        
        private void LoadNextStage()
        {
            try
            {
                Invoke((MethodInvoker)delegate //using this because thread
                {
                    if (comboWarp.SelectedIndex < comboWarp.Items.Count - 1) //if selected entry isn't the last one
                        comboWarp.SelectedIndex += 1; //select the next one
                    else if (comboWarp.SelectedIndex == comboWarp.Items.Count - 1 || comboWarp.SelectedIndex == -1) //if selected entry is the last one or is empty
                        comboWarp.SelectedIndex = 0; //go to the first one
                    buttonWarp_Click(null, null);
                });
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException || ex is InvalidOperationException)
                    print(ex.Message);
            }
        }
        #endregion

        #region warp
        private void checkEarlyBroom_CheckedChanged(object sender, EventArgs e)
        {
            if (checkEarlyBroom.Checked)
            {
                comboWarp.Items.Clear();
                comboWarp.Items.Add("Cirno"); comboWarp.Items.Add("Eirin"); comboWarp.Items.Add("Yuyuko"); comboWarp.Items.Add("Reimu");
                comboWarp.Items.Add("Youmu"); comboWarp.Items.Add("Remilia"); comboWarp.Items.Add("Sakuya"); comboWarp.Items.Add("Reisen");
            }
            else
            {
                comboWarp.Items.Clear();
                comboWarp.Items.Add("Cirno"); comboWarp.Items.Add("Eirin"); comboWarp.Items.Add("Yuyuko"); comboWarp.Items.Add("Reisen");
                comboWarp.Items.Add("Reimu"); comboWarp.Items.Add("Remilia"); comboWarp.Items.Add("Sakuya"); comboWarp.Items.Add("Youmu");
            }
            comboWarp.Items.Add("Patchouli 1"); comboWarp.Items.Add("Patchouli 2"); comboWarp.Items.Add("Patchouli 3");
            comboWarp.Items.Add("Patchouli 4"); comboWarp.Items.Add("Patchouli 5"); comboWarp.Items.Add("Patchouli 6");
        }

        private void buttonWarp_Click(object sender, EventArgs e)
        {
            bool checkLivesWasChecked = false;

            if (checkLives.Checked) { checkLivesWasChecked = true; checkLives.Checked = false; }

            //fast respawn
            pm.Write(FIRST_OFFSET, LIVES_OFFSET, BitConverter.GetBytes(0));
            pm.WriteStatic(STATE, BitConverter.GetBytes(DEAD));
            pm.WriteStatic(DELAY, BitConverter.GetBytes(240));

            for (int i = 0; i < 5000; i++)
                pm.WriteStatic(SCREEN_TYPE, BitConverter.GetBytes(CONTINUE));

            switch (comboWarp.Text)
            {
                case "Cirno":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(CIRNO_STAGE));
                    SetWeapons();
                    break;
                case "Eirin":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(EIRIN_STAGE));
                    SetWeapons(cirno: ON_ALICE);
                    break;
                case "Yuyuko":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(YUYUKO_STAGE));
                    if (checkEarlyBroom.Checked)
                        SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                                   cirno: ON_ALICE, eirin: ON_ALICE);
                    else
                        SetWeapons(cirno: ON_ALICE, eirin: ON_ALICE);
                    break;
                case "Reimu":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(REIMU_STAGE));
                    if (checkEarlyBroom.Checked)
                        SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                                   cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA);
                    else
                        SetWeapons(broom: SPECIAL_WEAPON_ON,
                                   cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reisen: ON_MARISA);
                    break;
                case "Youmu":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(YOUMU_STAGE));
                    if (checkEarlyBroom.Checked)
                        SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                                   cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA);
                    else
                        SetWeapons(broom: SPECIAL_WEAPON_ON,
                                   cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                                   remilia: ON_MARISA, sakuya: ON_MARISA, reisen: ON_MARISA);
                    break;
                case "Remilia":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(REMILIA_STAGE));
                    if (checkEarlyBroom.Checked)
                        SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                                   cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                                   youmu: ON_ALICE);
                    else
                        SetWeapons(broom: SPECIAL_WEAPON_ON,
                                   cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                                   reisen: ON_MARISA);
                    break;
                case "Sakuya":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(SAKUYA_STAGE));
                    if (checkEarlyBroom.Checked)
                        SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                                   cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                                   youmu: ON_ALICE, remilia: ON_MARISA);
                    else
                        SetWeapons(broom: SPECIAL_WEAPON_ON,
                                   cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                                   reisen: ON_MARISA, remilia: ON_MARISA);
                    break;
                case "Reisen":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(REISEN_STAGE));
                    if (checkEarlyBroom.Checked)
                        SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                                   cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                                   youmu: ON_ALICE, remilia: ON_MARISA, sakuya: ON_MARISA);
                    else
                        SetWeapons(cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA);
                    break;
                case "Patchouli 1":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(PATCHY_1));
                    SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                               cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                               youmu: ON_ALICE, remilia: ON_MARISA, sakuya: ON_MARISA, reisen: ON_MARISA);
                    break;
                case "Patchouli 2":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(PATCHY_2));
                    SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                               cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                               youmu: ON_ALICE, remilia: ON_MARISA, sakuya: ON_MARISA, reisen: ON_MARISA);
                    break;
                case "Patchouli 3":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(PATCHY_3));
                    SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                               cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                               youmu: ON_ALICE, remilia: ON_MARISA, sakuya: ON_MARISA, reisen: ON_MARISA);
                    break;
                case "Patchouli 4":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(PATCHY_4));
                    SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                               cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                               youmu: ON_ALICE, remilia: ON_MARISA, sakuya: ON_MARISA, reisen: ON_MARISA);
                    break;
                case "Patchouli 5":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(PATCHY_5));
                    SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                               cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                               youmu: ON_ALICE, remilia: ON_MARISA, sakuya: ON_MARISA, reisen: ON_MARISA);
                    break;
                case "Patchouli 6":
                    pm.WriteStatic(STAGE_ID, BitConverter.GetBytes(PATCHY_6));
                    SetWeapons(broom: SPECIAL_WEAPON_ON, doll: SPECIAL_WEAPON_ON,
                               cirno: ON_ALICE, eirin: ON_ALICE, yuyuko: ON_MARISA, reimu: ON_MARISA,
                               youmu: ON_ALICE, remilia: ON_MARISA, sakuya: ON_MARISA, reisen: ON_MARISA);
                    break;
            }

            if (checkLivesWasChecked) { Thread.Sleep(500); checkLives.Checked = true; }
        }

        private void SetWeapons(int broom = SPECIAL_WEAPON_OFF, int doll = SPECIAL_WEAPON_OFF,
                        int reimu = ON_NOBODY, int remilia = ON_NOBODY, int youmu = ON_NOBODY, int reisen = ON_NOBODY,
                        int cirno = ON_NOBODY, int sakuya = ON_NOBODY, int yuyuko = ON_NOBODY, int eirin = ON_NOBODY)
        {
            pm.Write(FIRST_OFFSET, BROOM_FLAG_OFFSET, new byte[1] { (byte)broom });
            pm.Write(FIRST_OFFSET, DOLL_FLAG_OFFSET, new byte[1] { (byte)doll });
            pm.Write(FIRST_OFFSET, REIMU_FLAG_OFFSET, new byte[1] { (byte)reimu });
            pm.Write(FIRST_OFFSET, REMILIA_FLAG_OFFSET, new byte[1] { (byte)remilia });
            pm.Write(FIRST_OFFSET, YOUMU_FLAG_OFFSET, new byte[1] { (byte)youmu });
            pm.Write(FIRST_OFFSET, REISEN_FLAG_OFFSET, new byte[1] { (byte)reisen });
            pm.Write(FIRST_OFFSET, CIRNO_FLAG_OFFSET, new byte[1] { (byte)cirno });
            pm.Write(FIRST_OFFSET, SAKUYA_FLAG_OFFSET, new byte[1] { (byte)sakuya });
            pm.Write(FIRST_OFFSET, YUYUKO_FLAG_OFFSET, new byte[1] { (byte)yuyuko });
            pm.Write(FIRST_OFFSET, EIRIN_FLAG_OFFSET, new byte[1] { (byte)eirin });
        }
        #endregion

        #region freezer
        private void checkFreezeAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkFreezeAll.Checked)
            {
                weaponCheckBroom.Checked = true; weaponCheckDoll.Checked = true;
                weaponCheckReimu.Checked = true; weaponCheckRemilia.Checked = true;
                weaponCheckYoumu.Checked = true; weaponCheckReisen.Checked = true;
                weaponCheckCirno.Checked = true; weaponCheckSakuya.Checked = true;
                weaponCheckYuyuko.Checked = true; weaponCheckEirin.Checked = true;
            }
            else
            {
                weaponCheckBroom.Checked = false; weaponCheckDoll.Checked = false;
                weaponCheckReimu.Checked = false; weaponCheckRemilia.Checked = false;
                weaponCheckYoumu.Checked = false; weaponCheckReisen.Checked = false;
                weaponCheckCirno.Checked = false; weaponCheckSakuya.Checked = false;
                weaponCheckYuyuko.Checked = false; weaponCheckEirin.Checked = false;
            }
        }

        private void SetWeapon(int offset, int flag, bool regularWeapon)
        {
            if (regularWeapon)
            {
                switch (flag)
                {
                    case ON_NOBODY: pm.Write(FIRST_OFFSET, offset, new byte[1] { ON_MARISA }); break;
                    case ON_MARISA: pm.Write(FIRST_OFFSET, offset, new byte[1] { ON_ALICE }); break;
                    case ON_ALICE: pm.Write(FIRST_OFFSET, offset, new byte[1] { ON_NOBODY }); break;
                }
            }
            else
            {
                switch (flag)
                {
                    case SPECIAL_WEAPON_OFF: pm.Write(FIRST_OFFSET, offset, new byte[1] { SPECIAL_WEAPON_ON }); break;
                    case SPECIAL_WEAPON_ON: pm.Write(FIRST_OFFSET, offset, new byte[1] { SPECIAL_WEAPON_OFF }); break;
                }
            }
        }

        private void SetTank(int offset, PictureBox tankBox)
        {
            int curTank = pm.Read(FIRST_OFFSET, offset)[0];

            switch (curTank)
            {
                case NO_TANK: tankBox.Image = Properties.Resources.etank; pm.Write(FIRST_OFFSET, offset, new byte[1] { ETANK }); break;
                case ETANK: tankBox.Image = Properties.Resources.startank; pm.Write(FIRST_OFFSET, offset, new byte[1] { STAR_TANK }); break;
                case STAR_TANK: tankBox.Image = Properties.Resources.doubletank; pm.Write(FIRST_OFFSET, offset, new byte[1] { DOUBLE_ETANK }); break;
                case DOUBLE_ETANK: tankBox.Image = Properties.Resources.tank_off; pm.Write(FIRST_OFFSET, offset, new byte[1] { NO_TANK }); break;
            }
        }
        #endregion

        private void print(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
