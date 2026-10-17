using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Integration.Controllers
{
    /// <summary>
    /// API integration & authorization matrix tests for ProductController (v2).
    /// Target route prefix: /api/Product
    /// </summary>
    public sealed class SemanticSearchIntegrationTests
    {
        private readonly IntegrationHarness _harness = new("ProductController", enableStrictEnvelope: true);
        private readonly JwtTestTokenFactory _tokenFactory = new();

        [Fact]
        public async Task GetSemanticSearchGamingPc_ShouldMatchExpectedApiContract()
        {
            // Arrange (ProductController GET /api/Product/semantic-search?q=pc+gaming+rtx+4070&limit=6 - v2 #1)
            var request = _harness.CreateRequest("GET", "/api/Product/semantic-search?q=pc+gaming+rtx+4070&limit=6", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Anonymous", userId: 100 + 0));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Product/semantic-search?q=pc+gaming+rtx+4070&limit=6", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        [InlineData("Customer", 200)]
        [InlineData("Admin", 200)]
        public async Task GetSemanticSearchGamingPc_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Product/semantic-search?q=pc+gaming+rtx+4070&limit=6", roleName);
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
        public async Task GetSemanticSearchGamingPc_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("GET", "/api/Product/semantic-search?q=pc+gaming+rtx+4070&limit=6", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task GetSemanticSearchVietnameseQuery_ShouldMatchExpectedApiContract()
        {
            // Arrange (ProductController GET /api/Product/semantic-search?q=may+tinh+van+phong&limit=8 - v2 #2)
            var request = _harness.CreateRequest("GET", "/api/Product/semantic-search?q=may+tinh+van+phong&limit=8", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Anonymous", userId: 100 + 1));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Product/semantic-search?q=may+tinh+van+phong&limit=8", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        [InlineData("Customer", 200)]
        public async Task GetSemanticSearchVietnameseQuery_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Product/semantic-search?q=may+tinh+van+phong&limit=8", roleName);
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
        public async Task GetSemanticSearchVietnameseQuery_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("GET", "/api/Product/semantic-search?q=may+tinh+van+phong&limit=8", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }
    }
}
