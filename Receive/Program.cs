using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "hello",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel); // Event này để nhận thông điệp từ hàng đợi 
consumer.Received += (model, ea) => // Received được sử dụng để xử lý các thông điệp nhận được từ hàng đợi RabbitMQ.
{
    var body = ea.Body.ToArray(); // đây là lấy dữ liệu được gửi đi thông qua body 
    var message = Encoding.UTF8.GetString(body); // giải mã cái thông điệp vừa lấy được 
    Console.WriteLine($" [x] Received {message}");
};
// xử lí thông tin nhận được
channel.BasicConsume(queue: "hello", // tên hàng đợi chúng ta muốn xử lí 
                     autoAck: true,
                     consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();