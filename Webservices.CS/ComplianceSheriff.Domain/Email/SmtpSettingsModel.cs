﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Email
{
    public class SmtpSettingsModel
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string From { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
