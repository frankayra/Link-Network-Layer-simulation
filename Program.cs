using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Link_layer
{
    public enum Interfacee { SIMPLE, BLOCKS }
    public enum Value { UNACTIVE, ZERO, ONE }

    public static class FlowControler
    {
        public static Interfacee View = Interfacee.SIMPLE;
        private static Random random = new Random();
        public static double RandomDouble => random.NextDouble();

        public static int Turn = 0;
        public static int Commands_max_turn = 0;

        public static Dictionary<int, Queue<string[]>> Commands = new Dictionary<int, Queue<string[]>>();
        public static Queue<string> PrintQueue = new Queue<string>();
        public static string SCRIPT_PATH = "script.txt";
        public static string CONFIG_PATH = "config.txt";
        public static int TemporalSIgnalTime = 0;

        public static Criterium Protocol;

        public static void ReadTxt(string path)
        {
            if (File.Exists(path)) SCRIPT_PATH = path;

            StreamReader sr = new StreamReader(SCRIPT_PATH);


            for (string line; (line = sr.ReadLine()) != null;)
            {
                string[] line_splited = line.Split();
                int command_turn;
                if (TextParser.IsCorrectCommand(line_splited))
                {
                    command_turn = int.Parse(line_splited[0]) + Turn;
                    Commands_max_turn = Math.Max(command_turn, Commands_max_turn);
                }
                else command_turn = -1;

                if (!Commands.ContainsKey(command_turn) || Commands[command_turn] is null)
                {
                    Commands[command_turn] = new Queue<string[]>();
                }
                Commands[command_turn].Enqueue(line_splited);
            }
        }

        public static void ReadConfig(string path)
        {
            if (File.Exists(path)) CONFIG_PATH = path;

            StreamReader sr = new StreamReader(CONFIG_PATH);
            string line = sr.ReadLine();
            string[] line_splited = line.Split();
            bool error = true;
            if (line_splited.Length < 2) throw new Exception("Error de config.txt");
            if (line_splited[0] == "signal_time" && int.TryParse(line_splited[1], out TemporalSIgnalTime))
            {
                string[] line2_splited = sr.ReadLine().Split();
                if(line2_splited[0] == "error_detection")
                {
                    switch (line2_splited[1])
                    {
                        case "TwoDimParity":
                            FlowControler.Protocol = new TwoDimensionalParity();
                            break;
                        default:
                            throw new Exception("Error en config.txt");
                    }
                    error = false;
                }
            }
            if(error) throw new Exception("Error en config.txt");
        }

        public static bool MoveNext(bool force_next = false)
        {
            Dervice a;
            if (!(Manager.Dervices is null)) a = Manager.Dervices.ValuesSet[0];
            //if (Commands_max_turn <= Turn && !force_next) return false;
            if (Commands.ContainsKey(Turn))
            {
                for (string[] command_; Commands[Turn].Count > 0;)
                {
                    command_ = Commands[Turn].Dequeue();
                    TextParser.IsCorrectCommand(command_, execute: true);
                }
            }
            Manager.Send();
            Turn++;
            return true;
        }

    }
    static class Program
    {
        public static char ToChar(this Value value)
        {
            char c = '-';
            switch (value)
            {
                case Value.ZERO:
                    c = '0';
                    break;
                case Value.ONE:
                    c = '1';
                    break;              
            }
            return c;
        }
        static void Main(string[] args)
        {
            FlowControler.ReadTxt(FlowControler.SCRIPT_PATH);
            FlowControler.ReadConfig(FlowControler.CONFIG_PATH);

            int automatic_turns = 0;
            int simulation_time = 100;
            string readline = "";
            while((automatic_turns > 0 || ((readline = Console.ReadLine()) != "exit"))  &&  FlowControler.MoveNext())
            {
                if (automatic_turns > 0)
                {
                    automatic_turns--;
                    Thread.Sleep(simulation_time);
                }
                if (readline == "auto") automatic_turns = 100;

                #region Print Disjoin Sets
                foreach (var par in Manager.Dervices.Classes)
                {
                    Console.WriteLine("CC: |" + par.Key + "|" +
                                    " | Corrects Frames: " + ((Host)par.Key).FramesRecived.Count +
                                    " | Value Emited: " + ((Host)par.Key).ValueEmited +
                                    " | Value Recived: " + ((Host)par.Key).ValueRecived +
                                    " | Complete: " + ((Host)par.Key).currentFrame.ToHex());
                    Console.WriteLine("-------");
                    foreach (Dervice dervice in Manager.Dervices.ValuesSet)
                    {
                        for (int i = 0; i < dervice.Adj.Length; i++)
                        {
                            if (dervice.Adj[i] != null && (!Manager.Dervices.ClassRepresentantOf(dervice.Adj[i]).Value.Equals(dervice)) && Manager.Dervices.ClassRepresentantOf(dervice.Adj[i]).Value.Equals(par.Key))
                            {
                                switch (FlowControler.View)
                                {
                                    case Interfacee.SIMPLE:
                                        Console.Write("|---> " + dervice);
                                        if (dervice is Host)
                                        {
                                            Console.Write("|| Corrects Frames: " + ((Host)dervice).FramesRecived.Count);
                                            Console.Write("|| Value Emited: " + ((Host)dervice).ValueEmited);
                                            Console.Write("|| Value Recived: " + ((Host)dervice).ValueRecived);
                                            Console.WriteLine("|| Complete: " + ((Host)dervice).currentFrame.ToHex());
                                            Console.WriteLine("|| ----");
                                        }
                                        else Console.WriteLine("\n|| ----");
                                        break;
                                    case Interfacee.BLOCKS:
                                        Console.WriteLine("|---> " + dervice);
                                        if (dervice is Host)
                                        {
                                            Console.WriteLine("|| Corrects Frames: " + ((Host)dervice).FramesRecived.Count);
                                            Console.WriteLine("|| Value Emited: " + ((Host)dervice).ValueEmited);
                                            Console.WriteLine("|| Value Recived: " + ((Host)dervice).ValueRecived);
                                            Console.WriteLine("|| Complete: " + ((Host)dervice).currentFrame.ToHex());
                                            Console.WriteLine("|| ----");
                                        }
                                        break;
                                }
                                break;
                            }
                        }
                    }
                }
                Console.WriteLine($"Turno {FlowControler.Turn} terminado");
                Console.WriteLine("\n\n");

                #endregion


            }


        }

        public static Value ToValue(this bool b)
        {
            return b ? Value.ONE : Value.ZERO;
        }
        
    }
}
