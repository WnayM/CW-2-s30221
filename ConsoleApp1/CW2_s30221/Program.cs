using System;
using System.Collections.Generic;
using System.Linq;

class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

interface IHazardNotifier
{
    void NotifyHazard(string message);
}

abstract class Container
{
    private static int counter = 1;
    public string SerialNumber { get; private set; }
    public double MaxCapacity { get; private set; }
    public double Height { get; private set; }
    public double Depth { get; private set; }
    public double OwnWeight { get; private set; }
    protected double currentLoad;

    public double CurrentLoad
    {
        get { return currentLoad; }
        protected set { currentLoad = value; }
    }

    protected Container(string type, double maxCapacity, double height, double depth, double ownWeight)
    {
        SerialNumber = $"KON-{type}-{counter++}";
        MaxCapacity = maxCapacity;
        Height = height;
        Depth = depth;
        OwnWeight = ownWeight;
        CurrentLoad = 0;
    }

    public virtual void Load(double weight)
    {
        if (CurrentLoad + weight > MaxCapacity)
            throw new OverfillException($"Przeładowanie kontenera {SerialNumber}! Maksymalna pojemność to {MaxCapacity} kg.");
        CurrentLoad += weight;
    }

    public virtual void Unload()
    {
        CurrentLoad = 0;
    }

    public override string ToString()
    {
        return $"{SerialNumber} - Załadunek: {CurrentLoad}/{MaxCapacity} kg, Wysokość: {Height} cm, Głębokość: {Depth} cm, Waga własna: {OwnWeight} kg";
    }
}

class LiquidContainer : Container, IHazardNotifier
{
    public bool IsHazardous { get; private set; }

    public LiquidContainer(double maxCapacity, bool isHazardous, double height, double depth, double ownWeight) 
        : base("L", maxCapacity, height, depth, ownWeight)
    {
        IsHazardous = isHazardous;
    }

    public override void Load(double weight)
    {
        double limit = IsHazardous ? MaxCapacity * 0.5 : MaxCapacity * 0.9;
        if (CurrentLoad + weight > limit)
            throw new OverfillException($"Próba przeładowania kontenera {SerialNumber}! Maksymalna pojemność: {limit} kg.");
        base.Load(weight);
    }/*
    */
    

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[UWAGA] {message}");
    }
}

class GasContainer : Container, IHazardNotifier
{
    public double Pressure { get; private set; }

    public GasContainer(double maxCapacity, double pressure, double height, double depth, double ownWeight) 
        : base("G", maxCapacity, height, depth, ownWeight)
    {
        Pressure = pressure;
    }

    public override void Unload()
    {
        CurrentLoad *= 0.05;
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[UWAGA] {message}");
    }
}

class RefrigeratedContainer : Container
{
    public string ProductType { get; private set; }
    public double Temperature { get; private set; }

    public RefrigeratedContainer(double maxCapacity, string productType, double temperature, double height, double depth, double ownWeight)
        : base("C", maxCapacity, height, depth, ownWeight)
    {
        ProductType = productType;
        Temperature = temperature;
    }

    public override string ToString()
    {
        return base.ToString() + $" - Produkt: {ProductType}, Temperatura: {Temperature}°C";
    }
}

class Ship
{
    public string Name { get; private set; }
    public int MaxContainers { get; private set; }
    public double MaxWeight { get; private set; }
    public List<Container> Containers { get; private set; }

    public Ship(string name, int maxContainers, double maxWeight)
    {
        Name = name;
        MaxContainers = maxContainers;
        MaxWeight = maxWeight;
        Containers = new List<Container>();
    }

    public void AddContainer(Container container)
    {
        if (Containers.Count >= MaxContainers)
            throw new Exception("Statek osiągnął maksymalną liczbę kontenerów!");
        double totalWeight = Containers.Sum(c => c.CurrentLoad) + container.CurrentLoad;
        if (totalWeight > MaxWeight)
            throw new Exception("Statek nie może przewozić więcej ładunku!");
        Containers.Add(container);
    }

    public void RemoveContainer(string serialNumber)
    {
        Containers.RemoveAll(c => c.SerialNumber == serialNumber);
    }

    public void ReplaceContainer(string serialNumber, Container newContainer)
    {
        RemoveContainer(serialNumber);
        AddContainer(newContainer);
    }

    public static void TransferContainer(Ship fromShip, Ship toShip, string serialNumber)
    {
        var container = fromShip.Containers.FirstOrDefault(c => c.SerialNumber == serialNumber);
        if (container == null)
            throw new Exception($"Kontener {serialNumber} nie znajduje się na statku {fromShip.Name}.");
        fromShip.RemoveContainer(serialNumber);
        toShip.AddContainer(container);
    }

    public void ShowInfo()
    {
        Console.WriteLine($"Statek: {Name}, Maksymalna ilość kontenerów: {MaxContainers}, Maksymalna waga: {MaxWeight}kg");
        Console.WriteLine("Kontenery na pokładzie:");
        foreach (var container in Containers)
        {
            Console.WriteLine(container);
        }
    }
}


class Program
{
    static void Main()
    {
        Ship ship1 = new Ship("OceanKing", 5, 10000);
        
        Container c1 = new RefrigeratedContainer(5000, "Bananas", 13.3, 250, 300, 1000);
        try
        {
            c1.Load(2000);
            Console.WriteLine($"Załadowano ładunek: {c1}");
        }
        catch (OverfillException ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
        
        Container c2 = new LiquidContainer(3000, true, 250, 300, 800);
        Container c3 = new GasContainer(4000, 2.5, 250, 300, 900);
        
        try
        {
            ship1.AddContainer(c1);
            ship1.AddContainer(c2);
            ship1.AddContainer(c3);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
        
        ship1.ShowInfo();
    }
}
