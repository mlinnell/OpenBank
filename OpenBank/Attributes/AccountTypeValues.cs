﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBank
{
    public class AccountTypeValues : ParameterValues
    {
		public AccountTypeValues()
            : base(string.Join(", ", Enum.GetNames(typeof(OfxData.OfxAccountType))))
        {
        }
    }
}
