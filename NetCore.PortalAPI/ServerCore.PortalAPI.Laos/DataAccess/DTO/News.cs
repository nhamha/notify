﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    public class News
    {
        public string Tittle { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public string Icon { get; set; }
    }
}
