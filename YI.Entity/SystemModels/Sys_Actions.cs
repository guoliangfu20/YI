using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YI.Entity.SystemModels
{
    public class Sys_Actions
    {
        [Key]
        public int Action_Id { get; set; }
        public int Menu_Id { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
