using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser
{
    /**
 * ロングノート
 * 
 * @author exch
 */
    public class LongNote : Note
    {
        /**
         * ロングノート終端かどうか
         */
        private bool end;
        /**
         * ペアになっているロングノート
         */
        private LongNote pair;
        /**
         * ロングノートの種類
         */
        private int type;

        /**
         * ロングノートの種類:未定義
         */
        public const int TYPE_UNDEFINED = 0;
        /**
         * ロングノートの種類:ロングノート
         */
        public const int TYPE_LONGNOTE = 1;
        /**
         * ロングノートの種類:チャージノート
         */
        public const int TYPE_CHARGENOTE = 2;
        /**
         * ロングノートの種類:ヘルチャージノート
         */
        public const int TYPE_HELLCHARGENOTE = 3;

        /**
         * 指定のTimeLineを始点としたロングノートを作成する
         * @param start
         */
        public LongNote(int wav)
        {
            this.setWav(wav);
        }

        public LongNote(int wav, long starttime, long duration)
        {
            this.setWav(wav);
            this.setStarttime(starttime);
            this.setDuration(duration);
        }

        public int getType()
        {
            return type;
        }

        public void setType(int type)
        {
            this.type = type;
        }

        public void setPair(LongNote pair)
        {
            pair.pair = this;
            this.pair = pair;

            pair.end = pair.getSection() > this.getSection();
            this.end = !pair.end;
            type = pair.type = (type != TYPE_UNDEFINED ? type : pair.type);
        }

        public LongNote getPair()
        {
            return pair;
        }

        public bool isEnd()
        {
            return end;
        }

        public override Object Clone()
        {
            return clone(true);
        }

        private Object clone(bool copypair)
        {
            LongNote ln = (LongNote)base.Clone();
            if (copypair)
                ln.setPair((LongNote)pair.clone(false));
            return ln;
        }
    }
}
