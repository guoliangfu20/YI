using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YI.Entity.DomainModel.flow;

namespace YI.Core.WorkFlow
{
    public class WorkFlowTableOptions : Sys_WorkFlow
    {
        public List<FilterOptions> FilterList { get; set; }
    }

    public class FilterOptions : Sys_WorkFlowStep
    {
        public List<FieldFilter> FieldFilters { get; set; }

        public object Expression { get; set; }

        public string[] ParentIds { get; set; }


    }

}
