﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EveryDayBlog.Web.ViewModels.AdminNavbars.ViewModels
{
    public class AdminNavbarViewModel
    {
        public int AllRequestsCount { get; set; }

        public int ReadedRequests { get; set; }

        public int UnReadedRequests { get; set; }
    }
}
