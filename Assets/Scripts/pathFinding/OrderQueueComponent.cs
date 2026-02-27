using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderQueueComponent : MonoBehaviour
{
    private Queue<Order> orderQueue = new Queue<Order>();

    public void EnOrder(Order order)
    {
        orderQueue.Enqueue(order);
    }

    public void RemoveOrder(Order order)
    {
        var list = new List<Order>(orderQueue);
        list.Remove(order);
        orderQueue = new Queue<Order>(list);
    }

    public void ClearOrder()
    {
        orderQueue.Clear();
    }

    public void DeOrder()
    {
        if (orderQueue.Count > 0)
        {
            orderQueue.Dequeue();
        }
    }

    public Order GetTopOrder()
    {
        return orderQueue.Count > 0 ? orderQueue.Peek() : Order.Null;
    }
    
    public Order GetTopOrderAndRemove()
    {
        return orderQueue.Count > 0 ? orderQueue.Dequeue() : Order.Null;
    }

    public List<Order> GetOrders()
    {
        return new List<Order>(orderQueue);
    }
}
