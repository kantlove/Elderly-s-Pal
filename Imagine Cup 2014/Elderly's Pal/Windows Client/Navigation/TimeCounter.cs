using System;
using System.Diagnostics;
using System.IO;

namespace NiceDreamers.Windows.Navigation
{
    public class TimeCounter
    {
        private static TimeCounter instance;

        private TimeCounter()
        {
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");
        }

        public static TimeCounter Instance
        {
            get { return instance ?? (instance = new TimeCounter()); }
        }

        public int this[string name]
        {
            get { return Read(GetFileName(name)); }
            set { Write(GetFileName(name), value); }
        }

        private static string GetFileName(string name)
        {
            return "Data/" + name;
        }

        private static int Read(string fileName)
        {
            try
            {
                return Int32.Parse(File.ReadAllText(fileName));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static void Write(string fileName, int value)
        {
            try
            {
                using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(stream))
                    writer.Write(value.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}