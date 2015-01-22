using System;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace QueueExample
{
    public class Program
    {
        public static string QueueName = "Orders";
        private static QueueClient queueClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Creating a Queue");
            CreateQueue();
            while (true)
            {
                Console.WriteLine("Press S to start sending a message, Press R to receive messages, Press E to exit the console...");
                ConsoleKeyInfo info = Console.ReadKey();
                if (info.Key == ConsoleKey.S)
                {
                    //Sending Message
                    SendMessage();
                }
                else if (info.Key == ConsoleKey.R)
                {
                    //Receive Messages
                    ReceiveMessages();
                }
                else if (info.Key == ConsoleKey.E)
                {
                    //Exit the console & close the queue client
                    queueClient.Close();
                    break;
                }
            }
        }

        private static void CreateQueue()
        {
            NamespaceManager namespaceManager = NamespaceManager.Create();
            Console.WriteLine("\nCreating Queue '{0}'...", QueueName);

            // Delete the queue when it exists
            if (namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.DeleteQueue(QueueName);
            }

            namespaceManager.CreateQueue(QueueName);
        }

        private static void SendMessage()
        {
            queueClient = QueueClient.Create(QueueName);
                
            BrokeredMessage message = new BrokeredMessage(string.Format("Sending message with Guid: {0}",Guid.NewGuid()));
            message.MessageId = Guid.NewGuid().ToString();
         
            Console.WriteLine("\nSending message to Queue...");

            try
            {
                queueClient.Send(message);
            }
            catch (MessagingException e)
            {
                if (!e.IsTransient)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
                else
                {
                    HandleTransientErrors(e);
                }
            }
            Console.WriteLine(string.Format("Message sent: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));
            Console.WriteLine();
        }

        private static void ReceiveMessages()
        {
            Console.WriteLine("\nReceiving message from Queue...");
            BrokeredMessage message = null;
            while (true)
            {
                try
                {
                    //receive messages from Queue
                    message = queueClient.Receive(TimeSpan.FromSeconds(5));
                    if (message != null)
                    {
                        Console.WriteLine(string.Format("Message received: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));
                        Console.WriteLine();
                        // Further custom message processing could go here...
                        message.Complete();
                    }
                    else
                    {
                        //no more messages in the queue
                        break;
                    }
                }
                catch (MessagingException e)
                {
                    if (!e.IsTransient)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine();
                        throw;
                    }
                    else
                    {
                        HandleTransientErrors(e);
                    }
                }
            }
        }


        private static void HandleTransientErrors(MessagingException e)
        {
            //If transient error/exception, let's back-off for 2 seconds and retry
            Console.WriteLine(e.Message);
            Console.WriteLine("Will retry sending the message in 2 seconds");
            Thread.Sleep(2000);
        }
    }
}
