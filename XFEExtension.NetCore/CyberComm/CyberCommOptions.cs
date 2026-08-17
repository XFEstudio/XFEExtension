using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace XFEExtension.NetCore.CyberComm;

/// <summary>
/// CyberComm 服务端状态。
/// </summary>
public enum CyberCommServerState
{
    Created,
    Starting,
    Running,
    Stopping,
    Stopped,
    Faulted
}

/// <summary>
/// CyberComm 客户端状态。
/// </summary>
public enum CyberCommClientState
{
    Created,
    Connecting,
    Connected,
    Disconnecting,
    Disconnected,
    Faulted
}

/// <summary>
/// CyberComm 传输限制。
/// </summary>
public sealed record CyberCommLimitOptions
{
    public int MaxRequestLineBytes { get; init; } = 8 * 1024;
    public int MaxHeaderBytes { get; init; } = 32 * 1024;
    public long MaxHttpBodyBytes { get; init; } = 8 * 1024 * 1024;
    public int MaxHttpChunkBytes { get; init; } = 1024 * 1024;
    public int MaxWebSocketFrameBytes { get; init; } = 1024 * 1024;
    public long MaxWebSocketMessageBytes { get; init; } = 8 * 1024 * 1024;
    public int MaxConnections { get; init; } = 4096;
    public int MaxConnectionsPerIp { get; init; } = 128;
    public int InboundQueueCapacity { get; init; } = 256;
    public int OutboundQueueCapacity { get; init; } = 256;
    public TimeSpan ReceiveTimeout { get; init; } = TimeSpan.FromMinutes(2);
    public TimeSpan SendTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan IdleTimeout { get; init; } = TimeSpan.FromMinutes(2);
    public TimeSpan HandlerTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan CloseTimeout { get; init; } = TimeSpan.FromSeconds(10);

    internal void Validate()
    {
        if (MaxRequestLineBytes < 256) throw new ArgumentOutOfRangeException(nameof(MaxRequestLineBytes));
        if (MaxHeaderBytes < 1024) throw new ArgumentOutOfRangeException(nameof(MaxHeaderBytes));
        if (MaxHttpBodyBytes < 0) throw new ArgumentOutOfRangeException(nameof(MaxHttpBodyBytes));
        if (MaxHttpBodyBytes > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(MaxHttpBodyBytes), "当前缓冲式 HTTP 管线最多支持 2GB 请求体");
        if (MaxHttpChunkBytes <= 0 || MaxHttpChunkBytes > MaxHttpBodyBytes) throw new ArgumentOutOfRangeException(nameof(MaxHttpChunkBytes));
        if (MaxWebSocketFrameBytes <= 0) throw new ArgumentOutOfRangeException(nameof(MaxWebSocketFrameBytes));
        if (MaxWebSocketMessageBytes < MaxWebSocketFrameBytes) throw new ArgumentOutOfRangeException(nameof(MaxWebSocketMessageBytes));
        if (MaxConnections <= 0) throw new ArgumentOutOfRangeException(nameof(MaxConnections));
        if (MaxConnectionsPerIp <= 0 || MaxConnectionsPerIp > MaxConnections) throw new ArgumentOutOfRangeException(nameof(MaxConnectionsPerIp));
        if (InboundQueueCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(InboundQueueCapacity));
        if (OutboundQueueCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(OutboundQueueCapacity));
        ValidateTimeout(ReceiveTimeout, nameof(ReceiveTimeout));
        ValidateTimeout(SendTimeout, nameof(SendTimeout));
        ValidateTimeout(IdleTimeout, nameof(IdleTimeout));
        ValidateTimeout(HandlerTimeout, nameof(HandlerTimeout));
        ValidateTimeout(CloseTimeout, nameof(CloseTimeout));
    }

    private static void ValidateTimeout(TimeSpan value, string name)
    {
        if (value <= TimeSpan.Zero && value != Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(name);
    }
}

/// <summary>
/// TLS 证书配置。证书密码应由调用方从环境变量或安全输入传入。
/// </summary>
public sealed record CyberCommTlsOptions
{
    public string? PfxPath { get; init; }
    public string? PfxPassword { get; init; }
    public string? CertificatePemPath { get; init; }
    public string? PrivateKeyPemPath { get; init; }
    public X509Certificate2? Certificate { get; init; }

    internal X509Certificate2 LoadCertificate()
    {
        if (Certificate is not null)
            return Certificate;
        if (!string.IsNullOrWhiteSpace(PfxPath))
            return X509CertificateLoader.LoadPkcs12FromFile(PfxPath, PfxPassword, GetServerKeyStorageFlags());
        if (!string.IsNullOrWhiteSpace(CertificatePemPath) && !string.IsNullOrWhiteSpace(PrivateKeyPemPath))
        {
            var pemCertificate = X509Certificate2.CreateFromPemFile(CertificatePemPath, PrivateKeyPemPath);
            if (!OperatingSystem.IsWindows()) return pemCertificate;
            using (pemCertificate)
                return X509CertificateLoader.LoadPkcs12(pemCertificate.Export(X509ContentType.Pkcs12), null, GetServerKeyStorageFlags());
        }
        throw new InvalidOperationException("HTTPS/WSS 监听地址需要配置 TLS 证书");
    }

    private static X509KeyStorageFlags GetServerKeyStorageFlags() => OperatingSystem.IsWindows()
        ? X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet
        : X509KeyStorageFlags.EphemeralKeySet;

