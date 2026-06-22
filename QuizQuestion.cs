using System.Collections.Generic;

namespace Cybersecurity_Awareness_Bot
{
    public class QuizQuestion
    {
        public string Question { get; set; }

        public List<string> Options { get; set; }

        public int CorrectAnswerIndex { get; set; }

        public string Explanation { get; set; }

        public bool IsTrueFalse
        {
            get
            {
                return Options != null && Options.Count == 2;
            }
        }
    }
}
