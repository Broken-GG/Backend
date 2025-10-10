using DotNetEnv;
using api.service;
using api.test;
using System.Reflection;
using System.Linq;

Console.WriteLine("🚀 Starting Integration Test Runner...");
Console.WriteLine("========================================");

// Load environment variables from .env file
Console.WriteLine("📁 Loading environment variables...");
Env.Load("src/.env");

try
{
    // Create an instance of the integration test class
    Console.WriteLine("🧪 Initializing Integration Tests...");
    var integrationTests = new SummonerInfoIntegrationTests();
    
    // Get all test methods from the class
    var testMethods = typeof(SummonerInfoIntegrationTests)
        .GetMethods()
        .Where(m => m.GetCustomAttributes(typeof(Xunit.FactAttribute), false).Any())
        .ToList();

    Console.WriteLine($"📋 Found {testMethods.Count} integration tests to run:");
    
    int passedTests = 0;
    int failedTests = 0;

    foreach (var method in testMethods)
    {
        Console.WriteLine($"\n🔬 Running test: {method.Name}");
        Console.WriteLine(new string('=', 60));
        
        try
        {
            // Run the test method
            var task = (Task)method.Invoke(integrationTests, null)!;
            await task;
            
            Console.WriteLine($"✅ PASSED: {method.Name}");
            passedTests++;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FAILED: {method.Name}");
            Console.WriteLine($"💥 Error: {ex.GetBaseException().Message}");
            failedTests++;
        }
        
        Console.WriteLine(new string('=', 60));
    }

    // Summary
    Console.WriteLine($"\n📊 TEST SUMMARY:");
    Console.WriteLine($"   ✅ Passed: {passedTests}");
    Console.WriteLine($"   ❌ Failed: {failedTests}");
    Console.WriteLine($"   📈 Total:  {passedTests + failedTests}");
    
    if (failedTests == 0)
    {
        Console.WriteLine($"\n🎉 ALL TESTS PASSED! Your API integration is working perfectly!");
    }
    else
    {
        Console.WriteLine($"\n⚠️  Some tests failed. Check the output above for details.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"💥 Failed to run integration tests: {ex.Message}");
    Console.WriteLine($"� Make sure your API key is valid and network is available.");
}

Console.WriteLine($"\n🏁 Integration test run completed. Press any key to exit...");
Console.ReadKey();