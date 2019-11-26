using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public class EISLecturer : EISDataPoint<EISLecturer>
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public EISFaculty Faculty { get; set; }

        public string FacultyName { get { return Faculty.Name; } }

        public EISLecturer(int id, string name, string surname, EISFaculty faculty)
        {
            ID = id;
            Name = name;
            Surname = surname;
            Faculty = faculty;
        }

        public EISLecturer(string name, string surname, EISFaculty faculty)
        {
            ID = -1;
            Name = name;
            Surname = surname;
            Faculty = faculty;
        }

        public EISLecturer(int id)
        {
            ID = id;
            Name = "";
            Surname = "";
            Faculty = null;
        }

        private int id;
        private string name;
        private string surname;
        private EISFaculty faculty;
        public override void Store()
        {
            id = ID;
            name = Name;
            surname = Surname;
            faculty = Faculty;
        }

        public override void Restore()
        {
            ID = id;
            Name = name;
            Surname = surname;
            Faculty = faculty;
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Lecturers", $"ID = { ID }");
            var sql = cmd.Create(connection, "Name", $"'{ Name }'", "Surname", $"'{ Surname }'", "FacultyID", Faculty.ID.ToString());
            return sql.ExecuteNonQuery();
        }

        public override int UpdateWhere(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISUpdateCommand("Lecturers", where);
            var sql = cmd.Create(connection, "ID", ID.ToString(), "Name", $"'{ Name }'", "Surname", $"'{ Surname }'", "FacultyID", Faculty.ID.ToString());
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
            var cmd = new EISInsertCommand("Lecturers");
            var sql = cmd.Create(connection, "ID", ID.ToString(), "Name", $"'{ Name }'", "Surname", $"'{ Surname }'", "FacultyID", Faculty.ID.ToString());
            try
            {
                return sql.ExecuteNonQuery(); ;
            }
            catch (SQLiteException)
            {
                return -1;
            }
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("Lecturers", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("Lecturers", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("Lecturers");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override EISLecturer SelectT(SQLiteConnection connection, string where = "")
        {
            using (var val = Select(connection, where))
            {
                if (!val.HasRows)
                    return null;

                val.Read();

                var lec = new EISLecturer(val.GetInt32(0));         // ID
                lec.Name = val.GetString(1);                        // Name
                lec.Surname = val.GetString(2);                     // Surname
                lec.Faculty = EISSystem.GetFaculty(val.GetInt32(3));// Faculty

                return lec;
            }
        }
    }
}
