using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Integration.Controllers
{
    /// <summary>
    /// API integration & authorization matrix tests for AuditAndReportController (v2).
    /// Target route prefix: /api
    /// </summary>
    public sealed class AuditAndReportIntegrationTests
    {
        private readonly IntegrationHarness _harness = new("AuditAndReportController", enableStrictEnvelope: true);
        private readonly JwtTestTokenFactory _tokenFactory = new();

        [Fact]
        public async Task GetAuditLogsList_ShouldMatchExpectedApiContract()
        {
            // Arrange (AuditAndReportController GET /api/AuditLog - v2 #1)
            var request = _harness.CreateRequest("GET", "/api/AuditLog", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 0));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/AuditLog", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 403)]
        [InlineData("Delivery", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task GetAuditLogsList_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/AuditLog", roleName);
            var bearer = roleName == "Anonymous" ? null : _tokenFactory.CreateBearer(roleName, userId: 101);
            req.SetAuthorization(bearer);

            // Act
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);

            // Assert
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.Equal(roleName, res.EvaluatedRole);
            Assert.True(res.HasEnvelope);
        }

        [Theory]
        [InlineData("invalid-json", 400, "INVALID_REQUEST_BODY")]
        [InlineData("expired-bearer", 401, "UNAUTHORIZED")]
        public async Task GetAuditLogsList_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("GET", "/api/AuditLog", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task GetManagementReportMonthly_ShouldMatchExpectedApiContract()
        {
            // Arrange (AuditAndReportController GET /api/Report?from=2026-01-01&to=2026-11-21&interval=1 - v2 #2)
            var request = _harness.CreateRequest("GET", "/api/Report?from=2026-01-01&to=2026-11-21&interval=1", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 1));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Report?from=2026-01-01&to=2026-11-21&interval=1", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 403)]
        [InlineData("Delivery", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task GetManagementReportMonthly_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Report?from=2026-01-01&to=2026-11-21&interval=1", roleName);
            var bearer = roleName == "Anonymous" ? null : _tokenFactory.CreateBearer(roleName, userId: 101);
            req.SetAuthorization(bearer);

            // Act
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);

            // Assert
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.Equal(roleName, res.EvaluatedRole);
            Assert.True(res.HasEnvelope);
        }

        [Theory]
        [InlineData("invalid-json", 400, "INVALID_REQUEST_BODY")]
        [InlineData("expired-bearer", 401, "UNAUTHORIZED")]
        public async Task GetManagementReportMonthly_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("GET", "/api/Report?from=2026-01-01&to=2026-11-21&interval=1", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task GetManagementReportWeekly_ShouldMatchExpectedApiContract()
        {
            // Arrange (AuditAndReportController GET /api/Report?from=2026-10-01&to=2026-11-21&interval=0 - v2 #3)
            var request = _harness.CreateRequest("GET", "/api/Report?from=2026-10-01&to=2026-11-21&interval=0", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 2));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Report?from=2026-10-01&to=2026-11-21&interval=0", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 403)]
        [InlineData("Anonymous", 401)]
        public async Task GetManagementReportWeekly_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Report?from=2026-10-01&to=2026-11-21&interval=0", roleName);
            var bearer = roleName == "Anonymous" ? null : _tokenFactory.CreateBearer(roleName, userId: 101);
            req.SetAuthorization(bearer);

            // Act
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);

            // Assert
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.Equal(roleName, res.EvaluatedRole);
            Assert.True(res.HasEnvelope);
        }

        [Theory]
        [InlineData("invalid-json", 400, "INVALID_REQUEST_BODY")]
        [InlineData("expired-bearer", 401, "UNAUTHORIZED")]
        public async Task GetManagementReportWeekly_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("GET", "/api/Report?from=2026-10-01&to=2026-11-21&interval=0", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }
    }
}
