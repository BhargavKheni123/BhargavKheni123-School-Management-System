using System.Threading;
using System.Threading.Tasks;


namespace Digital.Services.Reports
{
    public interface IStudentReportService
    {
        Task<(byte[] bytes, string fileName)> GenerateStudentProfileDocAsync(CancellationToken ct = default);
        
    }
}