using System;
using System.Collections.Generic;
using System.IO;

namespace ParkingLotSystem
{
    public class Slot
    {
        public string SlotLabel { get; set; }
        public bool IsOccupied { get; set; }

        public Slot(string slotLabel)
        {
            SlotLabel = slotLabel;
            IsOccupied = false;
        }
    }

    public class ParkingSlot : Slot
    {
        public VehicleBase OccupiedVehicle { get; set; }

        public ParkingSlot(string slotLabel) : base(slotLabel)
        {
            OccupiedVehicle = null;
        }
    }

    public class VehicleBase
    {
        public string LicensePlate { get; set; }
        public DateTime EntryTime { get; set; }

        public VehicleBase(string licensePlate)
        {
            LicensePlate = licensePlate;
            EntryTime = DateTime.Now;
        }
    }

    public class Vehicle : VehicleBase
    {
        public Vehicle(string licensePlate) : base(licensePlate) { }
    }

    public class ParkingLot
    {
        public int TotalSlots { get; set; }
        public List<ParkingSlot> Slots { get; set; }
        private decimal totalEarnings;
        private decimal weeklyEarnings;
        private decimal monthlyEarnings;

        public ParkingLot(int totalSlots)
        {
            TotalSlots = totalSlots;
            Slots = new List<ParkingSlot>();
            InitializeSlots();
            LoadSlotData();
            LoadEarningsData();
        }

        private void InitializeSlots()
        {
            char row = 'A';
            int column = 1;

            for (int i = 1; i <= TotalSlots; i++)
            {
                Slots.Add(new ParkingSlot($"{row}{column}"));
                column++;
                if (column > 10)
                {
                    column = 1;
                    row++;
                }
            }
        }

        public bool ParkVehicle(Vehicle vehicle, string slotLabel)
        {
            var slot = Slots.Find(s => s.SlotLabel == slotLabel);
            if (slot == null)
            {
                Console.WriteLine("Invalid slot.");
                return false;
            }

            if (slot.IsOccupied)
            {
                Console.WriteLine($"Slot {slot.SlotLabel} is already occupied.");
                return false;
            }

            slot.OccupiedVehicle = vehicle;
            slot.IsOccupied = true;
            SaveSlotData();

            Console.WriteLine("\n+----------------------+----------------------+");
            Console.WriteLine("|       Details        |       Values         |");
            Console.WriteLine("+----------------------+----------------------+");
            Console.WriteLine($"| Slot Label           | {slot.SlotLabel.PadRight(20)}|");
            Console.WriteLine($"| Vehicle License Plate| {vehicle.LicensePlate.PadRight(20)}|");
            Console.WriteLine($"| Entry Time           | {vehicle.EntryTime.ToString("yyyy-MM-dd HH:mm:ss").PadRight(20)}|");
            Console.WriteLine("+----------------------+----------------------+");
            return true;
        }

        public bool ExitVehicle(string slotLabel)
        {
            var slot = Slots.Find(s => s.SlotLabel == slotLabel);
            if (slot != null && slot.IsOccupied)
            {
                var vehicle = slot.OccupiedVehicle;
                DateTime exitTime = DateTime.Now;
                TimeSpan duration = exitTime - vehicle.EntryTime;
                decimal fee = CalculateParkingFee(duration);

                totalEarnings += fee;
                weeklyEarnings += fee;
                monthlyEarnings += fee;

                Console.WriteLine("\n+----------------------+----------------------+");
                Console.WriteLine("|       Details        |       Values         |");
                Console.WriteLine("+----------------------+----------------------+");
                Console.WriteLine($"| Slot Label           | {slot.SlotLabel.PadRight(20)}|");
                Console.WriteLine($"| Vehicle License Plate| {vehicle.LicensePlate.PadRight(20)}|");
                Console.WriteLine($"| Entry Time           | {vehicle.EntryTime.ToString("yyyy-MM-dd HH:mm:ss").PadRight(20)}|");
                Console.WriteLine($"| Exit Time            | {exitTime.ToString("yyyy-MM-dd HH:mm:ss").PadRight(20)}|");
                Console.WriteLine($"| Duration (hours)     | {duration.TotalHours.ToString("F2").PadRight(20)}|");
                Console.WriteLine($"| Parking Fee          | P{fee.ToString("F2").PadRight(19)}|");
                Console.WriteLine("+----------------------+----------------------+");

                slot.OccupiedVehicle = null;
                slot.IsOccupied = false;
                SaveSlotData();
                SaveEarningsData();
                return true;
            }

            Console.WriteLine("Slot is empty or invalid slot number.");
            return false;
        }

        private decimal CalculateParkingFee(TimeSpan duration)
        {
            decimal hourlyRate = 50;
            return (decimal)duration.TotalHours * hourlyRate;
        }

