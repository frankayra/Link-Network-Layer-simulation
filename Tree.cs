using System;
using System.Collections.Generic;
using System.Text;

namespace Link_layer
{
    public class Tree<T>
    {
        public T Value;
        public Tree<T> Father;
        public List<Tree<T>> Adj = new List<Tree<T>>();
        public Tree(T value, Tree<T> father = null)
        {
            Value = value;
            Father = father;
        }
    }
}
