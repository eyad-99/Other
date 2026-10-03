namespace server.jobs
{
    public class DemoJobs
    {
        public async Task SendEmail()
        {
            Console.WriteLine("Sending email...");
            await Task.Delay(3000);
            Console.WriteLine("Email sent!");
        }

        public async Task SendReminder()
        {
            Console.WriteLine("Sending reminder...");
            await Task.Delay(2000);
            Console.WriteLine("Reminder sent!");
        }

        public async Task Cleanup()
        {
            Console.WriteLine($"Cleanup executed: {DateTime.Now}");
            await Task.Delay(1000);
        }

        public async Task ProcessFile()
        {
            Console.WriteLine("1. Processing file...");
            await Task.Delay(3000);
            Console.WriteLine("1. Processing complete");
        }

        public async Task ResizeFile()
        {
            Console.WriteLine("2. Resizing file...");
            await Task.Delay(3000);
            Console.WriteLine("2. Resize complete");
        }

        public async Task OptimizeFile()
        {
            Console.WriteLine("3. Optimizing file...");
            await Task.Delay(3000);
            Console.WriteLine("3. Optimization complete");
        }
    }
}
