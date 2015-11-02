using System;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace EventhubExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            EventHubClient ehc = EventHubClient.CreateFromConnectionString(
                "Endpoint=sb://orderexample.servicebus.windows.net/;SharedAccessKeyName=sendToOrdershub;SharedAccessKey=YOURSHAREDKEY",
                "ordershub");

            Console.WriteLine("Start");

            for (int i = 0; i < 100; i++)
            {
                Order order = new Order()
                {
                    Id = i,
                    Name = "Order_" + i,
                    Quantity = new Random().Next(1,100),
                    Price = new Random().NextDouble()
                };

                string svJson = JsonConvert.SerializeObject(order);
                Byte[] svBytes = Encoding.UTF8.GetBytes(svJson);
                ehc.Send(new EventData(svBytes));
            }

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
