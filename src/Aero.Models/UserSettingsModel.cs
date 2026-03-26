using Aero.Core.Entities;

namespace Aero.Models;

public class AeroUserSettings : Entity
{
    public string UserId { get; set; } // foreign key
    public object Stuff { get; set; }
}