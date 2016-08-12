using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Globalization;


namespace CalendarAnalyzer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void Stuff(string[] args)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Created", typeof(DateTime));
            dt.Columns.Add("UID");
            dt.Columns.Add("TotalDays", typeof(double));
            //Harvest data
            using (StreamReader sr = new StreamReader(args[0]))
            {
                DataRow dr = dt.NewRow();
                bool started = false;
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();
                    if (line.Length > 8)
                    {
                        if (line.Contains("BEGIN:VEVENT")) { dr = dt.NewRow(); started = true; }
                        if (started && line.Contains("DTSTART")) dr["Date"] = getDate(line);
                        if (started && line.Substring(0, 8) == "SUMMARY:") dr["Name"] = line.Substring(8);
                        if (started && line.Substring(0, 8) == "CREATED:") dr["Created"] = getDate(line);
                        if (started && line.Substring(0, 4) == "UID:") dr["UID"] = line.Substring(4);
                        if (started && line.Contains("END:VEVENT")) { dt.Rows.Add(dr); started = false; }
                    }
                }
            }
            //Compute
            foreach (DataRow dr in dt.Rows)
            {
                DateTime t0 = (DateTime)dr["Date"];
                DateTime t1 = (DateTime)dr["Created"];
                TimeSpan ts = (t0.Subtract(t1));
                dr["TotalDays"] = (ts.TotalDays);
            }
            //Report
            DataView dv = new DataView(dt);
            dv.Sort = "TotalDays DESC";
            for (int i = 0; i < 20; i++)
            {
                DataRow r = dv.ToTable().Rows[i];
                DateTime t0 = (DateTime)r["Date"];
                DateTime t1 = (DateTime)r["Created"];
                Console.WriteLine();
                Console.WriteLine("Name:       " + r["Name"].ToString());
                Console.WriteLine("Date:       " + t0.ToShortDateString());
                Console.WriteLine("Created:    " + t1.ToShortDateString());
                Console.WriteLine("Total Days: " + r["TotalDays"].ToString());
            }
            Console.ReadKey();
        }

        static DateTime getDate(string line)
        {
            DateTime rv = DateTime.Now;
            string format = "yyyyMMdd";
            IFormatProvider provider = System.Globalization.CultureInfo.InvariantCulture;
            DateTimeStyles style = DateTimeStyles.None;
            if (!DateTime.TryParseExact(line.Substring(line.Length - 16, 8), format, provider, style, out rv))
                if (!DateTime.TryParseExact(line.Substring(line.Length - 15, 8), format, provider, style, out rv))
                    if (!DateTime.TryParseExact(line.Substring(line.Length - 8), format, provider, style, out rv))
                        rv = DateTime.Now;
            return rv;
        }
    }
}

