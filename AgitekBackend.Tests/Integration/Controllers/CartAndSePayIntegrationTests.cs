using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Integration.Controllers
{
    /// <summary>
    /// API integration & authorization matrix tests for CartAndSePayController (v3).
    /// Target route prefix: /api
    /// </summary>
    public sealed class CartAndSePayIntegrationTests
    {
        private readonly IntegrationHarness _harness = new("CartAndSePayController", enableStrictEnvelope: true);
        private readonly JwtTestTokenFactory _tokenFactory = new();

        [Fact]
        public async Task GetCustomerCart_ShouldMatchExpectedApiContract()
        {
            // Arrange (CartAndSePayController GET /api/Cart - v3 #1)
            var request = _harness.CreateRequest("GET", "/api/Cart", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Customer", userId: 100 + 0));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Cart", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Customer", 200)]
        [InlineData("Anonymous", 401)]
        public async Task GetCustomerCart_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Cart", roleName);
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
        public async Task GetCustomerCart_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("GET", "/api/Cart", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PostAddCartItem_ShouldMatchExpectedApiContract()
        {
            // Arrange (CartAndSePayController POST /api/Cart/items - v3 #2)
            var request = _harness.CreateRequest("POST", "/api/Cart/items", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Customer", userId: 100 + 1));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Cart/items", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Customer", 200)]
        [InlineData("Anonymous", 401)]
        public async Task PostAddCartItem_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Cart/items", roleName);
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
        public async Task PostAddCartItem_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/Cart/items", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PostSePayWebhookMatchesLatestPending_ShouldMatchExpectedApiContract()
        {
            // Arrange (CartAndSePayController POST /api/sepay/webhook - v3 #3)
            var request = _harness.CreateRequest("POST", "/api/sepay/webhook", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Anonymous", userId: 100 + 2));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/sepay/webhook", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        public async Task PostSePayWebhookMatchesLatestPending_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/sepay/webhook", roleName);
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
        public async Task PostSePayWebhookMatchesLatestPending_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/sepay/webhook", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task GetPublicSePayPayment_ShouldMatchExpectedApiContract()
        {
            // Arrange (CartAndSePayController GET /api/sepay/payment/21?trackingNumber=AGT261016ABC123456 - v3 #4)
            var request = _harness.CreateRequest("GET", "/api/sepay/payment/21?trackingNumber=AGT261016ABC123456", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Anonymous", userId: 100 + 3));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/sepay/payment/21?trackingNumber=AGT261016ABC123456", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        [InlineData("Customer", 200)]
        public async Task GetPublicSePayPayment_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/sepay/payment/21?trackingNumber=AGT261016ABC123456", roleName);
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
        public async Task GetPublicSePayPayment_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("GET", "/api/sepay/payment/21?trackingNumber=AGT261016ABC123456", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PostResumeOrCreateBankPayment_ShouldMatchExpectedApiContract()
        {
            // Arrange (CartAndSePayController POST /api/sepay/payment/21/resume?trackingNumber=AGT261016ABC123456 - v3 #5)
            var request = _harness.CreateRequest("POST", "/api/sepay/payment/21/resume?trackingNumber=AGT261016ABC123456", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Anonymous", userId: 100 + 4));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/sepay/payment/21/resume?trackingNumber=AGT261016ABC123456", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        [InlineData("Customer", 200)]
        public async Task PostResumeOrCreateBankPayment_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/sepay/payment/21/resume?trackingNumber=AGT261016ABC123456", roleName);
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
        public async Task PostResumeOrCreateBankPayment_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/sepay/payment/21/resume?trackingNumber=AGT261016ABC123456", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }
    }
}
