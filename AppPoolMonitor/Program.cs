using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;
using System.Diagnostics;
using System.Threading;

namespace AppPoolMonitor
{
    class Program
    {
        static int intCounter = 0;
        static List<DateTime> lstDT = new List<DateTime>();
        static void RestartAppPools()
        {
            ServerManager objSvrMgr = new ServerManager();
            ApplicationPoolCollection objAppPoolCollectn = objSvrMgr.ApplicationPools;
            WorkerProcessCollection objW3WPCollectn = objSvrMgr.WorkerProcesses;
            foreach (ApplicationPool appPool in objAppPoolCollectn)
            {
                System.IO.File.AppendAllText(System.Environment.CurrentDirectory + "\\Logs.txt",
                    "\n" + DateTime.Now.AddMinutes(630).ToString("yyyy-MMM-dd HH:mm:ss")
                    + " App pool restart! Name => " + appPool.Name
                    + "; State => " + appPool.State);

                Console.WriteLine("Name => " + appPool.Name + "; State => " + appPool.State);
                appPool.Start();
            }
        }

        static void Main(string[] args)
        {
            try
            {
                while (true)
                {
                    ServerManager objSvrMgr = new ServerManager();
                    ApplicationPoolCollection objAppPoolCollectn = objSvrMgr.ApplicationPools;
                    WorkerProcessCollection objW3WPCollectn = objSvrMgr.WorkerProcesses;
                    foreach (ApplicationPool appPool in objAppPoolCollectn)
                    {
                        System.IO.File.AppendAllText(System.Environment.CurrentDirectory + "\\Logs.txt",
                        "\n" + DateTime.Now.AddMinutes(630).ToString("yyyy-MMM-dd HH:mm:ss") + " Name => " + appPool.Name + "; State => " + appPool.State);

                        Console.WriteLine("Name => " + appPool.Name + "; State => " + appPool.State);
                        if (appPool.State == ObjectState.Stopped)
                        {
                            appPool.Start();
                        }
                    }

                    PerformanceCounterCategory a = new PerformanceCounterCategory("Process");

                    var w3wpInstances = a.GetInstanceNames().Where(delegate(string s) { return s.Contains("w3wp"); });

                    foreach (string instanceName in w3wpInstances) //(Process process in Process.GetProcessesByName("w3wp"))
                    {
                        try
                        {
                            PerformanceCounter obj = new PerformanceCounter("Process", "% Processor Time", instanceName);
                            var c = a.GetCounters(instanceName);
                            float p = obj.NextValue();
                            Thread.Sleep(1000);
                            p = obj.NextValue();

                            if (Convert.ToInt32(Math.Ceiling(p)) == 90)
                            {
                                lstDT.Add(DateTime.Now.AddMinutes(630));
                                if (lstDT.Count >= 10)
                                {
                                    CheckErrorRate();
                                }
                            }

                            System.IO.File.AppendAllText(System.Environment.CurrentDirectory + "\\Logs.txt",
                                "\n" + DateTime.Now.AddMinutes(630).ToString("yyyy-MMM-dd HH:mm:ss") + " Instance name = " + instanceName + "; CPU Usage = " + p.ToString());

                            Console.WriteLine("\nInstance name = " + instanceName + "; CPU Usage = " + p.ToString());
                        }
                        catch (Exception ex)
                        {
                            System.IO.File.AppendAllText(System.Environment.CurrentDirectory + "\\Logs.txt",
                                "\n" + DateTime.Now.AddMinutes(630).ToString("yyyy-MMM-dd HH:mm:ss") + " Instance name = " + instanceName + "; Message = " + ex.Message + "; Trace = " + ex.StackTrace);

                            Console.WriteLine("\nInstance name = " + instanceName + ex.Message + "; " + ex.StackTrace);
                        }
                    }
                    System.IO.File.AppendAllText(System.Environment.CurrentDirectory + "\\Logs.txt",
                        "\n");

                    Thread.Sleep(60000);
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(System.Environment.CurrentDirectory + "\\Logs.txt",
                    "\n" + DateTime.Now.AddMinutes(630).ToString("yyyy-MMM-dd HH:mm:ss") + " Message = " + ex.Message + "; Trace = " + ex.StackTrace);
                Console.WriteLine("\nMessage = " + ex.Message + "; Trace = " + ex.StackTrace);
            }
            Console.ReadLine();
        }

        private static void CheckErrorRate()
        {
            int intThreshold = (lstDT[lstDT.Count - 1] - lstDT[0]).Minutes;
            if (intThreshold <= 10)
            {
                RestartAppPools();
            }
            lstDT.Clear();
        }
    }
}
