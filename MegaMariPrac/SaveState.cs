using System;
using System.Text;

namespace MegaMariPrac
{
    internal class SaveState
    {
        public int _X { get; set; } public int _Y { get; set; }
        public float _XF { get; set; } public float _YF { get; set; }
        public int _CameraViewX { get; set; } public int _CameraViewY { get; set; }
        public int _Camera1X { get; set; } public int _Camera1Y { get; set; }
        public int _Camera2X { get; set; } public int _Camera2Y { get; set; }
        public int _MarisaHP { get; set; } public int _AliceHP { get; set; }
        public short _Character { get; set; } public int _CharacterWeapon { get; set; } public short _CharacterSprite { get; set; }
        public int _BroomAmmo { get; set; } public int _BroomFlag { get; set; }
        public int _CirnoAmmo { get; set; } public int _CirnoFlag { get; set; }
        public int _DollAmmo { get; set; } public int _DollFlag { get; set; }
        public int _EirinAmmo { get; set; } public int _EirinFlag { get; set; }
        public int _ReimuAmmo { get; set; } public int _ReimuFlag { get; set; }
        public int _ReisenAmmo { get; set; } public int _ReisenFlag { get; set; }
        public int _RemiliaAmmo { get; set; } public int _RemiliaFlag { get; set; }
        public int _SakuyaAmmo { get; set; } public int _SakuyaFlag { get; set; }
        public int _YoumuAmmo { get; set; } public int _YoumuFlag { get; set; }
        public int _YuyukoAmmo { get; set; } public int _YuyukoFlag { get; set; }
        public int _MenuCursor { get; set; } public int _Tanks { get; set; } public int _Lives { get; set; }

        public SaveState(int X = 1, int Y = 1, float XF = 1, float YF = 1,
                         int CameraViewX = 0, int CameraViewY = 0, int Camera1X = 0, int Camera1Y = 0, int Camera2X = 0, int Camera2Y = 0,
                         int MarisaHP = 28, int AliceHP = 28, short Character = 0, int CharacterWeapon = 0, short CharacterSprite = 0,
                         int BroomAmmo = 112, int BroomFlag = 255, int CirnoAmmo = 112, int CirnoFlag = 255,
                         int DollAmmo = 112, int DollFlag = 255, int EirinAmmo = 112, int EirinFlag = 255,
                         int ReimuAmmo = 112, int ReimuFlag = 255, int ReisenAmmo = 112, int ReisenFlag = 255,
                         int RemiliaAmmo = 112, int RemiliaFlag = 255, int SakuyaAmmo = 112, int SakuyaFlag = 255,
                         int YoumuAmmo = 112, int YoumuFlag = 255, int YuyukoAmmo = 112, int YuyukoFlag = 255,
                         int MenuCursor = 0, int Tanks = 0, int Lives = 0)
        {
            _X = X; _Y = Y; _XF = XF; _YF = YF;
            _CameraViewX = CameraViewX; _CameraViewY = CameraViewY;
            _Camera1X = Camera1X; _Camera1Y = Camera1Y;
            _Camera2X = Camera2X; _Camera2Y = Camera2Y;
            _MarisaHP = MarisaHP; _AliceHP = AliceHP;
            _Character = Character; _CharacterWeapon = CharacterWeapon; _CharacterSprite = CharacterSprite;
            _BroomAmmo = BroomAmmo; _BroomFlag = BroomFlag;
            _CirnoAmmo = CirnoAmmo; _CirnoFlag = CirnoFlag;
            _DollAmmo = DollAmmo; _DollFlag = DollFlag;
            _EirinAmmo = EirinAmmo; _EirinFlag = EirinFlag;
            _ReimuAmmo = ReimuAmmo; _ReimuFlag = ReimuFlag;
            _ReisenAmmo = ReisenAmmo; _ReisenFlag = ReisenFlag;
            _RemiliaAmmo = RemiliaAmmo; _RemiliaFlag = RemiliaFlag;
            _SakuyaAmmo = SakuyaAmmo; _SakuyaFlag = SakuyaFlag;
            _YoumuAmmo = YoumuAmmo; _YoumuFlag = YoumuFlag;
            _YuyukoAmmo = YuyukoAmmo; _YuyukoFlag = YuyukoFlag;
            _MenuCursor = MenuCursor; _Tanks = Tanks; _Lives = Lives;
        }

        public SaveState(string save)
        {
            string[] split = save.Split(',');
            _XF = float.Parse(split[0].Trim()); _YF = float.Parse(split[1].Trim()); _X = int.Parse(split[2]); _Y = int.Parse(split[3]);
            _CameraViewX = int.Parse(split[4]); _CameraViewY = int.Parse(split[5]);
            _Camera1X = int.Parse(split[6]); _Camera1Y = int.Parse(split[7]);
            _Camera2X = int.Parse(split[8]); _Camera2Y = int.Parse(split[9]);
            _MarisaHP = int.Parse(split[10]); _AliceHP = int.Parse(split[11]);
            _Character = short.Parse(split[12]); _CharacterWeapon = int.Parse(split[13]); _CharacterSprite = short.Parse(split[14]);
            _BroomAmmo = int.Parse(split[15]); _BroomFlag = int.Parse(split[16]);
            _CirnoAmmo = int.Parse(split[17]); _CirnoFlag = int.Parse(split[18]);
            _DollAmmo = int.Parse(split[19]); _DollFlag = int.Parse(split[20]);
            _EirinAmmo = int.Parse(split[21]); _EirinFlag = int.Parse(split[22]);
            _ReimuAmmo = int.Parse(split[23]); _ReimuFlag = int.Parse(split[24]);
            _ReisenAmmo = int.Parse(split[25]); _ReisenFlag = int.Parse(split[26]);
            _RemiliaAmmo = int.Parse(split[27]); _RemiliaFlag = int.Parse(split[28]);
            _SakuyaAmmo = int.Parse(split[29]); _SakuyaFlag = int.Parse(split[30]);
            _YoumuAmmo = int.Parse(split[31]); _YoumuFlag = int.Parse(split[32]);
            _YuyukoAmmo = int.Parse(split[33]); _YuyukoFlag = int.Parse(split[34]);
            _MenuCursor = int.Parse(split[35]); _Tanks = int.Parse(split[36]); _Lives = int.Parse(split[37]);
        }

        public override string ToString()
        {
            return _XF.ToString("0.000") + "," + _YF.ToString("0.000") + "," + _X + "," + _Y + "," +
                   _CameraViewX + "," + _CameraViewY + "," +
                   _Camera1X + "," + _Camera1Y + "," + _Camera2X + "," + _Camera2Y + "," +
                   _MarisaHP + "," + _AliceHP + "," + _Character + "," + _CharacterWeapon + "," + _CharacterSprite + "," +
                   _BroomAmmo + "," + _BroomFlag + "," + _CirnoAmmo + "," + _CirnoFlag + "," +
                   _DollAmmo + "," + _DollFlag + "," + _EirinAmmo + "," + _EirinFlag + "," +
                   _ReimuAmmo + "," + _ReimuFlag + "," + _ReisenAmmo + "," + _ReisenFlag + "," +
                   _RemiliaAmmo + "," + _RemiliaFlag + "," + _SakuyaAmmo + "," + _SakuyaFlag + "," +
                   _YoumuAmmo + "," + _YoumuFlag + "," + _YuyukoAmmo + "," + _YuyukoFlag + "," +
                   _MenuCursor + "," + _Tanks + "," + _Lives;
        }
    }
}
