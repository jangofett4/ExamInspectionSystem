﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace ExamEvaluationSystem
{
    public static class Where
    {
        public static string Equals(params string[] args)
        {
            if (args.Length % 2 != 0)
                return "";

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i += 2)
            {
                sb.Append($"{ args[i] } = { args[i + 1] }");
                if (i != args.Length - 2)
                    sb.Append(" AND ");
            }

            return sb.ToString();
        }

        public static string Greater(params string[] args)
        {
            if (args.Length % 2 != 0)
                return "";

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i += 2)
            {
                sb.Append($"{ args[i] } > { args[i + 1] }");
                if (i != args.Length - 2)
                    sb.Append(" AND ");
            }

            return sb.ToString();
        }

        public static string Smaller(params string[] args)
        {
            if (args.Length % 2 != 0)
                return "";

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i += 2)
            {
                sb.Append($"{ args[i] } < { args[i + 1] }");
                if (i != args.Length - 2)
                    sb.Append(" AND ");
            }

            return sb.ToString();
        }

        public static string And(params string[] args)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i != args.Length - 1)
                {
                    sb.Append(args[i]);
                    sb.Append(" AND ");
                }
                else
                    sb.Append(args[i]);
            }

            return sb.ToString();
        }

        public static string Or(params string[] args)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i != args.Length - 1)
                {
                    sb.Append(args[i]);
                    sb.Append(" OR ");
                }
                else
                    sb.Append(args[i]);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Base command structure used for all other command types
    /// </summary>
    public abstract class EISBaseCommand
    {
        public StringBuilder CommandBase;

        public EISBaseCommand()
        {
            CommandBase = new StringBuilder();
        }

        public abstract SQLiteCommand Create(SQLiteConnection connection, params string[] args);
    }

    /// <summary>
    /// SQL update command
    /// </summary>
    public class EISUpdateCommand : EISBaseCommand
    {
        public string Table;
        public string Where;

        public EISUpdateCommand(string table, string where) : base()
        {
            Table = table;
            Where = where;
            CommandBase = new StringBuilder();
        }

        public override SQLiteCommand Create(SQLiteConnection connection, params string[] args)
        {
            if (args.Length == 0 || args.Length % 2 != 0)
                return null;

            CommandBase.Append("UPDATE ");
            CommandBase.Append(Table);
            CommandBase.Append(" SET ");

            for (int i = 0; i < args.Length; i += 2)
            {
                CommandBase.Append($"{ args[i] } = { args[i + 1]}");
                if (i != args.Length - 2)
                    CommandBase.Append(',');
            }

            CommandBase.Append(" WHERE ");
            CommandBase.Append(Where);

            return new SQLiteCommand(CommandBase.ToString(), connection);
        }
    }

    /// <summary>
    /// SQL select command
    /// </summary>
    public class EISSelectCommand : EISBaseCommand
    {
        public string Table;
        public string Where;

        public EISSelectCommand(string table, string where = "") : base()
        {
            Table = table;
            Where = where;
        }

        public override SQLiteCommand Create(SQLiteConnection connection, params string[] args)
        {
            StringBuilder selector = new StringBuilder();
            if (args.Length == 0)
                selector.Append('*');
            else
                for (int i = 0; i < args.Length; i++)
                    if (i != args.Length - 1)
                    {
                        selector.Append(args[i]);
                        selector.Append(',');
                    }
                    else
                        selector.Append(args[i]);

            CommandBase.Append($"SELECT { selector.ToString() } FROM { Table }");
            if (Where != "")
                CommandBase.Append($" WHERE { Where }");

            return new SQLiteCommand(CommandBase.ToString(), connection);
        }
    }

    /// <summary>
    /// SQL delete command
    /// </summary>
    public class EISDeleteCommand : EISBaseCommand
    {
        public string Table;
        public string Where;

        public EISDeleteCommand(string table, string where) : base()
        {
            Table = table;
            Where = where;
        }

        public override SQLiteCommand Create(SQLiteConnection connection, params string[] args)
        {
            if (Where == "") return null;
            CommandBase.Append($"DELETE FROM { Table } WHERE { Where }");
            return new SQLiteCommand(CommandBase.ToString(), connection);
        }
    }

    /// <summary>
    /// SQL insert command
    /// </summary>
    public class EISInsertCommand : EISBaseCommand
    {
        public string Table;

        public EISInsertCommand(string table) : base()
        {
            Table = table;
        }
        public override SQLiteCommand Create(SQLiteConnection connection, params string[] args)
        {
            if (args.Length == 0 || args.Length % 2 != 0)
                return null;

            CommandBase.Append($"INSERT INTO { Table } (");

            int len = args.Length / 2 + 1;
            List<string> values = new List<string>(len);

            for (int i = 0; i < args.Length; i += 2)
            {
                CommandBase.Append($"{ args[i] }");
                if (i != args.Length - 2)
                    CommandBase.Append(',');
                values.Add(args[i + 1]);
            }
            CommandBase.Append(") VALUES (");
            for (int i = 0; i < values.Count; i++)
            {
                CommandBase.Append(values[i]);
                if (i != values.Count - 1)
                    CommandBase.Append(',');
            }
            CommandBase.Append(')');
            return new SQLiteCommand(CommandBase.ToString(), connection);
        }
    }

    public static class EISHelper
    {
    }

    /// <summary>
    /// SQL data point. Must implement base functions.
    /// </summary>
    public abstract class EISDataPoint<T>
    {
        public abstract int Update(SQLiteConnection connection);
        public abstract int Insert(SQLiteConnection connection);
        public abstract int Delete(SQLiteConnection connection);
        public abstract SQLiteDataReader Select(SQLiteConnection connection, string where = "");
        public abstract SQLiteDataReader SelectAll(SQLiteConnection connection);
        public virtual T SelectT(SQLiteConnection connection, string where = "")
        {
            return default;
        }
        public virtual List<T> SelectAllT(SQLiteConnection connection)
        {
            return default;
        }
    }
}