using KdTree3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LinkedHashSet<T> : HashSet<T>
{
    //override Add
}


public class LinkedTree<T,M> : KdTree<M>.Vector3i<T> where M : IMetric
{

}