    internal void Validate()
    {
        if (Certificate is not null)
        {
            if (!Certificate.HasPrivateKey) throw new InvalidOperationException("TLS 证书必须包含私钥");
            return;
        }
        if (!string.IsNullOrWhiteSpace(PfxPath))
        {
            if (!File.Exists(PfxPath)) throw new FileNotFoundException("未找到 TLS PFX 证书", PfxPath);
            return;
        }
        if (string.IsNullOrWhiteSpace(CertificatePemPath) || string.IsNullOrWhiteSpace(PrivateKeyPemPath))
            throw new InvalidOperationException("PEM TLS 配置必须同时包含证书和私钥路径");
        if (!File.Exists(CertificatePemPath)) throw new FileNotFoundException("未找到 TLS PEM 证书", CertificatePemPath);
        if (!File.Exists(PrivateKeyPemPath)) throw new FileNotFoundException("未找到 TLS PEM 私钥", PrivateKeyPemPath);
    }
}

/// <summary>
/// 自动重连配置。
/// </summary>
public sealed record CyberCommReconnectOptions
{
    public bool Enabled { get; init; } = true;
    public int MaxAttempts { get; init; } = -1;
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromMilliseconds(100);
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(30);
    public double JitterRatio { get; init; } = 0.2;

    internal void Validate()
    {
        if (MaxAttempts < -1) throw new ArgumentOutOfRangeException(nameof(MaxAttempts));
        if (InitialDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(InitialDelay));
        if (MaxDelay < InitialDelay) throw new ArgumentOutOfRangeException(nameof(MaxDelay));
        if (JitterRatio is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(JitterRatio));
    }

    internal TimeSpan GetDelay(int attempt)
    {
        var multiplier = Math.Pow(2, Math.Clamp(attempt - 1, 0, 20));
        var milliseconds = Math.Min(MaxDelay.TotalMilliseconds, InitialDelay.TotalMilliseconds * multiplier);
        var jitter = milliseconds * JitterRatio * ((Random.Shared.NextDouble() * 2) - 1);
        return TimeSpan.FromMilliseconds(Math.Max(0, milliseconds + jitter));
    }
}

/// <summary>
/// CyberComm 服务端配置。
/// </summary>
public sealed record CyberCommServerOptions
{
    public IReadOnlyList<string> ListenUrls { get; init; } = [];
    public CyberCommLimitOptions Limits { get; init; } = new();
    public CyberCommTlsOptions? Tls { get; init; }
    public TimeSpan WebSocketKeepAliveInterval { get; init; } = TimeSpan.FromSeconds(30);
    public bool AssembleWebSocketMessages { get; init; } = true;
    public bool ReadHttpRequestBody { get; init; } = true;

    internal IReadOnlyList<CyberCommListenEndpoint> ValidateAndGetEndpoints()
    {
        Limits.Validate();
        if (WebSocketKeepAliveInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(WebSocketKeepAliveInterval));
        if (ListenUrls.Count == 0)
            throw new InvalidOperationException("至少需要一个监听地址");

        var endpoints = ListenUrls.Select(CyberCommListenEndpoint.Parse).Distinct().ToArray();
        if (endpoints.Any(endpoint => endpoint.UseTls))
        {
            if (Tls is null) throw new InvalidOperationException("HTTPS/WSS 监听地址需要配置 TLS 证书");
            Tls.Validate();
        }
        return endpoints;
    }
}

/// <summary>
/// CyberComm 客户端配置。
/// </summary>
public sealed record CyberCommClientOptions
{
    public required Uri ServerUri { get; init; }
    public CyberCommLimitOptions Limits { get; init; } = new();
    public CyberCommReconnectOptions Reconnect { get; init; } = new();
    public TimeSpan KeepAliveInterval { get; init; } = TimeSpan.FromSeconds(30);
    public bool AssembleWebSocketMessages { get; init; } = true;
    public IReadOnlyDictionary<string, string> RequestHeaders { get; init; } = new Dictionary<string, string>();
    /// <summary>可选的服务端证书验证回调；默认使用操作系统信任链。</summary>
    public RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; init; }

    internal void Validate()
    {
        if (ServerUri.Scheme is not ("ws" or "wss"))
            throw new ArgumentException("客户端地址必须使用 ws 或 wss", nameof(ServerUri));
        Limits.Validate();
        Reconnect.Validate();
        if (KeepAliveInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(KeepAliveInterval));
    }
}

internal readonly record struct CyberCommListenEndpoint(IPAddress Address, int Port, bool UseTls)
{
    public static CyberCommListenEndpoint Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value
            .Replace("://*", "://0.0.0.0", StringComparison.Ordinal)
            .Replace("://+", "://0.0.0.0", StringComparison.Ordinal);
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new ArgumentException($"无效的监听地址：{value}", nameof(value));

        IPAddress address;
        if (uri.Host is "0.0.0.0" or "::")
            address = uri.Host == "::" ? IPAddress.IPv6Any : IPAddress.Any;
        else if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            address = IPAddress.Loopback;
        else if (!IPAddress.TryParse(uri.Host, out address!))
        {
            var resolvedAddresses = Dns.GetHostAddresses(uri.Host);
            address = resolvedAddresses.FirstOrDefault(candidate => candidate.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?? resolvedAddresses.FirstOrDefault()
                ?? throw new ArgumentException($"监听地址无法解析：{value}", nameof(value));
        }

        return new(address, uri.Port, uri.Scheme == "https");
    }
}
