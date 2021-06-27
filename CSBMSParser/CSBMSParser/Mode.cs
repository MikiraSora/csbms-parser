using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser
{
    public class Mode
    {
        public int id;
        /**
		 * モードの名称。bmsonのmode_hintに対応
		 */
        public string hint;
        /**
         * プレイヤー数
         */
        public int player;
        /**
		 * 使用するキーの数
		 */
        public int key;
        /**
		 * スクラッチキーアサイン
		 */
        public int[] scratchKey;

        Mode(int id, string hint, int player, int key, int[] scratchKey)
        {
            this.id = id;
            this.hint = hint;
            this.player = player;
            this.key = key;
            this.scratchKey = scratchKey;
        }


        /**
         * 指定するkeyがスクラッチキーかどうかを返す
         * 
         * @param key キー番号
         * @return スクラッチであればtrue
         */
        public bool isScratchKey(int key)
        {
            foreach (int sc in scratchKey)
            {
                if (key == sc)
                {
                    return true;
                }
            }
            return false;
        }


        /**
         * mode_hintに対応するModeを取得する
         * 
         * @param hint
         *            mode_hint
         * @return 対応するMode
         */
        public static Mode getMode(string hint)
        {
            foreach (var pair in values)
            {
                if (pair.Value.hint.Equals(hint))
                {
                    return pair.Value;
                }
            }
            return null;
        }

        public static readonly Dictionary<string, Mode> values = new Dictionary<string, Mode>()
        {
            ["BEAT_5K"] = new Mode(5, "beat-5k", 1, 6, new int[] { 5 }),
            ["BEAT_7K"] = new Mode(7, "beat-7k", 1, 8, new int[] { 7 }),
            ["BEAT_10K"] = new Mode(10, "beat-10k", 2, 12, new int[] { 5, 11 }),
            ["BEAT_14K"] = new Mode(14, "beat-14k", 2, 16, new int[] { 7, 15 }),
            ["POPN_5K"] = new Mode(9, "popn-5k", 1, 5, new int[] { }),
            ["POPN_9K"] = new Mode(9, "popn-9k", 1, 9, new int[] { }),
            ["KEYBOARD_24K"] = new Mode(25, "keyboard-24k", 1, 26, new int[] { 24, 25 }),
            ["KEYBOARD_24K_DOUBLE"] = new Mode(50, "keyboard-24k-double", 2, 52, new int[] { 24, 25, 50, 51 }),
        };

        public static Mode BEAT_5K => values["BEAT_5K"];
        public static Mode BEAT_7K => values["BEAT_7K"];
        public static Mode BEAT_10K => values["BEAT_10K"];
        public static Mode BEAT_14K => values["BEAT_14K"];
        public static Mode POPN_5K => values["POPN_5K"];
        public static Mode POPN_9K => values["POPN_9K"];
        public static Mode KEYBOARD_24K => values["KEYBOARD_24K"];
        public static Mode KEYBOARD_24K_DOUBLE => values["KEYBOARD_24K_DOUBLE"];
    }
}
