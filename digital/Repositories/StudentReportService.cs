using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Digital.Services.Reports
{
    public class StudentReportService : IStudentReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StudentReportService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private sealed class StudentReportRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public string SubCategoryName { get; set; } = string.Empty;
            public DateTime? DOB { get; set; }
            public string Gender { get; set; } = string.Empty;
            public string MobileNumber { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public DateTime CreatedDate { get; set; }
        }

        public async Task<(byte[] bytes, string fileName)> GenerateStudentProfileDocAsync(CancellationToken ct = default)
        {
            var rows = await (from s in _context.Student
                              join c in _context.Categories on s.CategoryId equals c.Id
                              join sc in _context.SubCategories on s.SubCategoryId equals sc.Id
                              orderby s.Name
                              select new StudentReportRow
                              {
                                  Id = s.Id,
                                  Name = s.Name,
                                  CategoryName = c.Name,
                                  SubCategoryName = sc.Name,
                                  DOB = s.DOB,
                                  Gender = s.Gender,
                                  MobileNumber = s.MobileNumber,
                                  Address = s.Address,
                                  Email = s.Email,
                                  CreatedDate = s.CreatedDate
                              }).ToListAsync(ct);

            int total = rows.Count;
            byte[] bytes;

            using (var mem = new MemoryStream())
            {
                using (var wordDoc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
                {
                    var mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());

                    var body = mainPart.Document.Body;

                   
                    body.AppendChild(new Paragraph(new Run(new Text("DIGITAL - Student Profile Report"))));
                    body.AppendChild(new Paragraph(new Run(new Text($"Generated On: {DateTime.Now:dd MMM yyyy, HH:mm}"))));
                    body.AppendChild(new Paragraph(new Run(new Text($"Total Students: {total}"))));

                    body.AppendChild(new Paragraph(new Run(new Text("")))); 

                    
                    var table = new Table();

                    
                    TableProperties tblProps = new TableProperties(
                        new TableBorders(
                            new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                        )
                    );
                    table.AppendChild(tblProps);

                    
                    var headers = new[] { "#", "Name", "Standard", "Division", "DOB", "Gender", "Mobile", "Address", "Email", "Created" };
                    var headerRow = new TableRow();
                    foreach (var h in headers)
                    {
                        var cell = new TableCell(new Paragraph(new Run(new Text(h))));
                        cell.Append(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                        headerRow.Append(cell);
                    }
                    table.Append(headerRow);

                    
                    for (int r = 0; r < total; r++)
                    {
                        var row = rows[r];
                        var tr = new TableRow();

                        string[] values =
                        {
                            (r+1).ToString(),
                            row.Name,
                            row.CategoryName,
                            row.SubCategoryName,
                            row.DOB.HasValue ? row.DOB.Value.ToString("dd-MMM-yyyy") : "-",
                            row.Gender,
                            row.MobileNumber,
                            row.Address,
                            row.Email,
                            row.CreatedDate.ToString("dd-MMM-yyyy")
                        };

                        foreach (var v in values)
                        {
                            var cell = new TableCell(new Paragraph(new Run(new Text(v ?? "-"))));
                            tr.Append(cell);
                        }

                        table.Append(tr);
                    }

                    body.Append(table);
                }

                bytes = mem.ToArray();
            }

            var fileName = $"StudentProfiles_{DateTime.Now:yyyyMMdd_HHmm}.docx";
            return (bytes, fileName);
        }
    }
}
