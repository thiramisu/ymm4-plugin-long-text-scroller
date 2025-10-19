using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;

namespace LongTextScrollerPlugin.Utils;

public readonly record struct VersionCheckResult
{
    public string? LatestTagName { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// 指定された GitHub リポジトリの最新リリースを確認します。
/// </summary>
public static class GitHubUpdateChecker
{
    /// <summary>
    /// リポジトリごとのキャッシュ結果
    /// </summary>
    static readonly ConcurrentDictionary<(string repoOwner, string repoName), VersionCheckResult> _cachedResults = new();

    /// <summary>
    /// リポジトリごとの進行中リクエスト
    /// </summary>
    static readonly ConcurrentDictionary<(string repoOwner, string repoName), Task<VersionCheckResult>> _ongoingRequests = new();

    /// <summary>
    /// 指定された GitHub リポジトリの最新リリースを確認します。
    /// </summary>
    public static Task<VersionCheckResult> CheckLatestVersionAsync(string repoOwner, string repoName)
    {
        if (string.IsNullOrWhiteSpace(repoOwner))
            throw new ArgumentException($"{nameof(repoOwner)} が空です。");
        if (string.IsNullOrWhiteSpace(repoName))
            throw new ArgumentException($"{nameof(repoName)} が空です。");

        // キャッシュがあればそれを返す
        // クライアントごとのAPI呼び出し回数に時間あたりの制限があるので、キャッシュがあれば常にそれを利用
        if (_cachedResults.TryGetValue((repoOwner, repoName), out var cachedResult))
        {
            return Task.FromResult(cachedResult);
        }

        // 進行中のリクエストがあればそれを返し、なければ新しいリクエストを登録してから返す
        var task = _ongoingRequests.GetOrAdd(
            (repoOwner, repoName),
            _ => FetchLatestVersionAsync(repoOwner, repoName)
        );
        return task;
    }

    private static async Task<VersionCheckResult> FetchLatestVersionAsync(string repoOwner, string repoName)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", $"{repoName}-UpdateChecker");

            var url = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";
            var json = await client.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            var tagName = doc.RootElement.GetProperty("tag_name").GetString();

            if (string.IsNullOrEmpty(tagName))
            {
                return new VersionCheckResult { ErrorMessage = "JSON 中の tag_name の解析に失敗しました。" };
            }

            var result = new VersionCheckResult
            {
                LatestTagName = tagName
            };

            // 結果をキャッシュに追加
            _cachedResults[(repoOwner, repoName)] = result;
            return result;
        }
        catch (Exception ex)
        {
            return new VersionCheckResult { ErrorMessage = ex.Message };
        }
        finally
        {
            // リクエスト完了後は進行中リストから削除
            _ = _ongoingRequests.Remove((repoOwner, repoName), out _);
        }
    }
}
