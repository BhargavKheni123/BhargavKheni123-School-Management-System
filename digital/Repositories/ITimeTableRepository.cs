using digital.Models;
using System.Collections.Generic;

namespace digital.Repositories
{
    public interface ITimeTableRepository
    {
        List<TimeTable> GetAllTimeTables();
        void AddTimeTable(TimeTable timeTable);
    }
}
