﻿using System.Net;

namespace NScan.Core
{
    public class ScanService(IPAddress ipAddress, int startPort, int endPort, int timeoutMilliseconds)
    {
        private readonly IPAddress _ipAddress = ipAddress;
        private readonly int _startPort = startPort;
        private readonly int _endPort = endPort;
        private readonly int _timeoutMilliseconds = timeoutMilliseconds;
        private int _openPorts = 0;
        private List<int> _openPortList = [];
        private readonly PortScanner portScanner = new();

        public int GetPortsScanned()
        {
            return portScanner.PortsScanned;
        }

        public async Task<List<int>> ScanPorts(ScanMethod scanMethod)
        {
            switch (scanMethod)
            {
                case ScanMethod.SingleThreaded:
                    PerformSingleThreadedScan(portScanner);
                    break;
                case ScanMethod.MultiThreaded:
                    await PerformMultiThreadedScan(portScanner);
                    break;
                default:
                    throw new ArgumentException("Invalid scan method");
            }

            return _openPortList;
        }

        private void PerformSingleThreadedScan(PortScanner portScanner)
        {
            for (int port = _startPort; port <= _endPort; port++)
            {
                _openPortList = portScanner.Scan(_ipAddress, _timeoutMilliseconds, port, port, ref _openPorts, _openPortList);
            }
        }

        private async Task PerformMultiThreadedScan(PortScanner portScanner)
        {
            int threadCount = GetThreadCount();
            List<Task> tasks = [];

            int portRange = _endPort - _startPort + 1;
            int batchSize = portRange / threadCount;
            int remainingPorts = portRange % threadCount;

            for (int i = 0; i < threadCount; i++)
            {
                int extraPorts = (i < remainingPorts) ? 1 : 0; // Distribute remaining ports
                int batchStartPort = _startPort + i * (batchSize + extraPorts);
                int batchEndPort = Math.Min(_endPort, batchStartPort + batchSize + extraPorts - 1);

                tasks.Add(ScanPortRangeAsync(portScanner, batchStartPort, batchEndPort));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ScanPortRangeAsync(PortScanner portScanner, int startPort, int endPort)
        {
            for (int port = startPort; port <= endPort; port++)
            {
                await portScanner.ScanAsync(_ipAddress, _timeoutMilliseconds, port, _openPorts, _openPortList);
            }
        }

        private static int GetThreadCount()
        {
            // Multiplier can be adjusted in the future for performance tuning
            return GetThreadCountWithMultiplier(1);
        }

        private static int GetThreadCountWithMultiplier(int multiplier)
        {
            int threadCount = Environment.ProcessorCount * multiplier;

            // Ensure thread count is within bounds
            if (threadCount < 1)
            {
                threadCount = 1;
            }
            else if (threadCount > 64)
            {
                threadCount = 64;
            }

            return threadCount;
        }
    }
}
