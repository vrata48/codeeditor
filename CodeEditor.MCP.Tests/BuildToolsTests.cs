using CodeEditor.MCP.Models;
using CodeEditor.MCP.Services;
using CodeEditor.MCP.Tools;
using FluentAssertions;
using Moq;
using System.Text.Json;

namespace CodeEditor.MCP.Tests;

public class BuildToolsTests
{
    private readonly Mock<IDotNetService> _mockDotNetService;

    public BuildToolsTests()
    {
        _mockDotNetService = new Mock<IDotNetService>();
    }

    [Fact]
    public async Task BuildProject_Success_ReturnsSuccessResult()
    {
        // Arrange
        var path = "project.csproj";
        var buildResult = new BuildResult
        {
            Success = true,
            ExitCode = 0,
            Duration = TimeSpan.FromSeconds(5),
            Output = "Build succeeded",
            Errors = "",
            ParsedErrors = new List<BuildError>()
        };

        _mockDotNetService.Setup(x => x.BuildProjectAsync(path))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.BuildProject(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("\"success\": true");
        result.Should().Contain("\"exitCode\": 0");
        result.Should().Contain("\"errorCount\": 0");
        result.Should().NotContain("Build succeeded"); // Output should not be included on success
        result.Should().NotContain("duration"); // Duration should be removed
        _mockDotNetService.Verify(x => x.BuildProjectAsync(path), Times.Once);
    }

    [Fact]
    public async Task BuildProject_Failure_ReturnsFailureResultWithOutput()
    {
        // Arrange
        var path = "project.csproj";
        var buildResult = new BuildResult
        {
            Success = false,
            ExitCode = 1,
            Duration = TimeSpan.FromSeconds(3),
            Output = "Build failed",
            Errors = "Compilation error",
            ParsedErrors = new List<BuildError>
            {
                new BuildError
                {
                    File = "Program.cs",
                    Line = 10,
                    Column = 5,
                    ErrorCode = "CS0103",
                    Message = "The name 'variable' does not exist in the current context",
                    Severity = "Error"
                }
            }
        };

        _mockDotNetService.Setup(x => x.BuildProjectAsync(path))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.BuildProject(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("\"success\": false");
        result.Should().Contain("\"exitCode\": 1");
        result.Should().Contain("\"errorCount\": 1");
        result.Should().Contain("Build failed"); // Output should be included on failure
        result.Should().Contain("Compilation error"); // Errors should be included
        result.Should().Contain("Error CS0103"); // Parsed errors should be included
        result.Should().Contain("Program.cs(10,5)");
        result.Should().NotContain("duration"); // Duration should be removed
        _mockDotNetService.Verify(x => x.BuildProjectAsync(path), Times.Once);
    }

    [Fact]
    public async Task BuildSolution_Success_ReturnsSuccessResult()
    {
        // Arrange
        var path = "solution.sln";
        var buildResult = new BuildResult
        {
            Success = true,
            ExitCode = 0,
            Duration = TimeSpan.FromSeconds(10),
            Output = "Solution built successfully",
            Errors = "",
            ParsedErrors = new List<BuildError>()
        };

        _mockDotNetService.Setup(x => x.BuildSolutionAsync(path))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.BuildSolution(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("\"success\": true");
        result.Should().Contain("\"exitCode\": 0");
        result.Should().Contain("\"errorCount\": 0");
        result.Should().NotContain("Solution built successfully"); // Output should not be included on success
        _mockDotNetService.Verify(x => x.BuildSolutionAsync(path), Times.Once);
    }

    [Fact]
    public async Task CleanProject_Success_ReturnsSuccessResult()
    {
        // Arrange
        var path = "project.csproj";
        var buildResult = new BuildResult
        {
            Success = true,
            ExitCode = 0,
            Duration = TimeSpan.FromSeconds(2),
            Output = "Clean succeeded",
            Errors = "",
            ParsedErrors = new List<BuildError>()
        };

        _mockDotNetService.Setup(x => x.CleanProjectAsync(path))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.CleanProject(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("\"success\": true");
        result.Should().Contain("\"exitCode\": 0");
        result.Should().Contain("\"errorCount\": 0");
        result.Should().NotContain("Clean succeeded"); // Output should not be included on success
        _mockDotNetService.Verify(x => x.CleanProjectAsync(path), Times.Once);
    }

    [Fact]
    public async Task CleanSolution_Success_ReturnsSuccessResult()
    {
        // Arrange
        var path = "solution.sln";
        var buildResult = new BuildResult
        {
            Success = true,
            ExitCode = 0,
            Duration = TimeSpan.FromSeconds(3),
            Output = "Solution cleaned",
            Errors = "",
            ParsedErrors = new List<BuildError>()
        };

        _mockDotNetService.Setup(x => x.CleanSolutionAsync(path))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.CleanSolution(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("\"success\": true");
        result.Should().Contain("\"exitCode\": 0");
        result.Should().Contain("\"errorCount\": 0");
        result.Should().NotContain("Solution cleaned"); // Output should not be included on success
        _mockDotNetService.Verify(x => x.CleanSolutionAsync(path), Times.Once);
    }

    [Fact]
    public async Task RestorePackages_Success_ReturnsSuccessResult()
    {
        // Arrange
        var path = "project.csproj";
        var buildResult = new BuildResult
        {
            Success = true,
            ExitCode = 0,
            Duration = TimeSpan.FromSeconds(15),
            Output = "Restore completed successfully",
            Errors = "",
            ParsedErrors = new List<BuildError>()
        };

        _mockDotNetService.Setup(x => x.RestorePackagesAsync(path))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.RestorePackages(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("\"success\": true");
        result.Should().Contain("\"exitCode\": 0");
        result.Should().Contain("\"errorCount\": 0");
        result.Should().NotContain("Restore completed successfully"); // Output should not be included on success
        _mockDotNetService.Verify(x => x.RestorePackagesAsync(path), Times.Once);
    }

    [Fact]
    public async Task RestorePackages_Failure_ReturnsFailureResultWithOutput()
    {
        // Arrange
        var path = "project.csproj";
        var buildResult = new BuildResult
        {
            Success = false,
            ExitCode = 1,
            Duration = TimeSpan.FromSeconds(5),
            Output = "Restore failed",
            Errors = "Package not found",
            ParsedErrors = new List<BuildError>
            {
                new BuildError
                {
                    File = "project.csproj",
                    Line = 15,
                    Column = 4,
                    ErrorCode = "NU1101",
                    Message = "Unable to find package 'NonExistentPackage'",
                    Severity = "Error"
                }
            }
        };

        _mockDotNetService.Setup(x => x.RestorePackagesAsync(path))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.RestorePackages(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("\"success\": false");
        result.Should().Contain("\"exitCode\": 1");
        result.Should().Contain("\"errorCount\": 1");
        result.Should().Contain("Restore failed"); // Output should be included on failure
        result.Should().Contain("Package not found"); // Errors should be included
        result.Should().Contain("Error NU1101"); // Parsed errors should be included
        _mockDotNetService.Verify(x => x.RestorePackagesAsync(path), Times.Once);
    }

    [Fact]
    public async Task RunTests_Success_ReturnsTestResult()
    {
        // Arrange
        var path = "tests.csproj";
        var testResult = new TestResult
        {
            Success = true,
            ExitCode = 0,
            Duration = TimeSpan.FromSeconds(8),
            Output = "Tests passed",
            Errors = "",
            ParsedErrors = new List<BuildError>(),
            TotalTests = 10,
            TestsPassed = 10,
            TestsFailed = 0,
            TestsSkipped = 0,
            FailedTests = new List<FailedTest>()
        };

        _mockDotNetService.Setup(x => x.RunTestsAsync(path))
            .ReturnsAsync(testResult);

        // Act
        var result = await BuildTools.RunTests(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("Test Result:");
        result.Should().Contain("\"success\": true");
        result.Should().Contain("\"totalTests\": 10");
        result.Should().Contain("\"passed\": 10");
        result.Should().Contain("\"failed\": 0");
        result.Should().Contain("\"skipped\": 0");
        result.Should().NotContain("Tests passed"); // Output should not be included on success
        result.Should().NotContain("duration"); // Duration should be removed
        _mockDotNetService.Verify(x => x.RunTestsAsync(path), Times.Once);
    }

    [Fact]
    public async Task RunTests_Failure_ReturnsTestResultWithFailures()
    {
        // Arrange
        var path = "tests.csproj";
        var testResult = new TestResult
        {
            Success = false,
            ExitCode = 1,
            Duration = TimeSpan.FromSeconds(6),
            Output = "Some tests failed",
            Errors = "Test execution errors",
            ParsedErrors = new List<BuildError>(),
            TotalTests = 10,
            TestsPassed = 8,
            TestsFailed = 2,
            TestsSkipped = 0,
            FailedTests = new List<FailedTest>
            {
                new FailedTest
                {
                    TestName = "TestMethod1",
                    ClassName = "TestClass",
                    ErrorMessage = "Assert.Equal() Failure",
                    StackTrace = "   at TestClass.TestMethod1() in TestClass.cs:line 25"
                },
                new FailedTest
                {
                    TestName = "TestMethod2",
                    ClassName = "TestClass",
                    ErrorMessage = "Null reference exception",
                    StackTrace = "   at TestClass.TestMethod2() in TestClass.cs:line 35"
                }
            }
        };

        _mockDotNetService.Setup(x => x.RunTestsAsync(path))
            .ReturnsAsync(testResult);

        // Act
        var result = await BuildTools.RunTests(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("Test Result:");
        result.Should().Contain("\"success\": false");
        result.Should().Contain("\"totalTests\": 10");
        result.Should().Contain("\"passed\": 8");
        result.Should().Contain("\"failed\": 2");
        result.Should().Contain("Some tests failed"); // Output should be included on failure
        result.Should().Contain("Test execution errors"); // Errors should be included
        result.Should().Contain("Failed Tests:");
        result.Should().Contain("TestMethod1");
        result.Should().Contain("Assert.Equal() Failure");
        result.Should().Contain("TestMethod2");
        result.Should().Contain("Null reference exception");
        _mockDotNetService.Verify(x => x.RunTestsAsync(path), Times.Once);
    }

    [Fact]
    public async Task RunTestsFiltered_Success_ReturnsFilteredTestResult()
    {
        // Arrange
        var path = "tests.csproj";
        var filter = "ClassName=UnitTests";
        var testResult = new TestResult
        {
            Success = true,
            ExitCode = 0,
            Duration = TimeSpan.FromSeconds(3),
            Output = "Filtered tests passed",
            Errors = "",
            ParsedErrors = new List<BuildError>(),
            TotalTests = 5,
            TestsPassed = 5,
            TestsFailed = 0,
            TestsSkipped = 0,
            FailedTests = new List<FailedTest>()
        };

        _mockDotNetService.Setup(x => x.RunTestsAsync(path, filter))
            .ReturnsAsync(testResult);

        // Act
        var result = await BuildTools.RunTestsFiltered(_mockDotNetService.Object, path, filter);

        // Assert
        result.Should().Contain("Test Result:");
        result.Should().Contain("\"success\": true");
        result.Should().Contain("\"totalTests\": 5");
        result.Should().Contain("\"passed\": 5");
        result.Should().Contain("\"failed\": 0");
        result.Should().NotContain("Filtered tests passed"); // Output should not be included on success
        _mockDotNetService.Verify(x => x.RunTestsAsync(path, filter), Times.Once);
    }

    [Fact]
    public async Task PublishProject_Success_ReturnsSuccessResult()
    {
        // Arrange
        var path = "project.csproj";
        var outputPath = "publish/";
        var buildResult = new BuildResult
        {
            Success = true,
            ExitCode = 0,
            Duration = TimeSpan.FromSeconds(20),
            Output = "Publish succeeded",
            Errors = "",
            ParsedErrors = new List<BuildError>()
        };

        _mockDotNetService.Setup(x => x.PublishProjectAsync(path, outputPath))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.PublishProject(_mockDotNetService.Object, path, outputPath);

        // Assert
        result.Should().Contain("\"success\": true");
        result.Should().Contain("\"exitCode\": 0");
        result.Should().Contain("\"errorCount\": 0");
        result.Should().NotContain("Publish succeeded"); // Output should not be included on success
        _mockDotNetService.Verify(x => x.PublishProjectAsync(path, outputPath), Times.Once);
    }

    [Fact]
    public async Task PublishProject_WithoutOutputPath_CallsServiceWithNullOutputPath()
    {
        // Arrange
        var path = "project.csproj";
        var buildResult = new BuildResult
        {
            Success = true,
            ExitCode = 0,
            Duration = TimeSpan.FromSeconds(15),
            Output = "Publish succeeded",
            Errors = "",
            ParsedErrors = new List<BuildError>()
        };

        _mockDotNetService.Setup(x => x.PublishProjectAsync(path, null))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.PublishProject(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("\"success\": true");
        _mockDotNetService.Verify(x => x.PublishProjectAsync(path, null), Times.Once);
    }

    [Fact]
    public async Task FormatBuildResult_WithMultipleErrors_FormatsAllErrors()
    {
        // Arrange
        var path = "project.csproj";
        var buildResult = new BuildResult
        {
            Success = false,
            ExitCode = 1,
            Duration = TimeSpan.FromSeconds(5),
            Output = "Build output",
            Errors = "Build errors",
            ParsedErrors = new List<BuildError>
            {
                new BuildError
                {
                    File = "Class1.cs",
                    Line = 10,
                    Column = 5,
                    ErrorCode = "CS0103",
                    Message = "Error message 1",
                    Severity = "Error"
                },
                new BuildError
                {
                    File = "Class2.cs",
                    Line = 20,
                    Column = 15,
                    ErrorCode = "CS0104",
                    Message = "Error message 2",
                    Severity = "Warning"
                }
            }
        };

        _mockDotNetService.Setup(x => x.BuildProjectAsync(path))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.BuildProject(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("\"errorCount\": 2");
        result.Should().Contain("Error CS0103: Error message 1");
        result.Should().Contain("Class1.cs(10,5)");
        result.Should().Contain("Warning CS0104: Error message 2");
        result.Should().Contain("Class2.cs(20,15)");
    }

    [Fact]
    public async Task FormatBuildResult_WithNoErrors_DoesNotIncludeErrorSection()
    {
        // Arrange
        var path = "project.csproj";
        var buildResult = new BuildResult
        {
            Success = false,
            ExitCode = 1,
            Duration = TimeSpan.FromSeconds(5),
            Output = "Build output",
            Errors = "",
            ParsedErrors = new List<BuildError>()
        };

        _mockDotNetService.Setup(x => x.BuildProjectAsync(path))
            .ReturnsAsync(buildResult);

        // Act
        var result = await BuildTools.BuildProject(_mockDotNetService.Object, path);

        // Assert
        result.Should().Contain("\"success\": false");
        result.Should().Contain("\"errorCount\": 0");
        result.Should().Contain("Build output"); // Should still include output on failure
        result.Should().NotContain("Errors:"); // Should not have errors section
        result.Should().NotContain("Parsed Errors:"); // Should not have parsed errors section
    }
}
