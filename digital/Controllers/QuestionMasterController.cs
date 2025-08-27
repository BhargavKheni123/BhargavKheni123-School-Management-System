using digital.Interfaces;
using digital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace digital.Controllers
{
    public class QuestionMasterController : Controller
    {
        private readonly IQuestionMasterRepository _repository;
        private readonly ApplicationDbContext _context;

        public QuestionMasterController(IQuestionMasterRepository repository, ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.SubjectList = new SelectList(_context.Subjects.ToList(), "Id", "Name");

            var questions = _repository.GetAllQuestions()
                .Where(q => q.QuestionText != null)
                .ToList();

            ViewBag.QuestionList = questions;

            return View(new QuestionMaster());
        }

        [HttpPost]
        public IActionResult Create(QuestionMaster question, List<string> answerOptions, int? correctAnswerIndex)
        {
            var opts = (answerOptions ?? new List<string>())
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Take(4).ToList();

            question.Answer1 = opts.ElementAtOrDefault(0);
            question.Answer2 = opts.ElementAtOrDefault(1);
            question.Answer3 = opts.ElementAtOrDefault(2);
            question.Answer4 = opts.ElementAtOrDefault(3);

            if (correctAnswerIndex.HasValue && correctAnswerIndex.Value < answerOptions.Count)
            {
                question.RightAnswer = answerOptions[correctAnswerIndex.Value];
            }

            var answers = new List<AnswerOptions>();
            for (int i = 0; i < answerOptions.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(answerOptions[i]))
                {
                    answers.Add(new AnswerOptions
                    {
                        OptionText = answerOptions[i],
                        AnswerText = answerOptions[i],
                        IsCorrect = (correctAnswerIndex.HasValue && i == correctAnswerIndex.Value)
                    });
                }
            }

            _repository.AddQuestion(question, answers);

            return RedirectToAction("Create");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var question = _repository.GetById(id);
            if (question == null)
                return NotFound();

            ViewBag.CategoryList = new SelectList(_context.Categories.ToList(), "Id", "Name", question.CategoryId);
            ViewBag.SubjectList = new SelectList(_context.Subjects.ToList(), "Id", "Name", question.SubjectId);

            return View(question);
        }

        [HttpPost]
        public IActionResult Edit(int id, QuestionMaster form, List<string> answerOptions, int? correctAnswerIndex)
        {
            var question = _repository.GetById(id);
            if (question == null)
                return NotFound();

            question.CategoryId = form.CategoryId;
            question.SubjectId = form.SubjectId;
            question.ExamType = form.ExamType;
            question.QuestionText = form.QuestionText;

            var answers = new List<AnswerOptions>();
            string rightAnswer = null;

            for (int i = 0; i < answerOptions.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(answerOptions[i]))
                {
                    bool isCorrect = (correctAnswerIndex.HasValue && i == correctAnswerIndex.Value);
                    if (isCorrect) rightAnswer = answerOptions[i];

                    answers.Add(new AnswerOptions
                    {
                        OptionText = answerOptions[i],
                        AnswerText = answerOptions[i],
                        IsCorrect = isCorrect
                    });
                }
            }

            question.RightAnswer = rightAnswer;

            _repository.UpdateQuestion(question, answers);

            return RedirectToAction(nameof(Create));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            _repository.DeleteQuestion(id);
            return RedirectToAction("Create");
        }

        public IActionResult ExportQuestionsToExcel()
        {
            var data = _repository.GetAllQuestions();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Questions");

                
                worksheet.Cell(1, 1).Value = "Standard";
                worksheet.Cell(1, 2).Value = "Subject";
                worksheet.Cell(1, 3).Value = "Exam Type";
                worksheet.Cell(1, 4).Value = "Exam Date";
                worksheet.Cell(1, 5).Value = "Exam Time";
                worksheet.Cell(1, 6).Value = "Question";
                worksheet.Cell(1, 7).Value = "Answer1";
                worksheet.Cell(1, 8).Value = "Answer2";
                worksheet.Cell(1, 9).Value = "Answer3";
                worksheet.Cell(1, 10).Value = "Answer4";
                worksheet.Cell(1, 11).Value = "Correct Answer";

                int row = 2;
                foreach (var q in data)
                {
                    worksheet.Cell(row, 1).Value = q.Category?.Name ?? "";
                    worksheet.Cell(row, 2).Value = q.Subject?.Name ?? "";
                    worksheet.Cell(row, 3).Value = q.ExamType ?? "";
                    worksheet.Cell(row, 4).Value = q.ExamDate?.ToString("yyyy-MM-dd") ?? "";
                    worksheet.Cell(row, 5).Value = $"{q.StartHour:D2}:{q.StartMinute:D2} - {q.EndHour:D2}:{q.EndMinute:D2}";
                    worksheet.Cell(row, 6).Value = q.QuestionText ?? "";
                    worksheet.Cell(row, 7).Value = q.Answer1 ?? "";
                    worksheet.Cell(row, 8).Value = q.Answer2 ?? "";
                    worksheet.Cell(row, 9).Value = q.Answer3 ?? "";
                    worksheet.Cell(row, 10).Value = q.Answer4 ?? "";
                    worksheet.Cell(row, 11).Value = q.RightAnswer ?? "";
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Questions.xlsx");
                }
            }
        }

        public IActionResult ExportQuestionsToPdf()
        {
            var data = _repository.GetAllQuestions();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());

                    page.Header()
                        .Text("Question Bank Report")
                        .FontSize(18)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);  
                                columns.ConstantColumn(80);  
                                columns.ConstantColumn(70);  
                                columns.ConstantColumn(80);  
                                columns.ConstantColumn(80);  
                                columns.RelativeColumn(2);   
                                columns.ConstantColumn(80);  
                            });

                            
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Standard").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Subject").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Exam Type").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Exam Date").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Exam Time").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Question").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Correct Answer").Bold();
                            });

                            foreach (var q in data)
                            {
                                table.Cell().Padding(5).Text(q.Category?.Name ?? "");
                                table.Cell().Padding(5).Text(q.Subject?.Name ?? "");
                                table.Cell().Padding(5).Text(q.ExamType ?? "");
                                table.Cell().Padding(5).Text(q.ExamDate?.ToString("yyyy-MM-dd") ?? "");
                                table.Cell().Padding(5).Text($"{q.StartHour:D2}:{q.StartMinute:D2} - {q.EndHour:D2}:{q.EndMinute:D2}");
                                table.Cell().Padding(5).Text(q.QuestionText ?? "");
                                table.Cell().Padding(5).Text(q.RightAnswer ?? "");
                            }
                        });
                });
            });

            var stream = new MemoryStream();
            pdf.GeneratePdf(stream);
            return File(stream.ToArray(), "application/pdf", "Questions.pdf");
        }

    }
}
