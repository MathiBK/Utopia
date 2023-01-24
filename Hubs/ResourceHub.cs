using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using utopia.Data;

namespace utopia.Hubs
{
    public class ResourceHub : Hub
    {
        public async Task DecrementResource(string resource)
        {

        }
    }
}