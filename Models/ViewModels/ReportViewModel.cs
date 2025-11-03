using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BikeStores_HW03.Models.ViewModels
{
    public class ReportViewModel
    {
        public string ReportTitle { get; set; }

        public List<string> Labels { get; set; }
        public List<int> Values { get; set; }

        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Description { get; set; }

        public List<string> SavedReports { get; set; }

        public string ReportType { get; set; } //report
        public string ChartImageData { get; set; }
    }
}
