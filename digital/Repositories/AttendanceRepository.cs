using digital.Interfaces;
using digital.Models;
using System.Collections.Generic;
using System.Linq;

namespace digital.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly ApplicationDbContext _context;

        public AttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Attendance> GetAttendanceByStudentId(int studentId)
        {
            return _context.Attendance.Where(a => a.StudentId == studentId).ToList();
        }

        public List<Attendance> GetAttendanceByFilters(List<int> studentIds, int month, int year)
        {
            return _context.Attendance
                .Where(a => studentIds.Contains(a.StudentId) && a.Month == month && a.Year == year)
                .ToList();
        }

        public void SaveAttendance(List<Attendance> attendanceRecords)
        {
            foreach (var record in attendanceRecords)
            {
                var existing = _context.Attendance.FirstOrDefault(a =>
                    a.StudentId == record.StudentId &&
                    a.Day == record.Day &&
                    a.Month == record.Month &&
                    a.Year == record.Year
                );

                if (existing != null)
                {
                    existing.Attend = record.Attend;
                    _context.Attendance.Update(existing);
                }
                else
                {
                    _context.Attendance.Add(record);
                }
            }

            _context.SaveChanges();
        }
    }
}