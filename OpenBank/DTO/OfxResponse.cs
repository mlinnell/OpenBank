﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBank
{
    public class OfxResponse
    {
        public StatementResponse Statement { get; set; }
        public AccountResponse Account { get; set; }
    }
}