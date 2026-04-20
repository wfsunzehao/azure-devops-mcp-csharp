using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureDevOpsMcp;

/// <summary>
/// Azure DevOps API 客户端
/// 用于调用 Azure DevOps REST API 获取测试计划、测试用例等数据
/// </summary>
public class AzureDevOpsClient
{
    private readonly HttpClient _httpClient;
    private readonly string _org;
    private readonly string _project;
    private readonly JsonSerializerOptions _jsonOptions;

    public AzureDevOpsClient(string org, string project, string token)
    {
        _org = org;
        _project = project;
        _httpClient = new HttpClient();

        // 设置基础 URL
        _httpClient.BaseAddress = new Uri($"{org}/{project}/_apis/");

        // 设置认证头（PAT Token）
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{token}"));
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", credentials);

        // JSON 序列化配置
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// 获取所有测试计划
    /// </summary>
    public async Task<List<TestPlan>> GetTestPlansAsync()
    {
        try
        {
            var url = "test/plans?api-version=7.0";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TestPlan>>(json, _jsonOptions);

            return apiResponse?.Value ?? new List<TestPlan>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching test plans: {ex.Message}");
            throw new Exception($"Failed to fetch test plans: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取特定测试计划的详情
    /// </summary>
    public async Task<TestPlan?> GetTestPlanDetailsAsync(int planId)
    {
        try
        {
            var url = $"test/plans/{planId}?api-version=7.0";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var plan = JsonSerializer.Deserialize<TestPlan>(json, _jsonOptions);

            return plan;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching test plan {planId}: {ex.Message}");
            throw new Exception($"Failed to fetch test plan {planId}", ex);
        }
    }

    /// <summary>
    /// 获取测试计划中的测试用例
    /// </summary>
    public async Task<List<TestCase>> GetTestCasesAsync(int planId, int? suiteId = null, string? search = null)
    {
        try
        {
            // 如果没有指定 suiteId，先获取计划的根 suite
            int rootSuiteId = suiteId ?? (await GetTestPlanDetailsAsync(planId))?.RootSuiteId ?? 0;

            if (rootSuiteId == 0)
                throw new Exception("Could not determine test suite ID");

            var url = $"test/plans/{planId}/suites/{rootSuiteId}/testcases?api-version=7.0";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TestCase>>(json, _jsonOptions);
            var testCases = apiResponse?.Value ?? new List<TestCase>();

            // 如果提供了搜索关键词，进行过滤
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                testCases = testCases.Where(tc =>
                    tc.WorkItem?.Name?.ToLower().Contains(searchLower) == true ||
                    tc.TestMethod?.ToLower().Contains(searchLower) == true
                ).ToList();
            }

            return testCases;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching test cases for plan {planId}: {ex.Message}");
            throw new Exception($"Failed to fetch test cases for plan {planId}", ex);
        }
    }

    /// <summary>
    /// 获取测试运行结果
    /// </summary>
    public async Task<List<TestResult>> GetTestResultsAsync(int? runId = null, string? state = null, int limit = 50)
    {
        try
        {
            string url = runId.HasValue
                ? $"test/runs/{runId}/results?api-version=7.0&$top={limit}"
                : $"test/runs?api-version=7.0&$top={limit}";

            if (!string.IsNullOrWhiteSpace(state))
                url += $"&state={state}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TestResult>>(json, _jsonOptions);

            return apiResponse?.Value ?? new List<TestResult>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching test results: {ex.Message}");
            throw new Exception($"Failed to fetch test results: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取所有测试运行
    /// </summary>
    public async Task<List<TestRun>> GetTestRunsAsync(int limit = 50)
    {
        try
        {
            var url = $"test/runs?api-version=7.0&$top={limit}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TestRun>>(json, _jsonOptions);

            return apiResponse?.Value ?? new List<TestRun>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching test runs: {ex.Message}");
            throw new Exception("Failed to fetch test runs", ex);
        }
    }

    /// <summary>
    /// 创建测试运行
    /// </summary>
    public async Task<TestRun?> CreateTestRunAsync(int testPlanId, int testSuiteId, string name)
    {
        try
        {
            var url = $"test/runs?api-version=7.0";

            var payload = new
            {
                name = name,
                plan = new { id = testPlanId },
                pointIds = new[] { testSuiteId },
                isAutomated = false
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var testRun = JsonSerializer.Deserialize<TestRun>(responseJson, _jsonOptions);

            return testRun;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error creating test run: {ex.Message}");
            throw new Exception("Failed to create test run", ex);
        }
    }
}
