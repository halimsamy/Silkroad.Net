using System.Diagnostics;

namespace Silkroad {
    public static class Loader {
        /// <summary>
        /// Starts a new Silkroad client instance.
        ///
        /// This requires `Silkroad.Loader.exe` and `Silkroad.Loader.dll` to be
        /// present at the same directory.
        /// 
        /// If the Gateway was redirected it will be redirected to the
        /// localhost/127.0.0.1 and the port will be the same as the started `sro_client.exe` process ID.
        /// </summary>
        /// <param name="clientDirectory">The path of the client directory (e.g. C:\Silkroad)</param>
        /// <param name="redirectGateway">Indicates if the Gateway connection should be redirected</param>
        /// <returns>The sro_client process or null of can't start it</returns>
        public static Process Start(string clientDirectory, bool redirectGateway = false) {
            var loader = Process.Start("Silkroad.Loader.exe",
                clientDirectory + " Silkroad.Loader.dll" + (redirectGateway ? " redirect" : ""));

            if (loader == null) return null;

            loader.WaitForExit();

            var processId = loader.ExitCode;

            return processId == 0 ? null : Process.GetProcessById(processId);
        }
    }
}