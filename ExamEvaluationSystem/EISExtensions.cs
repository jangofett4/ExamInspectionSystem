﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public static class EISExtensions
    {
        public static string EncapsulateQuote(this string str)
        {
            return $"'{ str }'";
        }
    }
}
