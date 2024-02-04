using System.Collections.Generic;

namespace MegaMariPrac
{
    internal class KeyboardKeys
    {
        public Dictionary<string, int> dictModifierKeys = new Dictionary<string, int>();
        public Dictionary<string, int> dictKeys = new Dictionary<string, int>();

        public KeyboardKeys()
        {
            dictModifierKeys.Add("None", 0);
            dictModifierKeys.Add("LShift", 0xE34B2);
            dictModifierKeys.Add("RShift", 0xE34BE);
            dictModifierKeys.Add("LControl", 0xE34A5);
            dictModifierKeys.Add("RControl", 0xE3525);
            dictModifierKeys.Add("LAlt", 0xE34C0);
            dictModifierKeys.Add("RAlt", 0xE3540);

            dictKeys.Add("1", 0xE348A);
            dictKeys.Add("2", 0xE348B);
            dictKeys.Add("3", 0xE348C);
            dictKeys.Add("4", 0xE348D);
            dictKeys.Add("5", 0xE348E);
            dictKeys.Add("6", 0xE348F);
            dictKeys.Add("7", 0xE3490);
            dictKeys.Add("8", 0xE3491);
            dictKeys.Add("9", 0xE3492);
            dictKeys.Add("0", 0xE3493);
            dictKeys.Add("A", 0xE34A6);
            dictKeys.Add("B", 0xE34B8);
            dictKeys.Add("C", 0xE34B6);
            dictKeys.Add("D", 0xE34A8);
            dictKeys.Add("E", 0xE349A);
            dictKeys.Add("F", 0xE34A9);
            dictKeys.Add("G", 0xE34AA);
            dictKeys.Add("H", 0xE34AB);
            dictKeys.Add("I", 0xE349F);
            dictKeys.Add("J", 0xE34AC);
            dictKeys.Add("K", 0xE34AD);
            dictKeys.Add("L", 0xE34AE);
            dictKeys.Add("M", 0xE34BA);
            dictKeys.Add("N", 0xE34B9);
            dictKeys.Add("O", 0xE34A0);
            dictKeys.Add("P", 0xE34A1);
            dictKeys.Add("Q", 0xE3498);
            dictKeys.Add("R", 0xE349B);
            dictKeys.Add("S", 0xE34A7);
            dictKeys.Add("T", 0xE349C);
            dictKeys.Add("U", 0xE349E);
            dictKeys.Add("V", 0xE34B7);
            dictKeys.Add("W", 0xE3499);
            dictKeys.Add("X", 0xE34B5);
            dictKeys.Add("Y", 0xE349D);
            dictKeys.Add("Z", 0xE34B4);
            dictKeys.Add("NumPad0", 0xE34DA);
            dictKeys.Add("NumPad1", 0xE34D7);
            dictKeys.Add("NumPad2", 0xE34D8);
            dictKeys.Add("NumPad3", 0xE34D9);
            dictKeys.Add("NumPad4", 0xE34D3);
            dictKeys.Add("NumPad5", 0xE34D4);
            dictKeys.Add("NumPad6", 0xE34D5);
            dictKeys.Add("NumPad7", 0xE34CF);
            dictKeys.Add("NumPad8", 0xE34D0);
            dictKeys.Add("NumPad9", 0xE34D1);
            dictKeys.Add("Multiply", 0xE34BF);
            dictKeys.Add("Add", 0xE34D6);
            dictKeys.Add("Subtract", 0xE34D2);
            dictKeys.Add("Decimal", 0xE34DB);
            dictKeys.Add("Divide", 0xE353D);
        }
    }
}
