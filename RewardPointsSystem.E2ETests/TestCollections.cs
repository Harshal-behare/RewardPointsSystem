using Xunit;

namespace RewardPointsSystem.E2ETests;

/// <summary>
/// Test collection definitions for controlling parallel execution.
/// Tests in the same collection run sequentially; different collections run in parallel.
/// </summary>
/// 
// Authentication tests can run in parallel with other collections
[CollectionDefinition("AuthenticationTests")]
public class AuthenticationTestsCollection { }

// Admin tests share potential state, run sequentially within collection
[CollectionDefinition("AdminTests")]
public class AdminTestsCollection { }

// Employee tests share potential state, run sequentially within collection
[CollectionDefinition("EmployeeTests")]
public class EmployeeTestsCollection { }

// Cross-browser tests
[CollectionDefinition("CrossBrowserTests")]
public class CrossBrowserTestsCollection { }

// Tests that require sequential execution
[CollectionDefinition("SequentialTests", DisableParallelization = true)]
public class SequentialTestsCollection { }
