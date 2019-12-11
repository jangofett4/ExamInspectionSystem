using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public class EISExamResult : EISDataPoint<EISExamResult>
    {
        public int ID { get; set; }
        public EISStudent Student { get; set; }
        public EISExam Exam { get; set; }
        public string Answers { get { return enc_answers.Substring(1); } set { enc_answers = enc_answers[0] + value; } }

        private string enc_answers { get; set; }

        public string Name { get { return Student.Name; } set { Student.Name = value; } }
        public string Surname { get { return Student.Surname; } set { Student.Surname = value; } }
        public string No { get { return Student.ID; } set { Student.ID = value; } }
        public char Group { 
            get 
            { 
                return enc_answers[0]; 
            } set { 
                enc_answers = value + enc_answers.Substring(1);
            } 
        }

        public void CutAnswer(int start, int length)
        {
            var newans = enc_answers.Substring(start, length + 1);
            enc_answers = newans;
        }

        public void SetEncryptedAnswer(string answer)
        {
            enc_answers = answer;
        }

        public EISExamResult(int id, EISStudent student, EISExam exam, string answers)
        {
            ID = id;
            Student = student;
            Exam = exam;
            enc_answers = answers;
        }

        public EISExamResult(int id)
        {
            ID = id;
            Student = new EISStudent("");
            Exam = new EISExam(-1);
            enc_answers = "";
        }

        public EISExamResult(EISStudent student, EISExam exam, string answers)
        {
            ID = -1;
            Student = student;
            Exam = exam;
            enc_answers = answers;
        }

        public override int Update(SQLiteConnection connection)
        {
            {
                var cmd = new EISSelectCommand("ExamResults", Where.Equals("StudentID", Student.ID, "ExamID", Exam.ID.ToString(), "Answers", enc_answers.EncapsulateQuote()));
                var sql = cmd.Create(connection);
                var res = sql.ExecuteReader();
                if (res.HasRows)
                {
                    res.Read();
                    if (res.GetInt32(0) != ID) // If not self
                        return -1;
                }
            }
            {
                var cmd = new EISUpdateCommand("ExamResults", $"ID = { ID }");
                var sql = cmd.Create(connection, "StudentID", Student.ID, "ExamID", Exam.ID.ToString(), "Answers", enc_answers.EncapsulateQuote());
                return sql.ExecuteNonQuery();
            }
        }

        public override int Insert(SQLiteConnection connection)
        {
            {
                var cmd = new EISSelectCommand("ExamResults", Where.Equals("StudentID", Student.ID, "ExamID", Exam.ID.ToString(), "Answers", enc_answers.EncapsulateQuote()));
                var sql = cmd.Create(connection);
                var res = sql.ExecuteScalar();
                if (res != null)
                    return -1;
            }
            {
                var cmd = new EISInsertCommand("ExamResults");
                var sql = cmd.Create(connection, "StudentID", Student.ID, "ExamID", Exam.ID.ToString(), "Answers", enc_answers.EncapsulateQuote());
                var res = sql.ExecuteNonQuery();

                sql = new SQLiteCommand("SELECT * FROM ExamResults ORDER BY ID DESC LIMIT 1", connection);
                var res2 = sql.ExecuteScalar();
                var id = (int)(long)res2;

                ID = id;

                return res;
            }
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("ExamResults", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("ExamResults", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public List<EISExamResult> SelectList(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("ExamResults", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            var rd = sql.ExecuteReader();
            var res = new List<EISExamResult>();
            while (rd.Read())
                res.Add(new EISExamResult(rd.GetInt32(0), EISSystem.GetStudent(rd.GetInt32(1).ToString()), EISSystem.GetExam(rd.GetInt32(2)), rd.GetString(3)));
            return res;
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("ExamResults");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public static List<EISExamResult> ParseResults(string content)
        {
            var result = new List<EISExamResult>();
            var lines = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var dummy = new EISDepartment(-1);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.Length < 33)
                    return null; // error
                string name = line.Substring(0, 12).Trim().Capitalize();
                string surname = line.Substring(12, 12).Trim().Capitalize();
                string stno = line.Substring(24, 9);
                string answers = line.Substring(33);
                var student = new EISStudent(stno, name, surname, dummy);
                var exresult = new EISExamResult(-1, student, null, answers);

                result.Add(exresult);
            }
            return result;
        }
    }
}
