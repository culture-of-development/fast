// using System;

// namespace fast.search
// {
//     public interface INearestNeighbor<TKey, TValue>
//     {
//         TValue FindNearestNeighbor(TKey key);
//     }

//     public class NaiveNearestNeighbors<TKey, TValue> : INearestNeighbor<TKey, TValue>
//     {
        
//         public NaiveNearestNeighbors()
//         {

//         }

//         // public TValue FindNearestNeighbor(TKey key)
//         // {
//         //     var min = default(TValue);
//         //     foreach(var item in items)
//         //     {
//         //         if (min = default(TValue))
//         //         {
//         //             from = node;
//         //             distFrom = DistanceHelper.Haversine(latFrom, lonFrom, node.Latitude, node.Longitude);
//         //             to = node;
//         //             distTo = DistanceHelper.Haversine(latTo, lonTo, node.Latitude, node.Longitude);
//         //             continue;
//         //         }
//         //         var nodeDistFrom = DistanceHelper.Haversine(latFrom, lonFrom, node.Latitude, node.Longitude);
//         //         if (nodeDistFrom < distFrom)
//         //         {
//         //             distFrom = nodeDistFrom;
//         //             from = node;
//         //         }
//         //         var nodeDistTo = DistanceHelper.Haversine(latTo, lonTo, node.Latitude, node.Longitude);
//         //         if (nodeDistTo < distTo)
//         //         {
//         //             distTo = nodeDistTo;
//         //             to = node;
//         //         }
//         //     }
//         // }
//     }
// }