using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser
{
    public class Lane
    {
        private Note[] notes;
        private int notebasepos;
        private int noteseekpos;

        private Note[] hiddens;
        private int hiddenbasepos;
        private int hiddenseekpos;

        public Lane(BMSModel model, int lane)
        {
            var note = new Queue<Note>();
            var hnote = new Queue<Note>();
            foreach (TimeLine tl in model.getAllTimeLines())
            {
                if (tl.existNote(lane))
                {
                    note.Enqueue(tl.getNote(lane));
                }
                if (tl.getHiddenNote(lane) != null)
                {
                    hnote.Enqueue(tl.getHiddenNote(lane));
                }
            }
            notes = note.ToArray();
            hiddens = hnote.ToArray();
        }

        public Note[] getNotes()
        {
            return notes;
        }

        public Note[] getHiddens()
        {
            return hiddens;
        }

        public Note getNote()
        {
            if (noteseekpos < notes.Length)
            {
                return notes[noteseekpos++];
            }
            return null;
        }

        public Note getHidden()
        {
            if (hiddenseekpos < hiddens.Length)
            {
                return hiddens[hiddenseekpos++];
            }
            return null;
        }

        public void reset()
        {
            noteseekpos = notebasepos;
            hiddenseekpos = hiddenbasepos;
        }

        public void mark(int time)
        {
            for (; notebasepos < notes.Length - 1 && notes[notebasepos + 1].getTime() < time; notebasepos++)
                ;
            for (; notebasepos > 0 && notes[notebasepos].getTime() > time; notebasepos--)
                ;
            noteseekpos = notebasepos;
            for (; hiddenbasepos < hiddens.Length - 1
                    && hiddens[hiddenbasepos + 1].getTime() < time; hiddenbasepos++)
                ;
            for (; hiddenbasepos > 0 && hiddens[hiddenbasepos].getTime() > time; hiddenbasepos--)
                ;
            hiddenseekpos = hiddenbasepos;
        }
    }

}
