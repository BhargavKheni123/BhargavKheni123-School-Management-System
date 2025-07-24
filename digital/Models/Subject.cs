﻿using System.ComponentModel.DataAnnotations.Schema;

namespace digital.Models
{
    [Table("Subject")] // 👈 FIX this line (was "Subjects")
    public class Subject

    {

        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<TeacherMaster> TeacherMasters { get; set; }
    }
}