using System.Net.Sockets;
using System.Net;
using NModbus;
using WarehouseManagementSystem.Hubs.TcpClient.Hubs;
using System.Data;
using WarehouseManagementSystem.Models.IO;
using WarehouseManagementSystem.Db;
using Dapper;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using WarehouseManagementSystem.Hubs;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Serilog.Core;

namespace Services.Tasks
{
    public class TaskMonitoringService : BackgroundService
    {
        private readonly ILogger<TaskMonitoringService> _logger;
        private readonly IDatabaseService _db;
        private readonly ITaskGenerationService _taskGenerationService;

        public TaskMonitoringService(
            ILogger<TaskMonitoringService> logger,
            IDatabaseService db,
            ITaskGenerationService taskGenerationService)
        {
            _logger = logger;
            _db = db;
            _taskGenerationService = taskGenerationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("任务监测开始");

            while (!stoppingToken.IsCancellationRequested)
            {
               
            }
        }
    }
} 