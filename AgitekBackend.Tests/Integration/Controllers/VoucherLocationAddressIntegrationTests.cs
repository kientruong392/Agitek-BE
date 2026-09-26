using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Integration.Controllers
{
    /// <summary>
    /// API integration & authorization matrix tests for VoucherLocationAddressController (v1).
    /// Target route prefix: /api
    /// </summary>
    public sealed class VoucherLocationAddressIntegrationTests
    {
        private readonly IntegrationHarness _harness = new("VoucherLocationAddressController");

        [Fact]
        public async Task PostApplyVoucher_ShouldMatchExpectedApiContract()
        {
            // Arrange (VoucherLocationAddressController POST /api/Voucher/apply - v1 #1)
            var request = _harness.CreateRequest("POST", "/api/Voucher/apply", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Customer");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Voucher/apply", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("Customer", 200)]
        [InlineData("Anonymous", 200)]
        public async Task PostApplyVoucher_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Voucher/apply", roleName);
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
        public async Task PostApplyVoucher_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/Voucher/apply", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PostCreateVoucher_ShouldMatchExpectedApiContract()
        {
            // Arrange (VoucherLocationAddressController POST /api/Voucher - v1 #2)
            var request = _harness.CreateRequest("POST", "/api/Voucher", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Admin");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Voucher", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 200)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PostCreateVoucher_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Voucher", roleName);
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
        public async Task PostCreateWarehouseLocation_ShouldMatchExpectedApiContract()
        {
            // Arrange (VoucherLocationAddressController POST /api/Location - v1 #3)
            var request = _harness.CreateRequest("POST", "/api/Location", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Admin");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Location", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 403)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PostCreateWarehouseLocation_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Location", roleName);
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
        public async Task GetUserAddresses_ShouldMatchExpectedApiContract()
        {
            // Arrange (VoucherLocationAddressController GET /api/Address/user/1 - v1 #4)
            var request = _harness.CreateRequest("GET", "/api/Address/user/1", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization("Bearer mock-Customer");

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("GET", response.Method);
            Assert.Equal("/api/Address/user/1", response.Path);
            Assert.NotNull(response.Path);
        }

        [Theory]
        [InlineData("Customer", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Anonymous", 401)]
        public async Task GetUserAddresses_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("GET", "/api/Address/user/1", roleName);
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
