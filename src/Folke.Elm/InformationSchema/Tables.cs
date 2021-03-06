﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Folke.Elm.InformationSchema
{
    [Table("TABLES", Schema = "INFORMATION_SCHEMA")]
    internal class Tables
    {
        [Column("TABLE_NAME")]
        public string Name { get; set; }
        [Column("TABLE_SCHEMA")]
        public string Schema { get; set; }
    }
}
