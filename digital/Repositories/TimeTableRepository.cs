using digital.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace digital.Repositories
{
    public class TimeTableRepository : ITimeTableRepository
    {
        private readonly ApplicationDbContext _context;
        public TimeTableRepository(ApplicationDbContext context) { _context = context; }

        public List<TimeTable> GetAllTimeTables()
        {
            return _context.TimeTables.Include(t => t.Teacher).ToList();
        }

        public void AddTimeTable(TimeTable tt)
        {
            _context.TimeTables.Add(tt);
            _context.SaveChanges(); // <<< must be present
        }
        
        public void UpdateTimeTable(TimeTable tt)
        {
            _context.TimeTables.Update(tt);
            _context.SaveChanges();
        }

    }

}
