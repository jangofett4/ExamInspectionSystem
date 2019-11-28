﻿using System;
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
        public List<char> Groups { 
            get
            {
                List<char> g = new List<char>();
                foreach (var q in Questions)
                    g.Add(q.Group[0]);
                return g;
            } 
        }

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
            var sql = cmd.Create(connection, "LectureID", Lecture.ID.ToString(), "PeriodID", Period.ID.ToString(), "TypeID", Type.ID.ToString(), "Questions", JsonConvert.SerializeObject(Questions).EncapsulateQuote());
            return sql.ExecuteNonQuery();
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Exams");
            var sql = cmd.Create(connection, "LectureID", Lecture.ID.ToString(), "PeriodID", Period.ID.ToString(), "TypeID", Type.ID.ToString(), "Questions", JsonConvert.SerializeObject(Questions).EncapsulateQuote());
            var res = sql.ExecuteNonQuery();

            sql = new SQLiteCommand("SELECT * FROM Exams ORDER BY ID DESC LIMIT 1", connection);
            var id = (int)(long)sql.ExecuteScalar();

            ID = id;

            return res;
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

        public static List<EISQuestion> ParseAnswers(string content)
        {
            var split = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var lst = new List<EISQuestion>();
            foreach (var line in split)
            {
                var group = "";
                int i = 0;
                while (i < line.Length && !char.IsWhiteSpace(line[i]))
                    group += line[i++];
                if (i >= line.Length)
                    return null;
                while (i < line.Length && (char.IsWhiteSpace(line[i]) || line[i] == ';'))
                    i++;
                if (i >= line.Length)
                    return null;
                var ans = "";
                while (i < line.Length && (!char.IsWhiteSpace(line[i]) && char.IsLetter(line[i])))
                    ans += line[i++];
                lst.Add(new EISQuestion(ans) { Group = group });
            }
            return lst;
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
            var res = sql.ExecuteNonQuery();

            sql = new SQLiteCommand("SELECT * FROM ExamTypes ORDER BY ID DESC LIMIT 1", connection);
            var id = (int)(long)sql.ExecuteScalar();

            ID = id;

            return res;
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
        public string Group { get; set; }
        public List<List<int>> Earnings { get; set; }


        [JsonIgnore]
        public List<List<EISEarning>> EarningsWithType { get; set; }

        [JsonIgnore]
        public string FriendlyEarnings { get { return $"[{ EarningsWithType.Count } soru kazanımı]"; } }

        public EISQuestion(string answer)
        {
            Answer = answer;
            Earnings = new List<List<int>>();
            EarningsWithType = new List<List<EISEarning>>();
            for (int i = 0; i < Answer.Length; i++)
                Earnings.Add(new List<int>());
        }

        public void ConvertEarnings(List<EISSingleQuestion> questions)
        {
            EarningsWithType.Clear();
            Earnings.Clear();

            List<List<int>> lst = new List<List<int>>();
            foreach (var q in questions)
            {
                var l = new List<int>();
                foreach (var e in q.Earnings)
                    l.Add(e.ID);
                lst.Add(l);
            }

            Earnings = lst;
            ConvertEarnings();
        }

        public void ConvertEarnings()
        {
            EarningsWithType.Clear();
            foreach (var e in Earnings)
            {
                var lst = new List<EISEarning>();
                foreach (var ee in e)
                    lst.Add(EISSystem.GetEarning(ee));
                EarningsWithType.Add(lst);
            }
        }
    }
}
