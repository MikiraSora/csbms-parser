﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CSBMSParser
{
	/**
	 * タイムライン
	 * 
	 * @author exch
	 */
	public class TimeLine
	{

		/**
		 * タイムラインの時間(us)
		 */
		private long time;
		/**
		 * タイムラインの小節
		 */
		private double section;
		/**
		 * タイムライン上に配置されている演奏レーン分のノート。配置されていないレーンにはnullを入れる。
		 */
		private Note[] notes;

		/**
		 * タイムライン上に配置されている演奏レーン分の不可視ノート。配置されていないレーンにはnullを入れる。
		 */
		private Note[] hiddennotes;
		/**
		 * タイムライン上に配置されているBGMノート
		 */
		private List<Note> bgnotes = Note.EMPTYARRAY;
		/**
		 * 小節線の有無
		 */
		private bool sectionLine = false;
		/**
		 * タイムライン上からのBPM変化
		 */
		private double bpm;
		/**
		 * ストップ時間(us)
		 */
		private long stop;
		/**
		 * スクロールスピード
		 */
		private double scroll = 1.0;

		/**
		 * 表示するBGAのID
		 */
		private int bga = -1;
		/**
		 * 表示するレイヤーのID
		 */
		private int layer = -1;
		/**
		 * POORレイヤー
		 */
		private Layer[] eventlayer = Layer.EMPTY;

		public TimeLine(double section, long time, int notesize)
		{
			this.section = section;
			this.time = time;
			notes = new Note[notesize];
			hiddennotes = new Note[notesize];
		}

		public long getMilliTime()
		{
			return time / 1000;
		}

		public long getMicroTime()
		{
			return time;
		}

		public void setMicroTime(long time)
		{
			this.time = time;
			foreach (Note n in notes)
			{
				if (n != null)
				{
					n.setMicroTime(time);
				}
			}
			foreach (Note n in hiddennotes)
			{
				if (n != null)
				{
					n.setMicroTime(time);
				}
			}
			foreach (Note n in bgnotes)
			{
				n.setMicroTime(time);
			}
		}

		public int getLaneCount()
		{
			return notes.Length;
		}

		public void setLaneCount(int lanes)
		{
			if (notes.Length != lanes)
			{
				Note[] newnotes = new Note[lanes];
				Note[] newhiddennotes = new Note[lanes];
				for (int i = 0; i < lanes; i++)
				{
					if (i < notes.Length)
					{
						newnotes[i] = notes[i];
						newhiddennotes[i] = hiddennotes[i];
					}
				}
				notes = newnotes;
				hiddennotes = newhiddennotes;
			}
		}

		/**
		 * タイムライン上の総ノート数を返す
		 * 
		 * @return
		 */
		public int getTotalNotes()
		{
			return getTotalNotes(BMSModel.LNTYPE_LONGNOTE);
		}

		/**
		 * タイムライン上の総ノート数を返す
		 * 
		 * @return
		 */
		public int getTotalNotes(int lntype)
		{
			int count = 0;
			foreach (Note note in notes)
			{
				if (note != null)
				{
					if (note is LongNote ln)
					{
						if (ln.getType() == LongNote.TYPE_CHARGENOTE || ln.getType() == LongNote.TYPE_HELLCHARGENOTE
								|| (ln.getType() == LongNote.TYPE_UNDEFINED && lntype != BMSModel.LNTYPE_LONGNOTE)
								|| !ln.isEnd())
						{
							count++;
						}
					}
					else if (note is NormalNote)
					{
						count++;
					}
				}
			}
			return count;
		}

		public bool existNote()
		{
			foreach (Note n in notes)
			{
				if (n != null)
				{
					return true;
				}
			}
			return false;
		}

		public bool existNote(int lane)
		{
			return notes[lane] != null;
		}

		public Note getNote(int lane)
		{
			return notes[lane];
		}

		public void setNote(int lane, Note note)
		{
			notes[lane] = note;
			if (note == null)
			{
				return;
			}
			note.setSection(section);
			note.setMicroTime(time);
		}

		public void setHiddenNote(int lane, Note note)
		{
			hiddennotes[lane] = note;
			if (note == null)
			{
				return;
			}
			note.setSection(section);
			note.setMicroTime(time);
		}

		public bool existHiddenNote()
		{
			foreach (Note n in hiddennotes)
			{
				if (n != null)
				{
					return true;
				}
			}
			return false;
		}

		public Note[] getHiddenNotes() => hiddennotes;

		public Note getHiddenNote(int lane)
		{
			return hiddennotes[lane];
		}

		public void addBackGroundNote(Note note)
		{
			if (note == null)
			{
				return;
			}
			if (Note.EMPTYARRAY == bgnotes)
			{
				bgnotes = new List<Note>();
			}
			note.setSection(section);
			note.setMicroTime(time);
			/*
    bgnotes = Arrays.copyOf(bgnotes, bgnotes.length + 1);
    bgnotes[bgnotes.length - 1] = note;*/
			bgnotes.Add(note);
		}

		public void removeBackGroundNote(Note note)
		{
			for (int i = 0; i < bgnotes.Count; i++)
			{
				if (bgnotes[i] == note)
				{
					var newbg = new List<Note>(bgnotes.Count - 1);
					for (int j = 0, index = 0; j < bgnotes.Count; j++)
					{
						if (i != j)
						{
							newbg[index] = bgnotes[j];
							index++;
						}
					}
					bgnotes = newbg;
					break;
				}
			}
		}

		public List<Note> getBackGroundNotes()
		{
			return bgnotes;
		}

		public void setBPM(double bpm)
		{
			Debug.WriteLine(String.Format("section : {0:F6} bpm : {1:F6} -> {2:F6}", section, this.bpm, bpm));
			this.bpm = bpm;
		}

		public double getBPM()
		{
			return bpm;
		}

		public void setSectionLine(bool section)
		{
			this.sectionLine = section;
		}

		public bool getSectionLine()
		{
			return sectionLine;
		}

		/**
         * 表示するBGAのIDを取得する
         * 
         * @return BGAのID
         */
		public int getBGA()
		{
			return bga;
		}

		/**
         * 表示するBGAのIDを設定する
         * 
         * @param bga
         *            BGAのID
         */
		public void setBGA(int bga)
		{
			this.bga = bga;
		}

		/**
         * 表示するレイヤーBGAのIDを取得する
         * 
         * @return レイヤーBGAのID
         */
		public int getLayer()
		{
			return layer;
		}

		public void setLayer(int layer)
		{
			this.layer = layer;
		}

		public Layer[] getEventlayer()
		{
			return eventlayer;
		}

		public void setEventlayer(Layer[] eventlayer)
		{
			this.eventlayer = eventlayer;
		}

		public double getSection()
		{
			return section;
		}

		public Note[] getNotes() => notes;

		public void setSection(double section)
		{
			foreach (Note n in notes)
			{
				if (n != null)
				{
					n.setSection(section);
				}
			}
			foreach (Note n in hiddennotes)
			{
				if (n != null)
				{
					n.setSection(section);
				}
			}
			foreach (Note n in bgnotes)
			{
				n.setSection(section);
			}
			this.section = section;
		}

		public int getStop()
		{
			return (int)(stop / 1000);
		}

		public long getMilliStop()
		{
			return stop / 1000;
		}

		public long getMicroStop()
		{
			return stop;
		}

		public void setStop(long stop)
		{
			this.stop = stop;
		}

		public double getScroll()
		{
			return scroll;
		}

		public void setScroll(double scroll)
		{
			this.scroll = scroll;
		}
	}
}
