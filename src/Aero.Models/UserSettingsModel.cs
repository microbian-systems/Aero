using Aero.Core.Entities;
using Aero.Models.Entities;

namespace Aero.Models;

public class AeroUserSettings : Entity
{
    public string UserId { get; set; } // foreign key
    public object Stuff { get; set; }
}