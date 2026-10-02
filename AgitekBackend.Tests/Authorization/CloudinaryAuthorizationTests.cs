using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgitekBackend.Tests.Integration.Infrastructure;
using Xunit;

namespace AgitekBackend.Tests.Authorization
{
    /// <summary>
    /// API integration & authorization matrix tests for CloudinaryController (v2).
    /// Target route prefix: /api/Cloudinary
    /// </summary>
    public sealed class CloudinaryAuthorizationTests
    {
        private readonly IntegrationHarness _harness = new("CloudinaryController", enableStrictEnvelope: true);
        private readonly JwtTestTokenFactory _tokenFactory = new();

        [Fact]
        public async Task PostSignatureProductsFolder_ShouldMatchExpectedApiContract()
        {
            // Arrange (CloudinaryController POST /api/Cloudinary/signature?folder=agitek/products - v2 #1)
            var request = _harness.CreateRequest("POST", "/api/Cloudinary/signature?folder=agitek/products", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 0));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Cloudinary/signature?folder=agitek/products", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Manager", 200)]
        [InlineData("Staff", 200)]
        [InlineData("Customer", 403)]
        [InlineData("Anonymous", 401)]
        public async Task PostSignatureProductsFolder_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Cloudinary/signature?folder=agitek/products", roleName);
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
        public async Task PostSignatureProductsFolder_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/Cloudinary/signature?folder=agitek/products", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PostSignatureAvatarsFolder_ShouldMatchExpectedApiContract()
        {
            // Arrange (CloudinaryController POST /api/Cloudinary/signature?folder=agitek/avatars - v2 #2)
            var request = _harness.CreateRequest("POST", "/api/Cloudinary/signature?folder=agitek/avatars", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 1));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Cloudinary/signature?folder=agitek/avatars", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("SuperAdmin", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Staff", 200)]
        [InlineData("Delivery", 200)]
        [InlineData("Customer", 200)]
        [InlineData("Anonymous", 401)]
        public async Task PostSignatureAvatarsFolder_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Cloudinary/signature?folder=agitek/avatars", roleName);
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
        public async Task PostSignatureAvatarsFolder_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/Cloudinary/signature?folder=agitek/avatars", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }

        [Fact]
        public async Task PostSignatureReviewsAndWarranties_ShouldMatchExpectedApiContract()
        {
            // Arrange (CloudinaryController POST /api/Cloudinary/signature?folder=agitek/reviews - v2 #3)
            var request = _harness.CreateRequest("POST", "/api/Cloudinary/signature?folder=agitek/reviews", "Admin");
            request.SetPayload("{}");
            request.SetAuthorization(_tokenFactory.CreateBearer("Admin", userId: 100 + 2));

            // Act
            var response = await _harness.ExecuteAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.True(response.HasEnvelope);
            Assert.Equal("POST", response.Method);
            Assert.Equal("/api/Cloudinary/signature?folder=agitek/reviews", response.Path);
            Assert.False(string.IsNullOrWhiteSpace(response.TraceId));
        }

        [Theory]
        [InlineData("Customer", 200)]
        [InlineData("Admin", 200)]
        [InlineData("Anonymous", 401)]
        public async Task PostSignatureReviewsAndWarranties_AuthorizationMatrix_ShouldEnforceRolePolicy(string roleName, int expectedStatusCode)
        {
            // Arrange
            var req = _harness.CreateRequest("POST", "/api/Cloudinary/signature?folder=agitek/reviews", roleName);
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
        public async Task PostSignatureReviewsAndWarranties_ErrorEnvelopeContract_ShouldReturnStructuredError(
            string scenario,
            int expectedStatusCode,
            string expectedErrorCode)
        {
            var req = _harness.CreateRequest("POST", "/api/Cloudinary/signature?folder=agitek/reviews", "Customer");
            req.SetPayload(scenario);
            req.SetAuthorization(scenario == "expired-bearer" ? null : "Bearer active-session");
            var res = await _harness.SimulateAuthorizationAsync(req, expectedStatusCode);
            Assert.Equal(expectedStatusCode, res.StatusCode);
            Assert.True(res.HasEnvelope);
            Assert.False(string.IsNullOrWhiteSpace(expectedErrorCode));
        }
    }
}
