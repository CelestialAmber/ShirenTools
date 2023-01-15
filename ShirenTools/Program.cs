using System;

namespace ShirenTextDumper {

	public enum TextEntryType {
		String,
		ClearTextCommand,
		LineBreakCommand,
		PlaySoundCommand,
		CommandF5,
		CommandF6,
		CommandF8,
		NumberVarCommand,
		CommandFA,
		StringVarCommand,
		FunctionCommand
	}

	//Class representing a full string, including text commands and regular text
	public class Text {
		public List<TextEntry> entries = new List<TextEntry>();
		public int address;

		public string ConvertToString() {
			string result = ";" + address.ToString("x6") + "\n";
			result += "Text_" + address.ToString("X6") + ":\n";

			foreach (TextEntry entry in entries) {
				string line = "";

				switch (entry.entryType) {
					case TextEntryType.String:
						line = "text \"" + entry.text + "\"";
						break;
					case TextEntryType.ClearTextCommand:
						line = "cleartext";
						break;
					case TextEntryType.LineBreakCommand:
						line = "next";
						break;
					case TextEntryType.PlaySoundCommand:
						line = "playsound $" + entry.commandId.ToString("X1");
						break;
					case TextEntryType.CommandF5:
						line = "cmdf5 $" + entry.commandId.ToString("X1");
						break;
					case TextEntryType.CommandF6:
						line = "cmdf6";
						break;
					case TextEntryType.CommandF8:
						line = "cmdf8 $" + entry.commandId.ToString("X1");
						break;
					case TextEntryType.StringVarCommand:
						line = "strvar $" + entry.commandId.ToString("X1");
						break;
					case TextEntryType.NumberVarCommand:
						line = "numvar $" + entry.commandId.ToString("X1");
						break;
					case TextEntryType.FunctionCommand:
						line = "textfunction $" + entry.commandId.ToString("X1") + (entry.hasCommandParameter ? " $" + entry.commandParamter.ToString("X1") : "");
						break;
					case TextEntryType.CommandFA:
						line = "cmdfa $" + entry.commandId.ToString("X1");
						break;
				}

				result += line + "\n";
			}

			return result;
		}
	}

	//Individual parts of string (regular text/commands)
	public class TextEntry {
		public TextEntryType entryType;
		//Used for regular strings
		public string text;
		//Used for text commands (0xF0-0xFF)
		public byte command;
		//Used for text commands that represent variables or do specific actions (wait a certain amount of time, etc...)
		public byte commandId;
		//Also used for the play sound command
		public byte commandParamter;
		//whether the command has a param or not
		public bool hasCommandParameter;
	}

	public class Program {


		/*
		The character map is split into 4 separate tables. The first 4 tables have 256 characters each,
		while the 5th only has 16.

		Tables 1-3 are referenced to with a prefix byte specifying which other table to use.
		Table 0 can also be referenced this way, and has to be for the last row, since the values
		0xF0-0xFF are reserved for special text commands.

		Table 4 is referenced to using the F4 prefix byte, which can be used to access any table.

		Special characters:
		E4 (table 2): golden condor icon
		4C (table 3): marmel icon

		*/

