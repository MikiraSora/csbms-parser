package bms.model;

import java.io.*;
import java.nio.file.Files;
import java.nio.file.Path;
import java.security.DigestInputStream;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.*;
import java.util.logging.Logger;

/**
 * BMSファイルをBMSModelにデコードするクラス
 * 
 * @author exch
 */
public class BMSDecoder {

	private int lntype;

	private List<DecodeLog> log = new ArrayList<DecodeLog>();

	final List<String> wavlist = new ArrayList<String>(36 * 36);
	private final int[] wm = new int[36 * 36];

	final List<String> bgalist = new ArrayList<String>(36 * 36);
	private final int[] bm = new int[36 * 36];

	private BMSGenerator generator;

	public BMSDecoder() {
		this(BMSModel.LNTYPE_LONGNOTE);
	}

	public BMSDecoder(int lntype) {
		this.lntype = lntype;
		// 予約語の登録
	}

	public BMSModel decode(File f) {
		return decode(f.toPath());
	}

	public BMSModel decode(Path f) {
		Logger.getGlobal().fine("BMSファイル解析開始 :" + f.toString());
		try {
			BMSModel model = this.decode(Files.readAllBytes(f), f.toString().toLowerCase().endsWith(".pms"), null);
			if (model == null) {
				return null;
			}
			model.setPath(f.toAbsolutePath().toString());
			Logger.getGlobal().fine("BMSファイル解析完了 :" + f.toString() + " - TimeLine数:" + model.getAllTimes().length);
			return model;
		} catch (IOException e) {
			log.add(new DecodeLog(DecodeLog.STATE_ERROR, "BMSファイルが見つかりません"));
			Logger.getGlobal().severe("BMSファイル解析中の例外 : " + e.getClass().getName() + " - " + e.getMessage());
		}
		return null;
	}

	private final List<String>[] lines = new List[1000];

	private final Map<Integer, Double> stoptable = new TreeMap<Integer, Double>();
	private final Map<Integer, Double> bpmtable = new TreeMap<Integer, Double>();
	private final Deque<Integer> randoms = new ArrayDeque<Integer>();
	private final Deque<Integer> srandoms = new ArrayDeque<Integer>();
	private final Deque<Integer> crandom = new ArrayDeque<Integer>();
	private final Deque<Boolean> skip = new ArrayDeque<Boolean>();

