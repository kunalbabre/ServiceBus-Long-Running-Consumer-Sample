using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace ServiceBusDemo
{
    class Program
    {

        const string ServiceBusConnectionString = "Endpoint=sb://<REPLACE_WITH_YOUR_CONN_STRING>";
        const string QueueName = "q-main";
        const string QueueName_Cleanup = "q-cleanup";
        static int processing_choice;
        static Random random = new Random();

        public static async Task Main(string[] args)
        {
            ConsoleWindows.Draw();

            var receiver = new MessageReceiver(ServiceBusConnectionString, QueueName, ReceiveMode.PeekLock);
            var receiverCleanUp = new MessageReceiver(ServiceBusConnectionString, QueueName_Cleanup, ReceiveMode.PeekLock);
            var senderCleanUp = new MessageSender(ServiceBusConnectionString, QueueName_Cleanup);



            while (true)
            {
                await ProcessMessages(receiver, senderCleanUp);
                await CleanUp(receiverCleanUp, receiver);

            }

        }

        private static async Task ProcessMessages(MessageReceiver receiver, MessageSender senderCleanUp)
        {
            ConsoleWindows.Clear(2);
            ConsoleWindows.WriteLine("Message Receiver", 2);
            
            do
            {
                var message = await receiver.ReceiveAsync(TimeSpan.FromSeconds(1));

                if (message == null)
                {
                    break;
                }

                ConsoleWindows.WriteLine(string.Format("Message Received{0}", message.MessageId), 2);

                var deferredSequenceNumber = message.SystemProperties.SequenceNumber;
                await PostInCleanUpQueue(senderCleanUp, deferredSequenceNumber);

   
                await receiver.DeferAsync(message.SystemProperties.LockToken);
                
                int choice = 1;

                if (processing_choice != 3)
                {
                    ConsoleWindows.WriteLine("Processing Options:", 2);
                    ConsoleWindows.WriteLine(" 1: Sucessful Processing \n 2: Simulate Crash \n 3: Random Mode(for all messages) ", 2);
                    ConsoleWindows.WriteLine("Enter choice: ", 2);
                    var input = Console.ReadLine();
                    Int32.TryParse(input, out choice);

                    if (choice == 3)
                    {
                        processing_choice = 3;
                    }

                } 

                if (processing_choice == 3)
                {
                   
                    choice = random.Next(0, 2);
                }
                

                if (choice == 1)
                {
                    
                    await CleanUpAfterProcessing(receiver, deferredSequenceNumber, 2);
                }
                else
                {
                    ConsoleWindows.WriteLine("Crashed", 2);
                }
            } while (true);
        }

  
        private static async Task CleanUp(MessageReceiver receiverCleanUpQueue, MessageReceiver receiver)
        {

            ConsoleWindows.Clear(1);
            ConsoleWindows.WriteLine("Message Clean up ...",1);
            do
            {
                var message = await receiverCleanUpQueue.ReceiveAsync(TimeSpan.FromSeconds(1));
                if (message != null)
                {
                   
                    if (long.TryParse(message.MessageId, out long deferredSequenceNumber))
                    {
                        await CleanUpAfterProcessing(receiver, deferredSequenceNumber, 1);
                    }
                    ConsoleWindows.WriteLine(string.Format("Cleaning up Message: {0}", message.MessageId), 1);
                    await receiverCleanUpQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    break;
                }
            } while (true);
        }



        private static async Task PostInCleanUpQueue(MessageSender senderCleanUp, long deferredSequenceNumber)
        {
            var m = new Message
            {
                MessageId = deferredSequenceNumber.ToString(),
                ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddSeconds(30)
            };
            await senderCleanUp.SendAsync(m);
        }

        private static async Task CleanUpAfterProcessing(MessageReceiver receiver, long deferredSequenceNumber, int area = 1)
        {

            // once process is complete lets process it
            try
            {
                var message = await receiver.ReceiveDeferredMessageAsync(deferredSequenceNumber);
                if (message != null)
                {
                    ConsoleWindows.WriteLine(string.Format("Cleaning up Message: {0}", message.MessageId), area);
                    await receiver.CompleteAsync(message.SystemProperties.LockToken);
                }
            }
            catch (MessageNotFoundException) {
                return;// it is okay most of the time this is what is expected 
            }
        }


    }
}
