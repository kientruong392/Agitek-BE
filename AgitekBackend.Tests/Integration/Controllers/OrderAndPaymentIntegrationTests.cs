using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Integration.Controllers
{
    /// <summary>
    /// API integration & authorization matrix tests for OrderAndPaymentController (v2).
    /// Target route prefix: /api
    /// </summary>
    public sealed class OrderAndPaymentIntegrationTests
    {
        private readonly IntegrationHarness _harness = new("OrderAndPaymentController", enableStrictEnvelope: true);
        private readonly JwtTestTokenFactory _tokenFactory = new();

        [Fact]
        public async Task PostCreateCustomerOrder_ShouldMatchExpectedApiContract()
        {
            // Arrange (OrderAndPaymentController POST /api/Order - v2 #1)
            var request = _harness.CreateRequest("POST", "/api/Order", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Customer", userId: 100 + 0));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Order", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Customer", 200)]
        [InlineData("Anonymous", 401)]
        public async Task PostCreateCustomerOrder_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Order", roleName);
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
        public async Task PatchConfirmOrder_ShouldMatchExpectedApiContract()
        {
            // Arrange (OrderAndPaymentController PATCH /api/Order/21/confirm - v2 #2)
            var request = _harness.CreateRequest("PATCH", "/api/Order/21/confirm", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 1));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("PATCH", response.Method);
            Assert.Equal("/api/Order/21/confirm", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Delivery", 403)]
        [InlineData("Staff", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PatchConfirmOrder_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("PATCH", "/api/Order/21/confirm", roleName);
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
        public async Task PatchConfirmOrder_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("PATCH", "/api/Order/21/confirm", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PatchStartOrderTransit_ShouldMatchExpectedApiContract()
        {
            // Arrange (OrderAndPaymentController PATCH /api/Order/21/in-transit - v2 #3)
            var request = _harness.CreateRequest("PATCH", "/api/Order/21/in-transit", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 2));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("PATCH", response.Method);
            Assert.Equal("/api/Order/21/in-transit", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Delivery", 200)]
        [InlineData("Staff", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PatchStartOrderTransit_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("PATCH", "/api/Order/21/in-transit", roleName);
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
        public async Task PatchStartOrderTransit_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("PATCH", "/api/Order/21/in-transit", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PostAddOrderTrackingLog_ShouldMatchExpectedApiContract()
        {
            // Arrange (OrderAndPaymentController POST /api/Order/21/tracking - v2 #4)
            var request = _harness.CreateRequest("POST", "/api/Order/21/tracking", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 3));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Order/21/tracking", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Delivery", 200)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PostAddOrderTrackingLog_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Order/21/tracking", roleName);
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
        public async Task PostAddOrderTrackingLog_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/Order/21/tracking", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PatchShipOrderWithMedia_ShouldMatchExpectedApiContract()
        {
            // Arrange (OrderAndPaymentController PATCH /api/Order/21/ship - v2 #5)
            var request = _harness.CreateRequest("PATCH", "/api/Order/21/ship", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 4));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("PATCH", response.Method);
            Assert.Equal("/api/Order/21/ship", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Delivery", 200)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PatchShipOrderWithMedia_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("PATCH", "/api/Order/21/ship", roleName);
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
        public async Task PatchShipOrderWithMedia_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("PATCH", "/api/Order/21/ship", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }
    }
}
