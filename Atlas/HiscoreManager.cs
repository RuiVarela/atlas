using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Atlas
{
    class HiscoreManager
    {
        public const int MaxHiScores = 10; //up to 99
        private const String scoreFile = "Config/hiscore";
        private const int NAME_MAX_CHARS = 12;

        class HiscoreEntry
        {
            private String _name;
            private int _hiscore;

            public HiscoreEntry(String name, int hiscore)
            {
                _name = name;
                _hiscore = hiscore;
            }

            public String Name { get { return _name; } }
            public int HighScore { get { return _hiscore; } }
            override public String ToString()
            {
                String result = "";
                for (int i = 0; i < (NAME_MAX_CHARS - _name.Length) / 2; i++) result += " ";
                result += _name;
                while (result.Length < NAME_MAX_CHARS) result += " ";
                result += "    ";

                //build center-aligned score string
                String scoreTemp = "";
                String scoreText = _hiscore.ToString();
                for (int i = 0; i < (7 - scoreText.Length) / 2; i++) scoreTemp += " ";
                scoreTemp += scoreText;
                while (scoreTemp.Length < 7) scoreTemp += " ";

                result += scoreTemp + " ";

                return result;
            }

            public String Serialized()
            {
                return _name + "=" + _hiscore;
            }

            static public HiscoreEntry Deserialize(String text)
            {
                String[] keyValue = text.Split("=".ToCharArray());
                if (keyValue.Length < 2) return null;
                int iScore;
                /*if (int.TryParse(keyValue[1], out iScore)) return new HiscoreEntry(keyValue[0], iScore);
                return null;*/
                iScore = int.Parse(keyValue[1]);
                return new HiscoreEntry(keyValue[0], iScore);
            }
        }

        static readonly HiscoreManager _instance = new HiscoreManager();
        public static HiscoreManager Instance { get { return _instance; } }

        List<HiscoreEntry> _entries;

        public int GetLowestScore()
        {
            if (_entries.Count < MaxHiScores) return 0;
            else return _entries[MaxHiScores-1].HighScore;
        }

        public void AddHighScore(String name, int score)
        {
            _entries.Add(new HiscoreEntry(name, score));
            _entries.Sort(CompareHiScores);
            while (_entries.Count > MaxHiScores) _entries.RemoveAt(MaxHiScores);
            Serialize();
        }

        /* ________________________________
         * |POS       NAME       HIGHSCORE|
         * | 1    ABCDEFGHIJKL    1234567 |
         * | 2    ABCDEFGHIJKL    1234567 |
         * |            ...               |
         * | 9    ABCDEFGHIJKL    1234567 |
         * |10    ABCDEFGHIJKL    1234567 |
         * |            ...               |
         * |99    ABCDEFGHIJKL    1234567 |
         * --------------------------------
        */
        public String GetHiscoreText()
        {
            String result = "POS       NAME       HIGHSCORE";
            int i;
            for (i = 0; i < MaxHiScores; i++)
            {
                result += "\n";
                result += String.Format("{0,2:G}    ", i + 1);
                if (i < _entries.Count) result += _entries[i].ToString();
                else result += "------------    -------";
            }
            return result;
        }

        private HiscoreManager()
        {
            _entries = new List<HiscoreEntry>();
            try
            {
                StreamReader reader = new StreamReader(scoreFile);
                while (!reader.EndOfStream)
                {
                    String line = reader.ReadLine();
                    HiscoreEntry entry = HiscoreEntry.Deserialize(line);
                    if (entry != null) _entries.Add(entry);
                }
                reader.Close();
            }
            catch (FileNotFoundException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        private void Serialize()
        {
            StreamWriter writer = new StreamWriter(scoreFile, false);
            foreach (HiscoreEntry entry in _entries)
            {
                writer.WriteLine(entry.Serialized());
            }
            writer.Close();
        }

        private static int CompareHiScores(HiscoreEntry x, HiscoreEntry y)
        {
            return y.HighScore - x.HighScore;
        }
    }
}
