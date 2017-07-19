using System;
using System.Threading;

namespace Blockchain.Altruism
{
    class Program
    {
        static void Main(string[] args)
        {
            string ownerAdr = "";
            string ownerPwd = "";
            string contractAdr = "";

            AltruismManager manager = new AltruismManager(ownerAdr, ownerPwd, contractAdr);
            manager.Connect("http://127.0.0.1:8545");
            Thread trd = new Thread(new ThreadStart(manager.Subscribe));
            trd.Start();
            Console.ReadLine();
        }
    }
}