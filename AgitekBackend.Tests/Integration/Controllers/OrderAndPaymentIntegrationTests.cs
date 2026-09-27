using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Integration.Controllers
{
    /// <summary>
    /// API integration & authorization matrix tests for OrderAndPaymentController (v1).
    /// Target route prefix: /api
    /// </summary>
    public sealed class OrderAndPaymentIntegrationTests
    {
        private readonly IntegrationHarness _harness = new("OrderAndPaymentController");

        [Fact]
        public async Task PostCreateCustomerOrder_ShouldMatchExpectedApiContract()
        {
            // Arrange (OrderAndPaymentController POST /api/Order - v1 #1)
            var request = _harness.CreateRequest("POST", "/api/Order", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Customer");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Order", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("Customer", 200)]
        [InlineData("Anonymous", 401)]
        public async Task PostCreateCustomerOrder_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Order", roleName);
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
        public async Task PostCreateCustomerOrder_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/Order", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PostCreateGuestOrder_ShouldMatchExpectedApiContract()
        {
            // Arrange (OrderAndPaymentController POST /api/Order/guest - v1 #2)
            var request = _harness.CreateRequest("POST", "/api/Order/guest", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Anonymous");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Order/guest", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        public async Task PostCreateGuestOrder_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Order/guest", roleName);
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
        public async Task GetAllPayments_ShouldMatchExpectedApiContract()
        {
            // Arrange (OrderAndPaymentController GET /api/Payment - v1 #3)
            var request = _harness.CreateRequest("GET", "/api/Payment", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Admin");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Payment", response.Path);
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
        public async Task GetAllPayments_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Payment", roleName);
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
