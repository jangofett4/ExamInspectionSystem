using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public class EISEarning : EISDataPoint<EISEarning>
    {
        public int ID { get; private set; }
        private string _name { get; set; }
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }
        private string _code { get; set; }
        public string Code { get { return _code; } set { _code = value; OnPropertyChanged("Code"); } }
        private EISEarningType _type;
        public EISEarningType EarningType { get { return _type; } set { _type = value; OnPropertyChanged("EarningType"); } }

        public string FriendlyEarningTypeName { get { return EarningType == EISEarningType.Department ? "Bölüm Kazanımı" : "Ders Kazanımı"; } }

        public EISEarning(int id, string code, string name, EISEarningType type)
        {
            ID = id;
            Code = code;
            Name = name;
            EarningType = type;
        }

        public EISEarning(string code, string name, EISEarningType type)
        {
            ID = -1;
            Code = code;
            Name = name;
            EarningType = type;
        }

        public EISEarning(int id)
        {
            ID = id;
            Code = "";
            Name = "";
            EarningType = 0;
        }

        private int id;
        private string name;
        private string code;
        private EISEarningType type;
        public override void Store()
        {
            id = ID;
            name = Name;
            code = Code;
            type = EarningType;
        }

        public override void Restore()
        {
            ID = id;
            Name = name;
            Code = code;
            EarningType = type;
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("Earnings", $"ID = { ID }");
            var sql = cmd.Create(connection, "Name", Name.EncapsulateQuote(), "Code", Code.EncapsulateQuote(), "EarningType", ((int)EarningType).ToString());
            return sql.ExecuteNonQuery();
        }

        public override int Insert(SQLiteConnection connection)
        {
            var cmd = new EISInsertCommand("Earnings");
            var sql = cmd.Create(connection, "Name", Name.EncapsulateQuote(), "Code", Code.EncapsulateQuote(), "EarningType", ((int)EarningType).ToString());
            var res = sql.ExecuteNonQuery();

            sql = new SQLiteCommand("SELECT * FROM Earnings ORDER BY ID DESC LIMIT 1", connection);
            var id = (int)(long)sql.ExecuteScalar();

            ID = id;

            return res;
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("Earnings", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("Earnings", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("Earnings");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override List<EISEarning> SelectAllT(SQLiteConnection connection)
        {
            var list = new List<EISEarning>();
            var sql = SelectAll(connection);
            while (sql.Read())
                list.Add(new EISEarning(sql.GetInt32(0), sql.GetString(1), sql.GetString(2), (EISEarningType)sql.GetInt32(3)));
            sql.Close();
            return list;
        }
    }

    public enum EISEarningType
    {
        Department = 0,
        Lecture = 1
    }
}
