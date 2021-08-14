using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace Kill_Long_Processes
{
    class Program
    {
        public static string process = "notepad"; //название процесса по умолчанию
        public static string lifeTime = "1"; //время жизни процесса в мин по умолчанию
        public static string repeatTime = "1"; //время повторной проверки в мин по умолчанию
        public static Process[] proc; // список процессов
        public static List<Process> myProc = new List<Process>();

        static int index(Process p) //индекс удаляемого процесса из списка по названию процесса
        {
            int i = 0;
            foreach (Process x in myProc) 
            {
                if (x.Id == p.Id) 
                { 
                    return i;  
                }
                i++;
            }
            return 0;
        }

        static double toLifeMin(DateTime dBegin, DateTime dEnd) //время текущей работы процесса в мин
        {
            return ((dEnd.Hour * 60 + dEnd.Minute + (double)dEnd.Second/60) - (dBegin.Hour * 60 + dBegin.Minute + (double)dBegin.Second / 60));
        }

        static async void Watch(Process root) //асинхронное слежение за процессом
        {
            Console.WriteLine("process with id {0} start in {1}",root.Id, root.StartTime);
            await Task.Run(()=> watchProc(root));
        }

        public static void watchProc(Process root) // проверка времени жизни процесса и ведение лога
        {
            double time;
            while (toLifeMin(root.StartTime, DateTime.Now) <= Convert.ToInt32(lifeTime))
            {
                Thread.Sleep(Convert.ToInt32(repeatTime) * 60000);
            }
            time = toLifeMin(root.StartTime, DateTime.Now);
            if (!root.HasExited)
            {
                root.Kill();
                Console.WriteLine("process with id {0} was killed. It takes {1} mins", root.Id, time.ToString("0.00"));
            }
            else { Console.WriteLine("process with id {0} was killed by user", root.Id); }
            
            myProc.RemoveAt(index(root));
            Thread.CurrentThread.Abort();

        }
       
        static void Main(string[] args)
        {
            try
            {
                process = args[0];
                lifeTime = args[1];
                repeatTime = args[2];

                while (true)
                {
                    int equal = 0;
                    proc = Process.GetProcessesByName(process.ToLower());
                    if (proc.Length == 0) 
                    {
                        Console.WriteLine("process not found"); 
                    }
                    foreach (Process pr in proc)
                    {
                        if (myProc.Count == 0)
                        {
                            myProc.Add(pr); Watch(pr);

                        }
                        foreach (Process p in myProc)
                        {
                            if (p.Id == pr.Id) equal++;
                        }
                        if (equal == 0) 
                        { 
                            myProc.Add(pr); Watch(pr); 
                        }
                        equal = 0;
                    }
                    Thread.Sleep(Convert.ToInt32(repeatTime) * 60100);
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
