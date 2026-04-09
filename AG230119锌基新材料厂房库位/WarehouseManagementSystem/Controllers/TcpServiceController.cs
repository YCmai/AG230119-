using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace WarehouseManagementSystem.Controllers;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

using WarehouseManagementSystem.Models.TcpService;
using WarehouseManagementSystem.Service.TcpService;
using Newtonsoft.Json;
using WarehouseManagementSystem.Services;

public class TcpServiceController : BaseController
{
    private readonly IClientDataService _dataService;
    private readonly IClientManagerService _clientManagerService;
    private readonly IConfiguration _configuration;
    private readonly IMessageHistoryService _messageHistoryService;
    public TcpServiceController(IMessageHistoryService messageHistoryService, IConfiguration configuration, ISystemExpirationService expirationService)
        : base(expirationService)
    {
        _messageHistoryService = messageHistoryService;
        _configuration = configuration;
    }

    public IActionResult Index(int page = 1, int pageSize = 10, string clientEndpoint = null)
    {
        var viewModel = new MessageViewModel
        {
            Messages = _messageHistoryService.GetMessages(page, pageSize, clientEndpoint),
            ClientStatuses = _messageHistoryService.GetClientStatusesAsync().Result,
            CurrentPage = page,
            PageSize = pageSize,
            TotalMessages = _messageHistoryService.GetTotalMessageCount(clientEndpoint),
            SelectedClient = clientEndpoint,
            Statistics = _messageHistoryService.GetMessageStatistics(
                DateTime.Now.AddHours(-24),
                DateTime.Now
            )
        };

        return View(viewModel);
    }

    // ����API�˵����ڻ�ȡ��ҳ����
    [HttpGet]
    public IActionResult GetMessages(int page = 1, int pageSize = 10, string clientEndpoint = null)
    {
        var messages = _messageHistoryService.GetMessages(page, pageSize, clientEndpoint);
        var total = _messageHistoryService.GetTotalMessageCount(clientEndpoint);

        return Json(new
        {
            data = messages,
            total = total,
            currentPage = page,
            totalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }


    [HttpGet]
    public IActionResult GetStatistics()
    {
        var statistics = _messageHistoryService.GetMessageStatistics(
            DateTime.Now.AddHours(-24),
            DateTime.Now
        );
        return Json(statistics);
    }

  
}



