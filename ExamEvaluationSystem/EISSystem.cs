using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public static class EISSystem
    {
        public static SQLiteConnection Connection;

        public static List<EISFaculty> Faculties;
        public static List<EISDepartment> Departments;
        public static List<EISEarning> Earnings;

        public static EISPeriod ActivePeriod;

        public static EISDepartment GetDepartment(int DID)
        {
            foreach (var d in Departments)
                if (d.ID == DID) return d;
            return null;
        }

        public static EISFaculty GetFaculty(int FID)
        {
            foreach (var f in Faculties)
                if (f.ID == FID) return f;
            return null;
        }

        public static EISEarning GetEarning(int EID)
        {
            foreach (var e in Earnings)
                if (e.ID == EID) return e;
            return null;
        }
    }
}
