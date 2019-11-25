using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public class EISStudent : EISDataPoint<EISStudent>
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public EISDepartment Department { get; set; }

        public EISStudent(int id, string name, string surname, EISDepartment department)
        {
            ID = id;
            Name = name;
            Surname = surname;
            Department = department;
        }

        public EISStudent(string name, string surname, EISDepartment department)
        {
            ID = -1;
            Name = name;
            Surname = surname;
            Department = department;
        }

        public EISStudent(int id)
        {
            ID = id;
            Name = "";
            Surname = "";
            Department = null;
        }

        private int id;
        private string name, surname;
        private EISDepartment dep;
        public override void Store()
        {
            id = ID;
            name = Name;
            surname = Surname;
            dep = Department;
        }

        public override void Restore()
        {
            ID = id;
            Name = name;
            Surname = surname;
            Department = dep;
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Students", $"ID = { ID }");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'", "Surname", $"'{ Surname }'", "Department", Department.ID.ToString());
            return sql.ExecuteNonQuery();
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Students");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'", "Surname", $"'{ Surname }'", "Department", Department.ID.ToString());
            return sql.ExecuteNonQuery();
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("Students", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("Students", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("Students");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }
    }
}
