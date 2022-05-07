using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            Console.WriteLine($"Turno {Turn} terminado");
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
            #region Prueba 1
            //Frame f = new Frame("AABBCCDD");
            //Console.WriteLine(2.ToString("X"));

            //Frame enc = new Frame();
            //Frame dec;

            //Console.WriteLine(enc.ToHex());
            //bool b1, b2;
            //Frame fmal = new Frame("AABBCCDD0202E42CC80D");
            //dec = tsp.Decrypt_andTryToFixFrame(fmal, was_fixed: out b1, correct_frame: out b2);
            //Console.WriteLine(dec.ToHex());
            //Console.WriteLine("Arreglado: " + b1);
            //Console.WriteLine("Correcto: " + b2);

            //Console.ReadKey();
            #endregion

            List<Dervice> dervices = new List<Dervice>();
            #region Prueba 2
            //dervices.Add(new Host("PC1"));
            //dervices.Add(new Host("PC2"));
            //dervices.Add(new Host("PC3"));
            //dervices.Add(new Host("PC4"));
            //dervices.Add(new Host("PC5"));
            //dervices.Add(new Hub(3, "h1"));
            //dervices.Add(new Hub(3, "h2"));
            //dervices.Add(new Hub(3, "h3"));
            //dervices.Add(new Switch(3, "S1"));
            //dervices.Add(new Switch(3, "S2"));

            //Dervice.cc(dervices[0], dervices[1], 0, 0);
            //Dervice.cc(dervices[8], dervices[2], 0 , 0);
            //Dervice.cc(dervices[8], dervices[3], 1, 0);
            #endregion

            #region Prueba 3

            dervices.Add(new Host("PC1"));
            dervices.Add(new Host("PC2"));
            dervices.Add(new Host("PC3"));

            dervices.Add(new Hub(3, "h"));
            dervices.Add(new Switch(3, "S1"));

            //////////////////////////////////////////////////////////////////////////////

            Dervice.cc(dervices[0], dervices[4], 0, 0);
            Dervice.cc(dervices[4], dervices[1], 1, 0);
            Dervice.cc(dervices[4], dervices[2], 2, 0);

            ((Host)dervices[0]).MAC = "AAAA";
            ((Host)dervices[1]).MAC = "BBBB";                             //
            ((Host)dervices[2]).MAC = "CCCC";

            ((Host)dervices[0]).Send(new Frame("CCCCAAAA02023AF8C20D"));           // CCCC--AAAA--0202--3AF8--C2--0D
            //((Host)dervices[2]).Send(new Frame("CCDDAABB0202F3B1420C"));
            //((Host)dervices[3]).Send(new Frame("AABBEEFF0202A580250D"));

            #endregion

            Manager.Initialize(dervices, 1);
            DisjoinSets<Dervice>.Print(Manager.Dervices);
            int i = 0;
            while (true)
            {

                FlowControler.MoveNext();
                foreach (var disp in Manager.Dervices.ValuesSet)
                {
                    if(disp is Host)Console.WriteLine(disp.name + ": ----> cf: " + ((Host)disp).FramesRecived.Count +
                        "   value_emited: " + ((Host)disp).ValueEmited + "   value_recived: " + ((Host)disp).ValueRecived +
                        " Complete: " + ((Host)disp).currentFrame.ToHex());
                }
                Console.WriteLine(i);
                Console.ReadKey();
                i++;
            }
        }

        public static Value ToValue(this bool b)
        {
            return b ? Value.ONE : Value.ZERO;
            TwoDimensionalParity tsp = new TwoDimensionalParity();


        }
        
    }
}
