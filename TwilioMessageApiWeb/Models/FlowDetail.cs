using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwilioMessageApiWeb.Models
{
    public class Flowmodel
    {
        public Flowmodel()
        {
            flowdetail = new List<FlowDetail>();
        }
        public List<FlowDetail> flowdetail { get; set; }
    }
    public class FlowDetail
    {
        public string MessageID { get; set; }
        public string PhoneNumber { get; set; }
        public string FlowID { get; set; }
        public string FlowName { get; set; }
        public string WidgetName { get; set; }
    }
}