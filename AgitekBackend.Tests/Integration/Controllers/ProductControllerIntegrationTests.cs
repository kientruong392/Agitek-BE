using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Integration.Controllers
{
    /// <summary>
    /// API integration & authorization matrix tests for ProductController (v1).
    /// Target route prefix: /api/Product
    /// </summary>
    public sealed class ProductControllerIntegrationTests
    {
        private readonly IntegrationHarness _harness = new("ProductController");

        [Fact]
        public async Task GetAllProducts_ShouldMatchExpectedApiContract()
        {
            // Arrange (ProductController GET /api/Product - v1 #1)
            var request = _harness.CreateRequest("GET", "/api/Product", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Anonymous");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Product", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        [InlineData("Customer", 200)]
        [InlineData("Staff", 200)]
        [InlineData("Admin", 200)]
        public async Task GetAllProducts_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Product", roleName);
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
        public async Task GetAllProducts_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("GET", "/api/Product", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task GetFeaturedProducts_ShouldMatchExpectedApiContract()
        {
            // Arrange (ProductController GET /api/Product/featured?count=4 - v1 #2)
            var request = _harness.CreateRequest("GET", "/api/Product/featured?count=4", "Anonymous");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Anonymous");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Product/featured?count=4", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("Anonymous", 200)]
        [InlineData("Customer", 200)]
        public async Task GetFeaturedProducts_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Product/featured?count=4", roleName);
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
        public async Task PostCreateProduct_ShouldMatchExpectedApiContract()
        {
            // Arrange (ProductController POST /api/Product - v1 #3)
            var request = _harness.CreateRequest("POST", "/api/Product", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Admin");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Product", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 200)]
        [InlineData("Delivery", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PostCreateProduct_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Product", roleName);
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
        public async Task DeleteSoftProduct_ShouldMatchExpectedApiContract()
        {
            // Arrange (ProductController DELETE /api/Product/10/soft - v1 #4)
            var request = _harness.CreateRequest("DELETE", "/api/Product/10/soft", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Admin");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("DELETE", response.Method);
            Assert.Equal("/api/Product/10/soft", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task DeleteSoftProduct_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("DELETE", "/api/Product/10/soft", roleName);
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
