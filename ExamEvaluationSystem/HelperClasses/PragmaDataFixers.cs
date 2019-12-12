using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace ExamEvaluationSystem.DataFixer
{
    public abstract class EISDataFixer
    {
        public bool FixDone;
        public int FixResultCode;

        public List<object> PossibleErrors;

        public string Name;
        public string DevName;

        public SQLiteConnection Connection;

        public EISDataFixer(string name, string devname, SQLiteConnection c)
        {
            Name = name;
            DevName = devname;
            Connection = c;
            FixDone = false;
            FixResultCode = -1;
            PossibleErrors = new List<object>();
        }

        public abstract bool NeedFix();
        public abstract void Fix(IProgress<int> p);

        public bool CheckFieldForTable(string table, string field)
        {
            var cmd = new SQLiteCommand($"SELECT COUNT(cid) FROM pragma_table_info('{ table }') WHERE name = '{ field }'", Connection);
            return Convert.ToInt32(cmd.ExecuteScalar()) != 0;
        }

        public bool CheckForTable(string table)
        {
            return Convert.ToInt32(CreateCommand($"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{ table }';").ExecuteScalar()) != 0;
        }

        public void DisableFK()
        {
            CreateCommand("PRAGMA foreign_keys = OFF").ExecuteNonQuery();
        }

        public void EnableFK()
        {
            CreateCommand("PRAGMA foreign_keys = ON").ExecuteNonQuery();
        }

        public SQLiteCommand CreateCommand(string cmd)
        {
            return new SQLiteCommand(cmd, Connection);
        }
    }

    public class EarningCodeFix : EISDataFixer
    {
        public EarningCodeFix(SQLiteConnection c) : base("Kazanım_Tablosu_Düzelt", "DF_EARNING_TABLE_FIX", c)
        {
            Connection = c;
        }

        public override bool NeedFix()
        {
            return !CheckFieldForTable("Earnings", "Code");
        }

        public override void Fix(IProgress<int> p)
        {
            DisableFK();
            CreateCommand("ALTER TABLE Earnings RENAME TO Earnings_old").ExecuteNonQuery();
            CreateCommand("CREATE TABLE Earnings (ID INTEGER NOT NULL, Code TEXT NOT NULL, Name TEXT NOT NULL, EarningType INTEGER NOT NULL, PRIMARY KEY(ID))").ExecuteNonQuery();
            int total = Convert.ToInt32(CreateCommand("SELECT COUNT(*) FROM Earnings_old").ExecuteScalar());
            int i = 0;
            using (var rd = CreateCommand("SELECT * FROM Earnings_old").ExecuteReader())
            {
                while (rd.Read())
                {
                    var id = rd.GetInt32(0);
                    var name = rd.GetString(1);
                    var type = rd.GetInt32(2);
                    var li = name.LastIndexOf('-');
                    var code = "HATA";
                    if (li != -1)
                    {
                        code = name.Substring(0, name.LastIndexOf('-')).Trim().TrimEnd('-');
                        name = name.Substring(name.LastIndexOf('-')).TrimStart('-', ' ');
                    }
                    else
                        PossibleErrors.Add($"ID: { id }\tName: { name }");

                    if (code.Length > 6)
                        PossibleErrors.Add($"ID: { id }\tCode: { code }\tName: { name }");

                    CreateCommand($"INSERT INTO Earnings (ID, Code, Name, EarningType) VALUES ({ id }, '{ code }', '{ name }', { type })").ExecuteNonQuery();
                    if (total != 0)
                        p.Report((int)(((float)i / total) * 100));
                    i++;
                }
            }
            EnableFK();
            p.Report(100); // complete
            FixResultCode = 0;
            FixDone = true;
        }
    }

    public class EarningTableFix : EISDataFixer
    {
        public EarningTableFix(SQLiteConnection c) : base("Kazanım_Kodu_Düzelt", "DF_EARNING_CODE_FIX", c)
        {
            Connection = c;
        }

        public override bool NeedFix()
        {
            return CheckForTable("Earnings_old");
        }

        public override void Fix(IProgress<int> p)
        {
            DisableFK();
            CreateCommand("ALTER TABLE Earnings RENAME TO Earnings1").ExecuteNonQuery();
            p.Report(25); // complete
            CreateCommand("ALTER TABLE Earnings_old RENAME TO Earnings").ExecuteNonQuery();
            p.Report(50); // complete
            CreateCommand("DROP TABLE Earnings").ExecuteNonQuery();
            p.Report(75); // complete
            CreateCommand("ALTER TABLE Earnings1 RENAME TO Earnings").ExecuteNonQuery();
            p.Report(100); // complete
            EnableFK();
            FixResultCode = 0;
            FixDone = true;
        }
    }
}
