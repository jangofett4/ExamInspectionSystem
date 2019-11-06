using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace ExamEvaluationSystem
{
    public class EISFaculty : EISDataPoint<EISFaculty>
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public List<EISDepartment> Departments { get; set; }

        public EISFaculty(int id, string name)
        {
            ID = id;
            Name = name;
            Departments = new List<EISDepartment>();
        }

        public EISFaculty(string name)
        {
            ID = -1;
            Name = name;
            Departments = new List<EISDepartment>();
        }

        public EISFaculty(int id)
        {
            ID = id;
            Name = "";
            Departments = new List<EISDepartment>();
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Faculties", $"ID = { ID }");
            var sqlcmd = cmd.Create(connection, "Name", $"'{ Name }'");
            return sqlcmd.ExecuteNonQuery();
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Faculties");
            var sqlcmd = cmd.Create(connection, "Name", $"'{ Name }'");
            return sqlcmd.ExecuteNonQuery();
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("Faculties", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("Faculties", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("Faculties");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }
    }

    public class EISDepartment : EISDataPoint<EISDepartment>
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public EISFaculty Faculty { get; set; }
        public List<EISEarning> Earnings { get; set; }

        public EISDepartment(int id, string name, EISFaculty faculty, List<EISEarning> earnings)
        {
            ID = id;
            Name = name;
            Faculty = faculty;
            Earnings = earnings;
        }

        public EISDepartment(string name, EISFaculty faculty, List<EISEarning> earnings)
        {
            ID = -1;
            Name = name;
            Faculty = faculty;
            Earnings = earnings;
        }

        public EISDepartment(int id)
        {
            ID = id;
            Name = "";
            Faculty = null;
            Earnings = new List<EISEarning>();
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Departments", $"ID = { ID }");
            var sqlcmd = cmd.Create(connection, "Name", $"'{ Name }'", "FacultyID", Faculty.ID.ToString());
            return sqlcmd.ExecuteNonQuery();
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Departments");
            var sqlcmd = cmd.Create(connection, "Name", $"'{ Name }'", "FacultyID", Faculty.ID.ToString());
            return sqlcmd.ExecuteNonQuery();
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("Departments", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("Departments", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("Departments");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override EISDepartment SelectT(SQLiteConnection connection, string where = "")
        {
            using (var val = Select(connection, where))
            {
                if (!val.HasRows)
                    return null;

                val.Read();

                var dep = new EISDepartment(val.GetInt32(0));
                dep.Name = val.GetString(2);
                dep.Faculty = EISSystem.GetFaculty(val.GetInt32(1));

                var cmd = new EISSelectCommand("DepartmentEarnings", $"DepartmentID = { dep.ID }");

                using (var sql = cmd.Create(connection).ExecuteReader())
                {
                    while (sql.Read())
                        dep.Earnings.Add(EISSystem.GetEarning(sql.GetInt32(1)));
                    return dep;
                }
            }
        }
    }
}
