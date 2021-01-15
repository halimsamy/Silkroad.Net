using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Silkroad.Network;
using Silkroad.Network.Messaging;

namespace Silkroad.Bench {
    internal class BenchingService {
        [MessageService(0xDEAD)]
        public Task Bench(Session session, Message msg) {
            return Task.CompletedTask;
        }
    }

    [MemoryDiagnoser]
    [MaxColumn]
    [MinColumn]
    public class ServiceRespondBench {
        private readonly Message _msg = new Message(0xDEAD);
        private readonly Session _session = new Session();

        public ServiceRespondBench() {
            for (var i = 0; i < 100; i++) {
                this._session.RegisterService<BenchingService>();
            }
        }

        [Benchmark]
        public void Bench() {
            this._session.RespondAsync(this._msg).GetAwaiter();
        }
    }
}