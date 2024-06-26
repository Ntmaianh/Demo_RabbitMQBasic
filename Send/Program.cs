using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" }; // tạo kết nối đến máy chủ 
using var connection = factory.CreateConnection(); // Sử dụng đối tượng ConnectionFactory để tạo kết nối (Connection) đến máy chủ RabbitMQ.
using var channel = connection.CreateModel(); // tạo channel để đại điện cho hàng đợi. Nhiệm vụ đó là gửi và nhận 

//thực hiện nhận/ tạo ra 1 hàng đợi 
channel.QueueDeclare(queue: "hello", // hàng đợi có tên là "hello" 
                     durable: false, //  Hàng đợi không được lưu trữ bền vững (không tồn tại sau khi RabbitMQ khởi động lại).
                     exclusive: false, // àng đợi không độc quyền (có thể được truy cập bởi các kênh khác).
                     autoDelete: false, // Hàng đợi không tự động xóa (sẽ không bị xóa sau khi không còn kênh nào kết nối).
                     arguments: null); //  Không có đối số bổ sung.

const string message = "Hello! Hi!"; // chuỗi được gửi đi 
var body = Encoding.UTF8.GetBytes(message); // mã hóa chuỗi gửi đi 

// thực hiện gửi thông điệp từ hàng đợi  đi 
channel.BasicPublish(exchange: string.Empty,
                     routingKey: "hello", // gửi thông điệp có tên hàng đợi là "hello"
                     basicProperties: null,
                     body: body); // nội dung của hàng đợi ( là thông tin sau khi được mã hóa )


Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();