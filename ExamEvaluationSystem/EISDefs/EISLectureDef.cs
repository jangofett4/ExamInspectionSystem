using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public class EISLecture : EISDataPoint<EISLecture>
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public int Credit { get; set; }
        public List<EISEarning> Earnings { get; set; }

        public EISLecture(int id, string name, int credit)
        {
            ID = id;
            Name = name;
            Credit = credit;
            Earnings = new List<EISEarning>();
        }

        public EISLecture(string name, int credit)
        {
            ID = -1;
            Name = name;
            Credit = credit;
            Earnings = new List<EISEarning>();
        }

        public EISLecture(int id)
        {
            ID = id;
            Name = "";
            Credit = -1;
            Earnings = new List<EISEarning>();
        }

        private int id;
        private string name;
        private int credit;
        private List<EISEarning> earnings;
        public override void Store()
        {
            id = ID;
            name = Name;
            credit = Credit;
            earnings = Earnings;
        }

        public override void Restore()
        {
            ID = id;
            Name = name;
            Credit = credit;
            Earnings = earnings;
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Lectures", $"ID = { ID }");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'", "Credit", Credit.ToString());
            return sql.ExecuteNonQuery();
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Lectures");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'", "Credit", Credit.ToString());
            var res = sql.ExecuteNonQuery();

            sql = new SQLiteCommand("SELECT * FROM Lectures ORDER BY ID DESC LIMIT 1", connection);
            var id = (int)(long)sql.ExecuteScalar();

            ID = id;

            return res;
        }
        public void InsertEarnings(SQLiteConnection connection)
        {
            var del = new EISDeleteCommand("LectureEarnings", Where.Equals("LectureID", ID.ToString()));
            var delcmd = del.Create(connection);
            delcmd.ExecuteNonQuery();

            foreach (var e in Earnings)
            {
                var cmd = new EISInsertCommand("LectureEarnings");
                var sqlcmd = cmd.Create(connection, "LectureID", ID.ToString(), "EarningID", e.ID.ToString());
                sqlcmd.ExecuteNonQuery();
            }
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("Lectures", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("Lectures", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("Lectures");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override EISLecture SelectT(SQLiteConnection connection, string where = "")
        {
            using (var val = Select(connection, where))
            {
                if (!val.HasRows)
                    return null;

                val.Read();

                var lec = new EISLecture(val.GetInt32(0));  // ID
                lec.Name = val.GetString(1);                // Name
                lec.Credit = val.GetInt32(2);               // Credit

                var cmd = new EISSelectCommand("LectureEarnings", $"LectureID = { lec.ID }");

                using (var sql = cmd.Create(connection).ExecuteReader())
                {
                    while (sql.Read())
                        lec.Earnings.Add(EISSystem.GetEarning(sql.GetInt32(0)));

                    return lec;
                }
            }
        }
    }
}
