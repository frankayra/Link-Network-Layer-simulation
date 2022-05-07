using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Link_layer
{

    public static class TextParser
    {
        /// <summary>
        /// Ejecuta el comando pasado como string, devolviendo si hubo un error en el formato del comando.
        /// </summary>
        public static bool IsCorrectCommand(string[] command_, bool execute = false)
        {
            if (command_.Length < 3) return false;
            int time;
            if (!int.TryParse(command_[0], out time)) return false;

            switch (command_[1])
            {
                case "send_frame":
                    if (execute && !Manager.DervicesNames.ContainsKey(command_[2])) return false;
                    if (execute && !(Manager.DervicesNames[command_[2]] is Host)) return false;
                    if (execute) Send_Frame_Handler((Host)(Manager.DervicesNames[command_[2]]), command_[3], command_[4]);
                    break;
                case "mac":
                    if (!Manager.DervicesNames.ContainsKey(command_[2]) || !(Manager.DervicesNames[command_[2]] is Host)) return false;
                    if (command_[3].Length != 4) return false;
                    if(execute)
                    {
                        ((Host)(Manager.DervicesNames[command_[2]])).MAC = command_[3];
                        Manager.DervicesMACs[command_[3]] = Manager.DervicesNames[command_[2]];
                    }

                    break;
                case "create":
                    switch (command_[2])
                    {
                        case "host":
                            if (command_.Length < 4) return false;
                            if (Manager.DervicesNames.ContainsKey(command_[3])) return false;
                            int ports2;
                            if (execute) Create_Handler(DerviceType.Host, command_[3]);
                            break;
                        case "hub":
                            if (command_.Length < 5) return false;
                            int ports;
                            if (Manager.DervicesNames.ContainsKey(command_[3])) return false;
                            if (!int.TryParse(command_[4], out ports)) return false;
                            if (execute) Create_Handler(DerviceType.Hub, command_[3], ports);
                            break;
                        case "switch":
                            if (Manager.DervicesNames.ContainsKey(command_[3])) return false;
                            if (!int.TryParse(command_[4], out ports)) return false;
                            if (execute) Create_Handler(DerviceType.Switch, command_[3], ports);
                            break;

                        default: return false;
                    }
                    return true;
                case "connect":
                    if (command_.Length < 4) return false;
                    string[] dervice1_and_port = command_[2].Split('_');
                    string[] dervice2_and_port = command_[3].Split('_');
                    string dervice1_name = "";
                    string dervice2_name = "";

                    for (int i = 0; i < dervice1_and_port.Length - 1; i++)
                    {
                        dervice1_name += dervice1_and_port[i];
                    }
                    for (int j = 0; j < dervice2_and_port.Length - 1; j++)
                    {
                        dervice2_name += dervice2_and_port[j];
                    }

                    if (execute)
                    {
                        if (!Manager.DervicesNames.ContainsKey(dervice1_name)) return false;
                        if (!Manager.DervicesNames.ContainsKey(dervice2_name)) return false;
                    }

                    int port_1, port_2;

                    if (!int.TryParse(dervice1_and_port[dervice1_and_port.Length - 1], out port_1)) return false;
                    if (!int.TryParse(dervice2_and_port[dervice2_and_port.Length - 1], out port_2)) return false;

                    if (execute)
                    {
                        Dervice a = Manager.DervicesNames[dervice1_name];
                        Dervice b = Manager.DervicesNames[dervice2_name];
                        Connect_Handler(a, b, port_1 - 1, port_2 - 1);
                    }

                    break;
                case "disconnect":
                    string[] dervice_and_port = command_[2].Split('_');                                                                  // No hace falta comprobar el Length de command_ ya que para este comando no debe ser menor que 3 y ya eso lo comprobe arriba.
                    string dervice_name = "";

                    for (int u = 0; u < dervice_and_port.Length - 1; u++)
                    {
                        dervice_name += dervice_and_port[u];
                    }

                    //if (Manager.DervicesNames.ContainsKey(dervice_name)) return false;

                    Dervice d = null;
                    if (execute) d = Manager.DervicesNames[dervice_name];
                    int port;
                    if (!int.TryParse(dervice_and_port[dervice_and_port.Length - 1], out port)) return false;

                    if (execute) Disconnect_Handler(d, port - 1);

                    break;
                case "send":
                    if (command_.Length < 4) return false;
                    Value[] number = Convert(command_[3]);
                    if (number is null) return false;

                    if (execute)
                    {
                        if (!Manager.DervicesNames.ContainsKey(command_[2])) return false;
                        Dervice dd = Manager.DervicesNames[command_[2]];
                        if (!(dd is Host)) return false;
                        Send_Handler((Host)dd, number);
                    }
                    break;
                default: return false;
            }
            return true;
        }





        #region Handlers

        private static void Create_Handler(DerviceType d_type, string name, int ports = 1)
        {
            if (Manager.DervicesNames.ContainsKey(name)) return;
            Dervice a;
            if (d_type == DerviceType.Host)
                a = new Host(name);
            else if (d_type == DerviceType.Hub)
                a = new Hub(ports, name);
            else a = new Switch(ports, name);
            Manager.DervicesNames[name] = a;
            if (Manager.Dervices is null) Manager.Initialize(new List<Dervice>(new Dervice[] { a }), FlowControler.TemporalSIgnalTime);
            Manager.Dervices.AddItem(new Dervice[] { a });
        }
        private static void Connect_Handler(Dervice a, Dervice b, int a_port, int b_port)
        {
            Dervice.cc(a, b, a_port, b_port);
            Manager.Dervices.Merge(a, b);
            Manager.Initialize(Manager.Dervices.ValuesSet, Manager.SIGNAL_TIME);
        }
        private static void Disconnect_Handler(Dervice a, int port)
        {
            if (!Manager.DervicesNames.ContainsKey(a.name)) return;
            a.Disconect(port);
            Manager.Initialize(Manager.Dervices.ValuesSet, Manager.SIGNAL_TIME);
        }
        private static void Send_Handler(Host pc, Value[] emision)
        {
            pc.Send(emision);
        }
        private static void Send_Frame_Handler(Host source_host, string destiny_mac, string _data)
        {
            source_host.Send(FlowControler.Protocol.Encrypt(source_host.MAC, destiny_mac, _data));
        }

        private enum DerviceType { Hub, Host, Switch}

        #endregion


        #region Utiles
        /// <summary>
        /// Convierte un numero binario en formato string, a un array de Value's.
        /// </summary>
        public static Value[] Convert(string number)
        {
            Value[] array = new Value[number.Length];
            for (int i = 0; i < number.Length; i++)
            {
                if (!char.IsDigit(number[i]) || (number[i] != 48 && number[i] != 49))
                {
                    return null;
                }
                array[i] = (Value)(number[i] - 47);
            }
            return array;
        }

        #endregion
    }

}
