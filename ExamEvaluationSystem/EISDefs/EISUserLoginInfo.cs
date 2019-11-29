using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    class EISUserLoginInfo : EISDataPoint<EISUserLoginInfo>
    {
        public int ID { get; private set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int UserID { get; set; }
        public int Privilege { get; set; }

        public EISUserLoginInfo(int id, string username, string password, int userid, int privilege)
        {
            ID = id;
            Username = username;
            Password = password;
            UserID = userid;
            Privilege = privilege;
        }

        public EISUserLoginInfo(string username, string password, int userid, int privilege)
        {
            ID = -1;
            Username = username;
            Password = password;
            UserID = userid;
            Privilege = privilege;
        }

        public EISUserLoginInfo(int id)
        {
            ID = id;
            Username = "";
            Password = "";
            UserID = -1;
            Privilege = -1;
        }

        private int id;
        private string username;
        private string password;
        private int userid;
        private int privilege;
        public override void Store()
        {
            id = ID;
            username = Username;
            password = Password;
            userid = UserID;
            privilege = Privilege;
        }

        public override void Restore()
        {
            ID = id;
            Username = username;
            Password = password;
            UserID = userid;
            Privilege = privilege;
        }

        public override int Update(SQLiteConnection connection)
        {
            var cmd = new EISUpdateCommand("UserLoginInfo", $"ID = { ID }");
            var sql = cmd.Create(connection, "Username", Username.EncapsulateQuote(), "Password", Password.EncapsulateQuote(), "UserID", UserID.ToString(), "MemberPrivilege", Privilege.ToString());
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
            var cmd = new EISInsertCommand("UserLoginInfo");
            var sql = cmd.Create(connection, "Username", Username.EncapsulateQuote(), "Password", Password.EncapsulateQuote(), "UserID", UserID.ToString(), "MemberPrivilege", Privilege.ToString());
            int res = -1;
           
            try
            {
                res = sql.ExecuteNonQuery();
            }
            catch (SQLiteException)
            {
                return -1;
            }

            sql = new SQLiteCommand("SELECT * FROM UserLoginInfo ORDER BY ID DESC LIMIT 1", connection);
            var id = (int)(long)sql.ExecuteScalar();

            ID = id;

            return res;
        }

        public override int Delete(SQLiteConnection connection)
        {
            var cmd = new EISDeleteCommand("UserLoginInfo", $"ID = { ID }");
            var sql = cmd.Create(connection);
            return sql.ExecuteNonQuery();
        }

        public override SQLiteDataReader Select(SQLiteConnection connection, string where = "")
        {
            var cmd = new EISSelectCommand("UserLoginInfo", where == "" ? $"ID = { ID }" : where);
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override SQLiteDataReader SelectAll(SQLiteConnection connection)
        {
            var cmd = new EISSelectCommand("UserLoginInfo");
            var sql = cmd.Create(connection);
            return sql.ExecuteReader();
        }

        public override EISUserLoginInfo SelectT(SQLiteConnection connection, string where = "")
        {
            using (var val = Select(connection, where))
            {
                if (!val.HasRows)
                    return null;

                val.Read();

                var inf = new EISUserLoginInfo(val.GetInt32(0));
                inf.Username = val.GetString(1);
                inf.Password = val.GetString(2);
                inf.UserID = val.GetInt32(3);
                inf.Privilege = val.GetInt32(4);

                return inf;
            }
        }
    }
}
