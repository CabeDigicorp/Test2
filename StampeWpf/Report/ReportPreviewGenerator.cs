using MasterDetailModel;
using Model;
using Model.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace StampeWpf
{
    public class ReportPreviewGenerator
    {
        public ReportDataSourceGenerator ReportDataSourceGenerator { get; set; }
        public ReportStructureGenerator ReportStructureGenerator { get; set; }

        //public const string DataSourceName = "DataSourceRpt";
        public ReportPreviewGenerator()
        {
           
        }
    }
}
