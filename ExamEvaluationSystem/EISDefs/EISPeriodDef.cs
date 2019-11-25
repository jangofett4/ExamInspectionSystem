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

        private int id;
        private string name;
        public override void Store()
        {
            id = ID;
            name = Name;
        }

        public override void Restore()
        {
            ID = id;
            Name = name;
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Periods", $"ID = { ID }");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'");
            try
            {
                return sql.ExecuteNonQuery();
            }
            catch (SQLiteException)
            {

                return -1;
            }
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Periods");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'");
            var res = sql.ExecuteNonQuery();
            var last = new EISSelectLastCommand("Periods", "ID");
            sql = last.Create(connection, "ID");
            ID = (int)(long)sql.ExecuteScalar();
            return res;
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

        public override EISPeriod SelectT(SQLiteConnection connection, string where = "")
        {
            using (var val = Select(connection, where))
            {
                if (!val.HasRows)
                    return null;

                val.Read();

                var per = new EISPeriod(val.GetInt32(0));
                per.Name = val.GetString(1);
                return per;
            }
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("Periods");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }
    }
}
