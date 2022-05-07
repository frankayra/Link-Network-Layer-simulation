using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link_layer
{
    public class Switch : Dervice
    {
        #region Constructor and Properties
        
        List<Value> frame = new List<Value>();
        Dictionary<string, List<(int port, int dead_turn)>> Memo = new Dictionary<string, List<(int port, int dead_turn)>>();                                            //
        List<Queue<List<Value>>> PortsQueues = new List<Queue<List<Value>>>();                  //
        public Switch(int ports, string name) : base(ports, name) {

            inbox = new Frame[ports];
            outbox = new IEnumerator<Value>[ports];
            valueEmited = new Value[ports];
            valueRecived = new Value[ports];
            queueInbox = new Queue<Frame>[ports];
            queueOutbox = new Queue<Frame>[ports];
            stCountInbox = new int[ports];
            stCountOutbox = new int[ports];
            emiting = new bool[ports];
            receiving =  new bool[ports];
            for (int i = 0; i<ports ; i++){
                inbox[i] = new Frame();
                valueEmited[i] = Value.UNACTIVE;
                valueRecived[i] = Value.UNACTIVE;
                queueInbox[i] = new Queue<Frame>();
                queueOutbox[i] = new Queue<Frame>();
            }
        }
        
        #endregion

        #region Propiedades Lachi

        private Frame[] inbox;
        private IEnumerator<Value>[] outbox;
        private Value[] valueEmited;
        private Value[] valueRecived;
        public Value ValueEmited(int port) => valueEmited[port];
        private int[] stCountInbox, stCountOutbox;
        public Queue<Frame>[] queueInbox, queueOutbox;
        private bool[] emiting;
        private bool[] receiving;

        #endregion 

        public override void Recive(Value value, int transmiting_dervices, int port = 0)
        {
            base.Recive(value, transmiting_dervices, port);

            if (receiving[port]){
                if(stCountInbox[port] > 0){
                    if (value != valueRecived[port]){
                        // Alteración en el mensaje recibido
                        if (inbox[port].IsComplete()){
                            bool fix, correct;
                            FlowControler.Protocol.Decrypt_andTryToFixFrame(inbox[port], out fix, out correct);

                            AddFrameToQueue(inbox[port],port);
                            UpdateMAC(inbox[port].SourceMAC, port, FlowControler.Turn);
                            inbox[port] = new Frame();
                            if (value != Value.UNACTIVE){
                                inbox[port].AddBit(value);
                                valueRecived[port] = value;
                                stCountInbox[port] = Manager.SIGNAL_TIME-1;
                            }else{
                                receiving[port] = false;
                            }
                        }
                    }else{
                        stCountInbox[port]--;
                    }
                }else{
                    // FrameCompletado
                    if (inbox[port].IsComplete()){
                        bool fix, correct;
                        FlowControler.Protocol.Decrypt_andTryToFixFrame(inbox[port], out fix, out correct);

                        AddFrameToQueue(inbox[port],port);
                        UpdateMAC(inbox[port].SourceMAC, port, FlowControler.Turn);
                        inbox[port] = new Frame();
                        if (value != Value.UNACTIVE){
                            inbox[port].AddBit(value);
                            valueRecived[port] = value;
                            stCountInbox[port] = Manager.SIGNAL_TIME-1;
                        }else{
                            receiving[port] = false;
                        }
                    } else{
                        stCountInbox[port] = Manager.SIGNAL_TIME-1;
                        inbox[port].AddBit(value);
                        valueRecived[port] = value;    
                    }
                }
            } else{
                if (value != Value.UNACTIVE){
                    inbox[port].AddBit(value);
                    valueRecived[port] = value;
                    stCountInbox[port] = Manager.SIGNAL_TIME-1;
                    receiving[port] = true;
                }
            }
            //Console.WriteLine(valueRecived[0]);
        }

        private void AddFrameToQueue(Frame f,int portSource)   {            
            foreach (int port in PortsToSend(f.DestinyMAC))
            {
                if (port == portSource)
                    continue;
                queueOutbox[port].Enqueue(f);
            }
        }

        public override Value Emit(int port = 0)
        {
            if(emiting[port]){
                if (stCountOutbox[port] > 0){
                    stCountOutbox[port]--;
                    return valueEmited[port] = outbox[port].Current;
                }
                else{
                    if (outbox[port].MoveNext()){
                        stCountOutbox[port] = Manager.SIGNAL_TIME-1;
                        return valueEmited[port] = outbox[port].Current;
                    }else{
                        if (queueOutbox[port].Count > 0){
                            outbox[port] = queueOutbox[port].Dequeue().GetEnumerator();                            
                        } else{
                            emiting[port] = false;
                        }                    
                        return valueEmited[port] =  Value.UNACTIVE;
                    }
                }
            }else{
                if (queueOutbox[port].Count > 0){
                    outbox[port] = queueOutbox[port].Dequeue().GetEnumerator();
                    stCountOutbox[port] = 0;
                    emiting[port]= true;
                    return valueEmited[port] = Emit(port);
                }
                return valueEmited[port] = Value.UNACTIVE;
            }
        }
      
        void UpdateMAC(string mac, int port, int actualTurn)
        {
            if(!Memo.ContainsKey(mac))
            {
                Memo.Add(mac, new List<(int port, int dead_turn)>());
            }
            var ports_list = Memo[mac];
            for (int i = 0; i < ports_list.Count; i++)
            {
                if(ports_list[i].port == port)
                {
                    ports_list[i] = (port, actualTurn + 100 * Manager.SIGNAL_TIME);
                    return;
                }
            }
            ports_list.Add((port, actualTurn + 40));
        }
        public void DestroyMACs(int actualTurn)
        {
            List<string> ToEliminate = new List<string>();
            foreach (var mac_and_ports in Memo)
            {
                List<(int port, int dead_turn)> ports_list = mac_and_ports.Value;
                for (int i = 0; i < ports_list.Count; i++)
                {
                    if(actualTurn == ports_list[i].dead_turn)
                    {
                        ports_list.RemoveAt(i);
                        i--;
                    }
                }
                if(ports_list.Count == 0)                                                          // Si al eliminar el puerto, la MAC dejo de tener puertos donde se suponia que estaba ubicada.
                {
                    ToEliminate.Add(mac_and_ports.Key);
                }
            }
            foreach (var item in ToEliminate)
            {
                Memo.Remove(item);
            }

        }
        public int[] PortsToSend(string mac)
        {
            if(!Memo.ContainsKey(mac) || mac == "FFFF")
            {
                int[] all_ports = new int[Adj.Length];
                for (int i = 1; i < all_ports.Length; i++)
                {
                    all_ports[i] = i;
                }
                return all_ports;
            }

            var ports_list = Memo[mac];
            int[] ports_to_send = new int[ports_list.Count];
            for (int l = 0; l < ports_to_send.Length; l++)
            {
                ports_to_send[l] = ports_list[l].port;
            }
            return ports_to_send;
        }
    }
}
