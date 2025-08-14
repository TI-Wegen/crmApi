namespace Boletos.Domain.Entities;

    public class Platform
{
    public Platform(string name)
    {
        Name = name;
    }

    public string Name { get; private set; }
}

