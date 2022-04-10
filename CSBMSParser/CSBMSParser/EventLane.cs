using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser
{
    public class EventLane
    {
        private TimeLine[] sections;
        private int sectionbasepos;
        private int sectionseekpos;

        private TimeLine[] bpms;
        private int bpmbasepos;
        private int bpmseekpos;

        private TimeLine[] stops;
        private int stopbasepos;
        private int stopseekpos;

        public EventLane(BMSModel model)
        {
            var section = new Queue<TimeLine>();
            var bpm = new Queue<TimeLine>();
            var stop = new Queue<TimeLine>();

            TimeLine prev = null;
            foreach (TimeLine tl in model.getAllTimeLines())
            {
                if (tl.getSectionLine())
                {
                    section.Enqueue(tl);
                }
                if (tl.getBPM() != (prev != null ? prev.getBPM() : model.getBpm()))
                {
                    bpm.Enqueue(tl);
                }
                if (tl.getStop() != 0)
                {
                    stop.Enqueue(tl);
                }
                prev = tl;
            }
            sections = section.ToArray();
            bpms = bpm.ToArray();
            stops = stop.ToArray();
        }

        public TimeLine[] getSections()
        {
            return sections;
        }

        public TimeLine[] getBpmChanges()
        {
            return bpms;
        }

        public TimeLine[] getStops()
        {
            return stops;
        }

        public TimeLine getSection()
        {
            if (sectionseekpos < sections.Length)
            {
                return sections[sectionseekpos++];
            }
            return null;
        }

        public TimeLine getBpm()
        {
            if (bpmseekpos < bpms.Length)
            {
                return bpms[bpmseekpos++];
            }
            return null;
        }

        public TimeLine getStop()
        {
            if (stopseekpos < stops.Length)
            {
                return stops[stopseekpos++];
            }
            return null;
        }

        public void reset()
        {
            sectionseekpos = sectionbasepos;
            bpmseekpos = bpmbasepos;
            stopseekpos = stopbasepos;
        }

        public void mark(int time)
        {
            for (; sectionbasepos < sections.Length - 1 && sections[sectionbasepos + 1].getMilliTime() > time; sectionbasepos++)
                ;
            for (; sectionbasepos > 0 && sections[sectionbasepos].getMilliTime() < time; sectionbasepos--)
                ;
            for (; bpmbasepos < bpms.Length - 1 && bpms[bpmbasepos + 1].getMilliTime() > time; bpmbasepos++)
                ;
            for (; bpmbasepos > 0 && bpms[bpmbasepos].getMilliTime() < time; bpmbasepos--)
                ;
            for (; stopbasepos < stops.Length - 1 && stops[stopbasepos + 1].getMilliTime() > time; stopbasepos++)
                ;
            for (; stopbasepos > 0 && stops[stopbasepos].getMilliTime() < time; stopbasepos--)
                ;
            sectionseekpos = sectionbasepos;
            bpmseekpos = bpmbasepos;
            stopseekpos = stopbasepos;
        }


    }

}
