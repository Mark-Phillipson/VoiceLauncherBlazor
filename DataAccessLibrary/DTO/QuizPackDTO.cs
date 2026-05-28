using System.Collections.Generic;

namespace DataAccessLibrary.DTO
{
    public class QuizPackDTO
    {
        public string? Source { get; set; }
        public string? ApplicationFilter { get; set; }
        public int QuestionCount { get; set; }
        public List<QuizQuestionDTO> Questions { get; set; } = new List<QuizQuestionDTO>();
    }

    public class QuizQuestionDTO
    {
        public int SourceCommandId { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int CorrectChoiceIndex { get; set; }
        public List<ChoiceDTO> Choices { get; set; } = new List<ChoiceDTO>();
        public int? RelatedCommandId { get; set; }
    }

    public class ChoiceDTO
    {
        public string Text { get; set; } = string.Empty;
        public int? CommandId { get; set; }
    }
}
