using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WaterMarker
{
    public partial class Form1 : Form
    {
        // Steps to go
        // -----------
        // 1 - Search for old report.
        // 2 - Convert .doc to odt.
        // 3 - Decompress report.
        // 4 - Add values and content.
        // 5 - Compress report on odt file.
        // 6 - Print report.
        //
        string globalReportPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void SearchReport(string UNC, int AccessionNumber)
        {
            string reportPath = izzilib.CDM.Ambulatorio.AccessionNumberToPath(UNC, AccessionNumber, null);
            if (File.Exists(reportPath))
            {
                cbFound.Checked = true;
                globalReportPath = reportPath;
            }
            else
            {
                cbFound.Checked = false;
                globalReportPath = "";
            }
        }

        private void InsertWaterMarkToDocFile()
        {
            // 2 - Convert .doc to odt.
            // soffice --headless --convert-to odt --outdir documents/ *.doc <- Useless!!! Generates an XML Only file.
            // soffice --headless --convert-to writer8 --outdir documents/ *.doc <- Use this one, take care about fucking .writer8 extension...

            // 3 - Decompress report.
            // 4 - Add values and content.
            // 5 - Compress report on odt file.
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            int AccessionNumber;
            if (int.TryParse(lblAccessionNumber.Text, out AccessionNumber))
            {
                SearchReport(Properties.Settings.Default.UNC, AccessionNumber);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {

        }
    }
}
