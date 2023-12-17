using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YI.Core.Configuration
{
    public abstract class TableDefaultColumns
    {
        public string UserIdField { get; set; }
        public string UserNameField { get; set; }
        public string DateField { get; set; }
    }

    public class CreateMember : TableDefaultColumns
    {

    }

    public class ModifyMember : TableDefaultColumns
    {

    }
}