	/**
	 * 指定したBMSファイルをモデルにデコードする
	 *
	 * @param data
	 * @return
	 */
	public BMSModel decode(byte[] data, boolean ispms, int[] random) {
		log.clear();
		final long time = System.currentTimeMillis();
		BMSModel model = new BMSModel();
		stoptable.clear();
		bpmtable.clear();

		MessageDigest md5digest, sha256digest;
		try {
			md5digest = MessageDigest.getInstance("MD5");
			sha256digest = MessageDigest.getInstance("SHA-256");
		} catch (NoSuchAlgorithmException e1) {
			e1.printStackTrace();
			return null;
		}

		// BMS読み込み、ハッシュ値取得
		try (BufferedReader br = new BufferedReader(new InputStreamReader(
				new DigestInputStream(new DigestInputStream(new ByteArrayInputStream(data), md5digest), sha256digest),
				"MS932"));) {
			if (ispms) {
				model.setMode(Mode.POPN_9K);
			}
			// Logger.getGlobal().info(
			// "BMSデータ読み込み時間(ms) :" + (System.currentTimeMillis() - time));

			String line = null;
			wavlist.clear();
			Arrays.fill(wm, -2);
			bgalist.clear();
			Arrays.fill(bm, -2);
			for(List l : lines) {
				if(l != null) {
					l.clear();
				}
			}

			randoms.clear();
			srandoms.clear();
			crandom.clear();
			int maxsec = 0;

			skip.clear();
			while ((line = br.readLine()) != null) {
				if (line.length() < 2 || line.charAt(0) != '#') {
					continue;
				}
				
//				line = line.substring(1, line.length());
				// RANDOM制御系
				if (matchesReserveWord(line, "RANDOM")) {
					try {
						final int r = Integer.parseInt(line.substring(8).trim());
						randoms.add(r);
						if (random != null) {
							crandom.add(random[randoms.size() - 1]);
						} else {
							crandom.add((int) (Math.random() * r) + 1);
							srandoms.add(crandom.getLast());
						}
					} catch (NumberFormatException e) {
						log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#RANDOMに数字が定義されていません"));
						Logger.getGlobal().warning(model.getTitle() + ":BMSファイルの解析中の例外:#RANDOMに数字が定義されていません" + line);
					}
				} else if (matchesReserveWord(line, "IF")) {
					// RANDOM分岐開始
					try {
						skip.add((crandom.getLast() != Integer.parseInt(line.substring(4).trim())));
					} catch (NumberFormatException e) {
						log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#IFに数字が定義されていません"));
						Logger.getGlobal().warning(model.getTitle() + ":BMSファイルの解析中の例外:#IFに数字が定義されていません" + line);
					}
				} else if (matchesReserveWord(line, "ENDIF")) {
					if (!skip.isEmpty()) {
						skip.removeLast();
					} else {
						log.add(new DecodeLog(DecodeLog.STATE_WARNING, "ENDIFに対応するIFが存在しません: " + line));
						Logger.getGlobal().warning(model.getTitle() + ":ENDIFに対応するIFが存在しません:" + line);
					}
				} else if (matchesReserveWord(line, "ENDRANDOM")) {
					if (!crandom.isEmpty()) {
						crandom.removeLast();
					} else {
						log.add(new DecodeLog(DecodeLog.STATE_WARNING, "ENDRANDOMに対応するRANDOMが存在しません: " + line));
						Logger.getGlobal().warning(model.getTitle() + ":ENDRANDOMに対応するRANDOMが存在しません:" + line);
					}
				} else if (skip.isEmpty() || !skip.getLast()) {
					final char c = line.charAt(1);
					if ('0' <= c && c <= '9' && line.length() > 6) {
						// line = line.toUpperCase();
						// 楽譜
						final char c2 = line.charAt(2);
						final char c3 = line.charAt(3);
						if ('0' <= c2 && c2 <= '9' && '0' <= c3 && c3 <= '9') {
							final int bar_index = (c - '0') * 100 + (c2 - '0') * 10 + (c3 - '0');
							List<String> l = lines[bar_index];
							if (l == null) {
								l = lines[bar_index] = new ArrayList<String>();
							}
							l.add(line);
							maxsec = (maxsec > bar_index) ? maxsec : bar_index;
						} else {
							log.add(new DecodeLog(DecodeLog.STATE_WARNING, "小節に数字が定義されていません : " + line));
							Logger.getGlobal().warning(model.getTitle() + ":BMSファイルの解析中の例外:" + line);
						}
					} else if (matchesReserveWord(line, "BPM")) {
						if (line.charAt(4) == ' ') {
							// BPMは小数点のケースがある(FREEDOM DiVE)
							try {
								final String arg = line.substring(5).trim();
								model.setBpm(Double.parseDouble(arg));
							} catch (NumberFormatException e) {
								log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#BPMに数字が定義されていません : " + line));
								Logger.getGlobal().warning(model.getTitle() + ":BMSファイルの解析中の例外::" + line);
							}
						} else {
							try {
								String bpm = line.substring(7).trim();
								bpmtable.put(parseInt36(line, 4), Double.parseDouble(bpm));
							} catch (NumberFormatException e) {
								log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#BPMxxに数字が定義されていません : " + line));
								Logger.getGlobal().warning(model.getTitle() + ":#BPMxxに数字が定義されていません : " + line);
							}
						}
					} else if (matchesReserveWord(line, "WAV")) {
						// 音源ファイル
						if (line.length() >= 8) {
							try {
								final String file_name = line.substring(7).trim().replace('\\', '/');
								wm[parseInt36(line, 4)] = wavlist.size();
								wavlist.add(file_name);
							} catch (NumberFormatException e) {
								log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#WAVxxは不十分な定義です : " + line));
								Logger.getGlobal()
										.warning(model.getTitle() + ":BMSファイルの解析中の例外:#WAVxxは不正な定義です : " + line);
							}
						} else {
							log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#WAVxxは不十分な定義です : " + line));
							Logger.getGlobal().warning(model.getTitle() + ":BMSファイルの解析中の例外:#WAVxxは不十分な定義です : " + line);
						}
					} else if (matchesReserveWord(line, "BMP")) {
						// BGAファイル
						if (line.length() >= 8) {
							try {
								final String file_name = line.substring(7).trim().replace('\\', '/');
								bm[parseInt36(line, 4)] = bgalist.size();
								bgalist.add(file_name);
							} catch (NumberFormatException e) {
								log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#WAVxxは不十分な定義です : " + line));
								Logger.getGlobal()
										.warning(model.getTitle() + ":BMSファイルの解析中の例外:#WAVxxは不正な定義です : " + line);
							}
						} else {
							log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#BMPxxは不十分な定義です : " + line));
							Logger.getGlobal().warning(model.getTitle() + ":BMSファイルの解析中の例外:#BMPxxは不十分な定義です : " + line);
						}
					} else if (matchesReserveWord(line, "STOP")) {
						if (line.length() >= 9) {
							try {
								String stop = line.substring(8).trim();
								stoptable.put(parseInt36(line, 5), Double.parseDouble(stop) / 192);
							} catch (NumberFormatException e) {
								log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#STOPxxに数字が定義されていません : " + line));
								Logger.getGlobal().warning(model.getTitle() + ":#STOPxxに数字が定義されていません : " + line);
							}
						} else {
							log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#STOPxxは不十分な定義です : " + line));
							Logger.getGlobal().warning(model.getTitle() + ":BMSファイルの解析中の例外:#STOPxxは不十分な定義です : " + line);
						}
					} else {
						for (CommandWord cw : CommandWord.values()) {
							if (line.length() > cw.name().length() + 2 && matchesReserveWord(line, cw.name())) {
								DecodeLog log = cw.execute(model, line.substring(cw.name().length() + 2).trim());
								if(log != null) {
									this.log.add(log);
									Logger.getGlobal().warning(model.getTitle() + " - " + log.getMessage() + " : " + line);
								}
								break;
							}
						}
					}
				}
			}
			model.setWavList(wavlist.toArray(new String[wavlist.size()]));
			model.setBgaList(bgalist.toArray(new String[bgalist.size()]));

			Section prev = null;
			Section[] sections = new Section[maxsec + 1];
			for (int i = 0; i <= maxsec; i++) {
				sections[i] = new Section(model, prev,
						lines[i] != null ? lines[i] : Collections.EMPTY_LIST, 
						bpmtable, stoptable, log);
				prev = sections[i];
			}
			
			final TreeMap<Double, TimeLine> timelines = new TreeMap<Double, TimeLine>();
			final TreeMap<Double, Double> timecache = new TreeMap<Double, Double>();
			
			final TimeLine basetl = new TimeLine(0, 0, model.getMode().key);
			basetl.setBPM(model.getBpm());
			timelines.put(0.0, basetl);
			timecache.put(0.0, 0.0);
			for(Section section : sections) {
				section.makeTimeLines(wm, bm, timelines, timecache);
			}
			// Logger.getGlobal().info(
			// "Section生成時間(ms) :" + (System.currentTimeMillis() - time));
			final int[] lastlnstatus = prev.getEndLNStatus(prev);
			final TimeLine[] tl = timelines.values().toArray(new TimeLine[timelines.size()]);
			model.setAllTimeLine(tl);

			for (int i = 0; i < 18; i++) {
				if (lastlnstatus[i] != 0) {
					log.add(new DecodeLog(DecodeLog.STATE_WARNING, "曲の終端までにLN終端定義されていないLNがあります。lane:" + (i + 1)));
					Logger.getGlobal().warning(model.getTitle() + ":曲の終端までにLN終端定義されていないLNがあります。lane:" + (i + 1));
					for (int index = tl.length - 1; index >= 0; index--) {
						final Note n = tl[index].getNote(i);
						if (n != null && n instanceof LongNote && ((LongNote) n).getPair() == null) {
							tl[index].setNote(i, null);
							break;
						}
					}
				}
			}

			model.setLntype(lntype);
			if (model.getTotal() <= 60.0) {
				log.add(new DecodeLog(DecodeLog.STATE_WARNING, "TOTALが未定義か、値が少なすぎます"));
			}
			if (tl.length > 0) {
				if (tl[tl.length - 1].getTime() >= model.getLastTime() + 30000) {
					log.add(new DecodeLog(DecodeLog.STATE_WARNING, "最後のノート定義から30秒以上の余白があります"));
				}
			}
			if (model.getPlayer() > 1 && (model.getMode() == Mode.BEAT_5K || model.getMode() == Mode.BEAT_7K)) {
				log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#PLAYER定義が2以上にもかかわらず2P側のノーツ定義が一切ありません"));
				Logger.getGlobal().warning(model.getTitle() + ":#PLAYER定義が2以上にもかかわらず2P側のノーツ定義が一切ありません");
			}
			if (model.getPlayer() == 1 && (model.getMode() == Mode.BEAT_10K || model.getMode() == Mode.BEAT_14K)) {
				log.add(new DecodeLog(DecodeLog.STATE_WARNING, "#PLAYER定義が1にもかかわらず2P側のノーツ定義が存在します"));
				Logger.getGlobal().warning(model.getTitle() + ":#PLAYER定義が1にもかかわらず2P側のノーツ定義が存在します");
			}
			model.setMD5(convertHexString(md5digest.digest()));
			model.setSHA256(convertHexString(sha256digest.digest()));
			Logger.getGlobal().fine("BMSデータ解析時間(ms) :" + (System.currentTimeMillis() - time));

			if (random == null) {
				random = new int[randoms.size()];
				final Iterator<Integer> ri = randoms.iterator();
				for (int i = 0; i < random.length; i++) {
					random[i] = ri.next();
				}
				generator = new BMSGenerator(data, ispms, random);
			}
			random = new int[srandoms.size()];
			final Iterator<Integer> ri = srandoms.iterator();
			for (int i = 0; i < random.length; i++) {
				random[i] = ri.next();
			}
			model.setRandom(random);
			return model;
		} catch (IOException e) {
			log.add(new DecodeLog(DecodeLog.STATE_ERROR, "BMSファイルへのアクセスに失敗しました"));
			Logger.getGlobal()
					.severe(model.getTitle() + ":BMSファイル解析失敗: " + e.getClass().getName() + " - " + e.getMessage());
		} catch (Exception e) {
			log.add(new DecodeLog(DecodeLog.STATE_ERROR, "何らかの異常によりBMS解析に失敗しました"));
			Logger.getGlobal()
					.severe(model.getTitle() + ":BMSファイル解析失敗: " + e.getClass().getName() + " - " + e.getMessage());
			e.printStackTrace();
		}
		return null;
	}

