using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aero.Actors.Abstractions;

public interface IAeroActor : IGrainWithIntegerKey;

public interface IPingGrain : IAeroActor
{
    Task<Message> Ping();
}
