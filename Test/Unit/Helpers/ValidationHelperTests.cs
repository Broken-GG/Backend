using Xunit;
using Backend.Helpers;

namespace Backend.Tests.Unit.Helpers
{
    /// <summary>
    /// Unit tests for ValidationHelper
    /// Tests cover all validation methods with valid and invalid inputs
    /// </summary>
    public class ValidationHelperTests
    {
        #region IsValidSummonerName Tests

        [Theory]
        [InlineData("Player123")] // Valid name
        [InlineData("Test Player")] // With space
        [InlineData("abc")] // Minimum length (3)
        [InlineData("SixteenCharacter")] // Maximum length (16)
        [InlineData("Player_Name")] // With underscore
        public void IsValidSummonerName_ValidNames_ReturnsTrue(string summonerName)
        {
            // Act
            bool result = ValidationHelper.IsValidSummonerName(summonerName);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("")] // Empty string
        [InlineData("   ")] // Whitespace
        [InlineData(null)] // Null
        [InlineData("ab")] // Too short (< 3)
        [InlineData("ThisNameIsTooLong")] // Too long (> 16)
        [InlineData("Player@123")] // Invalid character (@)
        [InlineData("Player#Name")] // Invalid character (#)
        public void IsValidSummonerName_InvalidNames_ReturnsFalse(string? summonerName)
        {
            // Act
            bool result = ValidationHelper.IsValidSummonerName(summonerName);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsValidTagline Tests

        [Theory]
        [InlineData("EUW")] // Valid tagline
        [InlineData("NA1")] // Valid with number
        [InlineData("ab")] // Minimum length (2)
        [InlineData("ABCDEFGHIJ")] // Maximum length (10)
        public void IsValidTagline_ValidTaglines_ReturnsTrue(string tagline)
        {
            // Act
            bool result = ValidationHelper.IsValidTagline(tagline);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("")] // Empty string
        [InlineData("   ")] // Whitespace
        [InlineData(null)] // Null
        [InlineData("A")] // Too short (< 2)
        [InlineData("ThisIsTooLong1")] // Too long (> 10)
        [InlineData("EU-W")] // Invalid character (-)
        [InlineData("NA_1")] // Invalid character (_)
        public void IsValidTagline_InvalidTaglines_ReturnsFalse(string? tagline)
        {
            // Act
            bool result = ValidationHelper.IsValidTagline(tagline);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsValidPuuid Tests

        [Theory]
        [InlineData("test-puuid-1234567890-abcdef-1234567890-abcdef-1234567890")] // Valid PUUID with hyphens (62 chars)
        [InlineData("testpuuid1234567890abcdef1234567890abcdef1234567890abcdefgh")] // Valid PUUID without hyphens (60 chars)
        [InlineData("a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6a7b8c9d0")] // 60 chars alphanumeric
        public void IsValidPuuid_ValidPuuids_ReturnsTrue(string puuid)
        {
            // Act
            bool result = ValidationHelper.IsValidPuuid(puuid);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("")] // Empty string
        [InlineData("   ")] // Whitespace
        [InlineData(null)] // Null
        [InlineData("short-puuid")] // Too short (< 50)
        [InlineData("abcdefghij")] // Only 10 chars
        [InlineData("test@puuid#123$456%789^012&345*678(901)234+567-890")] // Invalid characters (60 chars but with special chars)
        public void IsValidPuuid_InvalidPuuids_ReturnsFalse(string? puuid)
        {
            // Act
            bool result = ValidationHelper.IsValidPuuid(puuid);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsValidPaginationStart Tests

        [Theory]
        [InlineData(0)] // Zero (valid)
        [InlineData(1)] // Positive
        [InlineData(100)] // Large positive
        public void IsValidPaginationStart_ValidStartValues_ReturnsTrue(int start)
        {
            // Act
            bool result = ValidationHelper.IsValidPaginationStart(start);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(-1)] // Negative
        [InlineData(-10)] // Large negative
        public void IsValidPaginationStart_NegativeValues_ReturnsFalse(int start)
        {
            // Act
            bool result = ValidationHelper.IsValidPaginationStart(start);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsValidPaginationCount Tests

        [Theory]
        [InlineData(1)] // Minimum valid
        [InlineData(10)] // Normal value
        [InlineData(100)] // Maximum default
        public void IsValidPaginationCount_ValidCounts_ReturnsTrue(int count)
        {
            // Act
            bool result = ValidationHelper.IsValidPaginationCount(count);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(0)] // Too low
        [InlineData(-1)] // Negative
        [InlineData(101)] // Too high (exceeds default max of 100)
        [InlineData(1000)] // Way too high
        public void IsValidPaginationCount_InvalidCounts_ReturnsFalse(int count)
        {
            // Act
            bool result = ValidationHelper.IsValidPaginationCount(count);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidPaginationCount_CustomMaxCount_ValidatesCorrectly()
        {
            // Arrange
            int customMax = 50;

            // Act & Assert
            Assert.True(ValidationHelper.IsValidPaginationCount(25, customMax)); // Within custom limit
            Assert.True(ValidationHelper.IsValidPaginationCount(50, customMax)); // At custom limit
            Assert.False(ValidationHelper.IsValidPaginationCount(51, customMax)); // Exceeds custom limit
        }

        #endregion

        #region ValidatePagination Tests

        [Fact]
        public void ValidatePagination_ValidParameters_ReturnsTrue()
        {
            // Act
            var (isValid, errorMessage) = ValidationHelper.ValidatePagination(0, 10);

            // Assert
            Assert.True(isValid);
            Assert.Null(errorMessage);
        }

        [Fact]
        public void ValidatePagination_NegativeStart_ReturnsFalseWithMessage()
        {
            // Act
            var (isValid, errorMessage) = ValidationHelper.ValidatePagination(-1, 10);

            // Assert
            Assert.False(isValid);
            Assert.Equal("Start index cannot be negative", errorMessage);
        }

        [Fact]
        public void ValidatePagination_CountTooLow_ReturnsFalseWithMessage()
        {
            // Act
            var (isValid, errorMessage) = ValidationHelper.ValidatePagination(0, 0);

            // Assert
            Assert.False(isValid);
            Assert.Equal("Count must be at least 1", errorMessage);
        }

        [Fact]
        public void ValidatePagination_CountTooHigh_ReturnsFalseWithMessage()
        {
            // Act
            var (isValid, errorMessage) = ValidationHelper.ValidatePagination(0, 101);

            // Assert
            Assert.False(isValid);
            Assert.Equal("Count cannot exceed 100", errorMessage);
        }

        [Fact]
        public void ValidatePagination_CustomMaxCount_ValidatesCorrectly()
        {
            // Arrange
            int customMax = 50;

            // Act
            var (isValid, errorMessage) = ValidationHelper.ValidatePagination(0, 51, customMax);

            // Assert
            Assert.False(isValid);
            Assert.Equal("Count cannot exceed 50", errorMessage);
        }

        #endregion

        #region SanitizeInput Tests

        [Theory]
        [InlineData("NormalText", "NormalText")] // No changes needed
        [InlineData("Text With Spaces", "Text With Spaces")] // Spaces preserved
        [InlineData("  Leading and trailing  ", "Leading and trailing")] // Trimmed
        [InlineData("Text123", "Text123")] // Alphanumeric preserved
        public void SanitizeInput_ValidText_ReturnsCleanText(string input, string expected)
        {
            // Act
            string result = ValidationHelper.SanitizeInput(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("", "")] // Empty string
        [InlineData("   ", "")] // Whitespace only
        [InlineData(null, "")] // Null
        public void SanitizeInput_EmptyOrNull_ReturnsEmptyString(string? input, string expected)
        {
            // Act
            string result = ValidationHelper.SanitizeInput(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SanitizeInput_ControlCharacters_RemovesControlCharacters()
        {
            // Arrange
            string input = "Text\nWith\rControl\tCharacters";

            // Act
            string result = ValidationHelper.SanitizeInput(input);

            // Assert
            Assert.DoesNotContain("\n", result);
            Assert.DoesNotContain("\r", result);
            Assert.DoesNotContain("\t", result);
            Assert.Equal("TextWithControlCharacters", result);
        }

        #endregion
    }
}
