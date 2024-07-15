using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendEase.Shared.Services;
public interface IFmNetService
{
    Task Login(string username, string password);
}