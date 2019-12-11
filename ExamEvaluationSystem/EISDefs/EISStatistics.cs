using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;

namespace ExamEvaluationSystem
{
    public class EISStatistics
    {
        public class InternalExam
        {
            public List<InternalGroup> Groups;
            public float PointPerQuestion { get { return 100f / Groups[0].Answers.Length; } }

            public void ExportStatistics(string file, string file2)
            {
                XSSFWorkbook wb_1 = new XSSFWorkbook();
                XSSFWorkbook wb_2 = new XSSFWorkbook();

                var sheet_results = wb_1.CreateSheet("Soru Bazlı Değerlendirme");         // first sheet
                var sheet_success = wb_1.CreateSheet("Sorular için Ortalama ve Başarım"); // second sheet
                var sheet_ernasso = wb_1.CreateSheet("Soru-Kazanım Eşleştirmesi");        // third sheet
                var sheet_earning = wb_2.CreateSheet("Kazanım için Ortalama ve Başarım"); // first sheet of second workbook
                var sheet_ernasso2 = wb_2.CreateSheet("Soru-Kazanım Eşleştirmesi");       // second sheet

                int i1 = 0; // first sheet
                int i2 = 0; // second sheet
                int i3 = 0; // third sheet
                var i4 = 0;
                var i5 = 0;

                // create font style
                XSSFFont headersfnt = (XSSFFont)wb_1.CreateFont();
                headersfnt.FontHeightInPoints = 10;
                headersfnt.FontName = "Arial";
                headersfnt.IsBold = true;

                XSSFFont defaultfnt = (XSSFFont)wb_1.CreateFont();
                defaultfnt.FontHeightInPoints = 10;
                defaultfnt.FontName = "Arial";

                // create font style for second workbook
                XSSFFont headersfnt2 = (XSSFFont)wb_2.CreateFont();
                headersfnt2.FontHeightInPoints = 10;
                headersfnt2.FontName = "Arial";
                headersfnt2.IsBold = true;

                XSSFFont defaultfnt2 = (XSSFFont)wb_2.CreateFont();
                defaultfnt2.FontHeightInPoints = 10;
                defaultfnt2.FontName = "Arial";

                // create bordered cell style
                XSSFCellStyle borderedHeaderStyle = (XSSFCellStyle)wb_1.CreateCellStyle();
                borderedHeaderStyle.SetFont(headersfnt);
                borderedHeaderStyle.BorderLeft = BorderStyle.Thin;
                borderedHeaderStyle.BorderTop = BorderStyle.Thin;
                borderedHeaderStyle.BorderRight = BorderStyle.Thin;
                borderedHeaderStyle.BorderBottom = BorderStyle.Thin;
                borderedHeaderStyle.Alignment = HorizontalAlignment.Center;

                XSSFCellStyle borderedStyle = (XSSFCellStyle)wb_1.CreateCellStyle();
                borderedStyle.SetFont(defaultfnt);
                borderedStyle.BorderLeft = BorderStyle.Thin;
                borderedStyle.BorderTop = BorderStyle.Thin;
                borderedStyle.BorderRight = BorderStyle.Thin;
                borderedStyle.BorderBottom = BorderStyle.Thin;
                borderedStyle.Alignment = HorizontalAlignment.Left;

                XSSFCellStyle borderedCenteredStyle = (XSSFCellStyle)wb_1.CreateCellStyle();
                borderedCenteredStyle.SetFont(defaultfnt);
                borderedCenteredStyle.BorderLeft = BorderStyle.Thin;
                borderedCenteredStyle.BorderTop = BorderStyle.Thin;
                borderedCenteredStyle.BorderRight = BorderStyle.Thin;
                borderedCenteredStyle.BorderBottom = BorderStyle.Thin;
                borderedCenteredStyle.Alignment = HorizontalAlignment.Center;

                // create bordered cell style for second workbook
                XSSFCellStyle borderedHeaderStyle2 = (XSSFCellStyle)wb_2.CreateCellStyle();
                borderedHeaderStyle2.SetFont(headersfnt2);
                borderedHeaderStyle2.BorderLeft = BorderStyle.Thin;
                borderedHeaderStyle2.BorderTop = BorderStyle.Thin;
                borderedHeaderStyle2.BorderRight = BorderStyle.Thin;
                borderedHeaderStyle2.BorderBottom = BorderStyle.Thin;
                borderedHeaderStyle2.Alignment = HorizontalAlignment.Center;

                XSSFCellStyle borderedStyle2 = (XSSFCellStyle)wb_2.CreateCellStyle();
                borderedStyle2.SetFont(defaultfnt2);
                borderedStyle2.BorderLeft = BorderStyle.Thin;
                borderedStyle2.BorderTop = BorderStyle.Thin;
                borderedStyle2.BorderRight = BorderStyle.Thin;
                borderedStyle2.BorderBottom = BorderStyle.Thin;
                borderedStyle2.Alignment = HorizontalAlignment.Left;

                XSSFCellStyle borderedCenteredStyle2 = (XSSFCellStyle)wb_2.CreateCellStyle();
                borderedCenteredStyle2.SetFont(defaultfnt2);
                borderedCenteredStyle2.BorderLeft = BorderStyle.Thin;
                borderedCenteredStyle2.BorderTop = BorderStyle.Thin;
                borderedCenteredStyle2.BorderRight = BorderStyle.Thin;
                borderedCenteredStyle2.BorderBottom = BorderStyle.Thin;
                borderedCenteredStyle2.Alignment = HorizontalAlignment.Center;

                int earnasso_0 = 0;
                int earnasso_1 = 0;

                for (int gg = 0; gg < Groups.Count; gg++)
                {
                    var g = Groups[gg];

                    var st_qres = g.GetResultsForQuestions();       // Get results
                    // First sheet
                    {
                        var col_row = sheet_results.CreateRow(i1++);
                        var avg_start = i1 + 1;
                        int j = 0;
                        var _1 = col_row.CreateCell(j++); _1.SetCellValue("Öğrenci No"); _1.CellStyle = borderedHeaderStyle;
                        var _2 = col_row.CreateCell(j++); _2.SetCellValue("Adı / Soyadı"); _2.CellStyle = borderedHeaderStyle;
                        for (int q = 0; q < g.Answers.Length; q++)
                        {
                            var _3 = col_row.CreateCell(j++); _3.SetCellValue($"Soru { q + 1 }"); _3.CellStyle = borderedHeaderStyle;
                        }
                        for (int k = 0; k < g.Students.Count; k++)
                        {
                            var q_row = sheet_results.CreateRow(i1++);
                            var st = g.Students[k];
                            int l = 0;
                            var __1 = q_row.CreateCell(l++); __1.SetCellValue(st.No); __1.CellStyle = borderedStyle;
                            var __2 = q_row.CreateCell(l++); __2.SetCellValue(st.Name + " " + st.Surname); __2.CellStyle = borderedStyle;

                            for (int q = 0; q < st.Answers.Length; q++)
                            {
                                var __3 = q_row.CreateCell(l++); __3.SetCellValue(float.Parse(st_qres[q][k].ToString("0.##"))); __3.CellStyle = borderedStyle;
                            }

                            var prow = q_row.CreateCell(l++);
                            prow.CellStyle = borderedStyle;
                            prow.SetCellType(CellType.Formula);
                            prow.SetCellFormula($"ROUND(SUM({ ColumnIndexToColumnLetter(3) }{ i1 }:{ ColumnIndexToColumnLetter(l - 1) }{ i1 }), 0)");

                        }
                        var _4 = col_row.CreateCell(j++); _4.SetCellValue("Puan"); _4.CellStyle = borderedHeaderStyle;
                        int avg_end = i1;
                        var a_row = sheet_results.CreateRow(i1++);
                        int a = 0;
                        var _5 = a_row.CreateCell(a++); _5.SetCellValue("ORTALAMA"); _5.CellStyle = borderedHeaderStyle;
                        var __4 = a_row.CreateCell(a++); __4.CellStyle = borderedStyle;
                        for (int q = 0; q < g.Answers.Length + 1; q++)
                        {
                            var avgcell = a_row.CreateCell(a++);
                            avgcell.CellStyle = borderedStyle;
                            avgcell.SetCellType(CellType.Formula);
                            avgcell.SetCellFormula($"SUM({ ColumnIndexToColumnLetter(a) }{ avg_start }:{ ColumnIndexToColumnLetter(a)}{ avg_end })/{ g.Students.Count }");
                        }
                        i1++;
                    }
                    {
                        var col_row = sheet_success.CreateRow(i2++);
                        int j = 0;
                        var _1 = col_row.CreateCell(j++); _1.SetCellValue("Soru Numarası"); _1.CellStyle = borderedHeaderStyle;
                        var _2 = col_row.CreateCell(j++); _2.SetCellValue("Ortalaması (Puan)"); _2.CellStyle = borderedHeaderStyle;
                        var _3 = col_row.CreateCell(j++); _3.SetCellValue("Başarımı (%)"); _3.CellStyle = borderedHeaderStyle;
                        var avgs = g.GetAveragesForQuestions(st_qres);  // Get averages
                        var percs = g.GetPercentageForQuestions(avgs);  // Get percentages
                        for (int q = 0; q < g.Answers.Length; q++)
                        {
                            var q_row = sheet_success.CreateRow(i2++);
                            int l = 0;
                            var _4 = q_row.CreateCell(l++); _4.SetCellValue($"Soru { q + 1 }"); _4.CellStyle = borderedStyle;
                            var _5 = q_row.CreateCell(l++); _5.SetCellValue(avgs[q]); _5.CellStyle = borderedStyle;
                            var _6 = q_row.CreateCell(l++); _6.SetCellValue("%" + percs[q].ToString()); _6.CellStyle = borderedStyle;
                        }
                        i2++;
                    }
                    {
                        g.GetDistinctEarnings();
                        var col_row = sheet_ernasso.CreateRow(i4++);
                        int j = 0;
                        var _1 = col_row.CreateCell(j++); _1.SetCellValue("Soru Numarası"); _1.CellStyle = borderedHeaderStyle;
                        for (int q = 0; q < g.DisticntEarningList.Count; q++)
                        {
                            var de = g.DisticntEarningList.ElementAt(q);
                            var __2 = col_row.CreateCell(j++); __2.SetCellValue(de.Key.Item1); __2.CellStyle = borderedHeaderStyle;
                        }
                        if (gg == 0) // If this is first group
                        {
                            var _3 = col_row.CreateCell(j++ + 1); _3.SetCellValue("Kazanım Kodu"); _3.CellStyle = borderedHeaderStyle; earnasso_0 = j;
                            var _4 = col_row.CreateCell(j++ + 1); _4.SetCellValue(""); _4.CellStyle = borderedHeaderStyle; earnasso_1 = j;
                            for (int q = 0; q < g.Answers.Length; q++)
                            {
                                var qrow = sheet_ernasso.CreateRow(i4++);
                                int l = 0;
                                var __2 = qrow.CreateCell(l++); __2.SetCellValue($"Soru { q + 1 }"); __2.CellStyle = borderedStyle;
                                for (int qq = 0; qq < g.DisticntEarningList.Count; qq++)
                                {
                                    var de = g.DisticntEarningList.ElementAt(qq);
                                    if (de.Value.Contains(q))
                                    {
                                        var __3 = qrow.CreateCell(l++); __3.SetCellValue(" X "); __3.CellStyle = borderedCenteredStyle;
                                    }
                                    else
                                    {
                                        var __3 = qrow.CreateCell(l++); __3.SetCellValue(""); __3.CellStyle = borderedCenteredStyle;
                                    }
                                }
                            }
                            int ii4 = i4 - g.Answers.Length;
                            for (int q = 0; q < g.DisticntEarningList.Count; q++)
                            {
                                var de = g.DisticntEarningList.ElementAt(q);
                                var erow = sheet_ernasso.GetRow(ii4++);
                                if (erow == null)
                                {
                                    erow = sheet_ernasso.CreateRow(ii4 - 1);
                                    i4 = ii4;
                                }
                                int m = 2 + g.DisticntEarningList.Count;
                                var __2 = erow.CreateCell(m++); __2.SetCellValue(de.Key.Item1); __2.CellStyle = borderedHeaderStyle;
                                var __3 = erow.CreateCell(m++); __3.SetCellValue(de.Key.Item2); __3.CellStyle = borderedStyle;
                            }
                            if (g.DisticntEarningList.Count > g.Answers.Length)
                                sheet_ernasso.CreateRow(i4++); // lets add some broken-a** fixes here
                        }
                        else
                        {
                            for (int q = 0; q < g.Answers.Length; q++)
                            {
                                var qrow = sheet_ernasso.CreateRow(i4++);
                                int l = 0;
                                var __2 = qrow.CreateCell(l++); __2.SetCellValue($"Soru { q + 1 }"); __2.CellStyle = borderedStyle;
                                for (int qq = 0; qq < g.DisticntEarningList.Count; qq++)
                                {
                                    var de = g.DisticntEarningList.ElementAt(qq);
                                    if (de.Value.Contains(q))
                                    {
                                        var __3 = qrow.CreateCell(l++); __3.SetCellValue(" X "); __3.CellStyle = borderedCenteredStyle;
                                    }
                                    else
                                    {
                                        var __3 = qrow.CreateCell(l++); __3.SetCellValue(""); __3.CellStyle = borderedCenteredStyle;
                                    }
                                }
                            }
                        }
                        if (g.Answers.Length >= g.DisticntEarningList.Count)
                            sheet_ernasso.CreateRow(i4++); // see above, just above else
                    }
                    {
                        var col_row = sheet_ernasso2.CreateRow(i5++);
                        int j = 0;
                        var _1 = col_row.CreateCell(j++); _1.SetCellValue("Soru Numarası"); _1.CellStyle = borderedHeaderStyle2;
                        for (int q = 0; q < g.DisticntEarningList.Count; q++)
                        {
                            var de = g.DisticntEarningList.ElementAt(q);
                            var __2 = col_row.CreateCell(j++); __2.SetCellValue(de.Key.Item1); __2.CellStyle = borderedHeaderStyle2;
                        }
                        for (int q = 0; q < g.Answers.Length; q++)
                        {
                            var qrow = sheet_ernasso2.CreateRow(i5++);
                            int l = 0;
                            var __2 = qrow.CreateCell(l++); __2.SetCellValue($"Soru { q + 1 }"); __2.CellStyle = borderedStyle2;
                            for (int qq = 0; qq < g.DisticntEarningList.Count; qq++)
                            {
                                var de = g.DisticntEarningList.ElementAt(qq);
                                if (de.Value.Contains(q))
                                {
                                    var __3 = qrow.CreateCell(l++); __3.SetCellValue(" X "); __3.CellStyle = borderedCenteredStyle2;
                                }
                                else
                                {
                                    var __3 = qrow.CreateCell(l++); __3.SetCellValue(""); __3.CellStyle = borderedCenteredStyle2;
                                }
                            }
                        }
                        if (g.Answers.Length >= g.DisticntEarningList.Count)
                            sheet_ernasso2.CreateRow(i5++); // see above, just above else
                    }
                    {
                        var col_row = sheet_earning.CreateRow(i3++);
                        int j = 0;
                        var _1 = col_row.CreateCell(j++); _1.SetCellValue("Kazanım Numarası"); _1.CellStyle = borderedHeaderStyle2;
                        var _2 = col_row.CreateCell(j++); _2.SetCellValue("Ortalaması (Puan)"); _2.CellStyle = borderedHeaderStyle2;
                        var _3 = col_row.CreateCell(j++); _3.SetCellValue("Başarımı (%)"); _3.CellStyle = borderedHeaderStyle2;
                        // var _4 = col_row.CreateCell(j++); _4.SetCellValue("Sorular"); _4.CellStyle = borderedHeaderStyle2;
                        var _5 = col_row.CreateCell(j++); _5.SetCellValue("Kazanım Açıklaması"); _5.CellStyle = borderedHeaderStyle2;
                        var avgs = g.GetAveragesForQuestions(st_qres);  // Get averages
                        var e_avgs = g.GetAverageEarningPoints(avgs);
                        var percs = g.GetPercentageEarningPoints(e_avgs);
                        for (int q = 0; q < g.DisticntEarningList.Count; q++)
                        {
                            var de = g.DisticntEarningList.ElementAt(q);
                            var q_row = sheet_earning.CreateRow(i3++);
                            int l = 0;
                            var __6 = q_row.CreateCell(l++); __6.SetCellValue(de.Key.Item1); __6.CellStyle = borderedStyle2;
                            var __7 = q_row.CreateCell(l++); __7.SetCellValue(e_avgs[q]); __7.CellStyle = borderedStyle2;
                            var __8 = q_row.CreateCell(l++); __8.SetCellValue("%" + percs[q].ToString()); __8.CellStyle = borderedCenteredStyle2;
                            /*StringBuilder sb = new StringBuilder();
                            for (int m = 0; m < de.Value.Count; m++)
                            {
                                sb.Append(de.Value[m] + 1);
                                if (m != de.Value.Count - 1)
                                    sb.Append(", ");
                            }
                            var __9 = q_row.CreateCell(l++); __9.SetCellValue(sb.ToString()); __9.CellStyle = borderedStyle2;*/
                            var __10 = q_row.CreateCell(l++); __10.SetCellValue(de.Key.Item2); __10.CellStyle = borderedStyle2;
                        }
                        i3++;
                    }
                }

                sheet_results.AutoSizeColumn(0);
                sheet_results.AutoSizeColumn(1);

                sheet_success.AutoSizeColumn(0);
                sheet_success.AutoSizeColumn(1);
                sheet_success.AutoSizeColumn(2);

                sheet_ernasso.AutoSizeColumn(0);
                sheet_ernasso.AutoSizeColumn(earnasso_0);
                sheet_ernasso.AutoSizeColumn(earnasso_1);

                sheet_ernasso2.AutoSizeColumn(0);

                sheet_earning.AutoSizeColumn(0);
                sheet_earning.AutoSizeColumn(1);
                sheet_earning.AutoSizeColumn(2);
                sheet_earning.AutoSizeColumn(3);
                sheet_earning.AutoSizeColumn(4);

                XSSFFormulaEvaluator.EvaluateAllFormulaCells(wb_1);
                using (var wbfile = new FileStream(file, FileMode.OpenOrCreate))
                    wb_1.Write(wbfile);
                using (var wbfile = new FileStream(file2, FileMode.OpenOrCreate))
                    wb_2.Write(wbfile);
            }

