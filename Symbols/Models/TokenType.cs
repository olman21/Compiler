﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symbols.Models
{
    public enum TokenType
    {
        primitive,
        arithmetic,
        assigment,
        comparation,
        logic,
        conditional,
        loop,
        delimiter
    }
}
