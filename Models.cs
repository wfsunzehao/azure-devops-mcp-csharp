using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureDevOpsMcp;

/// <summary>
/// 测试计划数据模型
/// </summary>
public class TestPlan
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("state")]
    public string State { get; set; } = "";

    [JsonPropertyName("rootSuiteId")]
    public int RootSuiteId { get; set; }

    [JsonPropertyName("iteration")]
    public string Iteration { get; set; } = "";

    [JsonPropertyName("owner")]
    public Owner Owner { get; set; } = new();
}

public class Owner
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "";

    [JsonPropertyName("uniqueName")]
    public string UniqueName { get; set; } = "";
}

/// <summary>
/// 测试用例数据模型
/// </summary>
public class TestCase
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("workItem")]
    public WorkItem WorkItem { get; set; } = new();

    [JsonPropertyName("testMethod")]
    public string? TestMethod { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}

public class WorkItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

/// <summary>
/// 测试结果数据模型
/// </summary>
public class TestResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("testCase")]
    public TestCaseRef TestCase { get; set; } = new();

    [JsonPropertyName("state")]
    public string State { get; set; } = "";

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = "";

    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    [JsonPropertyName("startedDate")]
    public string StartedDate { get; set; } = "";

    [JsonPropertyName("completedDate")]
    public string CompletedDate { get; set; } = "";
}

public class TestCaseRef
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

/// <summary>
/// 测试运行数据模型
/// </summary>
public class TestRun
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("state")]
    public string State { get; set; } = "";

    [JsonPropertyName("createdDate")]
    public string CreatedDate { get; set; } = "";
}

/// <summary>
/// API 响应包装器
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("value")]
    public List<T> Value { get; set; } = new();

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
