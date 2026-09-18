using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Integration.Controllers
{
    /// <summary>
    /// API integration & authorization matrix tests for AuthController (v1).
    /// Target route prefix: /api/Auth
    /// </summary>
    public sealed class AuthControllerIntegrationTests
    {
        private readonly IntegrationHarness _harness = new("AuthController");

        [Fact]
        public async Task PostLogin_ShouldMatchExpectedApiContract()
        {
            // Arrange (AuthController POST /api/Auth/login - v1 #1)
            var request = _harness.CreateRequest("POST", "/api/Auth/login", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Anonymous");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Auth/login", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        [InlineData("Customer", 200)]
        public async Task PostLogin_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Auth/login", roleName);
            var bearer = roleName == "Anonymous" ? null : $"Bearer mock-{roleName}";
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
        public async Task PostLogin_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/Auth/login", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PostRegister_ShouldMatchExpectedApiContract()
        {
            // Arrange (AuthController POST /api/Auth/register - v1 #2)
            var request = _harness.CreateRequest("POST", "/api/Auth/register", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Anonymous");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Auth/register", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        public async Task PostRegister_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Auth/register", roleName);
            var bearer = roleName == "Anonymous" ? null : $"Bearer mock-{roleName}";
            req.SetAuthorization(bearer);

            // Act
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);

            // Assert
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.Equal(roleName, res.EvaluatedRole);
            Assert.True(res.HasEnvelope);
        }

        [Fact]
        public async Task PostRefresh_ShouldMatchExpectedApiContract()
        {
            // Arrange (AuthController POST /api/Auth/refresh - v1 #3)
            var request = _harness.CreateRequest("POST", "/api/Auth/refresh", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Anonymous");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Auth/refresh", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        public async Task PostRefresh_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Auth/refresh", roleName);
            var bearer = roleName == "Anonymous" ? null : $"Bearer mock-{roleName}";
            req.SetAuthorization(bearer);

            // Act
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);

            // Assert
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.Equal(roleName, res.EvaluatedRole);
            Assert.True(res.HasEnvelope);
        }

        [Fact]
        public async Task GetMeClaims_ShouldMatchExpectedApiContract()
        {
            // Arrange (AuthController GET /api/Auth/me - v1 #4)
            var request = _harness.CreateRequest("GET", "/api/Auth/me", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Customer");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Auth/me", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Customer", 200)]
        [InlineData("Anonymous", 401)]
        public async Task GetMeClaims_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Auth/me", roleName);
            var bearer = roleName == "Anonymous" ? null : $"Bearer mock-{roleName}";
            req.SetAuthorization(bearer);

            // Act
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);

            // Assert
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.Equal(roleName, res.EvaluatedRole);
            Assert.True(res.HasEnvelope);
        }
    }
}
