using System;
using VacStatus.Local;

namespace VacStatus
{
    class Program
    {
        static void Main(string[] args)
        {
            var cnf = new Configuration();
            cnf.ConfigureJsonAsync().GetAwaiter().GetResult();

            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
