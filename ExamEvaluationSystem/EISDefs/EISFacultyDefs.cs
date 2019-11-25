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
        public int ID { get; set; }
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

        private int id;
        private string name;
        private List<EISDepartment> departments;
        public override void Store()
        {
            id = ID;
            name = Name;
            departments = Departments;
        }

        public override void Restore()
        {
            ID = id;
            Name = name;
            Departments = departments;
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Faculties", $"ID = { ID }");
            var sqlcmd = cmd.Create(connection, "Name", $"'{ Name }'");
            try
            {
                return sqlcmd.ExecuteNonQuery();
            } catch (SQLiteException e)
            {
                return -1; // cant update name anyway
            }
        }

        public override int UpdateWhere(SQLiteConnection connection, string where)
        {
            var cmd = new EISUpdateCommand("Faculties", where);
            var sqlcmd = cmd.Create(connection, "ID", ID.ToString(), "Name", $"'{ Name }'");
            try
            {
                return sqlcmd.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                if (e.Message.Contains(".Name"))
                    return -1;
                return -2;
            }
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Faculties");
            var sqlcmd = cmd.Create(connection, "ID", ID.ToString(), "Name", $"'{ Name }'");
            try
            {
                return sqlcmd.ExecuteNonQuery();
            } catch (SQLiteException e)
            {
                if (e.Message.Contains(".Name"))
                    return -1;
                return -2;
            }
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
        public int ID { get; set; }
        public string Name { get; set; }
        public EISFaculty Faculty { get; set; }
        public List<EISEarning> Earnings { get; set; }
        
        public string FacultyName { get { return Faculty.Name; } }

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
            var sqlcmd = cmd.Create(connection, "Name", Name.EncapsulateQuote(), "FacultyID", Faculty.ID.ToString());
            try
            {
                return sqlcmd.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                if (e.Message.Contains(".Name"))
                    return -1;
                else
                    return -2;
            }
        }

        public override int UpdateWhere(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISUpdateCommand("Departments", where);
            var sqlcmd = cmd.Create(connection, "ID", ID.ToString(), "Name", Name.EncapsulateQuote(), "FacultyID", Faculty.ID.ToString());
            try
            {
                return sqlcmd.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                if (e.Message.Contains(".Name"))
                    return -1;
                else
                    return -2;
            }
        }

        public override int UpdateWhereFields(SQLiteConnection connection, string where = "", params string[] args)
        {
            var cmd = new EISUpdateCommand("Departments", where);
            var sqlcmd = cmd.Create(connection, args);
            try
            {
                return sqlcmd.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                if (e.Message.Contains(".Name"))
                    return -1;
                else
                    return -2;
            }
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Departments");
            var sqlcmd = cmd.Create(connection, "ID", ID.ToString(), "Name", Name.EncapsulateQuote(), "FacultyID", Faculty.ID.ToString());
            try
            {
                return sqlcmd.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                if (e.Message.Contains(".Name"))
                    return -1;
                return -2;
            }
        }

        public void InsertEarnings(SQLiteConnection connection)
        {
            var del = new EISDeleteCommand("DepartmentEarnings", Where.Equals("DepartmentID", ID.ToString()));
            var delcmd = del.Create(connection);
            delcmd.ExecuteNonQuery();

            foreach (var e in Earnings)
            {
                var cmd = new EISInsertCommand("DepartmentEarnings");
                var sqlcmd = cmd.Create(connection, "DepartmentID", ID.ToString(), "EarningID", e.ID.ToString());
                sqlcmd.ExecuteNonQuery();
            }
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("Departments", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override int DeleteWhere(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISDeleteCommand("Departments", where);
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