		static char[][] characterMap = new char[][]{
		//Table 0
		new char[]{
		'　','０','１','２','３','４','５','６','７','８','９','＋','－','？','！','／',
		'（','）','［','］','あ','い','う','え','お','か','が','き','ぎ','く','ぐ','け',
		'げ','こ','ご','さ','ざ','し','じ','す','ず','せ','ぜ','そ','ぞ','た','だ','ち',
		'ぢ','つ','づ','て','で','と','ど','な','に','ぬ','ね','の','は','ば','ぱ','ひ',
		'び','ぴ','ふ','ぶ','ぷ','へ','べ','ぺ','ほ','ぼ','ぽ','ま','み','む','め','も',
		'や','ゆ','よ','ら','り','る','れ','ろ','わ','を','ん','っ','ゃ','ゅ','ょ','ぁ',
		'ぃ','ぅ','ぇ','ぉ','ー','ア','イ','ウ','ヴ','エ','オ','カ','ガ','キ','ギ','ク',
		'グ','ケ','ゲ','コ','ゴ','サ','ザ','シ','ジ','ス','ズ','セ','ゼ','ソ','ゾ','タ',
		'ダ','チ','ヂ','ツ','ヅ','テ','デ','ト','ド','ナ','ニ','ヌ','ネ','ノ','ハ','バ',
		'パ','ヒ','ビ','ピ','フ','ブ','プ','ヘ','ベ','ペ','ホ','ボ','ポ','マ','ミ','ム',
		'メ','モ','ヤ','ユ','ヨ','ラ','リ','ル','レ','ロ','ワ','ヲ','ン','ッ','ャ','ュ',
		'ョ','ァ','ィ','ゥ','ェ','ォ','―','＊','、','・','。','「','」','人','壺','物',
		'新','屋','中','『','』','見','大','巻','規','草','金','場','風','事','備','盾',
		'女','装','入','目','子','上','地','腕','輪','杖','男','来','店','食','番','力',
		'気','料','下','今','発','別','行','持','時','生','黄','強','旅','撃','出','願',
		'変','度','者','回','言','谷','理','作','弟','方','開','客','私','剣','頭','絵'
		},
		//Table 1
		new char[]{
		'最','識','思','先','一','飛','前','山','呪','使','攻','武','車','色','老','座',
		'間','魔','少','主','酒','神','何','待','Ｐ','分','器','成','話','達','聞','部',
		'太','脚','解','切','Ｈ','復','取','肉','宿','三','村','身','能','心','手','合',
		'親','町','乗','腹','通','矢','会','Ｘ','日','置','所','投','払','自','頂','消',
		'倉','庫','洞','窟','受','返','峠','帰','伝','値','爆','母','眠','毒','：','長',
		'陽','師','全','落','動','無','仕','名','道','娘','形','当','面','敵','押','不',
		'渓','戦','学','足','文','口','防','小','説','助','運','預','焼','混','乱','元',
		'吸','満','戻','御','仲','特','刀','付','天','怪','竹','残','念','越','死','化',
		'同','背','種','林','試','意','世','知','拾','炎','遠','呼','痛','白','代','忘',
		'犬','困','棒','流','二','盗','泊','竜','占','練','用','家','悪','亭','連','読',
		'早','起','果','険','選','岩','必','弱','踏','妖','明','幸','甲','酔','将','昔',
		'飲','唱','％','歩','効','限','冒','Ｌ','箱','掘','向','売','真','青','賊','国',
		'組','失','送','郷','誰','立','信','姿','火','割','石','雑','抜','定','命','鉄',
		'的','根','虫','荒','供','替','奇','幻','後','書','兄','住','荷','数','薬','活',
		'界','突','吹','相','半','法','紙','守','技','穴','雷','存','十','到','記','修',
		'保','木','予','婆','辻','指','圧','本','都','裏','宝','外','感','声','直','配'
		},
		//Table 2
		new char[]{
		'夢','味','経','験','与','倒','速','位','品','性','様','次','系','封','字','空',
		'迷','左','断','引','夫','父','耳','剛','赤','土','敗','問','題','多','若','買',
		'初','利','礼','注','役','渡','倍','体','段','遅','以','状','終','印','報','Ｂ',
		'正','恵','王','銅','重','右','族','四','異','遺','水','月','馬','市','滝','高',
		'隠','陶','芸','議','挑','求','年','笑','商','追','完','頼','得','普','裂','逆',
		'銀','恨','壁','晴','光','寝','霊','態','勝','型','固','怒','Ａ','決','Ｙ','透',
		'択','胃','底','錆','号','五','農','跡','息','姉','増','緑','紫','茶','紅','込',
		'並','湿','原','愛','工','近','語','顔','教','表','便','減','敷','安','笠','実',
		'考','途','酬','毎','糸','睡','腐','転','寄','階','続','弾','巨','崩','跳','隊',
		'掛','鎌','想','枝','糧','超','縛','皮','避','仙','鬼','士','蝕','狗','妹','斬',
		'羽','妙','虹','細','深','森','廃','脈','準','還','ｖ','～','現','貫','届','暴',
		'波','類','路','鳥','頃','友','互','嫁','急','楽','沈','覚','健','康','俺','収',
		'要','離','放','許','野','創','展','示','個','材','員','談','然','対','協','評',
		'判','々','誤','走','影','術','液','止','床','始','湯','罠','音','井','在','移',
		'Ｒ','写','設','Ｇ',' ','Ｆ','格','゛','゜','拡','縮','骨','仏','捨','過','視',
		'垂','殊','標','畠','浪','橋','獄','逃','包','丁','去','張','黒','杉','角','坑'
		},
		//Table 3
		new char[]{
		'蔵','崖','幽','哭','軸','具','Ｕ','Ｃ','ｅ','ｒ','才','希','滅','順','美','祈',
		'涙','泥','精','望','惑','郎','似','鹿','沢','触','改','素','駄','血','腰','夜',
		'肩','照','情','苦','建','額','恩','由','飯','確','認','管','則','研','究','負',
		'魚','釣','我','辺','境','育','撮','孫','祝','疲','盛','景','奉','催','吐','飢',
		'軽','支','讐','囲','噴','警','召','喚','盤','整','頓','響',' ','録','価','未',
		'戸','他','室','灯','骸','常','属','幼','兵','豆','僧','鍛','冶','軍','締','匠',
		'斧','殺','迅','秘','製','灰','褐','肌','朱','紺','秋','玉','焦','桃','桜','梅',
		'松','柳','栗','牙','桐','鉛','錫','丸','星','卵','浅','傾','旧','街','瀑','布',
		'源','集','衝','好','棲','志','継','塔','己','紹','介','袋','花','恋','制','看',
		'板','鑑','登','偉','基','例','領','距','測','量','優','調','双','胸','謝','納',
		'申','違','汗','結','晶','繰','丈','約','束','傑','闇','証','拠','呂','盲','低',
		'葉','坊','瞬','片','件','派','貸','再','依','共','積','句','甘','良','借','杯',
		'際','休','刃','婦','遊','欲','狂','菜','末','春','危','熱','加','雨','燃','鉱',
		'栄','暮','留','団','埋','接','熟','期','海','飾','破','恐','勇','線','門','反',
		'碑','造','晩','彼','難','関','築','如','壊','打','奥','寒','有','絶','慢','仁',
		'義','習','泣','横','質','八','獣','広','喜','歌','静','余','進','応','内','繁'
		},
		//Table 4
		new char[]{
		'昌','踊','探','輝','夕','餓','怨','窯','般','観','貨','献','煙','清','球','側',
		}};


