using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public class EISExamType : EISDataPoint<EISExamType>
    {
        public int ID { get; private set; }
        public string Name;
        public bool Multiple;

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

    /*
    public class EISExam : EISDataPoint
    {
        public int ID { get; private set; }
        public EISLecture Lecture;
        public EISPeriod Period;
        public EISExamType Type;

    }
    */
    public struct EISQuestion
    {
        public string Answer;
        public EISEarning Earning;
    }
}