	private boolean matchesReserveWord(String line, String s) {
		final int len = s.length();
		if(line.length() <= len) {
			return false;
		}
		for(int i = 0;i < len;i++) {
			final char c = line.charAt(i + 1);
			final char c2 = s.charAt(i);
			if(c != c2 && c != c2 + 32) {
				return false;
			}
		}
		return true;
	}

	public static int parseInt36(String s, int index) throws NumberFormatException {
		int result = 0;
		final char c1 = s.charAt(index);
		if (c1 >= '0' && c1 <= '9') {
			result = (c1 - '0') * 36;
		} else if (c1 >= 'a' && c1 <= 'z') {
			result = ((c1 - 'a') + 10) * 36;
		} else if (c1 >= 'A' && c1 <= 'Z') {
			result = ((c1 - 'A') + 10) * 36;
		} else {
			throw new NumberFormatException();
		}

		final char c2 = s.charAt(index + 1);
		if (c2 >= '0' && c2 <= '9') {
			result += (c2 - '0');
		} else if (c2 >= 'a' && c2 <= 'z') {
			result += (c2 - 'a') + 10;
		} else if (c2 >= 'A' && c2 <= 'Z') {
			result += (c2 - 'A') + 10;
		} else {
			throw new NumberFormatException();
		}

		return result;
	}

