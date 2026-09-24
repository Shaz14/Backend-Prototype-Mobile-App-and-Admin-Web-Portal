using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.ViewModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DHAFacilitationAPIs.Infrastructure.Service;
public class SmartPayService : ISmartPayService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public SmartPayService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<BillUploadResult> UploadBillAsync(BillUploadRequest request, CancellationToken cancellationToken)
    {
        var url = $"{_config["SmartPay:BaseUrl"]}/api/1.0/bill/push";
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("MAPIKey", _config["SmartPay:MAPIKey"]);

        var response = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonConvert.DeserializeObject<BillUploadResult>(body)
               ?? throw new InvalidOperationException("Empty SmartPay bill upload response");
    }

    public async Task<BillInquiryResult> InquireBillAsync(string consumerNumber, CancellationToken cancellationToken)
    {
        var url = $"{_config["SmartPay:BaseUrl"]}/api/1.0/bill/billInquiry?consumerNumber={consumerNumber}";
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("MAPIKey", _config["SmartPay:MAPIKey"]);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonConvert.DeserializeObject<BillInquiryResult>(body)
               ?? throw new InvalidOperationException("Empty SmartPay inquiry response");
    }
}