        public void DisplayParkingLotMap()
        {
            Console.WriteLine("\nParking Lot Map:");
            int rows = 10;

            for (int i = 0; i < Slots.Count; i++)
            {
                Console.Write($"| {Slots[i].SlotLabel} [{(Slots[i].IsOccupied ? "X" : "O")}] ");
                if ((i + 1) % rows == 0 || i == Slots.Count - 1)
                {
                    Console.WriteLine("|");
                }
            }
        }

        public void DisplayEarnings()
        {
            Console.WriteLine($"Total Earnings: P{totalEarnings:F2}");
        }

        public void DisplayWeeklyEarnings()
        {
            Console.WriteLine($"Weekly Earnings: P{weeklyEarnings:F2}");
        }

        public void DisplayMonthlyEarnings()
        {
            Console.WriteLine($"Monthly Earnings: P{monthlyEarnings:F2}");
        }

        private void SaveSlotData()
        {
            using (StreamWriter writer = new StreamWriter("ParkingLotData.txt"))
            {
                foreach (var slot in Slots)
                {
                    string line = $"{slot.SlotLabel},{slot.IsOccupied},{slot.OccupiedVehicle?.LicensePlate ?? "null"},{slot.OccupiedVehicle?.EntryTime.ToString("o") ?? "null"}";
                    writer.WriteLine(line);
                }
            }
        }

        private void SaveEarningsData()
        {
            File.WriteAllText("TotalEarnings.txt", totalEarnings.ToString("F2"));
            File.WriteAllText("WeeklyEarnings.txt", weeklyEarnings.ToString("F2"));
            File.WriteAllText("MonthlyEarnings.txt", monthlyEarnings.ToString("F2"));
        }

        private void LoadSlotData()
        {
            if (File.Exists("ParkingLotData.txt"))
            {
                using (StreamReader reader = new StreamReader("ParkingLotData.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        string slotLabel = parts[0];
                        bool isOccupied = bool.Parse(parts[1]);
                        string licensePlate = parts[2];
                        string entryTimeString = parts[3];

                        var slot = Slots.Find(s => s.SlotLabel == slotLabel);
                        if (slot != null)
                        {
                            slot.IsOccupied = isOccupied;
                            if (isOccupied && licensePlate != "null" && entryTimeString != "null")
                            {
                                slot.OccupiedVehicle = new Vehicle(licensePlate)
                                {
                                    EntryTime = DateTime.Parse(entryTimeString)
                                };
                            }
                        }
                    }
                }
            }
        }

        private void LoadEarningsData()
        {
            if (File.Exists("TotalEarnings.txt"))
            {
                totalEarnings = decimal.Parse(File.ReadAllText("TotalEarnings.txt"));
            }
            if (File.Exists("WeeklyEarnings.txt"))
            {
                weeklyEarnings = decimal.Parse(File.ReadAllText("WeeklyEarnings.txt"));
            }
            if (File.Exists("MonthlyEarnings.txt"))
            {
                monthlyEarnings = decimal.Parse(File.ReadAllText("MonthlyEarnings.txt"));
            }
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the number of parking slots: ");
            int totalSlots = int.Parse(Console.ReadLine());
            ParkingLot lot = new ParkingLot(totalSlots);

            Console.WriteLine("Welcome to the Parking Lot System!");

            while (true)
            {
                Console.WriteLine("\n1. Park Vehicle");
                Console.WriteLine("2. Exit Vehicle");
                Console.WriteLine("3. Display Parking Lot Map");
                Console.WriteLine("4. Display Total Earnings");
                Console.WriteLine("5. Display Weekly Earnings");
                Console.WriteLine("6. Display Monthly Earnings");
                Console.WriteLine("0. Exit");
                Console.Write("\nChoose an option: ");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        Console.Write("Enter vehicle license plate: ");
                        string licensePlate = Console.ReadLine();
                        Console.Write("Enter parking slot label (e.g., A1, B2, etc.): ");
                        string slotLabel = Console.ReadLine();

                        Vehicle vehicle = new Vehicle(licensePlate);
                        lot.ParkVehicle(vehicle, slotLabel);
                        break;

                    case "2":
                        Console.Write("Enter parking slot label to exit vehicle: ");
                        string exitSlot = Console.ReadLine();
                        lot.ExitVehicle(exitSlot);
                        break;

                    case "3":
                        lot.DisplayParkingLotMap();
                        break;

                    case "4":
                        lot.DisplayEarnings();
                        break;

                    case "5":
                        lot.DisplayWeeklyEarnings();
                        break;

                    case "6":
                        lot.DisplayMonthlyEarnings();
                        break;

                    case "0":
                        Console.WriteLine("Exiting the system...");
                        return;

                    default:
                        Console.WriteLine("Invalid option! Please try again.");
                        break;
                }
            }
        }
    }
}
