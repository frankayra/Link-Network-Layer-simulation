using System;
using System.Collections.Generic;
using System.Text;

namespace Link_layer
{
    public class DisjoinSets<T>
    {
        internal List<T> ValuesSet = new List<T>();
        public Dictionary<T, Tree<T>> NodeOf = new Dictionary<T, Tree<T>>();
        public Dictionary<T, int> Classes = new Dictionary<T, int>();
        public Dictionary<T, Value> CCsValues = new Dictionary<T, Value>();
        public DisjoinSets(IEnumerable<T> elements)
        {
            foreach (T item in elements)
            {
                Tree<T> node = new Tree<T>(item);
                NodeOf[item] = node;
                CCsValues[item] = default;
                Classes[item] = 1;
                ValuesSet.Add(item);
            }

        }
        #region Merge
        /// <summary>
        /// Reagrupa desde cero los elementos, mezclandolos por clases segun la funcion de mezcla CanMerge.
        /// </summary>
        public void ResetAndMerge(Func<T, T, bool> canMerge)
        {

            #region Reseting
            Classes = new Dictionary<T, int>();
            CCsValues = new Dictionary<T, Value>();
            NodeOf = new Dictionary<T, Tree<T>>();

            foreach (T item in ValuesSet)
            {
                Tree<T> node = new Tree<T>(item);
                NodeOf[item] = node;
                CCsValues[item] = default;
                Classes[item] = 1;
            }
            #endregion

            #region Merging
            for (int i = 0; i < ValuesSet.Count; i++)                                        //
            {                                                                                                     //
                for (int k = i + 1; k < ValuesSet.Count; k++)                                //           Haciendo como un burbujeo en O(n^2)...
                {                                                                                                 //
                    if (canMerge(ValuesSet[i], ValuesSet[k]))                           //
                    {                                                                                       //
                        Merge(ValuesSet[i], ValuesSet[k]);                          //
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Actualiza el D_S a partir de las clases ya formadas, sin reordenar desde cero los elementos.
        /// </summary>
        public void UpdateMerge(Func<T, T, bool> canMerge)
        {
            T[] classes = new T[Classes.Count];
            Classes.Keys.CopyTo(classes, 0);
            for (int i = 0; i < classes.Length; i++)
            {
                for (int k = i + 1; k < classes.Length; k++)
                {
                    if (canMerge(classes[i], classes[k]))
                    {
                        Merge(classes[i], classes[k]);
                    }
                }
            }
        }

        /// <summary>
        /// Merge basico entre 2 objetos de la misma clase
        /// </summary>
        public void Merge(T item1, T item2)
        {
            Tree<T> tree1 = ClassRepresentantOf(item1);
            Tree<T> tree2 = ClassRepresentantOf(item2);

            if (tree1 == tree2) return;                                                         //Estan en la misma clase

            int heigth1 = Classes[tree1.Value];
            int heigth2 = Classes[tree2.Value];

            bool one_go_up = heigth1 > heigth2;
            Tree<T> upper = one_go_up ? tree1 : tree2;
            Tree<T> downer = one_go_up ? tree2 : tree1;

            upper.Adj.Add(downer);
            downer.Father = upper;
            Classes.Remove(downer.Value);
            CCsValues.Remove(downer.Value);

            if (heigth1 == heigth2)
                Classes[upper.Value]++;
        }
        #endregion

        public void AddItem(IEnumerable<T> new_items, Func<T, T, bool> canMerge = null, bool update_D_S = false)
        {
            foreach (T item in new_items)
            {
                if (NodeOf.ContainsKey(item)) continue;
                ValuesSet.Add(item);
                Classes[item] = 1;
                CCsValues[item] = default;

                Tree<T> node = new Tree<T>(item);
                NodeOf[item] = node;
            }

            if (update_D_S) UpdateMerge(canMerge);
        }

        /// <summary>
        /// Podria ejecutarse de manera eficiente, pero lo que hicimos fue eliminar el item de la lista de los elementos en general y  luego reordenar la misma.
        /// </summary>
        public void RemoveItem(T item, Func<T, T, bool> canMerge)
        {
            ValuesSet.Remove(item);
            ResetAndMerge(canMerge);
        }

        /// <summary>
        /// Devuelve el representante de la clase a la que pertenece value.
        /// </summary>
        public Tree<T> ClassRepresentantOf(T value)
        {
            Tree<T> tree;
            if (!NodeOf.TryGetValue(value, out tree)) return null;
            while (tree.Father != null) tree = tree.Father;
            return tree;
        }

        /// <summary>
        /// Limpia o inhabilita los cables para el proximo envio.
        /// </summary>
        public void CleanChannels()
        {
            foreach (var item in Classes)
            {
                CCsValues[item.Key] = Value.UNACTIVE;
            }
        }


        #region Print

        void Print<T>()
        {
            foreach (var par in Classes)
            {
                Console.WriteLine(par.Key);
            }
            Console.WriteLine("\n\n");
            foreach (var item in ValuesSet)
            {
                Console.WriteLine(item + " ---> " + ClassRepresentantOf(item).Value);
            }
        }

        #endregion
    }
}
