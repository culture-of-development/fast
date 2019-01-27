using System;
using System.Collections.Generic;

namespace search_problems
{
    public class MinHeap<TKey, TValue> : IPriorityQueue<TKey, TValue> where TKey : IComparable<TKey>
    {
        private int minCapacity;
        private int size;
        // TODO: this being a class makes for a slow copy, use a tuple instead
        private HeapNode<TKey, TValue>[] nodes;

        public MinHeap(int initialCapacity)
        {
            this.minCapacity = initialCapacity;
            this.nodes = new HeapNode<TKey, TValue>[initialCapacity];
            this.size = 0;
        }

        public bool IsEmpty { get { return size == 0; } }
        public void Push(TKey key, TValue value)
        {
            if (size == nodes.Length) Resize(nodes.Length * 2);
            var node = nodes[size] = new HeapNode<TKey, TValue> { Key = key, Value = value };
            int current = size;
            while (true)
            {
                int parent = (current - 1) / 2;
                if (parent < 0) break;
                if (nodes[parent].Key.CompareTo(key) <= 0) break;
                nodes[current] = nodes[parent];
                nodes[parent] = node;
                current = parent;
            }
            size++;
        }
        public TValue Pop()
        {
            var top = nodes[0];
            size--;
            var sink = nodes[0] = nodes[size];
            nodes[size] = null;
            int current = 0;
            while(true)
            {
                int leftChildIndex = current * 2 + 1;
                if (leftChildIndex >= size) break;
                var leftChild = nodes[leftChildIndex];
                int rightChildIndex = leftChildIndex + 1;
                if (rightChildIndex >= size)
                {
                    if (leftChild.Key.CompareTo(sink.Key) >= 0) break;
                    nodes[current] = leftChild;
                    nodes[leftChildIndex] = sink;
                    current = leftChildIndex;
                }
                else
                {
                    var rightChild = nodes[rightChildIndex];
                    if (leftChild.Key.CompareTo(rightChild.Key) < 0)
                    {
                        if (leftChild.Key.CompareTo(sink.Key) >= 0) break;
                        nodes[current] = leftChild;
                        nodes[leftChildIndex] = sink;
                        current = leftChildIndex;
                    }
                    else
                    {
                        if (rightChild.Key.CompareTo(sink.Key) >= 0) break;
                        nodes[current] = rightChild;
                        nodes[rightChildIndex] = sink;
                        current = rightChildIndex;
                    }
                }
            }
            if (size < nodes.Length / 4) Resize(nodes.Length / 2);
            return top.Value;
        }
        
        private void Resize(int newCapacity)
        {
            if (newCapacity < minCapacity) return;
            var resized = new HeapNode<TKey, TValue>[newCapacity];
            for(int i = 0; i < size; i++)
            {
                resized[i] = nodes[i];
            }
            nodes = resized;
        }

        private class HeapNode<TK, TV>
        {
            public TK Key { get; set; }
            public TV Value { get; set; }

            public override string ToString()
            {
                return this.Key.ToString();
            }
        }
    }
}