		public static List<Text> stringsList = new List<Text>();
		public const int baseAddress = 0xFE1298;


		public static void Main(string[] args) {
			byte[] data = File.ReadAllBytes("textdata.bin");

			ReadStrings(data);

			//Create a streamwriter to write all the strings to a file one by one
			StreamWriter sw = File.CreateText("shirentext.txt");

			foreach (Text text in stringsList) {
				sw.WriteLine(text.ConvertToString());
			}

			sw.Flush();
		}

		public static void ReadStrings(byte[] textData) {
			List<string> stringList = new List<string>();
			Text text = new Text();
			text.address = baseAddress;
			int stringStartOffset = 0;

			TextEntry textEntry = new TextEntry();
			bool inString = false;

			for (int i = 0; i < textData.Count();) {
				byte b = textData[i++];

				//If the current byte is a character byte, and the last entry wasn't a string, mark the new one as a string
				if (b <= 0xF4 || b >= 0xFE) {
					if (!inString) {
						inString = true;
						textEntry.entryType = TextEntryType.String;
					}
				} else {
					//If the last byte was the end of a string, add the current entry to the list
					if (inString) {
						inString = false;
						text.entries.Add(textEntry);
						textEntry = new TextEntry();
					}
				}

				switch (b) {
					//Commands (0xF0-0xFF)

					//Display character from table 0-3 (f0-f3)
					//format: 0: character index
					case 0xF0:
					case 0xF1:
					case 0xF2:
					case 0xF3:

						byte charIndex = textData[i++];

						//Check for special characters

						//Golden condor icon
						if (b == 0xF2 && charIndex == 0xE4) {
							textEntry.text += "[CONDOR]";
						}
						//Marmel icon
						else if (b == 0xF3 && charIndex == 0x4C) {
							textEntry.text += "[MAMEL]";
						} else {
							//Regular character
							textEntry.text += characterMap[b - 0xF0][charIndex];
						}

						break;
					//Display character from any table
					//format: 0: table index, 1: character index
					case 0xF4:
						charIndex = textData[i++];
						byte tableIndex = textData[i++];
						textEntry.text += characterMap[tableIndex][charIndex];
						break;
					//Unknown
					case 0xF6:
						textEntry.entryType = TextEntryType.CommandF6;
						text.entries.Add(textEntry);
						textEntry = new TextEntry();
						break;
					/*
					Display variable/execute command
					Sequences starting with FC seem to be commands?
					Format: 0: variable id?

					Sequences:
					FC 7F nn: wait nn frames?
					*/
					case 0xF5:
					case 0xF9:
					case 0xFA:
					case 0xFB:
					case 0xFC:
						byte varId = textData[i++];

						textEntry.commandId = varId;

						//Does the group start with FC and have a parameter byte?
						if (b == 0xFC && (varId == 0x7F || varId == 0x00)) {
							//Read an additional byte for the command parameter
							byte param = textData[i++];
							textEntry.hasCommandParameter = true;
							textEntry.commandParamter = param;
						}

						if (b == 0xF5) {
							textEntry.entryType = TextEntryType.CommandF5;
						} else if (b == 0xF9) {
							textEntry.entryType = TextEntryType.NumberVarCommand;
						} else if (b == 0xFA) {
							textEntry.entryType = TextEntryType.CommandFA;
						} else if (b == 0xFB) {
							textEntry.entryType = TextEntryType.StringVarCommand;
						} else if (b == 0xFC) {
							textEntry.entryType = TextEntryType.FunctionCommand;
						}

						text.entries.Add(textEntry);
						textEntry = new TextEntry();
						break;
					//Clear text after input
					case 0xF7:
						textEntry.entryType = TextEntryType.ClearTextCommand;
						text.entries.Add(textEntry);
						textEntry = new TextEntry();
						break;
					//Go to next line after input
					case 0xF8:
						textEntry.entryType = TextEntryType.LineBreakCommand;
						text.entries.Add(textEntry);
						textEntry = new TextEntry();
						break;
					//Play sfx (also music?)
					case 0xFD:
						byte sfxId = textData[i++];
						textEntry.entryType = TextEntryType.PlaySoundCommand;
						textEntry.commandId = sfxId;
						text.entries.Add(textEntry);
						textEntry = new TextEntry();
						break;
					//Line break
					case 0xFE:
						textEntry.text += "\\l";
						break;
					//End of string
					case 0xFF:
						textEntry.text += "@";
						text.entries.Add(textEntry);
						text.address = baseAddress + stringStartOffset;
						//Add the current text to the list
						stringsList.Add(text);
						textEntry = new TextEntry();
						text = new Text();
						stringStartOffset = i;
						inString = false;
						break;
					//Regular character
					default:
						textEntry.text += characterMap[0][b];
						break;
				}
			}
		}

	}
}

