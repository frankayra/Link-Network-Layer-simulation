﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Link_layer
{

    public enum Value { UNACTIVE, ZERO, ONE }

    public static class FlowControler
    {
        private static Random random = new Random();
        public static double RandomDouble => random.NextDouble();

        public static int Turn = 0;
        public static int Commands_max_turn = 0;

        public static Dictionary<int, Queue<string[]>> Commands = new Dictionary<int, Queue<string[]>>();
        public static Queue<string> PrintQueue = new Queue<string>();
        public static string SCRIPT_PATH = "script.txt";
        public static string CONFIG_PATH = "config.txt";
        public static int TemporalSIgnalTime = 0;

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
            if (line_splited.Length < 2) throw new Exception("Error de config.txt");
            if (!(line_splited[0] == "signal_time" && int.TryParse(line_splited[1], out TemporalSIgnalTime)))
                throw new Exception("Error de config.txt");
        }

        public static bool MoveNext(bool force_next = false)
        {
            Dervice a;
            if (!(Manager.Dervices is null)) a = Manager.Dervices.ValuesSet[0];
            if (Commands_max_turn <= Turn && !force_next) return false;
            if (Commands.ContainsKey(Turn))
            {
                for (string[] command_; Commands[Turn].Count > 0;)
                {
                    command_ = Commands[Turn].Dequeue();
                    TextParser.IsCorrectCommand(command_, execute: true);
                }
            }
            Manager.Send();
            Console.WriteLine($"Turno {Turn} terminado");
            Turn++;
            return true;
        }

    }
    class Program
    {
        static void Main(string[] args)
        {

            FlowControler.ReadTxt(FlowControler.SCRIPT_PATH);
            FlowControler.ReadConfig(FlowControler.CONFIG_PATH);
            //bool simulating = false;
            //int extra_time = 0;
            string console_ = "";
            //while (simulating || (console_ = Console.ReadLine()) != "exit")
            //{
            //    if (console_ == "simulate") simulating = true;

            //    if (!FlowControler.MoveNext())
            //    {
            //        if(extra_time <= 0)
            //        {
            //            Console.Write("Cuantos turnos extras necesita? \n >>> ");
            //            if (!int.TryParse(Console.ReadLine(), out extra_time) || extra_time <= 0) break;
            //            continue;
            //        }
            //        extra_time--;
            //    }
            //}
            while ((console_ = Console.ReadLine()) != "exit")
            {
                int simulate_turns = 0;

                if (int.TryParse(console_, out simulate_turns))
                {
                    while (simulate_turns > 0)
                    {
                        FlowControler.MoveNext();
                        simulate_turns--;
                    }
                }
                else
                {
                    FlowControler.MoveNext();
                }
            }
        }
    }
}
