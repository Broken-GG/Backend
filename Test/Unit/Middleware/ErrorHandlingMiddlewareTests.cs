using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Backend.Middleware;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Backend.Tests.Unit.Middleware
{
    /// <summary>
    /// Unit tests for ErrorHandlingMiddleware
    /// Tests cover exception handling and error response formatting
    /// </summary>
    public class ErrorHandlingMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_NoException_CallsNextMiddleware()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var called = false;
            var mockLogger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            
            RequestDelegate next = (HttpContext ctx) =>
            {
                called = true;
                return Task.CompletedTask;
            };

            var middleware = new ErrorHandlingMiddleware(next, mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(called);
            Assert.Equal(200, context.Response.StatusCode); // Default status when no error
        }

        [Fact]
        public async Task InvokeAsync_ExceptionThrown_Returns500WithErrorResponse()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var mockLogger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            
            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new Exception("Test exception");
            };

            var middleware = new ErrorHandlingMiddleware(next, mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_ArgumentException_Returns400BadRequest()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var mockLogger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            
            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new ArgumentException("Invalid argument");
            };

            var middleware = new ErrorHandlingMiddleware(next, mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(400, context.Response.StatusCode); // ArgumentException returns 400
        }
    }
}
