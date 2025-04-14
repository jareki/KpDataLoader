﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpDataLoader.ProbabilityFactory
{
    public class ImplementProbabilityType
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public double Probability { get; set; }
        public double Treshold { get; set; }
    }
}
