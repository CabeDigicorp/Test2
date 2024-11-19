using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoinApi.Models
{
    //using Stern-Brocot tree method
    public class OrderPosition
    {
        public int Num { get; private set; }
        public int Den { get; private set; }
        public double Result { get; private set; }

        public OrderPosition(int num, int den)
        {
            Num = num;
            Den = den;
            Result = Num / Den;
        }

        static OrderPosition Intermediate(OrderPosition a, OrderPosition b)
        {
            return new OrderPosition(a.Num + b.Num, a.Den + b.Den);
        }

    }
}
