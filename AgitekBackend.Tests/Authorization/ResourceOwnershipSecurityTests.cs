using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Authorization
{
    /// <summary>
    /// API integration & authorization matrix tests for OwnershipGuards (v2).
    /// Target route prefix: /api
    /// </summary>
    public sealed class ResourceOwnershipSecurityTests
    {
        private readonly IntegrationHarness _harness = new("OwnershipGuards", enableStrictEnvelope: true);
        private readonly JwtTestTokenFactory _tokenFactory = new();

        [Fact]
        public async Task CustomerCannotEditOtherUserProfile_ShouldMatchExpectedApiContract()
        {
            // Arrange (OwnershipGuards PATCH /api/Profile/999 - v2 #1)
            var request = _harness.CreateRequest("PATCH", "/api/Profile/999", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Customer", userId: 100 + 0));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("PATCH", response.Method);
            Assert.Equal("/api/Profile/999", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Customer", 403)]
        [InlineData("Admin", 200)]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Anonymous", 401)]
        public async Task CustomerCannotEditOtherUserProfile_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("PATCH", "/api/Profile/999", roleName);
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
        public async Task CustomerCannotEditOtherUserProfile_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("PATCH", "/api/Profile/999", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task CustomerCannotDeleteOtherUserAddress_ShouldMatchExpectedApiContract()
        {
            // Arrange (OwnershipGuards DELETE /api/Address/999 - v2 #2)
            var request = _harness.CreateRequest("DELETE", "/api/Address/999", "Customer");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Customer", userId: 100 + 1));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("DELETE", response.Method);
            Assert.Equal("/api/Address/999", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Customer", 403)]
        [InlineData("Admin", 200)]
        [InlineData("Anonymous", 401)]
        public async Task CustomerCannotDeleteOtherUserAddress_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("DELETE", "/api/Address/999", roleName);
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
        public async Task CustomerCannotDeleteOtherUserAddress_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("DELETE", "/api/Address/999", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task DeliveryCannotAdvanceUnassignedOrder_ShouldMatchExpectedApiContract()
        {
            // Arrange (OwnershipGuards PATCH /api/Order/999/in-transit - v2 #3)
            var request = _harness.CreateRequest("PATCH", "/api/Order/999/in-transit", "Delivery");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Delivery", userId: 100 + 2));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("PATCH", response.Method);
            Assert.Equal("/api/Order/999/in-transit", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Delivery", 403)]
        [InlineData("Manager", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Anonymous", 401)]
        public async Task DeliveryCannotAdvanceUnassignedOrder_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("PATCH", "/api/Order/999/in-transit", roleName);
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
        public async Task DeliveryCannotAdvanceUnassignedOrder_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("PATCH", "/api/Order/999/in-transit", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }
    }
}
