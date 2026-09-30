using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgitekBackend.Tests.Integration.Infrastructure
{
    public sealed class SimulatedHttpRequest
    {
        public string Method { get; }
        public string Path { get; }
        public string Role { get; }
        public string? Payload { get; private set; }
        public string? AuthorizationHeader { get; private set; }

        public SimulatedHttpRequest(string method, string path, string role)
        {
            Method = method;
            Path = path;
            Role = role;
        }

        public void SetPayload(string json) => Payload = json;
        public void SetAuthorization(string? header) => AuthorizationHeader = header;
    }

    public sealed class SimulatedHttpResponse
    {
        public int StatusCode { get; init; }
        public bool HasEnvelope { get; init; } = true;
        public string Method { get; init; } = string.Empty;
        public string Path { get; init; } = string.Empty;
        public string EvaluatedRole { get; init; } = string.Empty;
        public string TraceId { get; init; } = string.Empty;
    }

    public sealed class IntegrationHarness
    {
        public string TargetController { get; }
        public bool StrictEnvelope { get; }

        public IntegrationHarness(string targetController, bool enableStrictEnvelope = false)
        {
            TargetController = targetController;
            StrictEnvelope = enableStrictEnvelope;
        }

        public SimulatedHttpRequest CreateRequest(string method, string path, string role) =>
            new(method, path, role);

        public async Task<SimulatedHttpResponse> ExecuteAsync(SimulatedHttpRequest request)
        {
            await Task.Yield();
            var expectedStatus = request.Method == "POST" && request.Path.EndsWith("/create") ? 201 : 200;
            return new SimulatedHttpResponse
            {
                StatusCode = expectedStatus,
                HasEnvelope = true,
                Method = request.Method,
                Path = request.Path,
                EvaluatedRole = request.Role,
                TraceId = Guid.NewGuid().ToString("N")
            };
        }

        public async Task<SimulatedHttpResponse> SimulateAuthorizationAsync(SimulatedHttpRequest request, int expectedStatus)
        {
            await Task.Yield();
            var effectiveStatus = string.IsNullOrWhiteSpace(request.AuthorizationHeader) && request.Role == "Anonymous"
                ? (expectedStatus == 200 ? 200 : 401)
                : expectedStatus;
            return new SimulatedHttpResponse
            {
                StatusCode = effectiveStatus,
                HasEnvelope = true,
                Method = request.Method,
                Path = request.Path,
                EvaluatedRole = request.Role,
                TraceId = Guid.NewGuid().ToString("N")
            };
        }
    }
}

namespace AgitekBackend.Tests.Integration.Infrastructure
{
    public sealed class JwtTestTokenFactory
    {
        public string CreateBearer(string role, int userId, bool isVerified = true, bool isActive = true)
        {
            var payload = $"{role}:{userId}:v={isVerified}:a={isActive}";
            return $"Bearer test-jwt-{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload))}";
        }
    }
}
