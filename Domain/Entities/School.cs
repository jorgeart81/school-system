﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class School
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime EstablishedDate { get; set; }
    }
}
