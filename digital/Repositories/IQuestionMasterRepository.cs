using digital.Models;
using System.Collections.Generic;

namespace digital.Interfaces
{
    public interface IQuestionMasterRepository
    {
        IEnumerable<QuestionMaster> GetAllQuestions();
        QuestionMaster GetById(int id);
        void AddQuestion(QuestionMaster question, List<AnswerOptions> answers);
        void UpdateQuestion(QuestionMaster question, List<AnswerOptions> answers);
        void DeleteQuestion(int id);
    }
}
