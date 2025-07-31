using digital.Models;
using System.Collections.Generic;
using System.Linq;

namespace digital.Repositories
{
    public class TimeTableRepository : ITimeTableRepository
    {
        private readonly ApplicationDbContext _context;

        public TimeTableRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<TimeTable> GetAllTimeTables()
        {
            return _context.TimeTables.ToList();
        }

        public void AddTimeTable(TimeTable timeTable)
        {
            _context.TimeTables.Add(timeTable);
            _context.SaveChanges();
        }
    }
}
