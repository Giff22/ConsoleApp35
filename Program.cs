using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
    public class Request
    {
        public int Id { get; set; }
        public string ResidentName { get; set; }
        public string Description { get; set; }
        public string ApartmentNumber { get; set; } // Номер квартири
        public bool IsPriority { get; set; }
        public string Status { get; set; }
        public string AssignedWorker { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public bool ResidentNotified { get; set; } = false;
    }

    private static List<Request> requests = new List<Request>();
    private static List<string> workers = new List<string> { "Петро", "Андрій", "Іван" };
    public static event Action<string> NotifyResidents;
    public static event Action<string> NotifyWorkers;

    public static void Main(string[] args)
    {
        bool continueSimulation = true;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Підписка на події
        NotifyResidents += message => Console.WriteLine($"[Сповіщення для мешканця] {message}");
        NotifyWorkers += message => Console.WriteLine($"[Сповіщення для співробітника] {message}");

        while (continueSimulation)
        {
            Console.Clear();
            Console.WriteLine("Система заявок ЖКГ");
            Console.WriteLine("1. Додати заявку");
            Console.WriteLine("2. Переглянути заявки");
            Console.WriteLine("3. Виконати заявку");
            Console.WriteLine("4. Завершити симуляцію");
            Console.Write("Оберіть опцію: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddRequest();
                    break;
                case "2":
                    ViewRequests();
                    break;
                case "3":
                    ProcessRequest();
                    break;
                case "4":
                    continueSimulation = false;
                    Console.WriteLine("Симуляцію завершено.");
                    break;
                default:
                    Console.WriteLine("Неправильний вибір, спробуйте ще раз.");
                    break;
            }

            Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
            Console.ReadKey();
        }
    }

    private static void AddRequest()
    {
        Console.Write("Введіть ім'я мешканця: ");
        string name = Console.ReadLine();
        Console.Write("Введіть опис заявки: ");
        string description = Console.ReadLine();
        Console.Write("Введіть номер квартири: ");
        string apartmentNumber = Console.ReadLine();
        Console.Write("Ця заявка є пріоритетною? (так/ні): ");
        bool isPriority = Console.ReadLine()?.ToLower() == "так";

        var newRequest = new Request
        {
            Id = requests.Count + 1,
            ResidentName = name,
            Description = description,
            ApartmentNumber = apartmentNumber,
            IsPriority = isPriority,
            Status = "Очікує виконання"
        };

        requests.Add(newRequest);
        Console.WriteLine($"Заявка додана! ID: {newRequest.Id}");

        NotifyResidents?.Invoke($"Ваша заявка на '{description}' підтверджена.");
    }
    private static void ViewRequests()
    {
        if (requests.Count == 0)
        {
            Console.WriteLine("Немає активних заявок.");
            return;
        }

        Console.WriteLine("Список заявок:");
        foreach (var request in requests)
        {
            string priorityText = request.IsPriority ? "Так" : "Ні"; 
            Console.WriteLine($"ID: {request.Id}, Мешканець: {request.ResidentName}, Квартира: {request.ApartmentNumber}, Опис: {request.Description}, Пріоритет: {priorityText}, Статус: {request.Status}");
        }
    }

    private static void ProcessRequest()
    {
        var pendingRequests = requests.Where(r => r.Status == "Очікує виконання")
                                      .OrderByDescending(r => r.IsPriority)
                                      .ToList();

        if (pendingRequests.Count == 0)
        {
            Console.WriteLine("Немає заявок для виконання.");
            return;
        }

        var requestToProcess = pendingRequests.First();
        string assignedWorker = workers[new Random().Next(workers.Count)];
        requestToProcess.Status = "Виконано";
        requestToProcess.AssignedWorker = assignedWorker;
        requestToProcess.ScheduledTime = DateTime.Now.AddHours(1);

        Console.WriteLine($"Заявка ID: {requestToProcess.Id} виконана співробітником {assignedWorker}.");

        // Сповіщення мешканців
        if (!requestToProcess.ResidentNotified)
        {
            NotifyResidents?.Invoke(
                $"Ваша заявка на '{requestToProcess.Description}' підтверджена. Спеціаліст прибуде о {requestToProcess.ScheduledTime:HH:mm}."
            );
            requestToProcess.ResidentNotified = true;
        }

        // Сповіщення співробітників
        NotifyWorkers?.Invoke(
            $"Вам призначено виконання заявки на '{requestToProcess.Description}' у квартирі {requestToProcess.ApartmentNumber}, прибуття о {requestToProcess.ScheduledTime:HH:mm}."
        );

        requests.Remove(requestToProcess);
    }
}
