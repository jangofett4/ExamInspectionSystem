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
        public static List<EISEarning> DepartmentEarnings;
        public static List<EISEarning> LectureEarnings;

        public static List<EISPeriod> Periods;
        public static List<EISLecture> Lectures;
        
        public static List<EISLecturer> Lecturers;

        public static List<EISExam> Exams;
        public static List<EISExamType> ExamTypes;

        public static List<EISStudent> Students;

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

        public static EISPeriod GetPeriod(int PID)
        {
            foreach (var p in Periods)
                if (p.ID == PID) return p;
            return null;
        }

        public static EISLecture GetLecture(int LID)
        {
            foreach (var l in Lectures)
                if (l.ID == LID) return l;
            return null;
        }

        public static EISLecturer GetLecturer(int LID)
        {
            foreach (var l in Lecturers)
                if (l.ID == LID) return l;
            return null;
        }

        public static EISExamType GetExamType(int ETID)
        {
            foreach (var e in ExamTypes)
                if (e.ID == ETID) return e;
            return null;
        }

        public static EISExam GetExam(EISLecture lecture, EISPeriod period, int type)
        {
            foreach (var e in Exams)
                if (e.Lecture.ID == lecture.ID && e.Period.ID == period.ID && e.Type.ID == type)
                    return e;
            return null;
        }

        public static EISExam GetExam(int EXID)
        {
            foreach (var e in Exams)
                if (e.ID == EXID) return e;
            return null;
        }

        public static EISStudent GetStudent(string SID)
        {
            foreach (var e in Students)
                if (e.ID == SID) return e;
            return null;
        }
    }
}
