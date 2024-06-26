using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

class Worker
{
    private static Semaphore semaphore; // semaphore dùng để set up số lượng công việc có thể xử lí đồng thời 
    static void Main()
    {
        semaphore = new Semaphore(10, 10); // Số lượng công việc đồng thời 
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "work_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            // Đây là phần cân bằng tải (Round-robin).
            // Bằng cách đặt prefetchCount là 1, chúng ta chỉ cho phép một công việc được xử lý tại mỗi thời điểm.
            // Công việc tiếp theo sẽ chỉ được gửi đến worker khi worker hiện tại đã hoàn thành công việc trước đó và xác nhận (ack) nó.
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false); //  để chỉ định rằng mỗi công nhân chỉ xử lý một công việc tại một thời điểm.

            Console.WriteLine("Waiting for tasks...");

            //chúng ta tạo một EventingBasicConsumer và đăng ký sự kiện Received để xử lý công việc nhận được từ hàng đợi.
            var consumer = new EventingBasicConsumer(channel); 
            consumer.Received += (model, ea) =>
            {
                semaphore.WaitOne(); // để chờ đợi cho đến khi có một slot trống trong Semaphore (tức là có thể xử lý thêm công việc)
                var body = ea.Body.ToArray();
                var task = Encoding.UTF8.GetString(body);
                Console.WriteLine("Processing task: {0}", task);
                Thread.Sleep(5000); // Giả lập thời gian xử lý công việc
                Console.WriteLine("Task completed: {0}", task);
                // Đây là phần xác nhận (acknowledgment).
                // Sau khi worker đã hoàn thành công việc, chúng ta sẽ gửi một xác nhận (BasicAck) cho RabbitMQ để thông báo rằng công việc đã được xử lý thành công và RabbitMQ có thể gửi công việc tiếp theo đến worker khác.
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false); // chúng ta gửi xác nhận (BasicAck) cho RabbitMQ để cho biết công việc đã hoàn thành.
                semaphore.Release(); //  để giải phóng một slot và cho phép công việc tiếp theo được xử lý.

            };
            channel.BasicConsume(queue: "work_queue", autoAck: false, consumer: consumer); //  BasicConsume để bắt đầu lắng nghe công việc từ hàng đợi.

            // ví dụ sau khi xử lí xong nó sẽ trả về 1 object

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();

        }

    }

}
