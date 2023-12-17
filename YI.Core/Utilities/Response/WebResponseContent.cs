using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YI.Core.Enums;
using YI.Core.Extensions.Response;

namespace YI.Core.Utilities.Response
{
    public class WebResponseContent
    {
        public bool Status { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public WebResponseContent()
        {
        }
        public WebResponseContent(bool status)
        {
            this.Status = status;
        }

        public static WebResponseContent Instance
        {
            get { return new WebResponseContent(); }
        }

        public WebResponseContent Success()
        {
            this.Status = true;
            return this;
        }

        public WebResponseContent Success(string message = null, object data = null)
        {
            this.Status = true;
            this.Message = message;
            this.Data = data;
            return this;
        }

        public WebResponseContent Success(ResponseType responseType)
        {
            return this.Set(responseType, true);
        }

        public WebResponseContent Error(string msg = null)
        {
            this.Status = false;
            this.Message = msg;
            return this;
        }

        public WebResponseContent Error(ResponseType responseType)
        {
            return this.Set(responseType, false);
        }



        public WebResponseContent Set(ResponseType responseType, bool? status)
        {
            return this.Set(responseType, null, status);
        }
        public WebResponseContent Set(ResponseType responseType, string msg)
        {
            bool? b = null;
            return this.Set(responseType, msg, b);
        }
        public WebResponseContent Set(ResponseType responseType, string msg, bool? status)
        {
            if (status != null)
            {
                this.Status = (bool)status;
            }
            this.Code = ((int)responseType).ToString();
            if (!string.IsNullOrEmpty(msg))
            {
                Message = msg;
                return this;
            }
            Message = responseType.GetMsg();
            return this;
        }

    }
}