	public BMSGenerator getBMSGenerator() {
		return generator;
	}

	/**
	 * バイトデータを16進数文字列表現に変換する
	 * 
	 * @param data
	 *            バイトデータ
	 * @returnバイトデータの16進数文字列表現
	 */
	public static String convertHexString(byte[] data) {
		final StringBuilder sb = new StringBuilder(data.length * 2);
		for (byte b : data) {
			sb.append(Character.forDigit(b >> 4 & 0xf, 16));
			sb.append(Character.forDigit(b & 0xf, 16));
		}
		return sb.toString();
	}

	public DecodeLog[] getDecodeLog() {
		return log.toArray(new DecodeLog[log.size()]);
	}
}

/**
 * 予約語
 *
 * @author exch
 */
enum CommandWord {

	PLAYER {
		@Override
		public DecodeLog execute(BMSModel model, String arg) {
			try {
				model.setPlayer(Integer.parseInt(arg));
			} catch (NumberFormatException e) {
				return new DecodeLog(DecodeLog.STATE_WARNING, "#PLAYERに数字が定義されていません");
			}
			return null;
		}
	},
	GENRE {
		public DecodeLog execute(BMSModel model, String arg) {
			model.setGenre(arg);
			return null;
		}
	},
	TITLE {
		public DecodeLog execute(BMSModel model, String arg) {
			model.setTitle(arg);
			return null;
		}
	},
	SUBTITLE {
		public DecodeLog execute(BMSModel model, String arg) {
			model.setSubTitle(arg);
			return null;
		}
	},
	ARTIST {
		public DecodeLog execute(BMSModel model, String arg) {
			model.setArtist(arg);
			return null;
		}
	},
	SUBARTIST {
		public DecodeLog execute(BMSModel model, String arg) {
			model.setSubArtist(arg);
			return null;
		}
	},
	PLAYLEVEL {
		public DecodeLog execute(BMSModel model, String arg) {
			model.setPlaylevel(arg);
			return null;
		}
	},
	RANK {
		public DecodeLog execute(BMSModel model, String arg) {
			if(model.getJudgerank() >= 10) {
				return null;
			}
			try {
				final int rank = Integer.parseInt(arg);
				if (rank >= 0 && rank < 5) {
					model.setJudgerank(rank);
				} else {
					return new DecodeLog(DecodeLog.STATE_WARNING, "#RANKに規定外の数字が定義されています : " + rank);
				}
			} catch (NumberFormatException e) {
				return new DecodeLog(DecodeLog.STATE_WARNING, "#RANKに数字が定義されていません");
			}
			return null;
		}
	},
	DEFEXRANK {
		public DecodeLog execute(BMSModel model, String arg) {
			try {
				final int rank = Integer.parseInt(arg);
				if (rank >= 10) {
					model.setJudgerank(rank);
				} else {
					return new DecodeLog(DecodeLog.STATE_WARNING, "#DEFEXRANK 10以下はサポートしていません" + rank);
				}
			} catch (NumberFormatException e) {
				return new DecodeLog(DecodeLog.STATE_WARNING, "#DEFEXRANKに数字が定義されていません");
			}
			return null;
		}
	},
	TOTAL {
		public DecodeLog execute(BMSModel model, String arg) {
			try {
				model.setTotal(Double.parseDouble(arg));
			} catch (NumberFormatException e) {
				return new DecodeLog(DecodeLog.STATE_WARNING, "#TOTALに数字が定義されていません");
			}
			return null;
		}
	},
	VOLWAV {
		public DecodeLog execute(BMSModel model, String arg) {
			try {
				model.setVolwav(Integer.parseInt(arg));
			} catch (NumberFormatException e) {
				return new DecodeLog(DecodeLog.STATE_WARNING, "#VOLWAVに数字が定義されていません");
			}
			return null;
		}
	},
	STAGEFILE {
		public DecodeLog execute(BMSModel model, String arg) {
			model.setStagefile(arg.replace('\\', '/'));
			return null;
		}
	},
	BACKBMP {
		public DecodeLog execute(BMSModel model, String arg) {
			model.setBackbmp(arg.replace('\\', '/'));
			return null;
		}
	},
	PREVIEW {
		public DecodeLog execute(BMSModel model, String arg) {
			model.setPreview(arg.replace('\\', '/'));
			return null;
		}
	},
	LNOBJ {
		public DecodeLog execute(BMSModel model, String arg) {
			try {
				model.setLnobj(Integer.parseInt(arg.toUpperCase(), 36));
			} catch (NumberFormatException e) {
				return new DecodeLog(DecodeLog.STATE_WARNING, "#PLAYERに数字が定義されていません");
			}
			return null;
		}
	},
	LNMODE {
		public DecodeLog execute(BMSModel model, String arg) {
			try {
				int lnmode = Integer.parseInt(arg);
				if(lnmode < 0 || lnmode > 3) {
					return new DecodeLog(DecodeLog.STATE_WARNING, "#LNMODEに無効な数字が定義されています");
				}
				model.setLnmode(lnmode);
			} catch (NumberFormatException e) {
				return new DecodeLog(DecodeLog.STATE_WARNING, "#PLAYERに数字が定義されていません");
			}
			return null;
		}
	},
	DIFFICULTY {
		public DecodeLog execute(BMSModel model, String arg) {
			try {
				model.setDifficulty(Integer.parseInt(arg));
			} catch (NumberFormatException e) {
				return new DecodeLog(DecodeLog.STATE_WARNING, "#DIFFICULTYに数字が定義されていません");
			}
			return null;
		}
	},
	BANNER {
		public DecodeLog execute(BMSModel model, String arg) {
			model.setBanner(arg.replace('\\', '/'));
			return null;
		}
	},
	COMMENT {
		public DecodeLog execute(BMSModel model, String arg) {
			// TODO 未実装
			return null;
		}
	};

	public abstract DecodeLog execute(BMSModel model, String arg);

}
