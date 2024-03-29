﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public class EISStudent : EISDataPoint<EISStudent>
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public EISDepartment Department { get; set; }

        public EISStudent(string id, string name, string surname, EISDepartment department)
        {
            ID = id;
            Name = name;
            Surname = surname;
            Department = department;
        }

        public EISStudent(string name, string surname, EISDepartment department)
        {
            ID = "";
            Name = name;
            Surname = surname;
            Department = department;
        }

        public EISStudent(string id)
        {
            ID = "";
            Name = "";
            Surname = "";
            Department = null;
        }

        private string id;
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

        public override object GetStored(string prop)
        {
            switch (prop)
            {
                case "ID": return id;
                case "Name": return name;
                case "Surname": return surname;
                case "Department": return dep;
            }
            return null;
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Students", $"ID = { ID }");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'", "Surname", $"'{ Surname }'", "DepartmentID", Department.ID.ToString());
            return sql.ExecuteNonQuery();
        }

        public override int UpdateWhere(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISUpdateCommand("Students", where);
            var sql = cmd.Create(connection, "ID", ID, "Name", $"'{ Name }'", "Surname", $"'{ Surname }'", "DepartmentID", Department.ID.ToString());
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
            var cmd = new EISInsertCommand("Students");
            var sql = cmd.Create(connection, "ID", ID, "Name", $"'{ Name }'", "Surname", $"'{ Surname }'", "DepartmentID", Department.ID.ToString());
            try
            {
                return sql.ExecuteNonQuery();
            }
            catch (SQLiteException)
            {
                return -1;
            }
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
