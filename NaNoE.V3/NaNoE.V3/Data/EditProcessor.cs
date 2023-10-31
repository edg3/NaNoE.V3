using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NaNoE.V2.Data
{
    /// <summary>
    /// Static function that runs with the edit suggestions for us
    /// </summary>
    public class EditProcessor
    {
        /// <summary>
        /// Static reference, helps with the data binding and updating
        /// </summary>
        private static EditProcessor _instance;

        public static EditProcessor Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Private data it needs
        /// </summary>
        private List<string> _ignorables = new List<string>() { "i", "i'm", "i'll", "i'd" };

        /// <summary>
        /// User flagged ignore list
        /// </summary>
        private List<string> _ignored = new List<string>();
        public List<string> Ignored
        {
            get { return _ignored; }
            set { _ignored = value; }
        }

        /// <summary>
        /// User flagged phrases to remove
        /// </summary>
        private List<string> _phrases = new List<string>();
        public List<string> PhraseOptions
        {
            get { return _phrases; }
            set { _phrases = value; }
        }

        /// <summary>
        /// When created it makes the static reference, and loads the 'edits.txt'
        /// Note: ';' isn't allowed anywhere besides to split the 3 elements in EditOption data type
        /// </summary>
        public EditProcessor()
        {
            _instance = this;

            EditOptions = new List<EditOption>();

            if (File.Exists("edits.txt"))
            {
                using (var f = File.OpenRead("edits.txt"))
                {
                    using (var reader = new StreamReader(f))
                    {
                        string line = "";
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line != "")
                            {
                                var splt = line.Split(';');
                                EditOptions.Add(new EditOption(splt[0], splt[1], splt[2]));
                            }
                        }
                    }
                }
            }

            if (File.Exists("ignored.txt"))
            {
                using (var f = File.OpenRead("ignored.txt"))
                {
                    using (var reader = new StreamReader(f))
                    {
                        string line = "";
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line != "") _ignored.Add(line);
                        }
                    }
                }
            }

            if (File.Exists("shortening.txt"))
            {
                using (var f = File.OpenRead("shortening.txt"))
                {
                    using (var reader = new StreamReader(f))
                    {
                        string line = "";
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line != "") _phrases.Add(line.ToLower());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process the given text in full
        /// </summary>
        /// <param name="text">Paragraph to check</param>
        /// <returns>List of edit suggestions</returns>
        public List<string> Process(string text)
        {
            List<string> answer = new List<string>();

            // Ignore unneeded formating
            var splt = text.Split(' ');

            // Remove ignored formats, e.g. '"', ';'
            for (int k = 0; k < splt.Length; ++k)
            {
                splt[k] = splt[k].TrimStart(_trimChars).TrimEnd(_trimChars);
            }

            // Go through each word
            for (int j = 0; j < splt.Length; ++j)
            {
                if (splt[j].Length > 0) Check(splt[j], answer, j + 1);
            }

            // Go through phrase checks
            if (_position == "edit")
            {
                var lowered = text.ToLower();
                foreach (var line in _phrases)
                {
                    var splits = line.Split(';');
                    var a = TextContains(lowered, splits[0]);
                    if (a != -1)
                    {
                        answer.Add(a + "} " + splits[1]);
                    }
                }
            }

            // Process seperate sentences for repeated word sets
            var sentences = text.Split('.');
            bool broken = false;
            int which_sentence = 0;
            // ToDo - make this also split via "?" and "!"
            foreach (var sentence in sentences) 
            {
                ++which_sentence;
                if (sentence.Length > 1)
                {
                    var sentence_words = sentence.Split(' ');
                    for (int i = 0; i < sentence_words.Length - 3 && !broken; ++i)
                    {
                        var rep_words = sentence_words[i] + " " + sentence_words[i + 1];
                        for (int j = i + 2; j < sentence_words.Length - 1 && !broken; ++j)
                        {
                            var test_words = sentence_words[j] + " " + sentence_words[j + 1];
                            if (test_words.Trim(_trimChars) == rep_words.Trim(_trimChars))
                            {
                                answer.Add("Possible Repetition: [S " + which_sentence + "] " + rep_words);
                                broken = true;
                            }
                        }
                    }
                }

                if (broken) break;
            }

            return answer;
        }

        /// <summary>
        /// Find a paragraph in the text
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <param name="v">What to search for</param>
        /// <returns></returns>
        private int TextContains(string text, string v)
        {
            if (text.Length < v.Length) return -1;

            if (text.Contains(v))
            {
                for (int q = 0; q < text.Length - v.Length - 1; ++q)
                {
                    if (text.Substring(q, v.Length) == v)
                    {
                        string a = " ";
                        string b = " ";
                        if (q > 0)
                        {
                            a = text[q - 1].ToString();
                        }
                        if (q < text.Length - v.Length - 2)
                        {
                            b = text[q + v.Length].ToString();
                        }

                        a = a.Replace(',', ' ').Replace('"', ' ').Replace(';', ' ').Replace('\'', ' ');
                        b = b.Replace(',', ' ').Replace('"', ' ').Replace(';', ' ').Replace('\'', ' ');

                        if (a == " " && b == " ") return q;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Reference check used for if we are, or arent, in the edit view
        /// </summary>
        private string _position = "";
        public string Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary>
        /// List of options to check for 'edits'
        /// </summary>
        public List<EditOption> EditOptions { get; private set; }

        /// <summary>
        /// Check word an compile a set of answers for said word
        /// </summary>
        /// <param name="v">The word to check</param>
        /// <param name="answer">List to add suggestion answers to as needed</param>
        /// <param name="wordNum">The position of the word number in the original paragraph (see above)</param>
        private char[] _trimChars = new char[] { '"', ',', '.', '\'', ';', ' ' };
        private void Check(string v, List<string> answer, int wordNum)
        {
            // e.g. can put names here, words we ignore completely, etc
            if (_ignored.Contains(v.ToLower())
                || _ignored.Contains(v.ToLower().Trim(_trimChars)))
                return;

            // Only spell check unless in Edit mode.
            if (_position != "edit") return;

            if (v.Length > 0)
            {
                char a = v[0];
                while ((v.Length > 0) && !((a >= 97 && a <= 122) || (a >= 65 && a <= 90)))
                {
                    v = v.Substring(1);
                }
            }
            if (v.Length > 0)
            {
                char b = v[v.Length - 1];
                while ((v.Length > 0) && !((b >= 97 && b <= 122) || (b >= 65 && b <= 90)))
                {
                    v = v.Substring(0, v.Length - 1);
                }
            }

            // Linq use to find elements that match either full words or end of words
            var answersA = (from item in EditOptions
                            where item.Opt == v || (item.Opt[0] == '-' && v.EndsWith(item.SubOptimal))
                            select item).FirstOrDefault();

            // Note: this may only give one reasuly, however that should be fine as it would be edited regardless of it being in multiple options
            //  - e.g. You make the word 'thing' as an Opt, it flags for that AND for '-ing'
            if (null != answersA) answer.Add(wordNum + "] " + v + ": " + answersA.Detail);
        }

        /// <summary>
        /// Save all edit options to the file edits.txt
        /// </summary>
        public void SaveEditsOptions()
        {
            if (File.Exists("edits.txt")) File.Delete("edits.txt");

            using (var stream = File.Create("edits.txt"))
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var item in EditOptions)
                    {
                        writer.WriteLine(item.Opt + ";" + item.Detail + ";" + item.Message);
                    }
                }
            }

            if (File.Exists("ignored.txt")) File.Delete("ignored.txt");

            using (var stream = File.Create("ignored.txt"))
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var item in Ignored)
                    {
                        writer.WriteLine(item);
                    }
                }
            }

            if (File.Exists("shortening.txt")) File.Delete("shortening.txt");

            using (var stream = File.Create("shortening.txt"))
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var item in PhraseOptions)
                    {
                        writer.WriteLine(item);
                    }
                }
            }
        }
    }
}
