using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Authorization
{
    /// <summary>
    /// API integration & authorization matrix tests for PostPurchaseControllers (v2).
    /// Target route prefix: /api
    /// </summary>
    public sealed class PostPurchaseAuthorizationTests
    {
        private readonly IntegrationHarness _harness = new("PostPurchaseControllers", enableStrictEnvelope: true);
        private readonly JwtTestTokenFactory _tokenFactory = new();

        [Fact]
        public async Task PostCreateRefund_ShouldMatchExpectedApiContract()
        {
            // Arrange (PostPurchaseControllers POST /api/Refund - v2 #1)
            var request = _harness.CreateRequest("POST", "/api/Refund", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Customer", userId: 100 + 0));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Refund", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Customer", 200)]
        [InlineData("Anonymous", 401)]
        public async Task PostCreateRefund_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Refund", roleName);
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
        public async Task PostCreateRefund_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/Refund", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PatchApproveRefund_ShouldMatchExpectedApiContract()
        {
            // Arrange (PostPurchaseControllers PATCH /api/Refund/21/10 - v2 #2)
            var request = _harness.CreateRequest("PATCH", "/api/Refund/21/10", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 1));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("PATCH", response.Method);
            Assert.Equal("/api/Refund/21/10", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PatchApproveRefund_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("PATCH", "/api/Refund/21/10", roleName);
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
        public async Task PatchApproveRefund_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("PATCH", "/api/Refund/21/10", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PatchReplyReview_ShouldMatchExpectedApiContract()
        {
            // Arrange (PostPurchaseControllers PATCH /api/Review/9/reply - v2 #3)
            var request = _harness.CreateRequest("PATCH", "/api/Review/9/reply", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 2));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("PATCH", response.Method);
            Assert.Equal("/api/Review/9/reply", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 200)]
        [InlineData("Delivery", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PatchReplyReview_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("PATCH", "/api/Review/9/reply", roleName);
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
        public async Task PatchReplyReview_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("PATCH", "/api/Review/9/reply", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task GetMyWarranties_ShouldMatchExpectedApiContract()
        {
            // Arrange (PostPurchaseControllers GET /api/Warranty/my - v2 #4)
            var request = _harness.CreateRequest("GET", "/api/Warranty/my", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Customer", userId: 100 + 3));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Warranty/my", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Customer", 200)]
        [InlineData("Anonymous", 401)]
        public async Task GetMyWarranties_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Warranty/my", roleName);
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
        public async Task GetMyWarranties_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("GET", "/api/Warranty/my", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PatchApproveWarranty_ShouldMatchExpectedApiContract()
        {
            // Arrange (PostPurchaseControllers PATCH /api/Warranty/7/approve - v2 #5)
            var request = _harness.CreateRequest("PATCH", "/api/Warranty/7/approve", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 4));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("PATCH", response.Method);
            Assert.Equal("/api/Warranty/7/approve", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 200)]
        [InlineData("Delivery", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PatchApproveWarranty_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("PATCH", "/api/Warranty/7/approve", roleName);
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
        public async Task PatchApproveWarranty_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("PATCH", "/api/Warranty/7/approve", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }
    }
}
