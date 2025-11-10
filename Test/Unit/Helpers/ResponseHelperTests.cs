using Xunit;
using Backend.Helpers;
using System;

namespace Backend.Tests.Unit.Helpers
{
    /// <summary>
    /// Unit tests for ResponseHelper
    /// Tests cover all response formatting methods
    /// </summary>
    public class ResponseHelperTests
    {
        #region CreateErrorResponse Tests

        [Fact]
        public void CreateErrorResponse_WithMessageOnly_ReturnsFormattedError()
        {
            // Arrange
            string message = "Test error message";

            // Act
            var response = ResponseHelper.CreateErrorResponse(message);
            var responseType = response.GetType();

            // Assert
            Assert.NotNull(response);
            
            var successProp = responseType.GetProperty("success");
            var errorProp = responseType.GetProperty("error");
            
            Assert.NotNull(successProp);
            Assert.NotNull(errorProp);
            Assert.False((bool)successProp!.GetValue(response)!);
            
            var errorObj = errorProp!.GetValue(response);
            var errorType = errorObj!.GetType();
            var messageProp = errorType.GetProperty("message");
            
            Assert.Equal(message, messageProp!.GetValue(errorObj));
        }

        [Fact]
        public void CreateErrorResponse_WithAllParameters_ReturnsCompleteErrorObject()
        {
            // Arrange
            string message = "Error occurred";
            string details = "Detailed error information";
            string errorCode = "ERR_001";

            // Act
            var response = ResponseHelper.CreateErrorResponse(message, details, errorCode);
            var responseType = response.GetType();

            // Assert
            var errorProp = responseType.GetProperty("error");
            var errorObj = errorProp!.GetValue(response);
            var errorType = errorObj!.GetType();

            var messageProp = errorType.GetProperty("message");
            var detailsProp = errorType.GetProperty("details");
            var errorCodeProp = errorType.GetProperty("errorCode");
            var timestampProp = errorType.GetProperty("timestamp");

            Assert.Equal(message, messageProp!.GetValue(errorObj));
            Assert.Equal(details, detailsProp!.GetValue(errorObj));
            Assert.Equal(errorCode, errorCodeProp!.GetValue(errorObj));
            Assert.NotNull(timestampProp!.GetValue(errorObj));
        }

        [Fact]
        public void CreateErrorResponse_HasTimestamp_TimestampIsRecent()
        {
            // Arrange
            var beforeCall = DateTime.UtcNow;

            // Act
            var response = ResponseHelper.CreateErrorResponse("Test");
            var afterCall = DateTime.UtcNow;

            // Assert
            var responseType = response.GetType();
            var errorProp = responseType.GetProperty("error");
            var errorObj = errorProp!.GetValue(response);
            var errorType = errorObj!.GetType();
            var timestampProp = errorType.GetProperty("timestamp");
            var timestamp = (DateTime)timestampProp!.GetValue(errorObj)!;

            Assert.InRange(timestamp, beforeCall.AddSeconds(-1), afterCall.AddSeconds(1));
        }

        #endregion

        #region CreateSuccessResponse Tests

        [Fact]
        public void CreateSuccessResponse_WithData_ReturnsFormattedSuccess()
        {
            // Arrange
            var testData = new { id = 1, name = "Test" };

            // Act
            var response = ResponseHelper.CreateSuccessResponse(testData);
            var responseType = response.GetType();

            // Assert
            var successProp = responseType.GetProperty("success");
            var dataProp = responseType.GetProperty("data");

            Assert.True((bool)successProp!.GetValue(response)!);
            Assert.Equal(testData, dataProp!.GetValue(response));
        }

        [Fact]
        public void CreateSuccessResponse_WithDataAndMessage_ReturnsCompleteSuccessObject()
        {
            // Arrange
            var testData = new { id = 1 };
            string message = "Operation successful";

            // Act
            var response = ResponseHelper.CreateSuccessResponse(testData, message);
            var responseType = response.GetType();

            // Assert
            var successProp = responseType.GetProperty("success");
            var dataProp = responseType.GetProperty("data");
            var messageProp = responseType.GetProperty("message");
            var timestampProp = responseType.GetProperty("timestamp");

            Assert.True((bool)successProp!.GetValue(response)!);
            Assert.Equal(testData, dataProp!.GetValue(response));
            Assert.Equal(message, messageProp!.GetValue(response));
            Assert.NotNull(timestampProp!.GetValue(response));
        }

