///////////////////////////////////////////////////////////////////////////////
// INSTALL ADDINS & TOOLS
///////////////////////////////////////////////////////////////////////////////

// Eg: #addin nuget:?package=PackageName&version=1.1.x

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

#nullable enable // Enable C# nullability

// Flag indicating if the test coverage results should be opened automatically.
// Default value `false`.
readonly bool OPEN_COVERAGE_RESULTS = HasArgument("open-coverage-results");

// Configuration can have a value of "Release" or "Debug".
// Default configuration `Release`.
readonly string CONFIGURATION = Argument<string>("configuration", "Release");

// Environment can have a value of "Production", "Development", "Test" or "Local".
// Default environment `(string)null`.
readonly string? ENVIRONMENT = Argument<string?>("environment", null);

// Flag indicating if the test coverage results should be removed or not.
// Default value `false`.
readonly bool REMOVE_COVERAGE_ARTIFACTS = HasArgument("remove-coverage-artifacts");

#nullable disable // Disable C# nullability

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    // Executed BEFORE the first task.
    Information("Running tasks...");
});

Teardown(ctx =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

readonly var artifactsDirectory = Directory("./artifacts");
readonly var testsArtifactsDirectory = Directory($"{artifactsDirectory}/tests");
readonly var coverageArtifactsDirectory = Directory($"{artifactsDirectory}/coverage");

void DeleteDirectoriesByPattern(string pattern)
{
    foreach (var directory in GetDirectories(pattern))
        DeleteDirectory(directory);
}

void DeleteDirectory(DirectoryPath directory)
{
    if (!DirectoryExists(directory))
        return;

    Information($"Removing directory '{directory}'");
    DeleteDirectories(new DirectoryPath[] { directory }, new DeleteDirectorySettings() { Force = true, Recursive = true });
}

Task("clean")
    .Description("Cleans the artifacts, bin and obj directories.")
    .Does(() => {
        DeleteDirectory(testsArtifactsDirectory);

        if (REMOVE_COVERAGE_ARTIFACTS)
            DeleteDirectory(coverageArtifactsDirectory);

        DeleteDirectoriesByPattern($"./**/bin/{CONFIGURATION}");
        DeleteDirectoriesByPattern($"./**/obj/{CONFIGURATION}");
    });

Task("restore")
    .Description("Restores NuGet packages.")
    .Does(() => {
        DotNetRestore();
    });

Task("build")
    .Description("Builds the solution.")
    .IsDependentOn("restore")
    .Does(() => {
        DotNetBuild(".", new DotNetBuildSettings {
            Configuration = CONFIGURATION,
            NoLogo = true,
            NoRestore = true
        });
    });

Task("unit-tests")
    .Description($"Runs unit tests.")
    .IsDependentOn("build")
    .DoesForEach(GetFiles("./tests/**/*UnitTests.csproj"), project => {
        DotNetTest(project.ToString(), new DotNetTestSettings {
            Configuration = CONFIGURATION,
            NoLogo = true,
            NoRestore = true,
            NoBuild = true,
            ToolTimeout = TimeSpan.FromMinutes(5),
            Blame = true,
            Loggers = new string[] { "trx" },
            Collectors = new string[] { "XPlat Code Coverage" },
            ResultsDirectory = testsArtifactsDirectory
        });
    })
    .DeferOnError();

Task("integration-tests")
    .Description($"Runs integration tests.")
    .IsDependentOn("build")
    .DoesForEach(GetFiles("./tests/**/*IntegrationTests.csproj"), project => {
        DotNetTest(project.ToString(), new DotNetTestSettings {
            EnvironmentVariables = new Dictionary<string, string> {
                { "ASPNETCORE_ENVIRONMENT", ENVIRONMENT ?? "Test" }
            },
            Configuration = CONFIGURATION,
            NoLogo = true,
            NoRestore = true,
            NoBuild = false,
            ToolTimeout = TimeSpan.FromMinutes(5),
            Blame = true,
            Loggers = new string[] { "trx" },
            Collectors = new string[] { "XPlat Code Coverage" },
            ResultsDirectory = testsArtifactsDirectory
        });
    })
    .DeferOnError();

Task("acceptance-tests")
    .Description($"Runs acceptance tests.")
    .IsDependentOn("build")
    .DoesForEach(GetFiles("./tests/**/*AcceptanceTests.csproj"), project => {
        DotNetTest(project.ToString(), new DotNetTestSettings {
            EnvironmentVariables = new Dictionary<string, string> {
                { "ASPNETCORE_ENVIRONMENT", ENVIRONMENT ?? "Test" }
            },
            Configuration = CONFIGURATION,
            NoLogo = true,
            NoRestore = true,
            NoBuild = false,
            ToolTimeout = TimeSpan.FromMinutes(10),
            Blame = true,
            Loggers = new string[] { "trx" },
            Collectors = new string[] { "XPlat Code Coverage" },
            ResultsDirectory = testsArtifactsDirectory
        });
    })
    .DeferOnError();

Task("code-coverage")
    .Description($"Generate code coverage reports.")
    .Does(() => {
        var reportTypes = new string[]
        {
            "Badges",
            // "Clover",
            "Cobertura",
            // "CsvSummary",
            // "Html",
            // "Html_Dark",
            // "Html_Light",
            // "HtmlChart",
            // "HtmlInline",
            // "HtmlInline_AzurePipelines",
            "HtmlInline_AzurePipelines_Dark",
            // "HtmlInline_AzurePipelines_Light",
            "HtmlSummary",
            "JsonSummary",
            // "Latex",
            // "LatexSummary",
            // "lcov",
            "MarkdownSummary",
            // "MHtml",
            // "PngChart",
            // "SonarQube",
            // "TeamCitySummary",
            "TextSummary",
            "Xml",
            "XmlSummary"
        };

        var cmdCommand = $"reportgenerator" +
                         $" -verbosity:Warning" +
                         $" -title:OneTimePassGen" +
                         $" -reports:{testsArtifactsDirectory}/**/coverage.cobertura.xml" +
                         $" -targetdir:{coverageArtifactsDirectory}" +
                         $" -reporttypes:{string.Join(';', reportTypes)}";

        // Generate nice human readable coverage report
        DotNetTool(cmdCommand);

        var coverageIndexFilePath = $"{coverageArtifactsDirectory}/index.htm";

        Information($"Test coverage report generated to {coverageIndexFilePath}.");

        if (!FileExists(coverageIndexFilePath))
        {
            Warning($"File {coverageIndexFilePath} dose't exist.");
            return;
        }

        var canShowResults = IsRunningOnWindows() && BuildSystem.IsLocalBuild;
        var shouldShowResults = canShowResults && OPEN_COVERAGE_RESULTS;
        if (shouldShowResults)
        {
            StartProcess("cmd", new ProcessSettings {
                Arguments = $"/C start \"\" {coverageIndexFilePath}"
            });
        }
        else if (canShowResults)
        {
            Information($"Generated coverage results to {coverageIndexFilePath}.");
            Information($"Using '--open-coverage-results' option the code coverage will open automatically in your default browser.");
        }
    })
    .DeferOnError();

Task("publish-test-reports")
    .WithCriteria(() => BuildSystem.AzurePipelines.IsRunningOnAzurePipelines)
    .Description("Publish test reports to Azure Pipelines.")
    .Does(() => {
        var testResultsFiles = GetFiles($"{testsArtifactsDirectory}/**/*.trx").ToArray();
        if (!testResultsFiles.Any())
        {
            Warning($"No test results was found, no local report is generated.");
            return;
        }

        BuildSystem.AzurePipelines.Commands.PublishTestResults(
            new AzurePipelinesPublishTestResultsData {
                Configuration = CONFIGURATION,
                TestResultsFiles = testResultsFiles,
                MergeTestResults = true,
                TestRunner = AzurePipelinesTestRunnerType.VSTest
            }
        );
    });

Task("publish-code-coverage-reports")
    .WithCriteria(() => BuildSystem.AzurePipelines.IsRunningOnAzurePipelines)
    .Description("Publish code coverage reports to Azure Pipelines.")
    .Does(() => {
        if (!FileExists($"{coverageArtifactsDirectory}/Cobertura.xml"))
        {
            Warning($"No coverage results was found, no local report is generated.");
            return;
        }

        BuildSystem.AzurePipelines.Commands.PublishCodeCoverage(
            new AzurePipelinesPublishCodeCoverageData {
                CodeCoverageTool = AzurePipelinesCodeCoverageToolType.Cobertura,
                ReportDirectory = $"{coverageArtifactsDirectory}",
                SummaryFileLocation = $"{coverageArtifactsDirectory}/Cobertura.xml"
            }
        );
    });

Task("test")
    .IsDependentOn("clean")
    .IsDependentOn("unit-tests")
    .IsDependentOn("integration-tests")
    .IsDependentOn("acceptance-tests")
    .IsDependentOn("code-coverage");

Task("default")
    .IsDependentOn("clean")
    .IsDependentOn("unit-tests")
    .IsDependentOn("integration-tests")
    .IsDependentOn("acceptance-tests")
    .IsDependentOn("publish-test-reports")
    .IsDependentOn("code-coverage")
    .IsDependentOn("publish-code-coverage-reports");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(Argument("task", "default"));
