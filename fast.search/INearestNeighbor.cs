using System;

namespace fast.search
{
    public interface INearestNeighbor<T>
    {
        T FindNearestNeighbor(T source);
    }
}