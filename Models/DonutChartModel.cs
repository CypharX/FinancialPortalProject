using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Models
{
    public class DonutChartModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Data { get; set; } = new List<decimal>();
        public List<string> BackgroundColor { get; set; } = new List<string>();
    }
}
