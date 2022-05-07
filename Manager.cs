using System;
using System.Collections.Generic;
using System.Text;

namespace Link_layer
{
    public static class Manager
    {
        public static Criterium Error_Protocol;
        public static int SIGNAL_TIME { get; private set; }
        public static Dictionary<string, Dervice> DervicesNames = new Dictionary<string, Dervice>();
        public static DisjoinSets<Dervice> Dervices;

        private static void StructureD_S(List<Dictionary<Dervice, bool>> CCs, Dervice[] dervices)
        {
            Dervices = new DisjoinSets<Dervice>(dervices);
            foreach (var dicc in CCs)
            {
                Dervices.UpdateMerge((d1, d2) => (dicc.ContainsKey(d1) && dicc.ContainsKey(d2) &&                 // Funcion Lambda que permite mezclar
                                                             dicc[d1] && dicc[d2]));                                                                           // si pertenecen a la misma componente conexa
            }
        }


        public static void Initialize(List<Dervice> Static_dervices, int signal_time, bool Idle = false)
        {
            SIGNAL_TIME = signal_time;
            List<Dictionary<Dervice, bool>> CCs = new List<Dictionary<Dervice, bool>>(); // Quienes pertenecen a la misma CC.
            foreach (Dervice current_d in Static_dervices)
            {
                if (current_d is Switch) continue;
                bool explored = false;
                foreach (var dicc in CCs)
                {
                    if (dicc.ContainsKey(current_d))
                    {
                        explored = true;
                        break;
                    }
                }
                if (explored) continue;
                Dictionary<Dervice, bool> current_d_CC = new Dictionary<Dervice, bool>();
                current_d_CC[current_d] = true;
                DFS(current_d, current_d_CC);
                CCs.Add(current_d_CC);
            }

            StructureD_S(CCs, Static_dervices.ToArray());
        }

        /// <summary>
        /// Itera por todos los los dispositivos Host's
        /// </summary>
        public static void Send()
        {

            if (!(Dervices is null)) Dervices.CleanChannels();
            Dictionary<Dervice, int[]> DynamicUse = new Dictionary<Dervice, int[]>();
            foreach (Dervice dervice in Dervices.ValuesSet)
            {
                if(dervice is Switch)
                {
                    #region Se hace recibir a cada dispositivo adyacente al switch, lo que emite el switch en ese puerto.
                    for (int i = 0; i < dervice.Adj.Length; i++)
                    {
                        Dervice adj = dervice.Adj[i];
                        if (adj is null) continue;
                        adj.Recive(dervice.Emit(i), 1, Array.IndexOf(adj.Adj, dervice));
                    }
                    #endregion
                }
            }
            foreach (Dervice dervice in Dervices.ValuesSet)
            {
                if (dervice is Host)
                {
                    #region Si dervice emite algo sumamos su valor a la cantidad de ceros o unos emitiendose en la CC.
                    Value emited = dervice.Emit();
                    Dervice conected = dervice.Adj[0];
                    if(conected != null) conected.Recive(emited, 1, Array.IndexOf(conected.Adj, dervice));
                    if (emited != Value.UNACTIVE)
                    {
                        Dervice d = Dervices.ClassRepresentantOf(dervice).Value;
                        if(!DynamicUse.ContainsKey(d))
                        {
                            DynamicUse[d] = new int[2];
                        }
                        DynamicUse[d][(int)emited - 1]++;                                                        // verificar
                    }
                    #endregion
                }
            }
            foreach (Dervice d in Dervices.ValuesSet)
            {
                if (d is Hub)
                {
                    #region Se coge todo lo que esta conectado a el, que son solo Hosts y Switches, y se le hace a cada uno, Recive() con el XOR de los n-1 otros dispositivos transmisores de su CC.
                    Dervice d_class = Dervices.ClassRepresentantOf(d).Value;
                    for (int i = 0; i < d.Adj.Length; i++)
                    {
                        if (d.Adj[i] is null) continue;
                        Dervice current_dervice = d.Adj[i];
                        Value current_dervice_XOR_Emit = !DynamicUse.ContainsKey(d_class) ? Value.UNACTIVE : XOR_PA_TRAS(d_class, ((Host)d.Adj[i]).ValueEmited);

                        current_dervice.Recive(current_dervice_XOR_Emit, DynamicUse.ContainsKey(d_class) ? DynamicUse[d_class][0] + DynamicUse[d_class][1] : 0, i);
                    }
                    #endregion
                }
            }
            #region XOR pa tras
            Value XOR_PA_TRAS(Dervice d_class, Value value_emited)
            {
                if (!DynamicUse.ContainsKey(d_class)) return Value.UNACTIVE;


                int ceros = DynamicUse[d_class][0];
                int unos = DynamicUse[d_class][1];

                Value general_XOR = unos != 0 ? unos % 2 == 1 ? Value.ONE : Value.ZERO : ceros != 0 ? Value.ZERO : Value.UNACTIVE;
                if (value_emited == Value.UNACTIVE) return general_XOR;
                return value_emited == Value.ZERO ?
                    ceros + unos == 1 ?
                    Value.UNACTIVE :
                    general_XOR :
                    unos == 1 ?
                    ceros != 0 ?
                    Value.ZERO :
                    Value.UNACTIVE :
                    unos % 2 == 0 ?
                    Value.ONE :
                    Value.ZERO;
            }
            #endregion
        }


        public static Value XOR(Value a, Value b)
        {
            return a == Value.UNACTIVE ? b :
                b == Value.UNACTIVE ? a :
                a == Value.ONE ?
                b == Value.ONE ?
                Value.ZERO :
                Value.ONE :
                b;
        }

        /// <summary>
        /// DFS clasico, pero usa mask para dejar al final de la ejecucion, almacenados los dispositivos de la componente conexa de dervice.
        /// </summary>
        public static void DFS(Dervice dervice, Dictionary<Dervice, bool> mask, Action<Dervice> action = null)
        {
            if (mask == null) throw new ArgumentException("el diccionario de la CC es vacio");
            for (int i = 0; i < dervice.Adj.Length; i++)
            {
                if (dervice.Adj[i] is null) continue;
                if (mask.ContainsKey(dervice.Adj[i])) continue;                          // Si el nodo esta en el diccionario, ya es true su valor, por la forma en que los agrego

                mask[dervice.Adj[i]] = true;
                action?.Invoke(dervice.Adj[i]);
                if (dervice.Adj[i] is Switch) continue;                                          // Si es switch se agrega a la CC, pero no se lanza DFS desde el.
                DFS(dervice.Adj[i], mask);
            }
        }


        #region Utiles

        public static string ConverFromHexadecimal(string hex_n)
        {
            int dec_n = 0;
            for (int k = 0; k < hex_n.Length; k++)
            {
                int c_in_base16 = (int)(hex_n[k]) - 48;
                dec_n += c_in_base16 * (int)Math.Pow(16, hex_n.Length - k - 1);
            }
            string bin_n = "";
            while(dec_n != 0)
            {
                bin_n = (dec_n % 2) + bin_n;
                dec_n /= 2;
            }
            return bin_n;
        }

        #endregion
    }
}
