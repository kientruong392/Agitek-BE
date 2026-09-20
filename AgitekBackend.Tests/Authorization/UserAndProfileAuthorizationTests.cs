using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Authorization
{
    /// <summary>
    /// API integration & authorization matrix tests for UserController (v1).
    /// Target route prefix: /api/User
    /// </summary>
    public sealed class UserAndProfileAuthorizationTests
    {
        private readonly IntegrationHarness _harness = new("UserController");

        [Fact]
        public async Task GetCustomersList_ShouldMatchExpectedApiContract()
        {
            // Arrange (UserController GET /api/User/customers - v1 #1)
            var request = _harness.CreateRequest("GET", "/api/User/customers", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Admin");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/User/customers", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 403)]
        [InlineData("Delivery", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task GetCustomersList_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/User/customers", roleName);
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
        public async Task GetCustomersList_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("GET", "/api/User/customers", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task GetStaffList_ShouldMatchExpectedApiContract()
        {
            // Arrange (UserController GET /api/User/staff - v1 #2)
            var request = _harness.CreateRequest("GET", "/api/User/staff", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Admin");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/User/staff", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task GetStaffList_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/User/staff", roleName);
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
        public async Task PatchUpdateUserRole_ShouldMatchExpectedApiContract()
        {
            // Arrange (UserController PATCH /api/User/5/role - v1 #3)
            var request = _harness.CreateRequest("PATCH", "/api/User/5/role", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Admin");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("PATCH", response.Method);
            Assert.Equal("/api/User/5/role", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 403)]
        [InlineData("Staff", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PatchUpdateUserRole_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("PATCH", "/api/User/5/role", roleName);
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
        public async Task PatchOwnProfile_ShouldMatchExpectedApiContract()
        {
            // Arrange (UserController PATCH /api/User/me - v1 #4)
            var request = _harness.CreateRequest("PATCH", "/api/User/me", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Admin");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("PATCH", response.Method);
            Assert.Equal("/api/User/me", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Staff", 200)]
        [InlineData("Customer", 200)]
        [InlineData("Anonymous", 401)]
        public async Task PatchOwnProfile_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("PATCH", "/api/User/me", roleName);
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
