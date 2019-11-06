using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public class EISPeriod : EISDataPoint<EISPeriod>
    {
        public int ID { get; private set; }
        public string Name { get; set; }

        public EISPeriod(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public EISPeriod(string name)
        {
            ID = -1;
            Name = name;
        }

        public EISPeriod(int id)
        {
            ID = id;
            Name = "";
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Periods", $"ID = { ID }");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'");
            return sql.ExecuteNonQuery();
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Periods");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'");
            return sql.ExecuteNonQuery();
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("Periods", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("Periods", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("Periods");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }
    }
}
