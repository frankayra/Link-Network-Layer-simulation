using System;
using System.Collections.Generic;
using System.Text;

namespace Link_layer
{
    public abstract class Dervice
    {
        #region Constructor y propiedades
        public string Txt_path;
        public readonly string name;
        public Value value;
        public Dervice(int ports, string name)
        {
            Adj = new Dervice[ports];
            this.name = name;
        }

        public Dervice[] Adj;
        #endregion


        /// <summary>
        /// Concat unilateral
        /// </summary>
        /// <param name="verified">Si esta verificado que se puede realizar la conexion entre los dos nodos</param>
        private bool AddConection(Dervice dervice, int port, bool verified = true)
        {
            if (!verified && (port >= Adj.Length)) return false;
            if (Adj[port] != null) Disconect(port);
            Adj[port] = dervice;
            return true;
        }
        /// <summary>
        /// Concat de toa la vida.
        /// </summary>
        public static bool cc(Dervice a, Dervice b, int a_port, int b_port)
        {
            if (a_port >= a.Adj.Length /* || a.Adj[a_port] != null */  ||
                b_port >= b.Adj.Length /* || b.Adj[b_port] != null */ ) return false;
            return a.AddConection(b, a_port) && b.AddConection(a, b_port);
        }

        public bool Disconect(int port, bool unilateral = false)
        {
            if (port >= Adj.Length) return false;
            Dervice dervice_conected = Adj[port];
            Adj[port] = null;
            if (unilateral || dervice_conected == null) return true;

            return dervice_conected.Disconect(Array.IndexOf(dervice_conected.Adj, this), true);
        }

        public virtual void Recive(Value value, int transmiting_dervices, int port = 0) { }
        public virtual Value Emit(int port = 0) { return value; }

        public override string ToString() => name;
    }
}
