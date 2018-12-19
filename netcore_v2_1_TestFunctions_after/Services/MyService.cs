using System;
using System.Collections.Generic;
using System.Text;

namespace netcore_v2_1_TestFunctions_after.Services
{
    public class MyService : IMyService
    {
        public MyService()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
    }
}
