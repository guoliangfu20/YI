using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YI.Core.WorkFlow
{
    public enum AuditBack
    {
        流程结束 = 0,
        返回上一节点 = 1,
        流程重新开始 = 2
    }
}
