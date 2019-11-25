using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExamEvaluationSystem
{
    public class EISExam : EISDataPoint<EISExam>
    {
        public int ID { get; private set; }
        public EISLecture Lecture { get; set; }
        public EISPeriod Period { get; set; }
        public EISExamType Type { get; set; }
        public List<EISQuestion> Questions { get; set; }

        // Datagrid Helper
        public string LectureName { get { return Lecture.Name; } }
        public string PeriodName { get { return Period.Name; } }
        public string ExamType { get { return Type.Name; } }

        public EISExam(int id)
        {
            ID = id;
            Lecture = null;
            Period = null;
            Type = null;
            Questions = null;
        }

        public EISExam(EISLecture lecture, EISPeriod period, EISExamType type, List<EISQuestion> questions)
        {
            ID = -1;
            Lecture = lecture;
            Period = period;
            Type = type;
            Questions = questions;
        }

        public EISExam(int id, EISLecture lecture, EISPeriod period, EISExamType type, List<EISQuestion> questions)
        {
            ID = id;
            Lecture = lecture;
            Period = period;
            Type = type;
            Questions = questions;
        }

        private int id;
        private EISLecture lec;
        private EISPeriod period;
        private EISExamType type;
        private List<EISQuestion> questions;
        public override void Store()
        {
            id = ID;
            lec = Lecture;
            period = Period;
            type = Type;
            questions = Questions;
        }

        public override void Restore()
        {
            ID = id;
            Lecture = lec;
            Period = period;
            Type = type;
            Questions = questions;
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Exams", $"ID = { ID }");
            var sql = cmd.Create(connection, "LectureID", Lecture.ID.ToString(), "PeriodID", Period.ID.ToString(), "TypeID", Type.ID.ToString(), "Questions", JsonConvert.SerializeObject(Questions));
            return sql.ExecuteNonQuery();
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Exams");
            var sql = cmd.Create(connection, "LectureID", Lecture.ID.ToString(), "PeriodID", Period.ID.ToString(), "TypeID", Type.ID.ToString(), "Questions", JsonConvert.SerializeObject(Questions));
            return sql.ExecuteNonQuery();
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("Exams", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("Exams", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("Exams");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override EISExam SelectT(SQLiteConnection connection, string where = "")
        {
            using (var val = Select(connection, where))
            {
                if (!val.HasRows)
                    return null;

                val.Read();

                var exam = new EISExam(val.GetInt32(0))
                {
                    Lecture = EISSystem.GetLecture(val.GetInt32(1)),
                    Period = EISSystem.GetPeriod(val.GetInt32(2)),
                    Type = EISSystem.GetExamType(val.GetInt32(3)),
                    Questions = JsonConvert.DeserializeObject<IEnumerable<EISQuestion>>(val.GetString(4)).ToList()
                };

                return exam;
            }
        }
    }

    public class EISExamType : EISDataPoint<EISExamType>
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public bool Multiple { get; set; }

        public EISExamType(int id)
        {
            ID = id;
            Name = "";
            Multiple = false;
        }

        public EISExamType(string name, bool multiple)
        {
            ID = -1;
            Name = name;
            Multiple = multiple;
        }

        public EISExamType(int id, string name, bool multiple)
        {
            ID = id;
            Name = name;
            Multiple = multiple;
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("ExamTypes", $"ID = { ID }");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'", "Multiple", Multiple ? "1" : "0");
            return sql.ExecuteNonQuery();
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("ExamTypes");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'", "Multiple", Multiple ? "1" : "0");
            return sql.ExecuteNonQuery();
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("ExamTypes", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("ExamTypes", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("ExamTypes");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }
    }

    public class EISQuestion
    {
        public string Answer { get; set; }
        public List<EISEarning> Earnings { get; set; }

        public EISQuestion(string answer)
        {
            Answer = answer;
            Earnings = new List<EISEarning>();
        }
    }
}
