﻿namespace TriviaServer
{
    public class Question
    {
        public string? QuestionText { get; set; }
        public string[] OptionTexts { get; set; } = new string[4];
        public int AnswerIndex { get; set; }
    }
}
