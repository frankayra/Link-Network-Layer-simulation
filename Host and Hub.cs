using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace Link_layer
{
    public class Host : Dervice
    {
        public Host(string name) : base(1, name)
        {
            wait_time = 0;
            stCountOut = 0;
            emiting = false;
            sw = new StreamWriter(this.name + ".txt");
            secuences = new Queue<IEnumerable<Value>>();
            currentFrame = new Frame();
            FramesRecived =  new Queue<Frame>();
        }

        private Queue<IEnumerable<Value>> secuences;
        private int wait_time;
        private int stCountOut;
        private int stCountIn;
        private IEnumerator<Value> enumerator;
        public Value ValueEmited { get; private set; }
        public Value ValueRecived { get; private set; }
        public string MAC { get; set; }
        private bool emiting;
        public Frame currentFrame;
        public Queue<Frame> FramesRecived;
        private StreamWriter sw;

        public void Send(IEnumerable<Value> values)
        {
            secuences.Enqueue(values);
        }

        public override Value Emit(int port = 0)
        {
            if (emiting){
                if (stCountOut > 0){
                    stCountOut--;
                    return ValueEmited = enumerator.Current;
                } else{
                    if (enumerator.MoveNext()){
                        stCountOut = Manager.SIGNAL_TIME-1;
                        ValueEmited = enumerator.Current;
                        return ValueEmited;
                    } else{
                        secuences.Dequeue();
                        emiting = false;
                        ValueEmited = Value.UNACTIVE;
                        stCountOut = randomTime(4);
                        return ValueEmited;
                    }
                } 
            } else{
                ValueEmited = Value.UNACTIVE;
                return ValueEmited;
            }
        }

        public override void Recive(Value value, int transmiting_dervices, int port = 0)
        {   
            string txt = "";
            if (emiting){
                txt = FlowControler.Turn + " " + this.name + " send " + ValueEmited + " " ;
                // Colisión
                if (transmiting_dervices > 2){
                    txt += "colision";
                    wait_time = randomTime(8);
                    emiting = false;                    
                } else{
                    txt += "ok";
                }
            }else{
                if (wait_time > 0){
                    // Esperando
                    wait_time--;
                } else{
                    if (transmiting_dervices > 2){
                        wait_time = randomTime(8);
                    }else{
                        // Preparado
                        if (secuences.Count > 0){
                            emiting=true;
                            enumerator = (IEnumerator<Value>)secuences.Peek().GetEnumerator();
                            stCountOut = Manager.SIGNAL_TIME; 
                        }
                    }
                }
            }
            if (value != Value.UNACTIVE){
                txt += "\n" + FlowControler.Turn + " " + this.name + " recive " + ValueEmited;
            }
            recive(value);
            sw.WriteLine(txt);
            sw.Flush();
        }

        private void recive(Value value){
            if (ValueRecived != Value.UNACTIVE){
                if (stCountIn > 0) {
                    if (ValueRecived == value){
                        stCountIn --;
                    }else{
                        // Ruido en la trama
                        currentFrame.Clean();
                        stCountIn = Manager.SIGNAL_TIME-1;
                        ValueRecived = value;
                        //recive(value);
                    }
                }else{
                    currentFrame.AddBit(ValueRecived);
                    if (currentFrame.IsComplete()){
                        FramesRecived.Enqueue(currentFrame);
                        currentFrame = new Frame();
                    }
                    stCountIn = Manager.SIGNAL_TIME-1;
                    ValueRecived = value;
                    //recive(value);
                }
            }else{
                if (value != Value.UNACTIVE){
                    stCountIn = Manager.SIGNAL_TIME-1;
                    ValueRecived = value;
                    //recive(value);
                }
            }
        }
        private int randomTime(int secuence_len)
        {
            return (int)(secuence_len * Manager.SIGNAL_TIME * FlowControler.RandomDouble) + 1;
        }

    }
    public class Hub : Dervice
    {
        public Hub(int ports, string name) : base(ports, name)
        {
            sw = new StreamWriter("output/" + this.name + ".txt");
        }

        private StreamWriter sw;

        public override void Recive(Value value, int transmiting_dervices, int port = 0)
        {
            base.Recive(value, transmiting_dervices, port);
            if (value != Value.UNACTIVE)
            {
                int i = 1;
                foreach (Dervice d in this.Adj)
                {
                    if (d == null) sw.WriteLine(FlowControler.Turn + " " + name + "_" + i + " send " + value);
                    i++;
                }
                sw.Flush();
            }
        }
    }
}
