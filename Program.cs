/****************/
// Mã sinh viên: 202418929
// Họ tên: Hoàng Chí Kiệt
/****************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab05_EquipmentManagement
{
    // ================= ENUM & INTERFACE =================
    public enum DeviceStatus
    {
        Active,
        UnderMaintenance,
        Retired
    }

    public interface INetworkable
    {
        string IpAddress { get; }
        bool IsConnected { get; }
        void Connect(string ipAddress);
        void Disconnect();
    }

    // ================= ABSTRACT CLASS =================
    public abstract class Device
    {
        public string Id { get; private set; } // Chỉ set lúc khởi tạo
        public string Name { get; protected set; }
        public int Year { get; protected set; }
        public decimal Price { get; protected set; }
        public DeviceStatus Status { get; set; }

        public Device(string id, string name, int year, decimal price, DeviceStatus status = DeviceStatus.Active)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Mã thiết bị không được để trống.");
            if (price <= 0)
                throw new ArgumentException("Giá mua phải lớn hơn 0.");
            if (year > DateTime.Now.Year)
                throw new ArgumentException("Năm mua không thể đến từ tương lai.");

            Id = id;
            Name = name;
            Year = year;
            Price = price;
            Status = status;
        }

        public abstract decimal CalculateMaintenanceCost();

        public override string ToString()
        {
            return $"[{Id}] {Name} - Nam: {Year} - Gia: {Price:N0} - Trang thai: {Status}";
        }
    }

    // ================= CONCRETE CLASSES =================
    public class Computer : Device, INetworkable
    {
        public int Ram { get; set; }
        public string Cpu { get; set; }
        public bool HasGpu { get; set; }

        public string IpAddress { get; private set; }
        public bool IsConnected { get; private set; }

        public Computer(string id, string name, int year, decimal price, int ram, string cpu, bool hasGpu)
            : base(id, name, year, price)
        {
            Ram = ram;
            Cpu = cpu;
            HasGpu = hasGpu;
            IsConnected = false;
        }

        public override decimal CalculateMaintenanceCost()
        {
            decimal cost = Price * 0.05m;
            if (HasGpu) cost += Price * 0.02m;
            if (DateTime.Now.Year - Year > 5) cost += Price * 0.01m;
            return cost;
        }

        public void Connect(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) throw new ArgumentException("IP rỗng sao kết nối?");
            if (IsConnected) throw new InvalidOperationException($"Máy {Id} đang có mạng rồi, ngắt kết nối trước đi.");
            
            IpAddress = ipAddress;
            IsConnected = true;
        }

        public void Disconnect()
        {
            IpAddress = null;
            IsConnected = false;
        }

        public override string ToString() => base.ToString() + $" | PC: {Ram}GB, GPU: {HasGpu}";
    }

    public class Printer : Device
    {
        public bool IsLaser { get; set; }
        public int PageCount { get; set; }
        public bool IsColor { get; set; }

        public Printer(string id, string name, int year, decimal price, bool isLaser, int pageCount, bool isColor)
            : base(id, name, year, price)
        {
            IsLaser = isLaser;
            PageCount = pageCount;
            IsColor = isColor;
        }

        public override decimal CalculateMaintenanceCost()
        {
            decimal cost = Price * 0.04m;
            if (PageCount > 100000) cost += 500000m;
            if (IsColor) cost += 300000m;
            return cost;
        }

        public override string ToString() => base.ToString() + $" | Printer: {PageCount} trang, Mau: {IsColor}";
    }

    // Trade-off: Tách class máy in mạng ra riêng để không ảnh hưởng máy in thường
    public class NetworkPrinter : Printer, INetworkable
    {
        public string IpAddress { get; private set; }
        public bool IsConnected { get; private set; }

        public NetworkPrinter(string id, string name, int year, decimal price, bool isLaser, int pageCount, bool isColor) 
            : base(id, name, year, price, isLaser, pageCount, isColor) { }

        public void Connect(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) throw new ArgumentException("Thiếu IP.");
            if (IsConnected) throw new InvalidOperationException("Đã kết nối.");
            IpAddress = ipAddress;
            IsConnected = true;
        }

        public void Disconnect()
        {
            IpAddress = null;
            IsConnected = false;
        }
    }

    public class Projector : Device
    {
        public int Lumen { get; set; }
        public int LampHours { get; set; }

        public Projector(string id, string name, int year, decimal price, int lumen, int lampHours)
            : base(id, name, year, price)
        {
            Lumen = lumen;
            LampHours = lampHours;
        }

        public override decimal CalculateMaintenanceCost()
        {
            decimal cost = Price * 0.03m;
            if (LampHours > 3000) cost += 1500000m;
            return cost;
        }

        public override string ToString() => base.ToString() + $" | Projector: {LampHours}h bong den";
    }

    // ================= AGGREGATION CLASS =================
    public class LabRoom
    {
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        public int Capacity { get; set; }
        private List<Device> Devices { get; set; }

        public LabRoom(string id, string name, int capacity)
        {
            RoomId = id;
            RoomName = name;
            Capacity = capacity;
            Devices = new List<Device>();
        }

        public void AddDevice(Device device)
        {
            if (device == null) throw new ArgumentNullException("Thêm thiết bị null thì thêm làm gì?");
            if (Devices.Any(d => d.Id == device.Id))
            {
                Console.WriteLine($"[Cảnh báo] Trùng ID! Cục máy {device.Id} đang nằm trong phòng này rồi.");
                return;
            }
            Devices.Add(device);
        }

        public bool RemoveDevice(string deviceId)
        {
            var device = FindDevice(deviceId);
            if (device != null)
            {
                Devices.Remove(device);
                return true;
            }
            return false;
        }

        public Device FindDevice(string deviceId)
        {
            return Devices.FirstOrDefault(d => d.Id == deviceId);
        }

        // TÍNH ĐA HÌNH Ở ĐÂY: Quét danh sách tính tiền, không quan tâm nó là máy gì
        public decimal CalculateAnnualMaintenanceCost()
        {
            decimal total = 0;
            foreach (var d in Devices)
            {
                total += d.CalculateMaintenanceCost();
            }
            return total;
        }

        public List<Device> GetDevicesRequiringMaintenance()
        {
            return Devices.Where(d => d.Status == DeviceStatus.UnderMaintenance || (DateTime.Now.Year - d.Year > 5)).ToList();
        }

        public void PrintDevices()
        {
            Console.WriteLine($"\n=== Danh sach thiet bi phong {RoomName} ({RoomId}) ===");
            foreach (var d in Devices) Console.WriteLine(d.ToString());
            Console.WriteLine($"=> TONG PHI BAO TRI: {CalculateAnnualMaintenanceCost():N0} VND");
        }
        
        public List<Device> GetAllDevices() => Devices;
    }

    // ================= MAIN PROGRAM =================
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Khoi tao cac thiet bi theo kịch bản
                Computer pc1 = new Computer("PC01", "Dell Optiplex", 2023, 15000000m, 16, "i5", false);
                Computer pc2 = new Computer("PC02", "Alienware", 2021, 30000000m, 32, "i7", true); // Co GPU
                
                Printer prn1 = new Printer("PR01", "Canon 2900", 2022, 3500000m, true, 5000, false);
                NetworkPrinter prn2 = new NetworkPrinter("PR02", "HP Color", 2018, 8000000m, true, 120000, true); // >100k trang, cu > 5 nam

                Projector proj1 = new Projector("PJ01", "Sony P10", 2020, 20000000m, 3000, 3500); // > 3000 gio
                proj1.Status = DeviceStatus.UnderMaintenance; // Set thu trang thai

                // Khoi tao phong Lab
                LabRoom lab1 = new LabRoom("LAB_A1", "Phong May Tinh", 40);
                LabRoom lab2 = new LabRoom("LAB_A2", "Phong In An", 10);

                // 1. Them vao phong
                lab1.AddDevice(pc1);
                lab1.AddDevice(pc2);
                lab2.AddDevice(prn1);
                lab2.AddDevice(prn2);
                lab2.AddDevice(proj1);

                // 2. Thu them trung ma
                Console.WriteLine("--- TEST TRUNG MA ---");
                lab1.AddDevice(pc1);

                // 3 & 4. In danh sach va phi bao tri
                lab1.PrintDevices();
                lab2.PrintDevices();

                // 5. Liet ke can bao tri
                Console.WriteLine("\n--- THIET BI CAN BAO TRI (Cu hoac Dang hong) ---");
                var badDevices = new List<Device>();
                badDevices.AddRange(lab1.GetDevicesRequiringMaintenance());
                badDevices.AddRange(lab2.GetDevicesRequiringMaintenance());
                foreach (var bd in badDevices) Console.WriteLine(bd.Name);

                // 6 & 7. Test interface mang (Da hinh Interface)
                Console.WriteLine("\n--- TEST KET NOI MANG ---");
                var allNetworkDevices = lab1.GetAllDevices().Concat(lab2.GetAllDevices()).OfType<INetworkable>().ToList();
                
                string baseIp = "192.168.1.";
                int ipCounter = 10;
                
                foreach (var netDevice in allNetworkDevices)
                {
                    netDevice.Connect(baseIp + ipCounter);
                    Console.WriteLine($"Đã cấp IP {netDevice.IpAddress} cho thiết bị.");
                    ipCounter++;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi roi: " + ex.Message);
            }
        }
    }
}