            // Numaradan Excel sütun ismini bulur, örn 100 => CV
            public static string ColumnIndexToColumnLetter(int colIndex)
            {
                int div = colIndex;
                string colLetter = String.Empty;
                int mod = 0;

                while (div > 0)
                {
                    mod = (div - 1) % 26;
                    colLetter = (char)(65 + mod) + colLetter;
                    div = (div - mod) / 26;
                }
                return colLetter;
            }

            // Sınavdan istatistik iç türlerine dönüşüm sağlar
            public static InternalExam FromExam(EISExam e, List<EISExamResult> res)
            {
                Dictionary<char, InternalGroup> groupsdict = new Dictionary<char, InternalGroup>();
                for (int i = 0; i < e.Questions.Count; i++)
                {
                    var g = e.Questions[i];
                    var gg = new InternalGroup(g.Group, g.Answer);

                    if (g.EarningsWithType == null || g.EarningsWithType.Count == 0)
                        g.ConvertEarnings();

                    var conv = new List<List<(string, string)>>();
                    foreach (var eee in g.EarningsWithType)
                    {
                        var lst = new List<(string, string)>();
                        foreach (var eeee in eee)
                            lst.Add((eeee.Code, eeee.Name));
                        conv.Add(lst);
                    }
                    gg.Earnings = conv;

                    groupsdict.Add(g.Group[0], gg);
                }

                foreach (var result in res)
                {
                    if (!groupsdict.ContainsKey(result.Group))
                    {
                        // application will presume they left the questions empty and create statistics based on that
                        // also they will be associated to first group
                        int len = result.Answers.Length;
                        result.Answers = "";
                        for (int i = 0; i < len; i++) result.Answers += " ";
                        result.Group = groupsdict.ElementAt(0).Key;
                    }
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
            public List<List<(string, string)>> Earnings;
            public Dictionary<(string, string), List<int>> DisticntEarningList;
            public List<InternalStudent> Students;
            public InternalExam ParentObject { get; set; }

            public InternalGroup(string g, string answers)
            {
                Group = g;
                Answers = answers;
                ParentObject = null;
                Students = new List<InternalStudent>();
                DisticntEarningList = new Dictionary<(string, string), List<int>>();
                Earnings = new List<List<(string, string)>>();
            }

            // Ortamala kazanım puanlarını hesaplar
            public float[] GetAverageEarningPoints(float[] avgs)
            {
                float pts = 100f / DisticntEarningList.Count;
                var a = new float[DisticntEarningList.Count];
                for (int i = 0; i < DisticntEarningList.Count; i++)
                {
                    var de = DisticntEarningList.ElementAt(i);
                    var lst = de.Value;
                    var acc = 0f;
                    foreach (var q in lst)
                        acc += avgs[q];
                    acc /= lst.Count;
                    a[i] = acc;
                }
                return a;
            }

            // Yüzde kazanım puanlarını hesaplar
            public float[] GetPercentageEarningPoints(float[] avgs)
            {
                float pts = 100f / Answers.Length;
                var a = new float[DisticntEarningList.Count];
                for (int i = 0; i < DisticntEarningList.Count; i++)
                    a[i] = (avgs[i] / pts) * 100;
                return a;
            }

            // Tüm soruların sonuçlarını hesaplar
            public float[][] GetResultsForQuestions()
            {
                var m = new float[Answers.Length][];
                float pts = 100f / Answers.Length;
                for (int i = 0; i < Answers.Length; i++)
                {
                    m[i] = new float[Students.Count];
                    for (int j = 0; j < Students.Count; j++)
                        m[i][j] = (Students[j].Answers[i] == Answers[i]) ? pts : 0;
                }
                return m;
            }

            // Tek soru için ortalama puanı hesaplar
            public float GetAverageForQuestion(int q, float[][] m)
            {
                float acc = 0;
                for (int i = 0; i < Students.Count; i++)
                    acc += m[q][i];
                return acc / Students.Count;
            }

            // Tüm sorular için ortalama puanı hesaplar
            public float[] GetAveragesForQuestions(float[][] m)
            {
                var a = new float[Answers.Length];
                for (int i = 0; i < Answers.Length; i++)
                    a[i] = GetAverageForQuestion(i, m);
                return a;
            }

            public float[] GetPointSumsForQuestions(float[][] m)
            {
                var a = new float[Students.Count];
                for (int i = 0; i < Students.Count; i++)
                {
                    var acc = 0f;
                    for (int j = 0; j < Answers.Length; j++)
                        acc += m[j][i];
                    a[i] = acc;
                }
                return a;
            }

            // Tüm sorular için yüzdeleri hesaplar
            public float[] GetPercentageForQuestions(float[] avgs)
            {
                float pts = 100f / Answers.Length;
                var a = new float[Answers.Length];
                for (int i = 0; i < Answers.Length; i++)
                    a[i] = (avgs[i] / pts) * 100;
                return a;
            }

            // Birbirinden farklı kazanımları açığa çıkarır
            public void GetDistinctEarnings()
            {
                var lst = new Dictionary<(string, string), List<int>>();
                for (int i = 0; i < Earnings.Count; i++) // Question
                {
                    var q = Earnings[i];
                    for (int j = 0; j < q.Count; j++) // Earning
                    {
                        var e = Earnings[i][j];
                        if (!lst.ContainsKey(e))
                        {
                            var lst2 = new List<int>();
                            lst2.Add(i);
                            lst.Add(e, lst2);
                        }
                        else
                        {
                            var ee = lst[e];
                            if (!ee.Contains(i))
                                ee.Add(i);
                        }
                    }
                }
                // fix for issue where earning list gets a little confused
                var d = new Dictionary<(string, string), List<int>>();
                foreach (var e in lst.OrderBy((k1) => k1.Key))
                    d.Add(e.Key, e.Value);
                DisticntEarningList = d;
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
