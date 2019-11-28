using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public class EISStatistics
    {
        public class InternalExam
        {
            public List<InternalGroup> Groups;

            public static InternalExam FromExam(EISExam e, List<EISExamResult> res)
            {
                Dictionary<char, InternalGroup> groupsdict = new Dictionary<char, InternalGroup>();
                for (int i = 0; i < e.Questions.Count; i++)
                {
                    var g = e.Questions[i];
                    var gg = new InternalGroup(g.Group, g.Answer);
                    
                    if (g.EarningsWithType == null || g.EarningsWithType.Count == 0)
                        g.ConvertEarnings();

                    var conv = new List<List<string>>();
                    foreach (var eee in g.EarningsWithType)
                    {
                        var lst = new List<string>();
                        foreach (var eeee in eee)
                            lst.Add(eeee.Name);
                        conv.Add(lst);
                    }
                    gg.Earnings = conv;

                    groupsdict.Add(g.Group[0], gg);
                }

                foreach (var result in res)
                {
                    var g = groupsdict[result.Group];
                    InternalStudent student = new InternalStudent(result.No, result.Name, result.Surname, result.Answers);
                    g.AddStudent(student);
                }

                return new InternalExam() { Groups = groupsdict.Values.ToList() };
            }

            public void AddGroup(InternalGroup g)
            {
                g.ParentObject = this;
                Groups.Add(g);
            }
        }

        public class InternalGroup : IChildObject<InternalExam>
        {
            public string Group;
            public string Answers;
            public List<List<string>> Earnings;
            public List<InternalStudent> Students;
            public InternalExam ParentObject { get; set; }

            public InternalGroup(string g, string answers)
            {
                Group = g;
                Answers = answers;
                ParentObject = null;
                Students = new List<InternalStudent>();
            }

            public void AddStudent(InternalStudent s)
            {
                s.ParentObject = this;
                Students.Add(s);
            }
        }

        public class InternalStudent : IChildObject<InternalGroup>
        {
            public string Name;
            public string Surname;
            public string No;
            public string Answers;
            public InternalGroup ParentObject { get; set; }
            
            public InternalStudent(string no, string name, string surname, string answer)
            {
                Name = name;
                Surname = surname;
                No = no;
                Answers = answer;
                ParentObject = null;
            }
        }
    }
}
