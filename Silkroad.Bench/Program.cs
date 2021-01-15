using BenchmarkDotNet.Running;

namespace Silkroad.Bench {
    internal class Program {
        private static void Main(string[] args) {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAllJoined();
        }
    }
}