        [Fact]
        public void CreateSuccessResponse_HasTimestamp_TimestampIsRecent()
        {
            // Arrange
            var beforeCall = DateTime.UtcNow;

            // Act
            var response = ResponseHelper.CreateSuccessResponse(new { test = true });
            var afterCall = DateTime.UtcNow;

            // Assert
            var responseType = response.GetType();
            var timestampProp = responseType.GetProperty("timestamp");
            var timestamp = (DateTime)timestampProp!.GetValue(response)!;

            Assert.InRange(timestamp, beforeCall.AddSeconds(-1), afterCall.AddSeconds(1));
        }

        #endregion

        #region CreatePaginatedResponse Tests

        [Fact]
        public void CreatePaginatedResponse_WithBasicParameters_ReturnsFormattedPaginatedResponse()
        {
            // Arrange
            var items = new[] { 1, 2, 3, 4, 5 };
            int page = 1;
            int pageSize = 5;

            // Act
            var response = ResponseHelper.CreatePaginatedResponse(items, page, pageSize);
            var responseType = response.GetType();

            // Assert
            var successProp = responseType.GetProperty("success");
            var dataProp = responseType.GetProperty("data");
            var paginationProp = responseType.GetProperty("pagination");

            Assert.True((bool)successProp!.GetValue(response)!);
            Assert.Equal(items, dataProp!.GetValue(response));
            Assert.NotNull(paginationProp!.GetValue(response));
        }

        [Fact]
        public void CreatePaginatedResponse_VerifyPaginationDetails_ContainsCorrectMetadata()
        {
            // Arrange
            var items = new[] { "a", "b", "c" };
            int page = 2;
            int pageSize = 10;
            int totalCount = 25;

            // Act
            var response = ResponseHelper.CreatePaginatedResponse(items, page, pageSize, totalCount);
            var responseType = response.GetType();

            // Assert
            var paginationProp = responseType.GetProperty("pagination");
            var paginationObj = paginationProp!.GetValue(response);
            var paginationType = paginationObj!.GetType();

            var pageProp = paginationType.GetProperty("page");
            var pageSizeProp = paginationType.GetProperty("pageSize");
            var countProp = paginationType.GetProperty("count");
            var totalCountProp = paginationType.GetProperty("totalCount");

            Assert.Equal(page, pageProp!.GetValue(paginationObj));
            Assert.Equal(pageSize, pageSizeProp!.GetValue(paginationObj));
            Assert.Equal(items.Length, countProp!.GetValue(paginationObj));
            Assert.Equal(totalCount, totalCountProp!.GetValue(paginationObj));
        }

        [Fact]
        public void CreatePaginatedResponse_WithoutTotalCount_TotalCountIsNull()
        {
            // Arrange
            var items = new[] { 1, 2 };

            // Act
            var response = ResponseHelper.CreatePaginatedResponse(items, 1, 2);
            var responseType = response.GetType();

            // Assert
            var paginationProp = responseType.GetProperty("pagination");
            var paginationObj = paginationProp!.GetValue(response);
            var paginationType = paginationObj!.GetType();
            var totalCountProp = paginationType.GetProperty("totalCount");

            Assert.Null(totalCountProp!.GetValue(paginationObj));
        }

        [Fact]
        public void CreatePaginatedResponse_EmptyArray_ReturnsValidResponse()
        {
            // Arrange
            var items = Array.Empty<string>();

            // Act
            var response = ResponseHelper.CreatePaginatedResponse(items, 1, 10);
            var responseType = response.GetType();

            // Assert
            var successProp = responseType.GetProperty("success");
            var dataProp = responseType.GetProperty("data");
            var paginationProp = responseType.GetProperty("pagination");
            var paginationObj = paginationProp!.GetValue(response);
            var paginationType = paginationObj!.GetType();
            var countProp = paginationType.GetProperty("count");

            Assert.True((bool)successProp!.GetValue(response)!);
            Assert.Empty((string[])dataProp!.GetValue(response)!);
            Assert.Equal(0, countProp!.GetValue(paginationObj));
        }

        #endregion
    }
}
